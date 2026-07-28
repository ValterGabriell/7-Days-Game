using Godot;
using System;

namespace fiveyears3.Scripts.Globais
{
    public partial class GerenciadorDeAudiencia : Node
    {
        public static GerenciadorDeAudiencia Instance { get; private set; }

        public event Action<double, double> AudienciaAlterada;
        public event Action<double, double> ClimaSocialAlterado;

        public double AudienciaAtual { get; private set; } = 50.0;
        public double EsperancaAtual { get; private set; } = 40.0; 
        public double IrritacaoAtual { get; private set; } = 20.0;

        public double ConfiabilidadeGoverno => EsperancaAtual - IrritacaoAtual;
        public double ConfiabilidadeResistencia => IrritacaoAtual - EsperancaAtual;

        public override void _Ready()
        {
            if (Instance != null)
            {
                GD.PrintErr("Já existe uma instância de GerenciadorDeAudiencia. Esta instância será removida.");
                QueueFree();
                return;
            }
            Instance = this;
        }

        public void RegistrarImpactoAoIniciarOPrimeiroDia()
        {
            AudienciaAlterada?.Invoke(AudienciaAtual, 0.0);
            ClimaSocialAlterado?.Invoke(EsperancaAtual, IrritacaoAtual);
            GD.Print($"[GerenciadorAudiencia] Dia iniciado com Audiência: {AudienciaAtual}%, Esperança: {EsperancaAtual}, Irritação: {IrritacaoAtual}");
        }

        public void RegistrarImpactoNoticia(double variacaoEsperanca, double variacaoIrritacao, double audienciaGanha)
        {
            double audienciaAnterior = AudienciaAtual;
            AudienciaAtual = Math.Clamp(AudienciaAtual + audienciaGanha, 0.0, 100.0);

            EsperancaAtual += variacaoEsperanca;
            IrritacaoAtual += variacaoIrritacao;

            AudienciaAlterada?.Invoke(AudienciaAtual, AudienciaAtual - audienciaAnterior);
            ClimaSocialAlterado?.Invoke(EsperancaAtual, IrritacaoAtual);
        }

        public void RegistrarImpactoCasoFiqueSilencioDuranteATransmissaoJaIniciada(double tempoDeSilencio)
        {
            double audienciaAnterior = AudienciaAtual;

            // Perde ~0.833% de audiência por segundo de silêncio.
            // Em 120 segundos (2 minutos), a perda acumulada será de 100%.
            double taxaPorSegundo = 100.0 / 120.0; // ~0.833% por segundo

            // Como o método é chamado a cada 2 segundos, calculamos a perda referente a esses 2s
            double audienciaPerdida = 2.0 * taxaPorSegundo; // ~1.66% a cada chamada

            AudienciaAtual = Math.Clamp(AudienciaAtual - audienciaPerdida, 0.0, 100.0);

            // Irritação sobe suavemente também
            IrritacaoAtual += 2.0 * 0.1;

            AudienciaAlterada?.Invoke(AudienciaAtual, AudienciaAtual - audienciaAnterior);
            ClimaSocialAlterado?.Invoke(EsperancaAtual, IrritacaoAtual);
        }

        public EstadoClimaSocial ObterEstadoClimaSocial()
        {
            if (AudienciaAtual < 20.0)
                return EstadoClimaSocial.AudienciaBaixa;

            if (EsperancaAtual > IrritacaoAtual + 10.0)
                return EstadoClimaSocial.DominadoPeloGoverno;

            if (IrritacaoAtual > EsperancaAtual + 10.0)
                return EstadoClimaSocial.RevoltaPopular;

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