using Godot;
using System;
using System.Collections.Generic;
using fiveyears3.Scripts.Globais;

namespace fiveyears3.Scripts.UI
{
    // Altere de CanvasLayer para Control
    public partial class UiNoticiasRadio : Control
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
                ListaNoticias.FocusMode = FocusModeEnum.All;
                ListaNoticias.ItemSelected += OnNoticiaSelecionada;
            }

            VisibilityChanged += OnVisibilityChanged;
            AtualizarLista();
        }

        public override void _Process(double delta)
        {
            if (_inscritoNoGerenciador) return;

            if (TentarInscreverNoGerenciador())
            {
                AtualizarLista();
            }
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
            if (IsVisibleInTree())
            {
                AtualizarLista();
            }
        }

        private void OnNoticiaRecebida(NoticiaModel noticia, VariacaoNoticia variacao)
        {
            GD.Print($"[UiNoticiasRadioViewport] Notícia recebida: {noticia?.TituloOriginal}");
            CallDeferred(MethodName.AtualizarLista);
        }

        private void OnMusicaRecebida(MusicaModel musica)
        {
            GD.Print($"[UiNoticiasRadioViewport] Música recebida: {musica?.Titulo}");
            CallDeferred(MethodName.AtualizarLista);
        }

        private void OnNoticiaRemovida(NoticiaModel noticia)
        {
            CallDeferred(MethodName.AtualizarLista);
        }

        private void OnMusicaRemovida(MusicaModel musica)
        {
            CallDeferred(MethodName.AtualizarLista);
        }

        private bool TentarInscreverNoGerenciador()
        {
            if (_inscritoNoGerenciador || GerenciadorNoticiasImpressas.Instance == null) return false;

            GerenciadorNoticiasImpressas.Instance.NoticiaImpressa += OnNoticiaRecebida;
            GerenciadorNoticiasImpressas.Instance.NoticiaRemovidaDaFila += OnNoticiaRemovida;
            GerenciadorNoticiasImpressas.Instance.MusicaEnviadaNoRadio += OnMusicaRecebida;
            GerenciadorNoticiasImpressas.Instance.MusicaRemovidaDaFila += OnMusicaRemovida;
            GerenciadorNoticiasImpressas.Instance.NoticiaFinalizadaTransmissao += OnNoticiaFinalizadaTransmissao;
            GerenciadorNoticiasImpressas.Instance.MusicaFinalizadaTransmissao += OnMusicaFinalizadaTransmissao;
            _inscritoNoGerenciador = true;
            return true;
        }

        private void OnNoticiaFinalizadaTransmissao(NoticiaModel noticia)
        {
            CallDeferred(MethodName.AtualizarLista);
        }

        private void OnMusicaFinalizadaTransmissao(MusicaModel musica)
        {
            CallDeferred(MethodName.AtualizarLista);
        }

        private void AtualizarLista()
        {
            if (ListaNoticias == null) return;

            ListaNoticias.Clear();
            _itensExibidos.Clear();

            if (GerenciadorNoticiasImpressas.Instance == null) return;

            var noticiasPautadas = GerenciadorNoticiasImpressas.Instance.NoticiasImpressasDoDia;
            var musicasEnviadas = GerenciadorNoticiasImpressas.Instance.MusicasEnviadasDoDia;

            if (noticiasPautadas != null)
            {
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
            }

            if (musicasEnviadas != null)
            {
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
            }

            GD.Print($"[UiNoticiasRadioViewport] Lista atualizada: {ListaNoticias.ItemCount} itens.");
            AtualizarEstadoLista();
        }

        private void AtualizarEstadoLista()
        {
            if (ListaNoticias == null) return;

            bool podeEscolherNova = GerenciadorNoticiasImpressas.Instance?.PodeIniciarTransmissao == true;
            ListaNoticias.MouseFilter = podeEscolherNova ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;
            GD.Print($"[UiNoticiasRadioViewport] Estado Lista | PodeIniciarTransmissao={podeEscolherNova} | MouseFilter={ListaNoticias.MouseFilter} | ItemCount={ListaNoticias.ItemCount}");
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
                GD.Print($"[UiNoticiasRadioViewport] Transmitindo notícia: {noticiaParaTransmitir.TituloOriginal}");
                GerenciadorNoticiasImpressas.Instance.TransmitirNoticiaNoRadio(noticiaParaTransmitir);
            }
            else
            {
                MusicaModel musicaParaTransmitir = itemSelecionado.Musica;
                GD.Print($"[UiNoticiasRadioViewport] Transmitindo música: {musicaParaTransmitir.Titulo}");
                GerenciadorNoticiasImpressas.Instance.TransmitirMusicaNoRadio(musicaParaTransmitir);
            }

            AtualizarEstadoLista();
        }
    }
}