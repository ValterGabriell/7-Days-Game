using Godot;
using fiveyears3.Scripts.Utilidades;
using fiveyears3.Scripts.Globais;
using Scripts.Personagens.Principal;
using System.Runtime.InteropServices.Marshalling;

public partial class Cadeira : StaticBody3D, IItemInteracao
{
    [Export] public Node3D PontoAssento;
    [Export] public PersonagemPrincipal Jogador;

    public void Interagir()
    {
        if (Jogador != null)
        {
            if (PontoAssento != null)
            {
                Jogador.GlobalTransform = PontoAssento.GlobalTransform;
            }
            Jogador.AlternarEstado(PersonagemPrincipal.EstadoJogador.Sentado);
            GerenciadorMesa.Instance?.SentarNaCadeira();
        }
    }
}