using Godot;
using System;
using System.Collections.Generic;
using fiveyears3.Scripts.Globais;

public partial class UiProvisoriaNoticias : CanvasLayer
{
    [Export] public ItemList ListaNoticias;

    private List<NoticiaModel> _noticiasExibidas = new();

    public override void _Ready()
    {
        if (GerenciadorNoticiasImpressas.Instance != null)
        {
            GerenciadorNoticiasImpressas.Instance.NoticiaImpressa += OnNoticiaRecebida;
        }

        if (ListaNoticias != null)
        {
            ListaNoticias.ItemSelected += OnNoticiaSelecionada;
        }

        VisibilityChanged += OnVisibilityChanged;
        AtualizarLista();
    }

    public override void _ExitTree()
    {
        if (GerenciadorNoticiasImpressas.Instance != null)
        {
            GerenciadorNoticiasImpressas.Instance.NoticiaImpressa -= OnNoticiaRecebida;
        }
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
        GD.Print($"[UiProvisoriaNoticias]Notícia recebida: {noticia.TituloOriginal}");
        if (Visible)
        {
            AtualizarLista();
        }
    }

    private void AtualizarLista()
    {
        if (ListaNoticias == null) return;

        ListaNoticias.Clear();
        _noticiasExibidas.Clear();

        if (GerenciadorNoticiasImpressas.Instance == null) return;

        var noticiasPautadas = GerenciadorNoticiasImpressas.Instance.NoticiasImpressasDoDia;
        GD.Print("[UiProvisoriaNoticias]NoticiasPautadas");
        foreach (var noticia in noticiasPautadas)
        {
            _noticiasExibidas.Add(noticia);

            string tituloExibicao = ObterTituloDeAcordoComEscolha(noticia);
            ListaNoticias.AddItem(tituloExibicao);
        }
        GD.Print($"[UiProvisoriaNoticias]Noticias exibidas: {_noticiasExibidas.Count}");
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
        int idx = (int)index;

        bool indiceInvalido = idx < 0 || idx >= _noticiasExibidas.Count;
        if (indiceInvalido) return;

        NoticiaModel noticiaParaTransmitir = _noticiasExibidas[idx];

        bool transmitiu = GerenciadorNoticiasImpressas.Instance.TransmitirNoticiaNoRadio(noticiaParaTransmitir);

        if (transmitiu)
        {
            AtualizarLista();
        }
    }
}