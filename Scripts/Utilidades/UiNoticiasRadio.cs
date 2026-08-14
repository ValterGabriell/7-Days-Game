using Godot;
using System;
using System.Collections.Generic;
using fiveyears3.Scripts.Globais;

namespace fiveyears3.Scripts.UI
{
    public partial class UiNoticiasRadio : Control
    {
        [Export] public ItemList ListaNoticias;

        private readonly List<ItemRadioEntry> _itensExibidos = new();
        private bool _inscritoNoGerenciador;
        private bool _inicializado;

        private enum TipoItemRadio { Noticia, Musica }

        private sealed class ItemRadioEntry
        {
            public TipoItemRadio Tipo { get; init; }
            public NoticiaModel Noticia { get; init; }
            public MusicaModel Musica { get; init; }
        }

        public override void _Ready()
        {
            if (!Menu.JogoIniciado)
            {
                Menu.AoIniciarJogo += OnJogoIniciado;
                return;
            }

            InicializarUi();
        }

        private void OnJogoIniciado()
        {
            Menu.AoIniciarJogo -= OnJogoIniciado;
            InicializarUi();
        }

        private void InicializarUi()
        {
            if (_inicializado) return;
            _inicializado = true;

            TentarInscreverNoGerenciador();

            if (ListaNoticias != null)
            {
                ListaNoticias.FocusMode = FocusModeEnum.All;
                ListaNoticias.ItemSelected += OnNoticiaSelecionada;
                ConfigurarEstiloTerminalItemList();
            }

            VisibilityChanged += OnVisibilityChanged;
            AtualizarLista();
        }

        public override void _Process(double delta)
        {
            if (!_inicializado) return;
            if (_inscritoNoGerenciador) return;

            if (TentarInscreverNoGerenciador())
            {
                AtualizarLista();
            }
        }

        public override void _ExitTree()
        {
            Menu.AoIniciarJogo -= OnJogoIniciado;

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
            Log.Print($"[UiNoticiasRadioViewport] Notícia recebida: {noticia?.TituloOriginal}");
            CallDeferred(MethodName.AtualizarLista);
        }

        private void OnMusicaRecebida(MusicaModel musica)
        {
            Log.Print($"[UiNoticiasRadioViewport] Música recebida: {musica?.Titulo}");
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

            int indiceGlobal = 0;

            if (noticiasPautadas != null)
            {
                foreach (var noticia in noticiasPautadas)
                {
                    _itensExibidos.Add(new ItemRadioEntry
                    {
                        Tipo = TipoItemRadio.Noticia,
                        Noticia = noticia
                    });

                    string tituloNoticia = ObterTituloDeAcordoComEscolha(noticia);
                    string tituloExibicao = $"[{indiceGlobal + 1:00}] > [NOTICIA] {tituloNoticia.ToUpper()}";

                    // Removido SetItemCustomFgColor para permitir inversão de cores e hover no tema
                    ListaNoticias.AddItem(tituloExibicao);

                    indiceGlobal++;
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

                    string nomeMusica = string.IsNullOrEmpty(musica?.Titulo) ? "SEM_TITULO.RAW" : musica.Titulo;
                    string tituloMusica = $"[{indiceGlobal + 1:00}] > [MUSICA] {nomeMusica.ToUpper()}";

                    // Removido SetItemCustomFgColor para permitir inversão de cores e hover no tema
                    ListaNoticias.AddItem(tituloMusica);

                    indiceGlobal++;
                }
            }

            Log.Print($"[UiNoticiasRadioViewport] Lista atualizada: {ListaNoticias.ItemCount} itens.");
            AtualizarEstadoLista();
        }

        private void ConfigurarEstiloTerminalItemList()
        {
            if (ListaNoticias == null) return;

            ListaNoticias.SelectMode = ItemList.SelectModeEnum.Single;
            ListaNoticias.FocusMode = Control.FocusModeEnum.All;

            // 1. Fundo da caixa inteira (Terminal Escuro)
            var stylePanel = new StyleBoxFlat
            {
                BgColor = new Color("#051105"),
                ContentMarginLeft = 6,
                ContentMarginTop = 6,
                ContentMarginRight = 6,
                ContentMarginBottom = 6,
                BorderColor = new Color("#00FF41"),
                BorderWidthLeft = 1,
                BorderWidthTop = 1,
                BorderWidthRight = 1,
                BorderWidthBottom = 1
            };
            ListaNoticias.AddThemeStyleboxOverride("panel", stylePanel);

            // 2. Item SELECIONADO / EM TRANSMISSÃO (Fundo Verde Neon)
            var styleSelected = new StyleBoxFlat
            {
                BgColor = new Color("#00FF41"),
                CornerRadiusTopLeft = 0,
                CornerRadiusTopRight = 0,
                CornerRadiusBottomLeft = 0,
                CornerRadiusBottomRight = 0
            };
            ListaNoticias.AddThemeStyleboxOverride("selected", styleSelected);
            ListaNoticias.AddThemeStyleboxOverride("selected_focus", styleSelected);

            // 3. Item HOVER (Cursor passando em cima)
            var styleHover = new StyleBoxFlat
            {
                BgColor = new Color("#0d3810"), // Verde médio visível para dar destaque
                BorderColor = new Color("#00FF41"),
                BorderWidthLeft = 1,
                BorderWidthTop = 1,
                BorderWidthRight = 1,
                BorderWidthBottom = 1
            };
            ListaNoticias.AddThemeStyleboxOverride("hovered", styleHover);

            // 4. Cores do Texto
            ListaNoticias.AddThemeColorOverride("font_color", new Color("#00FF41"));          // Normal: Texto Verde Terminal
            ListaNoticias.AddThemeColorOverride("font_selected_color", new Color("#000000")); // Selecionado: Texto PRETO sobre fundo verde
            ListaNoticias.AddThemeColorOverride("font_hovered_color", new Color("#00FF41"));  // Hover: Texto Verde brilhante
        }

        private void AtualizarEstadoLista()
        {
            if (ListaNoticias == null) return;

            bool podeEscolherNova = GerenciadorNoticiasImpressas.Instance?.PodeIniciarTransmissao == true;
            ListaNoticias.MouseFilter = podeEscolherNova ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;
            Log.Print($"[UiNoticiasRadioViewport] Estado Lista | PodeIniciarTransmissao={podeEscolherNova} | MouseFilter={ListaNoticias.MouseFilter} | ItemCount={ListaNoticias.ItemCount}");
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
                Log.Print($"[UiNoticiasRadioViewport] Transmitindo notícia: {noticiaParaTransmitir.TituloOriginal}");
                GerenciadorNoticiasImpressas.Instance.TransmitirNoticiaNoRadio(noticiaParaTransmitir);
            }
            else
            {
                MusicaModel musicaParaTransmitir = itemSelecionado.Musica;
                Log.Print($"[UiNoticiasRadioViewport] Transmitindo música: {musicaParaTransmitir.Titulo}");
                GerenciadorNoticiasImpressas.Instance.TransmitirMusicaNoRadio(musicaParaTransmitir);
            }

            AtualizarEstadoLista();
        }
    }
}