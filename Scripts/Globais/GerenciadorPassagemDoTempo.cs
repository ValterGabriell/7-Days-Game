using Godot;
using System;

namespace fiveyears3.Scripts.Globais
{
    public partial class GerenciadorPassagemDoTempo : Node
    {
        public static GerenciadorPassagemDoTempo Instance { get; private set; }

        [Signal]
        public delegate void DiaAlteradoEventHandler(int novoDia);

        public int DiaAtual { get; private set; } = 1;

        public override void _EnterTree()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                QueueFree();
            }
        }

        public void AvancarDia()
        {
            DiaAtual++;
            EmitSignal(SignalName.DiaAlterado, DiaAtual);
        }

        public void ResetarTempo()
        {
            DiaAtual = 1;
            EmitSignal(SignalName.DiaAlterado, DiaAtual);
        }
    }
}