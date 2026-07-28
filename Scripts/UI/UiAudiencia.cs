using fiveyears3.Scripts.Globais;
using Godot;
using System;

public partial class UiAudiencia : CanvasLayer
{
    [Export] public Label LabelAudiencia;
    [Export] public ProgressBar BarraAudiencia; 

    public override void _Ready()
    {
        // Garante que o gerenciador existe antes de conectar
        if (GerenciadorDeAudiencia.Instance != null)
        {
            GerenciadorDeAudiencia.Instance.AudienciaAlterada += OnAudienciaAlterada;

            AtualizarUi(GerenciadorDeAudiencia.Instance.AudienciaAtual, 0);
        }
    }

    public override void _ExitTree()
    {
        // Sempre desinscreva do evento ao destruir/sair do nó
        if (GerenciadorDeAudiencia.Instance != null)
        {
            GerenciadorDeAudiencia.Instance.AudienciaAlterada -= OnAudienciaAlterada;
        }
    }

    private void OnAudienciaAlterada(double novaAudiencia, double variacao)
    {
        AtualizarUi(novaAudiencia, variacao);
    }

    private void AtualizarUi(double audiencia, double variacao)
    {
        if (LabelAudiencia != null)
        {
            LabelAudiencia.Text = $"Audiência: {audiencia:F1}%";
        }

        if (BarraAudiencia != null)
        {
            BarraAudiencia.Value = audiencia;
        }

        if (variacao < 0)
        {
            GD.Print($"[UI] Audiência caindo! Perdeu {Math.Abs(variacao):F1}%");
        }
    }
}