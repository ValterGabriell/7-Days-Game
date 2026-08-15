using Godot;
using System;
using System.Collections.Generic;
using fiveyears3.Scripts.Globais;

namespace fiveyears3.Scripts.UI
{
    public partial class TelaFinal : Control
    {
        [ExportGroup("Componentes da UI - Final")]
        [Export] public Control PainelTextoFinal { get; set; }
        [Export] public Label LabelTituloFinal { get; set; }
        [Export] public RichTextLabel LabelDescricaoFinal { get; set; }
        [Export] public Button ButtonAvancar { get; set; }

        [ExportGroup("Componentes da UI - Créditos")]
        [Export] public Control PainelCreditos { get; set; }
        [Export] public RichTextLabel ContetCreditos { get; set; }
        [Export] public Button ButtonMenuPrincipal { get; set; }

        [ExportGroup("Configurações de Animação")]
        [Export] public float VelocidadeRolarCreditos { get; set; } = 40.0f; // Pixels por segundo
        [Export] public string CenaMenuPrincipalPath { get; set; } = "res://Cenas/MenuPrincipal.tscn";

        private bool _rolandoCreditos = false;
        private Tween _tweenTexto;

        // Dicionário com os títulos e descrições narrativas sincronizados com o JSON e os enums do GerenciadorDeFinais
        private readonly Dictionary<TipoFinal, (string Titulo, string Descricao)> _textosDosFinais
            = new Dictionary<TipoFinal, (string, string)>
        {
            {
                TipoFinal.RevolucaoHades,
                ("A ERA DE HADES (VITÓRIA TOTAL DA RESISTÊNCIA)",
                 "Você usou o jornalismo para desmascarar totalmente os esquemas do FDP e expor os crimes da elite da cidade. O Hades liderou um levante popular implacável. As forças de segurança da FDP desmoronaram e os líderes da facção autoritária foram caçados. A rádio e os meios de comunicação tornaram-se a voz livre da revolução.")
            },
            {
                TipoFinal.OrdemCorporativa,
                ("A ORDEM DE FERRO (DOMÍNIO TOTAL DA FDP)",
                 "Suas transmissões distorceram e silenciaram as intenções do Hades, pintando-os apenas como terroristas sanguinários. A FDP massacrou os rebeldes com apoio da população manipulada. O controle estatal/corporativo sobre a informação é total, e você foi promovido a Chefe Oficial de Propaganda da FDP.")
            },
            {
                TipoFinal.PazArmada,
                ("O EQUILÍBRIO DAS SOMBRAS (PAZ VELADA)",
                 "Ao manobrar as notícias com maestria, você conseguiu agradar ambos os lados sem se comprometer inteiramente. Hades e FDP estabeleceram uma trégua tensa e não declarada. Você se tornou o intermediário de informação mais influente e perigoso da cidade, temido e respeitado por ambas as facções.")
            },
            {
                TipoFinal.CaosTotal,
                ("TERRA ARRASADA (GUERRA CIVIL ABSOLUTA)",
                 "Suas mentiras, omissões e manipulações foram descobertas por ambas as partes. Hades e FDP perderam a confiança na imprensa e se chocaram em um conflito aberto, sangrento e sem regras. A cidade virou ruínas, o sinal da rádio caiu e você agora precisa fugir para sobreviver à caçada liderada pelas duas facções.")
            },
            {
                TipoFinal.DemissaoAudienciaBaixa,
                ("O ESQUECIMENTO (DESCARTE DO SISTEMA)",
                 "Ninguém mais escuta o seu canal. Sua falta de alcance fez a FDP e a Hades considerarem sua estação irrelevante. Você foi sumariamente demitido e substituído por um operador automático. Sem proteção ou recursos, você aguarda o fim do mundo no completo silêncio das ruas.")
            },
            {
                TipoFinal.FugaSoloEden,
                ("O BILHETE DE OURO (SALVAÇÃO EGOÍSTA)",
                 "Você garantiu sua vaga a bordo da Eden 5 silenciando as denúncias mais graves contra a FDP. Enquanto os motores da nave aceleram rumo a Alpha Centauri, você olha pela janela e vê a Terra morrer abaixo. Você sobreviveu, mas ao custo de carregar a culpa de milhões de almas deixadas para trás.")
            },
            {
                TipoFinal.FimPrematuro,
                ("TRANSMISSÃO INTERROMPIDA",
                 "Um evento inesperado e trágico cortou sua jornada antes do tempo. As consequências diretas de suas últimas decisões selaram seu destino precipitado.")
            }
        };

        public override void _Ready()
        {
            // Oculta tudo de início
            Visible = false;
            if (PainelTextoFinal != null) PainelTextoFinal.Visible = false;
            if (PainelCreditos != null) PainelCreditos.Visible = false;

            // Inscreve botões
            if (ButtonAvancar != null)
            {
                ButtonAvancar.Pressed += OnButtonAvancarPressed;
            }

            if (ButtonMenuPrincipal != null)
            {
                ButtonMenuPrincipal.Pressed += OnButtonMenuPrincipalPressed;
            }

            // Inscreve-se no evento do GerenciadorDeFinais
            if (GerenciadorDeFinais.Instance != null)
            {
                GerenciadorDeFinais.Instance.FinalAlcancado += ExibirTelaDeFinal;

                // Caso o jogo já tenha sido finalizado antes dessa tela estar pronta no Ready
                if (GerenciadorDeFinais.Instance.JogoFinalizado)
                {
                    ExibirTelaDeFinal(GerenciadorDeFinais.Instance.FinalAtual);
                }
            }
        }

        public override void _ExitTree()
        {
            if (GerenciadorDeFinais.Instance != null)
            {
                GerenciadorDeFinais.Instance.FinalAlcancado -= ExibirTelaDeFinal;
            }
        }

        public override void _Process(double delta)
        {
            // Executa a rolagem contínua dos créditos
            if (_rolandoCreditos && ContetCreditos != null)
            {
                Vector2 pos = ContetCreditos.Position;
                pos.Y -= VelocidadeRolarCreditos * (float)delta;
                ContetCreditos.Position = pos;

                // Se os créditos subirem totalmente além da tela, exibe botão de sair
                if (ContetCreditos.Position.Y + ContetCreditos.Size.Y < 0)
                {
                    _rolandoCreditos = false;
                    if (ButtonMenuPrincipal != null) ButtonMenuPrincipal.Visible = true;
                }
            }
        }

        /// <summary>
        /// Chamado automaticamente pelo evento do GerenciadorDeFinais.
        /// </summary>
        public void ExibirTelaDeFinal(TipoFinal tipoFinal)
        {
            Visible = true;
            if (PainelTextoFinal != null) PainelTextoFinal.Visible = true;

            if (_textosDosFinais.TryGetValue(tipoFinal, out var dados))
            {
                if (LabelTituloFinal != null) LabelTituloFinal.Text = dados.Titulo;

                if (LabelDescricaoFinal != null)
                {
                    // Animação de digitação/aparecimento do texto
                    LabelDescricaoFinal.Text = dados.Descricao;
                    LabelDescricaoFinal.VisibleRatio = 0.0f;

                    _tweenTexto?.Kill();
                    _tweenTexto = CreateTween();
                    _tweenTexto.TweenProperty(LabelDescricaoFinal, "visible_ratio", 1.0f, 3.5f)
                               .SetTrans(Tween.TransitionType.Linear);
                }
            }
            else
            {
                Log.PrintErr($"[TelaFinal] Tipo de final {tipoFinal} não encontrado no dicionário de textos!");
            }
        }

        private void OnButtonAvancarPressed()
        {
            // Esconde o painel do texto e inicia os créditos
            if (PainelTextoFinal != null) PainelTextoFinal.Visible = false;
            IniciarCreditos();
        }

        private void IniciarCreditos()
        {
            if (PainelCreditos == null || ContetCreditos == null) return;

            PainelCreditos.Visible = true;

            // Posiciona o container de créditos logo abaixo da borda inferior da tela
            Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
            ContetCreditos.Position = new Vector2(
                (viewportSize.X - ContetCreditos.Size.X) / 2.0f,
                viewportSize.Y + 50.0f
            );

            _rolandoCreditos = true;
        }

        private void OnButtonMenuPrincipalPressed()
        {
            // Reseta o finalizador e recarrega para o Menu Principal
            if (GerenciadorDeFinais.Instance != null)
            {
                GerenciadorDeFinais.Instance.ResetarFinais();
            }

            GetTree().ChangeSceneToFile(CenaMenuPrincipalPath);
        }
    }
}