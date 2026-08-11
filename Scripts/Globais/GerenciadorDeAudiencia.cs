using Godot;
using System;

namespace fiveyears3.Scripts.Globais
{
    public partial class GerenciadorDeAudiencia : Node
    {
        public static GerenciadorDeAudiencia Instance { get; private set; }

        [Obsolete("Use MetricasAlteradas instead for a more complete event.")]
        public event Action<double, double> AudienciaAlterada;

        // Evento completo de métricas: (variacaoAudiencia, variacaoEsperanca, variacaoIrritacao)
        public event Action<double, double, double> MetricasAlteradas;

        public double AudienciaAtual { get; private set; } = 50.0;
        public double EsperancaAtual { get; private set; } = 40.0;
        public double IrritacaoAtual { get; private set; } = 20.0;

        public double ConfiabilidadeGoverno => EsperancaAtual - IrritacaoAtual;
        public double ConfiabilidadeResistencia => RetornaConfiabilidadeRestanteParaOOutroLado();

        public override void _Ready()
        {
            if (Instance != null)
            {
                Log.PrintErr("Já existe uma instância de GerenciadorDeAudiencia. Esta instância será removida.");
                QueueFree();
                return;
            }
            Instance = this;
        }

        private double RetornaConfiabilidadeRestanteParaOOutroLado()
        {
            return Math.Clamp(100.0 - ConfiabilidadeGoverno, 0.0, 100.0);
        }

        public void RegistrarImpactoAoIniciarOPrimeiroDia()
        {
            AudienciaAlterada?.Invoke(AudienciaAtual, 0.0);
            MetricasAlteradas?.Invoke(0.0, 0.0, 0.0);

            Log.Print($"[GerenciadorAudiencia] Dia iniciado com Audiência: {AudienciaAtual}%, Esperança: {EsperancaAtual}, Irritação: {IrritacaoAtual}");
        }

        public void RegistrarImpactoNoticia(double variacaoEsperanca, double variacaoIrritacao, double audienciaGanha)
        {
            double audienciaAnterior = AudienciaAtual;
            AudienciaAtual = Math.Clamp(AudienciaAtual + audienciaGanha, 0.0, 100.0);

            EsperancaAtual += variacaoEsperanca;
            IrritacaoAtual += variacaoIrritacao;

            double varAud = AudienciaAtual - audienciaAnterior;

            AudienciaAlterada?.Invoke(AudienciaAtual, varAud);
            MetricasAlteradas?.Invoke(varAud, variacaoEsperanca, variacaoIrritacao);
        }

        public void RegistrarImpactoCasoFiqueSilencioDuranteATransmissaoJaIniciada(double tempoDeSilencio)
        {
            if (GerenciadorPassagemDoTempo.Instance != null)
            {
                GerenciadorPassagemDoTempo.Instance.TempoEmSilencioNoDiaAtual = tempoDeSilencio;
            }

            double audienciaAnterior = AudienciaAtual;

            double taxaPorSegundo = 100.0 / 120.0; // ~0.833% por segundo
            double audienciaPerdida = 2.0 * taxaPorSegundo; // ~1.66% a cada chamada de 2s

            AudienciaAtual = Math.Clamp(AudienciaAtual - audienciaPerdida, 0.0, 100.0);

            double deltaIrritacao = 2.0 * 0.1;
            IrritacaoAtual += deltaIrritacao;

            double varAud = AudienciaAtual - audienciaAnterior;

            AudienciaAlterada?.Invoke(AudienciaAtual, varAud);
            MetricasAlteradas?.Invoke(varAud, 0.0, deltaIrritacao);
        }

        public void CarregarEstado(double audiencia, double esperanca, double irritacao)
        {
            AudienciaAtual = audiencia;
            EsperancaAtual = esperanca;
            IrritacaoAtual = irritacao;

            // Dispara eventos para que a Interface/UI atualize na hora que carregar
            AudienciaAlterada?.Invoke(AudienciaAtual, 0.0);
            MetricasAlteradas?.Invoke(0.0, 0.0, 0.0);
        }

        public EstadoClimaSocial ObterEstadoClimaSocial()
        {
            // 1. Prejudica o JOGADOR
            if (AudienciaAtual < 20.0)
                return EstadoClimaSocial.AudienciaBaixa;

            // 2. Prejudica a RESISTÊNCIA (Audiência Alta + Governo Dominante)
            if (AudienciaAtual >= 60.0 && EsperancaAtual > IrritacaoAtual + 10.0)
                return EstadoClimaSocial.DominadoPeloGoverno;

            // 3. Prejudica os RICOS/GOVERNO (Audiência Alta + Resistência Dominante)
            if (AudienciaAtual >= 60.0 && IrritacaoAtual > EsperancaAtual + 10.0)
                return EstadoClimaSocial.RevoltaPopular;

            // 4. Mediana / Tanto Faz
            return EstadoClimaSocial.TensaoEquilibrada;
        }

        public void ResetarDados()
        {
            AudienciaAtual = 50.0;
            EsperancaAtual = 0.0;
            IrritacaoAtual = 0.0;
        }
    }

    public enum EstadoClimaSocial
    {
        TensaoEquilibrada,
        DominadoPeloGoverno,
        RevoltaPopular,
        AudienciaBaixa
    }
}