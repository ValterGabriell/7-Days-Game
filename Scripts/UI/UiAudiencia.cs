using fiveyears3.Scripts.Globais;
using Godot;
using System;

public partial class UiAudiencia : CanvasLayer
{
    [ExportGroup("Audiência")]
    [Export] public Label LabelAudiencia;
    [Export] public ProgressBar BarraAudiencia;

    [ExportGroup("Métricas do Clima")]
    [Export] public Label LabelEsperanca;
    [Export] public Label LabelIrritacao;
    [Export] public Label LabelClimaSocial;

    public override void _Ready()
    {
        if (GerenciadorDeAudiencia.Instance != null)
        {
            // Inscreve no novo evento unificado de métricas
            GerenciadorDeAudiencia.Instance.MetricasAlteradas += OnMetricasAlteradas;

            // Atualização inicial com os dados atuais do gerenciador
            AtualizarUi(0.0, 0.0, 0.0);
        }
    }

    public override void _ExitTree()
    {
        if (GerenciadorDeAudiencia.Instance != null)
        {
            GerenciadorDeAudiencia.Instance.MetricasAlteradas -= OnMetricasAlteradas;
        }
    }

    private void OnMetricasAlteradas(double varAudiencia, double varEsperanca, double varIrritacao)
    {
        AtualizarUi(varAudiencia, varEsperanca, varIrritacao);
    }

    private void AtualizarUi(double varAudiencia, double varEsperanca, double varIrritacao)
    {
        var aud = GerenciadorDeAudiencia.Instance;
        if (aud == null) return;

        // 1. Atualiza Audiência
        if (LabelAudiencia != null)
        {
            LabelAudiencia.Text = $"Audiência: {aud.AudienciaAtual:F1}%";
        }

        if (BarraAudiencia != null)
        {
            BarraAudiencia.Value = aud.AudienciaAtual;
        }

        // 2. Atualiza Esperança e Irritação
        if (LabelEsperanca != null)
        {
            LabelEsperanca.Text = $"Esperança: {aud.EsperancaAtual:F1}";
        }

        if (LabelIrritacao != null)
        {
            LabelIrritacao.Text = $"Irritação: {aud.IrritacaoAtual:F1}";
        }

        // 3. Atualiza Clima Social
        if (LabelClimaSocial != null)
        {
            EstadoClimaSocial clima = aud.ObterEstadoClimaSocial();
            LabelClimaSocial.Text = $"Clima Social: {ObterTextoFormatadoClima(clima)}";
        }

        // Log de variação de audiência
        if (varAudiencia < 0)
        {
            Log.Print($"[UI] Audiência caindo! Perdeu {Math.Abs(varAudiencia):F1}%");
        }
    }

    private string ObterTextoFormatadoClima(EstadoClimaSocial clima)
    {
        return clima switch
        {
            EstadoClimaSocial.AudienciaBaixa => "Audiência Baixa (Crítico)",
            EstadoClimaSocial.DominadoPeloGoverno => "Dominado pelo Governo",
            EstadoClimaSocial.RevoltaPopular => "Revolta Popular",
            EstadoClimaSocial.TensaoEquilibrada => "Tensão Equilibrada",
            _ => "Desconhecido"
        };
    }
}