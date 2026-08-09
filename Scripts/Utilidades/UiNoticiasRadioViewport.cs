using Godot;
using System;
using System.Collections.Generic;
using fiveyears3.Scripts.Globais;

public partial class UiNoticiasRadioViewport : CanvasLayer
{
    [Export] public ItemList ListaNoticias;

    private readonly List<ItemRadioEntry> _itensExibidos = new();
    private bool _inscritoNoGerenciador;

    private enum TipoItemRadio { Noticia, Musica }

    private sealed class ItemRadioEntry
    {
        public TipoItemRadio Tipo { get; init; }
        public NoticiaModel Noticia { get; init; }
        public MusicaModel Musica { get; init; }
    }

    public override void _Ready()
    {
        TentarInscreverNoGerenciador();

        if (ListaNoticias != null)
        {
            ListaNoticias.ItemSelected += OnNoticiaSelecionada;
        }

        VisibilityChanged += OnVisibilityChanged;
        AtualizarLista();
    }

    public override void _Process(double delta)
    {
        if (_inscritoNoGerenciador) return;
        TentarInscreverNoGerenciador();
    }

    public override void _ExitTree()
    {
        if (GerenciadorNoticiasImpressas.Instance == null || !_inscritoNoGerenciador) return;

        GerenciadorNoticiasImpressas.Instance.NoticiaImpressa -= OnNoticiaRecebida;
        GerenciadorNoticiasImpressas.Instance.NoticiaRemovidaDaFila -= OnNoticiaRemovida;
        GerenciadorNoticiasImpressas.Instance.MusicaEnviadaNoRadio -= OnMusicaRecebida;
        GerenciadorNoticiasImpressas.Instance.MusicaRemovidaDaFila -= OnMusicaRemovida;
        GerenciadorNoticiasImpressas.Instance.NoticiaFinalizadaTransmissao -= OnNoticiaFinalizadaTransmissao;
        GerenciadorNoticiasImpressas.Instance.MusicaFinalizadaTransmissao -= OnMusicaFinalizadaTransmissao;
        _inscritoNoGerenciador = false;
    }

    private void OnVisibilityChanged()
    {
        if (Visible)
        {
            AtualizarLista();
        }
    }

    private void OnNoticiaRecebida(NoticiaModel noticia, VariacaoNoticia variacao)
    {
        Log.Print($"[UiNoticiasRadio]Notícia recebida: {noticia.TituloOriginal}");
        AtualizarLista();
    }

    private void OnMusicaRecebida(MusicaModel musica)
    {
        Log.Print($"[UiNoticiasRadio]Música recebida: {musica.Titulo}");
        AtualizarLista();
    }

    private void OnNoticiaRemovida(NoticiaModel noticia)
    {
        AtualizarLista();
    }

    private void OnMusicaRemovida(MusicaModel musica)
    {
        AtualizarLista();
    }

    private void TentarInscreverNoGerenciador()
    {
        if (_inscritoNoGerenciador || GerenciadorNoticiasImpressas.Instance == null) return;

        GerenciadorNoticiasImpressas.Instance.NoticiaImpressa += OnNoticiaRecebida;
        GerenciadorNoticiasImpressas.Instance.NoticiaRemovidaDaFila += OnNoticiaRemovida;
        GerenciadorNoticiasImpressas.Instance.MusicaEnviadaNoRadio += OnMusicaRecebida;
        GerenciadorNoticiasImpressas.Instance.MusicaRemovidaDaFila += OnMusicaRemovida;
        GerenciadorNoticiasImpressas.Instance.NoticiaFinalizadaTransmissao += OnNoticiaFinalizadaTransmissao;
        GerenciadorNoticiasImpressas.Instance.MusicaFinalizadaTransmissao += OnMusicaFinalizadaTransmissao;
        _inscritoNoGerenciador = true;
    }

    private void OnNoticiaFinalizadaTransmissao(NoticiaModel noticia)
    {
        AtualizarLista();
        AtualizarEstadoLista();
    }

    private void OnMusicaFinalizadaTransmissao(MusicaModel musica)
    {
        AtualizarLista();
        AtualizarEstadoLista();
    }

    private void AtualizarLista()
    {
        if (ListaNoticias == null) return;

        ListaNoticias.Clear();
        _itensExibidos.Clear();

        if (GerenciadorNoticiasImpressas.Instance == null) return;

        var noticiasPautadas = GerenciadorNoticiasImpressas.Instance.NoticiasImpressasDoDia;
        var musicasEnviadas = GerenciadorNoticiasImpressas.Instance.MusicasEnviadasDoDia;

        Log.Print("[UiNoticiasRadio]NoticiasPautadas");
        foreach (var noticia in noticiasPautadas)
        {
            _itensExibidos.Add(new ItemRadioEntry
            {
                Tipo = TipoItemRadio.Noticia,
                Noticia = noticia
            });

            string tituloExibicao = $"[NOTÍCIA] {ObterTituloDeAcordoComEscolha(noticia)}";
            ListaNoticias.AddItem(tituloExibicao);
            ListaNoticias.SetItemCustomFgColor(ListaNoticias.ItemCount - 1, new Color(0.95f, 0.95f, 1.0f));
        }

        foreach (var musica in musicasEnviadas)
        {
            _itensExibidos.Add(new ItemRadioEntry
            {
                Tipo = TipoItemRadio.Musica,
                Musica = musica
            });

            string tituloMusica = string.IsNullOrEmpty(musica?.Titulo) ? "[MÚSICA] Música sem título" : $"[MÚSICA] {musica.Titulo}";
            ListaNoticias.AddItem(tituloMusica);
            ListaNoticias.SetItemCustomFgColor(ListaNoticias.ItemCount - 1, new Color(0.75f, 1.0f, 0.75f));
        }

        Log.Print($"[UiNoticiasRadio]Itens exibidos: {_itensExibidos.Count}");
        AtualizarEstadoLista();
    }

    private void AtualizarEstadoLista()
    {
        if (ListaNoticias == null) return;

        bool podeEscolherNova = GerenciadorNoticiasImpressas.Instance?.PodeIniciarTransmissao == true;
        ListaNoticias.MouseFilter = podeEscolherNova ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;
    }

    private string ObterTituloDeAcordoComEscolha(NoticiaModel noticia)
    {
        if (noticia == null) return "Notícia sem dados";

        VariacaoNoticia variacao = null;
        bool temVariacao = noticia.Variacoes != null &&
                           noticia.Variacoes.TryGetValue(noticia.EscolhaJogador, out variacao);

        if (temVariacao && !string.IsNullOrEmpty(variacao.TituloAlterado))
        {
            return variacao.TituloAlterado;
        }

        return string.IsNullOrEmpty(noticia.TituloOriginal) ? $"Notícia {noticia.Id}" : noticia.TituloOriginal;
    }

    private void OnNoticiaSelecionada(long index)
    {
        if (GerenciadorNoticiasImpressas.Instance?.PodeIniciarTransmissao != true) return;

        int idx = (int)index;

        bool indiceInvalido = idx < 0 || idx >= _itensExibidos.Count;
        if (indiceInvalido) return;

        ItemRadioEntry itemSelecionado = _itensExibidos[idx];
        if (itemSelecionado.Tipo == TipoItemRadio.Noticia)
        {
            NoticiaModel noticiaParaTransmitir = itemSelecionado.Noticia;
            Log.Print($"[UiNoticiasRadio]Notícia selecionada para transmissão: {noticiaParaTransmitir.TituloOriginal}");
            GerenciadorNoticiasImpressas.Instance.TransmitirNoticiaNoRadio(noticiaParaTransmitir);
        }
        else
        {
            MusicaModel musicaParaTransmitir = itemSelecionado.Musica;
            Log.Print($"[UiNoticiasRadio]Música selecionada para transmissão: {musicaParaTransmitir.Titulo}");
            GerenciadorNoticiasImpressas.Instance.TransmitirMusicaNoRadio(musicaParaTransmitir);
        }

        AtualizarEstadoLista();

        //quando acabar a transmissoa de voz, atualiza a lista de notícias, para remover a notícia que acabou de ser transmitida
        //AtualizarLista();
    }
}