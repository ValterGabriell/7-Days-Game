using Godot;
using System;
using System.Collections.Generic;
using fiveyears3.Scripts.Globais;

namespace fiveyears3.Scripts.UI
{
    public partial class InterfaceOSUI : Control
    {
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

        private NoticiaModel _noticiaSelecionada;
        private int _pautasImpressasCount = 0;
        private const int MAX_PAUTAS_DIA = 3;
        private ButtonGroup _grupoOpcoesEditoriais;

        public override void _Ready()
        {
            ConfigurarGrupoDeOpcoes();

            if (GerenciadorDeNoticias.Instance != null)
            {
                GerenciadorDeNoticias.Instance.NoticiasCarregadas += AtualizarListaNoticias;
            }

            OptIntegra.Pressed += () => SelecionarAcaoEditorial(AcaoEditorial.ORIGINAL);
            OptSuprimir.Pressed += () => SelecionarAcaoEditorial(AcaoEditorial.OMITIR);
            OptOficial.Pressed += () => SelecionarAcaoEditorial(AcaoEditorial.MENTIR);
            OptInvestigativo.Pressed += () => SelecionarAcaoEditorial(AcaoEditorial.DISTORCER);

            BtnImprimir.Pressed += OnImprimirPauta;
            BtnEncerrar.Pressed += OnEncerrarSessao;

            LimparPainelCentral();
            AtualizarStatusDia();
        }

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
            foreach (Node child in ListaNoticias.GetChildren())
            {
                child.QueueFree();
            }

            List<NoticiaModel> noticias = GerenciadorDeNoticias.Instance.NoticiasDoDia;

            if (noticias == null || noticias.Count == 0)
            {
                LimparPainelCentral();
                return;
            }

            foreach (var noticia in noticias)
            {
                Button btnNoticia = new Button
                {
                    Text = noticia.TituloOriginal,
                    Alignment = HorizontalAlignment.Left,
                    AutowrapMode = TextServer.AutowrapMode.WordSmart,
                    CustomMinimumSize = new Vector2(0, 40)
                };

                btnNoticia.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

                NoticiaModel noticiaLocal = noticia;
                btnNoticia.Pressed += () => ExibirNoticia(noticiaLocal);

                ListaNoticias.AddChild(btnNoticia);
            }

            if (noticias.Count > 0)
            {
                ExibirNoticia(noticias[0]);
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
        }

        private void SelecionarAcaoEditorial(AcaoEditorial acao)
        {
            if (_noticiaSelecionada == null) return;

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
            GD.Print($"[InterfaceOSUI] Imprimir pauta");
            GD.Print($"[InterfaceOSUI] Notícia selecionada: {_noticiaSelecionada.TituloOriginal}");
            if (_noticiaSelecionada == null) return;

            GD.Print($"[InterfaceOSUI] GerenciadorNoticiasImpressas.Instance: {GerenciadorNoticiasImpressas.Instance}");
            if (GerenciadorNoticiasImpressas.Instance != null)
            {
                bool impressaComSucesso = GerenciadorNoticiasImpressas.Instance.ImprimirNoticia(_noticiaSelecionada);

                if (impressaComSucesso)
                {
                    _pautasImpressasCount++;
                    AtualizarStatusDia();
                }
            }
        }

        private void OnEncerrarSessao()
        {
            if (GerenciadorPassagemDoTempo.Instance != null)
            {
                GerenciadorPassagemDoTempo.Instance.AvancarDia();
            }

            _pautasImpressasCount = 0;
            AtualizarStatusDia();
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
        }
    }
}