using Godot;
using Scripts.Personagens.Principal;
using System;

namespace fiveyears3.Scripts.Globais
{
    public partial class GerenciadorMesa : Node3D
    {
        public static GerenciadorMesa Instance { get; private set; }

        public enum EquipamentoMesa { Radio, Computador, Telefone }
        public EquipamentoMesa EquipamentoAtual { get; private set; } = EquipamentoMesa.Computador;

        [ExportGroup("Câmeras")]
        [Export] public Camera3D CameraMesa;
        [Export] public Camera3D CameraRadio;
        [Export] public Camera3D CameraComputador;
        [Export] public Camera3D CameraTelefone;

        [ExportGroup("Marcações (Pontos na Mesa)")]
        [Export] public Marker3D PontoRadio;
        [Export] public Marker3D PontoComputador;
        [Export] public Marker3D PontoTelefone;

        [ExportGroup("Configurações de Movimento")]
        [Export] public float VelocidadeRotacao = 8.0f;

        [ExportGroup("Referências do Personagem")]
        [Export] public PersonagemPrincipal Jogador;

        [ExportGroup("UI")]
        [Export] public CanvasLayer UI;

        [ExportGroup("Equipamentos")]
        [Export] public Radio Radio;

        public bool EstaFocadoNoEquipamento { get; private set; } = false;

        private Quaternion _quaternionAlvo;
        private bool _emTransicao = false;
        private bool _aguardandoSoltarInteragir = false;

        public override void _EnterTree()
        {
            if (Instance == null) Instance = this;
            else QueueFree();
        }

        public override void _Process(double delta)
        {
            
            if (_emTransicao && CameraMesa != null)
            {
                Quaternion rotacaoAtual = CameraMesa.GlobalTransform.Basis.GetRotationQuaternion();
                Quaternion novaRotacao = rotacaoAtual.Slerp(_quaternionAlvo, (float)delta * VelocidadeRotacao);

                Vector3 posicaoAtual = CameraMesa.GlobalPosition;
                CameraMesa.GlobalTransform = new Transform3D(new Basis(novaRotacao), posicaoAtual);

                if (rotacaoAtual.AngleTo(_quaternionAlvo) < 0.001f)
                {
                    CameraMesa.GlobalTransform = new Transform3D(new Basis(_quaternionAlvo), posicaoAtual);
                    _emTransicao = false;
                }
            }
        }

        public void SentarNaCadeira()
        {
            AtivarCameraMesa();
            FocarEquipamento(EquipamentoMesa.Computador, instantaneo: false);
            _aguardandoSoltarInteragir = true;
            if (UI != null) UI.Visible = true;
        }

        public void LevantarDaCadeira()
        {
            DesfocarDoEquipamento();
            if (CameraMesa != null) CameraMesa.Current = false;
            if (UI != null) UI.Visible = false;

            Jogador?.DefinirSubEstadoMesa(PersonagemPrincipal.SubEstadoMesa.Nenhum);
            Jogador?.AlternarEstado(PersonagemPrincipal.EstadoJogador.Normal);
        }

        public void FocarEquipamento(EquipamentoMesa novoEquipamento, bool instantaneo = false)
        {
            
            if (EstaFocadoNoEquipamento && novoEquipamento != EquipamentoAtual)
            {
                DesfocarDoEquipamento();
            }
          
            EquipamentoAtual = novoEquipamento;

            if (Jogador != null)
            {
                if (Jogador.EstadoAtual != PersonagemPrincipal.EstadoJogador.Sentado)
                {
                    Jogador.AlternarEstado(PersonagemPrincipal.EstadoJogador.Sentado);
                }

                Jogador.DefinirSubEstadoMesa(novoEquipamento switch
                {
                    EquipamentoMesa.Computador => PersonagemPrincipal.SubEstadoMesa.Computador,
                    EquipamentoMesa.Radio => PersonagemPrincipal.SubEstadoMesa.Radio,
                    EquipamentoMesa.Telefone => PersonagemPrincipal.SubEstadoMesa.Telefone,
                    _ => PersonagemPrincipal.SubEstadoMesa.Nenhum
                });
            }

            Marker3D pontoAlvo = ObterPontoEquipamento(novoEquipamento);
            if (pontoAlvo == null || CameraMesa == null) return;

            Transform3D transformAlvo = CameraMesa.GlobalTransform.LookingAt(pontoAlvo.GlobalPosition, Vector3.Up);
            _quaternionAlvo = transformAlvo.Basis.GetRotationQuaternion();

            if (instantaneo)
            {
                Vector3 posAtual = CameraMesa.GlobalPosition;
                CameraMesa.GlobalTransform = new Transform3D(new Basis(_quaternionAlvo), posAtual);
                _emTransicao = false;
            }
            else
            {
                _emTransicao = true;
            }

            GD.Print($"Olhando para o equipamento: {EquipamentoAtual}");
        }

        public void PressinouInteragirEEntrouNoFocoNoEquipamento()
        {
            Camera3D cameraAlvo = ObterCameraEquipamento(EquipamentoAtual);
            LidaComFocoNoEquipamentoCorrente(EquipamentoAtual);
            if (cameraAlvo != null)
            {
                cameraAlvo.MakeCurrent();
                EstaFocadoNoEquipamento = true;
                Input.MouseMode = Input.MouseModeEnum.Visible;
                GD.Print($"Entrou no zoom do equipamento: {EquipamentoAtual}");
            }
            else
            {
                GD.PrintErr($"Nenhuma câmera configurada para o equipamento: {EquipamentoAtual}");
            }
        }

        private void LidaComFocoNoEquipamentoCorrente(EquipamentoMesa equipamento)
        {
            if (equipamento == EquipamentoMesa.Radio)
                Radio.FocandoNoRadio();
        }

        public void DesfocarDoEquipamento()
        {
            EstaFocadoNoEquipamento = false;
            AtivarCameraMesa();
            Input.MouseMode = Input.MouseModeEnum.Captured;
            GD.Print("Voltou para a câmera geral da mesa.");
            Radio.DesfocandoNoRadio();
        }

        private void AtivarCameraMesa()
        {
            if (CameraMesa != null)
            {
                CameraMesa.MakeCurrent();
            }
        }

        private Marker3D ObterPontoEquipamento(EquipamentoMesa equipamento)
        {
            return equipamento switch
            {
                EquipamentoMesa.Radio => PontoRadio,
                EquipamentoMesa.Computador => PontoComputador,
                EquipamentoMesa.Telefone => PontoTelefone,
                _ => PontoComputador
            };
        }

        private Camera3D ObterCameraEquipamento(EquipamentoMesa equipamento)
        {
            return equipamento switch
            {
                EquipamentoMesa.Radio => CameraRadio,
                EquipamentoMesa.Computador => CameraComputador,
                EquipamentoMesa.Telefone => CameraTelefone,
                _ => CameraComputador
            };
        }

        private bool TentarObterEquipamentoPorSubEstado(PersonagemPrincipal.SubEstadoMesa subEstado, out EquipamentoMesa equipamento)
        {
            equipamento = subEstado switch
            {
                PersonagemPrincipal.SubEstadoMesa.Radio => EquipamentoMesa.Radio,
                PersonagemPrincipal.SubEstadoMesa.Computador => EquipamentoMesa.Computador,
                PersonagemPrincipal.SubEstadoMesa.Telefone => EquipamentoMesa.Telefone,
                _ => EquipamentoAtual
            };

            return subEstado != PersonagemPrincipal.SubEstadoMesa.Nenhum;
        }

        private EquipamentoMesa ObterEquipamentoEsquerda(EquipamentoMesa equipamentoAtual)
        {
            return equipamentoAtual switch
            {
                EquipamentoMesa.Computador => EquipamentoMesa.Radio,
                EquipamentoMesa.Telefone => EquipamentoMesa.Computador,
                _ => equipamentoAtual
            };
        }

        private EquipamentoMesa ObterEquipamentoDireita(EquipamentoMesa equipamentoAtual)
        {
            return equipamentoAtual switch
            {
                EquipamentoMesa.Computador => EquipamentoMesa.Telefone,
                EquipamentoMesa.Radio => EquipamentoMesa.Computador,
                _ => equipamentoAtual
            };
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (Jogador == null || Jogador.EstadoAtual != PersonagemPrincipal.EstadoJogador.Sentado) return;

            if (_aguardandoSoltarInteragir)
            {
                if (@event.IsActionReleased("interagir")) _aguardandoSoltarInteragir = false;
                return;
            }

            if (@event.IsActionPressed("interagir"))
            {
                if (EstaFocadoNoEquipamento) return;
                //if (TentarObterEquipamentoPorSubEstado(Jogador.SubEstadoAtual, out EquipamentoMesa equipamentoSubEstado))
                //{
                //    FocarEquipamento(equipamentoSubEstado, instantaneo: true);
                //}
                PressinouInteragirEEntrouNoFocoNoEquipamento();
                GetViewport().SetInputAsHandled();
                return;
            }

            if (@event.IsActionPressed("ui_cancel"))
            {
                if (EstaFocadoNoEquipamento) DesfocarDoEquipamento();
                else LevantarDaCadeira();

                GetViewport().SetInputAsHandled();
                return;
            }

            if (EstaFocadoNoEquipamento) return;

            if (@event.IsActionPressed("ui_left"))
            {
                EquipamentoMesa equipamentoEsquerda = ObterEquipamentoEsquerda(EquipamentoAtual);
                if (equipamentoEsquerda != EquipamentoAtual) FocarEquipamento(equipamentoEsquerda);
                return;
            }

            if (@event.IsActionPressed("ui_right"))
            {
                EquipamentoMesa equipamentoDireita = ObterEquipamentoDireita(EquipamentoAtual);
                if (equipamentoDireita != EquipamentoAtual) FocarEquipamento(equipamentoDireita);
            }
        }
    }
}