using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using fiveyears3.Scripts.Globais;

namespace fiveyears3.Scripts.UI
{
    public partial class InterfaceOSUI : Control
    {
        [Export] public ScrollContainer Scroll; 
        [Export] public VBoxContainer ListaNoticias; 

        [Export] public Label LblTitulo;
        [Export] public Label LblRemetente;
        [Export] public RichTextLabel LblTextoOriginal;
        [Export] public CheckBox OptIntegra;
        [Export] public CheckBox OptSuprimir;
        [Export] public CheckBox OptOficial;
        [Export] public CheckBox OptInvestigativo;
        [Export] public Label LblPreview;
        [Export] public Button BtnImprimir;

        [Export] public Label LblPautas;
        [Export] public Label LblAudiencia;
        [Export] public Label LblEsperanca;
        [Export] public Label LblIrritacao;
        [Export] public Button BtnEncerrar;

        [ExportGroup("ABAS")]
        [Export] public Button BtnMusicas;
        [Export] public Button BtnPropagandaNoticias;
        [Export] public BoxContainer BoxMusicas;
        [Export] public BoxContainer BoxNoticiasPropaganda;
        [Export] public VBoxContainer ListaMusicas;

        private NoticiaModel _noticiaSelecionada;
        private int _pautasImpressasCount = 0;
        private const int MAX_PAUTAS_DIA = 3;
        private ButtonGroup _grupoOpcoesEditoriais;
        private readonly List<MusicaModel> _musicasDisponiveis = new();
        private ButtonGroup _grupoBotoesNoticias = new ButtonGroup();
        private bool _inicializado;
        private VBoxContainer ObterListaMusicas()
        {
            if (ListaMusicas != null) return ListaMusicas;
            if (BoxMusicas == null) return null;

            var encontrados = BoxMusicas.FindChildren("*", "VBoxContainer", true, false);
            if (encontrados.Count <= 0) return null;

            ListaMusicas = encontrados[0] as VBoxContainer;
            return ListaMusicas;
        }

        public override void _Ready()
        {
            if (!Menu.JogoIniciado)
            {
                Menu.AoIniciarJogo += OnJogoIniciado;
                return;
            }

            InicializarInterface();
        }

        private void OnJogoIniciado()
        {
            Menu.AoIniciarJogo -= OnJogoIniciado;
            InicializarInterface();
        }

        private void InicializarInterface()
        {
            if (_inicializado) return;
            _inicializado = true;

            ConfigurarGrupoDeOpcoes();

            if (GerenciadorDeNoticias.Instance != null)
            {
                GerenciadorDeNoticias.Instance.NoticiasCarregadas += AtualizarListaNoticias;
            }

            if (GerenciadorNoticiasImpressas.Instance != null)
            {
                GerenciadorNoticiasImpressas.Instance.MusicaEnviadaNoRadio += OnMusicaEnviadaNoRadio;
                GerenciadorNoticiasImpressas.Instance.MusicaRemovidaDaFila += OnMusicaRemovidaDaFila;
                GerenciadorNoticiasImpressas.Instance.MusicaFinalizadaTransmissao += OnMusicaFinalizadaTransmissao;
            }

            OptIntegra.Pressed += () => SelecionarAcaoEditorial(AcaoEditorial.ORIGINAL);
            OptSuprimir.Pressed += () => SelecionarAcaoEditorial(AcaoEditorial.OMITIR);
            OptOficial.Pressed += () => SelecionarAcaoEditorial(AcaoEditorial.MENTIR);
            OptInvestigativo.Pressed += () => SelecionarAcaoEditorial(AcaoEditorial.DISTORCER);

            BtnImprimir.Pressed += OnImprimirPauta;
 

            if (BtnMusicas != null)
            {
                BtnMusicas.Pressed += () => ExibirAba(true);
            }

            if (BtnPropagandaNoticias != null)
            {
                BtnPropagandaNoticias.Pressed += () => ExibirAba(false);
            }

            LimparPainelCentral();
            AtualizarStatusDia();
            CarregarMusicasDisponiveis();
            AtualizarListaMusicas();
            ExibirAba(false);

            // Força a atualização da lista caso as notícias já estejam carregadas no Gerenciador
            AtualizarListaNoticias();
        }

        public override void _ExitTree()
        {
            Menu.AoIniciarJogo -= OnJogoIniciado;

            if (GerenciadorDeNoticias.Instance != null)
            {
                GerenciadorDeNoticias.Instance.NoticiasCarregadas -= AtualizarListaNoticias;
            }

            if (GerenciadorNoticiasImpressas.Instance != null)
            {
                GerenciadorNoticiasImpressas.Instance.MusicaEnviadaNoRadio -= OnMusicaEnviadaNoRadio;
                GerenciadorNoticiasImpressas.Instance.MusicaRemovidaDaFila -= OnMusicaRemovidaDaFila;
                GerenciadorNoticiasImpressas.Instance.MusicaFinalizadaTransmissao -= OnMusicaFinalizadaTransmissao;
            }
        }

        private void ExibirAba(bool mostrarMusicas)
        {
            if (BoxMusicas != null)
            {
                BoxMusicas.Visible = mostrarMusicas;
            }

            if (BoxNoticiasPropaganda != null)
            {
                BoxNoticiasPropaganda.Visible = !mostrarMusicas;
            }

            if (mostrarMusicas)
            {
                AtualizarListaMusicas();
            }
        }

        private void CarregarMusicasDisponiveis()
        {
            _musicasDisponiveis.Clear();

            string caminhoRelativo = "res://Scripts/Dados/JSONS/musicas.json";
            string caminhoAbsoluto = ProjectSettings.GlobalizePath(caminhoRelativo);
            if (!File.Exists(caminhoAbsoluto)) return;

            string jsonString = File.ReadAllText(caminhoAbsoluto);
            JsonSerializerOptions opcoes = new()
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            List<MusicaModel> musicas = JsonSerializer.Deserialize<List<MusicaModel>>(jsonString, opcoes);
            if (musicas == null) return;

            Log.Print($"[InterfaceOSUI] Músicas carregadas: {musicas.Count}");
            _musicasDisponiveis.AddRange(musicas);
        }

        private void AtualizarListaMusicas()
        {
            VBoxContainer listaMusicas = ObterListaMusicas();
            if (listaMusicas == null)
            {
                Log.PrintErr("[InterfaceOSUI] ListaMusicas não encontrada. Vincule no Inspector ou adicione um VBoxContainer dentro de BoxMusicas.");
                return;
            }

            foreach (Node child in listaMusicas.GetChildren())
            {
                child.QueueFree();
            }

            foreach (MusicaModel musica in _musicasDisponiveis)
            {
                bool jaEnviada = GerenciadorNoticiasImpressas.Instance?.MusicasEnviadasDoDia.Exists(m => m.Id == musica.Id) == true;
                bool jaTransmitida = GerenciadorNoticiasImpressas.Instance?.MusicasTransmitidasDoDia.Exists(m => m.Id == musica.Id) == true;
                bool emTransmissao = GerenciadorNoticiasImpressas.Instance?.MusicaEmTransmissao?.Id == musica.Id;

                string prefixo = jaEnviada || jaTransmitida || emTransmissao ? "[ENVIADA]" : "[MÚSICA]";
                Color corItem = jaEnviada || jaTransmitida || emTransmissao
                    ? new Color(0.75f, 0.75f, 0.75f)
                    : new Color(0.75f, 1.0f, 0.75f);

                MusicaModel musicaLocal = musica;
                Button btnMusica = new()
                {
                    Text = $"{prefixo} {musica.Titulo}",
                    Alignment = HorizontalAlignment.Left,
                    AutowrapMode = TextServer.AutowrapMode.WordSmart,
                    CustomMinimumSize = new Vector2(0, 34)
                };

                btnMusica.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                btnMusica.AddThemeColorOverride("font_color", corItem);
                btnMusica.Pressed += () => OnMusicaSelecionada(musicaLocal);

                listaMusicas.AddChild(btnMusica);
            }

            Log.Print($"[InterfaceOSUI] Itens exibidos na ListaMusicas: {listaMusicas.GetChildCount()}");
        }

        private void OnMusicaSelecionada(MusicaModel musicaSelecionada)
        {
            if (musicaSelecionada == null) return;
            if (GerenciadorNoticiasImpressas.Instance == null) return;

            bool jaEnviada = GerenciadorNoticiasImpressas.Instance.MusicasEnviadasDoDia.Exists(m => m.Id == musicaSelecionada.Id);
            bool enviada = jaEnviada
                ? GerenciadorNoticiasImpressas.Instance.DesfazerEnvioMusicaParaRadio(musicaSelecionada)
                : GerenciadorNoticiasImpressas.Instance.EnviarMusicaParaRadio(musicaSelecionada);

            if (enviada)
            {
                AtualizarListaMusicas();
            }
        }

        private void OnMusicaEnviadaNoRadio(MusicaModel musica) => AtualizarListaMusicas();
        private void OnMusicaRemovidaDaFila(MusicaModel musica) => AtualizarListaMusicas();
        private void OnMusicaFinalizadaTransmissao(MusicaModel musica) => AtualizarListaMusicas();

        private void ConfigurarGrupoDeOpcoes()
        {
            _grupoOpcoesEditoriais = new ButtonGroup();
            OptIntegra.ButtonGroup = _grupoOpcoesEditoriais;
            OptSuprimir.ButtonGroup = _grupoOpcoesEditoriais;
            OptOficial.ButtonGroup = _grupoOpcoesEditoriais;
            OptInvestigativo.ButtonGroup = _grupoOpcoesEditoriais;
        }

        private void AtualizarListaNoticias()
        {
            if (ListaNoticias == null) return;

            // Limpa os botões antigos da lista
            foreach (Node child in ListaNoticias.GetChildren())
            {
                child.QueueFree();
            }

            List<NoticiaModel> noticias = GerenciadorDeNoticias.Instance?.NoticiasDoDia;

            if (noticias == null || noticias.Count == 0)
            {
                LimparPainelCentral();
                return;
            }

            _grupoBotoesNoticias = new ButtonGroup(); // Reseta o grupo a cada atualização

            // Criação do estilo padrão (Normal)
            StyleBoxFlat styleNormal = new StyleBoxFlat
            {
                BgColor = new Color("#000000"), // Fundo preto
                BorderWidthLeft = 1,
                BorderWidthTop = 1,
                BorderWidthRight = 1,
                BorderWidthBottom = 1,
                BorderColor = new Color("#00FF41"), // Borda verde terminal
                ContentMarginLeft = 8,
                ContentMarginRight = 8,
                ContentMarginTop = 4,
                ContentMarginBottom = 4
            };

            // Criação do estilo para quando o botão estiver SELECIONADO / PRESSIONADO (Inversão retro)
            StyleBoxFlat stylePressed = new StyleBoxFlat
            {
                BgColor = new Color("#00FF41"), // Fundo verde brilhante
                BorderWidthLeft = 1,
                BorderWidthTop = 1,
                BorderWidthRight = 1,
                BorderWidthBottom = 1,
                BorderColor = new Color("#00FF41"),
                ContentMarginLeft = 8,
                ContentMarginRight = 8,
                ContentMarginTop = 4,
                ContentMarginBottom = 4
            };

            for (int i = 0; i < noticias.Count; i++)
            {
                var noticia = noticias[i];
                Button btnNoticia = new Button
                {
                    Text = $"[{i + 1:00}] > {noticia.TituloOriginal.ToUpper()}",
                    Alignment = HorizontalAlignment.Left,
                    AutowrapMode = TextServer.AutowrapMode.WordSmart,
                    CustomMinimumSize = new Vector2(0, 36),
                    ToggleMode = true, // Permite ficar marcado/pressionado
                    ButtonGroup = _grupoBotoesNoticias // Faz com que apenas um botão fique selecionado por vez
                };

                btnNoticia.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

                // Sobrescreve as cores do texto
                btnNoticia.AddThemeColorOverride("font_color", new Color("#00FF41")); // Texto normal: verde
                btnNoticia.AddThemeColorOverride("font_pressed_color", new Color("#000000")); // Texto selecionado: preto
                btnNoticia.AddThemeColorOverride("font_hover_color", new Color("#00FF41"));
                btnNoticia.AddThemeColorOverride("font_focus_color", new Color("#000000"));

                // Aplica os StyleBoxes
                btnNoticia.AddThemeStyleboxOverride("normal", styleNormal);
                btnNoticia.AddThemeStyleboxOverride("pressed", stylePressed);
                btnNoticia.AddThemeStyleboxOverride("hover", styleNormal);
                btnNoticia.AddThemeStyleboxOverride("focus", stylePressed);

                NoticiaModel noticiaLocal = noticia;
                btnNoticia.Pressed += () => ExibirNoticia(noticiaLocal);

                ListaNoticias.AddChild(btnNoticia);

                // Marca o primeiro item como selecionado por padrão
                if (i == 0)
                {
                    btnNoticia.ButtonPressed = true;
                    ExibirNoticia(noticiaLocal);
                }
            }
        }

        private void ExibirNoticia(NoticiaModel noticia)
        {
            _noticiaSelecionada = noticia;

            LblTitulo.Text = $"TÍTULO: {noticia.TituloOriginal}";
            LblRemetente.Text = $"REMETENTE: {noticia.Remetente}";
            LblTextoOriginal.Text = noticia.TextoOriginal;

            switch (noticia.EscolhaJogador)
            {
                case AcaoEditorial.ORIGINAL: OptIntegra.ButtonPressed = true; break;
                case AcaoEditorial.OMITIR: OptSuprimir.ButtonPressed = true; break;
                case AcaoEditorial.MENTIR: OptOficial.ButtonPressed = true; break;
                case AcaoEditorial.DISTORCER: OptInvestigativo.ButtonPressed = true; break;
            }

            AtualizarPreview();
            AtualizarEstadoBotaoImprimir();
            AtualizarEstadoOpcoesEditoriais();
        }

        private void SelecionarAcaoEditorial(AcaoEditorial acao)
        {
            if (_noticiaSelecionada == null) return;
            if (NoticiaSelecionadaBloqueadaParaEdicao()) return;

            _noticiaSelecionada.EscolhaJogador = acao;
            AtualizarPreview();
        }

        private void AtualizarPreview()
        {
            if (_noticiaSelecionada == null)
            {
                LblPreview.Text = "PREVIEW DA PAUTA: Nenhum item selecionado.";
                return;
            }

            AcaoEditorial acao = _noticiaSelecionada.EscolhaJogador;

            if (_noticiaSelecionada.Variacoes != null && _noticiaSelecionada.Variacoes.TryGetValue(acao, out VariacaoNoticia variacao))
            {
                LblPreview.Text = $"[ {variacao.TituloAlterado} ]\n{variacao.TextoParaLer}";
            }
            else
            {
                LblPreview.Text = _noticiaSelecionada.TextoOriginal;
            }
        }

        private void OnImprimirPauta()
        {
            Log.Print($"[InterfaceOSUI] Imprimir pauta");
            if (_noticiaSelecionada == null) return;

            Log.Print($"[InterfaceOSUI] Notícia selecionada: {_noticiaSelecionada.TituloOriginal}");

            Log.Print($"[InterfaceOSUI] GerenciadorNoticiasImpressas.Instance: {GerenciadorNoticiasImpressas.Instance}");
            if (GerenciadorNoticiasImpressas.Instance == null) return;

            bool noticiaJaImpressa = GerenciadorNoticiasImpressas.Instance.NoticiasImpressasDoDia.Exists(n => n.Id == _noticiaSelecionada.Id);
            bool sucesso = noticiaJaImpressa
                ? GerenciadorNoticiasImpressas.Instance.DesfazerImpressaoNoticia(_noticiaSelecionada)
                : GerenciadorNoticiasImpressas.Instance.ImprimirNoticia(_noticiaSelecionada);

            if (!sucesso) return;

            if (noticiaJaImpressa)
            {
                _pautasImpressasCount = Math.Max(0, _pautasImpressasCount - 1);
            }
            else
            {
                _pautasImpressasCount++;
            }

            AtualizarStatusDia();
            AtualizarEstadoBotaoImprimir();
            AtualizarEstadoOpcoesEditoriais();
        }

        private void AtualizarEstadoOpcoesEditoriais()
        {
            bool desabilitar = _noticiaSelecionada == null || NoticiaSelecionadaBloqueadaParaEdicao();

            if (OptIntegra != null) OptIntegra.Disabled = desabilitar;
            if (OptSuprimir != null) OptSuprimir.Disabled = desabilitar;
            if (OptOficial != null) OptOficial.Disabled = desabilitar;
            if (OptInvestigativo != null) OptInvestigativo.Disabled = desabilitar;
        }

        private bool NoticiaSelecionadaBloqueadaParaEdicao()
        {
            if (_noticiaSelecionada == null || GerenciadorNoticiasImpressas.Instance == null) return false;

            bool jaTransmitida = GerenciadorNoticiasImpressas.Instance.NoticiasTransmitidasDoDia.Exists(n => n.Id == _noticiaSelecionada.Id);
            bool emTransmissao = GerenciadorNoticiasImpressas.Instance.NoticiaEmTransmissao?.Id == _noticiaSelecionada.Id;

            return jaTransmitida || emTransmissao;
        }

        private void AtualizarEstadoBotaoImprimir()
        {
            if (BtnImprimir == null) return;

            bool semSelecao = _noticiaSelecionada == null;
            if (semSelecao)
            {
                BtnImprimir.Disabled = true;
                BtnImprimir.Text = "Imprimir Notícia";
                return;
            }

            if (GerenciadorNoticiasImpressas.Instance == null)
            {
                BtnImprimir.Disabled = false;
                BtnImprimir.Text = "Imprimir Notícia";
                return;
            }

            bool jaImpressa = GerenciadorNoticiasImpressas.Instance.NoticiasImpressasDoDia.Exists(n => n.Id == _noticiaSelecionada.Id);
            bool bloqueada = NoticiaSelecionadaBloqueadaParaEdicao();

            BtnImprimir.Disabled = bloqueada;
            BtnImprimir.Text = jaImpressa ? "Editar Notícia" : "Imprimir Notícia";
        }

       
        private void AtualizarStatusDia()
        {
            LblPautas.Text = $"Pautas Impressas: {_pautasImpressasCount} / {MAX_PAUTAS_DIA}";
            BtnEncerrar.Disabled = _pautasImpressasCount < MAX_PAUTAS_DIA;
        }

        private void LimparPainelCentral()
        {
            _noticiaSelecionada = null;
            LblTitulo.Text = "Selecione uma notícia";
            LblRemetente.Text = "";
            LblTextoOriginal.Text = "Nenhuma notícia selecionada no momento.";
            LblPreview.Text = "";
            AtualizarEstadoBotaoImprimir();
            AtualizarEstadoOpcoesEditoriais();
        }
    }
}