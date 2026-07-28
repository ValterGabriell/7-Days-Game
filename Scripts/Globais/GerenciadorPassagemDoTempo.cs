using Godot;
using System;

namespace fiveyears3.Scripts.Globais
{
    public partial class GerenciadorPassagemDoTempo : Node
    {
        public enum EstadoDoDia { Parado, EmAndamento}
        public enum DiaCorrente { Primeiro, Segundo, Terceiro, Quarto, Quinto, Sexto, Setimo}
        public static GerenciadorPassagemDoTempo Instance { get; private set; }

        public event Action<int> DiaAlterado;
        public event Action HorarioDeTrabalhoIniciado;

        public EstadoDoDia EstadoAtual { get; private set; } = EstadoDoDia.Parado;
        public DiaCorrente DiaCorrenteAtual { get; private set; } = DiaCorrente.Primeiro;

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
            DiaAlterado?.Invoke(DiaAtual);
        }

        public void ResetarTempo()
        {
            DiaAtual = 1;
            DiaAlterado?.Invoke(DiaAtual);
        }

        public void IniciarHorarioDeTrabalho()
        {
            if(DiaAtual == 1)
            {
                GerenciadorDeAudiencia.Instance?.RegistrarImpactoAoIniciarOPrimeiroDia();
            }
            EstadoAtual = EstadoDoDia.EmAndamento;
            HorarioDeTrabalhoIniciado?.Invoke();
        }
    }
}