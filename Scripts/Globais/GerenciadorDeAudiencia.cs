using Godot;
using System;

namespace fiveyears3.Scripts.Globais
{
    public enum EstadoClimaSocial
    {
        AudienciaBaixa,             // Prejudica o JOGADOR (Demitido/Descartado)
        DominadoPeloGoverno,        // Audiência Alta + FDP/Governo Forte (Prejudica a RESISTÊNCIA)
        RevoltaPopular,             // Audiência Alta + Hades/Resistência Forte (Prejudica o GOVERNO/RICOS)
        SobrevivenciaNasSombras,    // Audiência Mediana + Resistência com mais Confiança
        ColaboracionistaSilencioso, // Audiência Mediana + Governo com mais Confiança
        TensaoEquilibrada           // Audiência Mediana + Forças Equilibradas
    }

    public partial class GerenciadorDeAudiencia : Node
    {
        public static GerenciadorDeAudiencia Instance { get; private set; }

        [Obsolete("Use MetricasAlteradas instead for a more complete event.")]
        public event Action<double, double> AudienciaAlterada;
        public event Action<double, double, double> MetricasAlteradas;

        public const double MIN_METRICA = 0.0;
        public const double MAX_METRICA = 100.0;

        public double AudienciaAtual { get; private set; } = 50.0;
        public double EsperancaAtual { get; private set; } = 40.0;  // Representa alinhamento/confiança com a FDP / Governo
        public double IrritacaoAtual { get; private set; } = 20.0;  // Representa alinhamento/confiança com a Hades / Resistência

        // Confiabilidades normalizadas (0 a 100)
        public double ConfiabilidadeGoverno => Math.Clamp(EsperancaAtual - IrritacaoAtual + 50.0, MIN_METRICA, MAX_METRICA);
        public double ConfiabilidadeResistencia => Math.Clamp(IrritacaoAtual - EsperancaAtual + 50.0, MIN_METRICA, MAX_METRICA);

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

        public void RegistrarImpactoAoIniciarOPrimeiroDia()
        {
            AudienciaAlterada?.Invoke(AudienciaAtual, 0.0);
            MetricasAlteradas?.Invoke(0.0, 0.0, 0.0);
            Log.Print($"[GerenciadorAudiencia] Dia iniciado - Audiência: {AudienciaAtual}%, Esperança: {EsperancaAtual}, Irritação: {IrritacaoAtual}");
        }

        public void RegistrarImpactoNoticia(double variacaoEsperanca, double variacaoIrritacao, double audienciaGanha)
        {
            double audienciaAnterior = AudienciaAtual;

            AudienciaAtual = Math.Clamp(AudienciaAtual + audienciaGanha, MIN_METRICA, MAX_METRICA);
            EsperancaAtual = Math.Clamp(EsperancaAtual + variacaoEsperanca, MIN_METRICA, MAX_METRICA);
            IrritacaoAtual = Math.Clamp(IrritacaoAtual + variacaoIrritacao, MIN_METRICA, MAX_METRICA);

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
            double taxaPorSegundo = 100.0 / 120.0;
            double audienciaPerdida = 2.0 * taxaPorSegundo;

            AudienciaAtual = Math.Clamp(AudienciaAtual - audienciaPerdida, MIN_METRICA, MAX_METRICA);
            IrritacaoAtual = Math.Clamp(IrritacaoAtual + 0.2, MIN_METRICA, MAX_METRICA);

            double varAud = AudienciaAtual - audienciaAnterior;

            AudienciaAlterada?.Invoke(AudienciaAtual, varAud);
            MetricasAlteradas?.Invoke(varAud, 0.0, 0.2);
        }

        public void CarregarEstado(double audiencia, double esperanca, double irritacao)
        {
            AudienciaAtual = Math.Clamp(audiencia, MIN_METRICA, MAX_METRICA);
            EsperancaAtual = Math.Clamp(esperanca, MIN_METRICA, MAX_METRICA);
            IrritacaoAtual = Math.Clamp(irritacao, MIN_METRICA, MAX_METRICA);

            AudienciaAlterada?.Invoke(AudienciaAtual, 0.0);
            MetricasAlteradas?.Invoke(0.0, 0.0, 0.0);
        }

        /// <summary>
        /// Avalia o clima social combinando o alcance da rádio (Audiência) 
        /// com a facção dominante (Confiança da FDP vs. Hades).
        /// </summary>
        public EstadoClimaSocial ObterEstadoClimaSocial()
        {
            // 1. REGRA 1: Audiência Baixa (< 20%) -> Prejudica o JOGADOR
            if (AudienciaAtual < 20.0)
            {
                return EstadoClimaSocial.AudienciaBaixa;
            }

            double confGoverno = ConfiabilidadeGoverno;
            double confResistencia = ConfiabilidadeResistencia;

            // 2. REGRA 2: Audiência Alta (>= 60%) -> Climas Extremos de Alto Impacto
            if (AudienciaAtual >= 60.0)
            {
                // Audiência alta e Governo com larga vantagem -> Prejudica a RESISTÊNCIA
                if (confGoverno >= confResistencia + 15.0)
                    return EstadoClimaSocial.DominadoPeloGoverno;

                // Audiência alta e Resistência com larga vantagem -> Prejudica os RICOS/GOVERNO
                if (confResistencia >= confGoverno + 15.0)
                    return EstadoClimaSocial.RevoltaPopular;
            }

            // 3. REGRA 3: Audiência Mediana ou Tensão Não-Extrema -> Avalia puramente quem tem mais Confiança
            if (confResistencia > confGoverno + 10.0)
            {
                return EstadoClimaSocial.SobrevivenciaNasSombras;
            }

            if (confGoverno > confResistencia + 10.0)
            {
                return EstadoClimaSocial.ColaboracionistaSilencioso;
            }

            // 4. Se a diferença for mínima entre as facções -> Tensão Equilibrada
            return EstadoClimaSocial.TensaoEquilibrada;
        }

        public void ResetarDados()
        {
            AudienciaAtual = 50.0;
            EsperancaAtual = 40.0;
            IrritacaoAtual = 20.0;
        }
    }
}