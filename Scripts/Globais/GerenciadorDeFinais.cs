using Godot;
using System;
using Flags;
using Scripts.SaveSystem;

namespace fiveyears3.Scripts.Globais
{
    public enum TipoFinal
    {
        Nenhum,
        VitoriaGoverno,         // Audiência alta e lealdade/esperança do governo dominante no fim da campanha
        VitoriaResistencia,     // Audiência alta e irritação/confiança da resistência dominante no fim da campanha
        DemissaoAudienciaBaixa, // Perdeu relevância: audiência caiu abaixo do limite crítico
        SobrevivenciaNeutro,    // Chegou ao fim da campanha mantendo o equilíbrio de forças
        FimPrematuro            // Encerramento forçado por decisões ou flags narrativas diretas
    }

    public partial class GerenciadorDeFinais : Node
    {
        public static GerenciadorDeFinais Instance { get; private set; }

        /// <summary>
        /// Evento disparado no exato instante em que o jogo chega ao fim.
        /// </summary>
        public event Action<TipoFinal> FinalAlcancado;

        /// <summary>
        /// Armazena qual final foi acionado.
        /// </summary>
        public TipoFinal FinalAtual { get; private set; } = TipoFinal.Nenhum;

        /// <summary>
        /// Indica se o jogo já chegou ao fim.
        /// </summary>
        public bool JogoFinalizado => FinalAtual != TipoFinal.Nenhum;

        [Export]
        public int DiaFinalDaCampanha { get; set; } = 7;

        [Export]
        public double LimiteAudienciaMinima { get; set; } = 5.0;

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

        public override void _Ready()
        {
            ConectarEventosDosGerenciadores();
        }

        public override void _ExitTree()
        {
            DesconectarEventosDosGerenciadores();
        }

        private void ConectarEventosDosGerenciadores()
        {
            if (GerenciadorPassagemDoTempo.Instance != null)
            {
                GerenciadorPassagemDoTempo.Instance.DiaAlterado += OnDiaAlterado;
            }

            if (GerenciadorDeAudiencia.Instance != null)
            {
                GerenciadorDeAudiencia.Instance.MetricasAlteradas += OnMetricasAudienciaAlteradas;
            }

            if (GerenciadorDeFlagsNarrativas.Instance != null)
            {
                GerenciadorDeFlagsNarrativas.Instance.OnFlagAtivada += OnFlagNarrativaAtivada;
            }
        }

        private void DesconectarEventosDosGerenciadores()
        {
            if (GerenciadorPassagemDoTempo.Instance != null)
            {
                GerenciadorPassagemDoTempo.Instance.DiaAlterado -= OnDiaAlterado;
            }

            if (GerenciadorDeAudiencia.Instance != null)
            {
                GerenciadorDeAudiencia.Instance.MetricasAlteradas -= OnMetricasAudienciaAlteradas;
            }

            if (GerenciadorDeFlagsNarrativas.Instance != null)
            {
                GerenciadorDeFlagsNarrativas.Instance.OnFlagAtivada -= OnFlagNarrativaAtivada;
            }
        }

        #region Callbacks de Eventos Automaticos

        private void OnDiaAlterado(int novoDia)
        {
            if (JogoFinalizado) return;

            Log.Print($"[GerenciadorDeFinais] Avaliando condições de término para o Dia {novoDia}...");
            VerificarCondicoesDeFimDeJogo(novoDia);
        }

        private void OnMetricasAudienciaAlteradas(double varAudiencia, double varEsperanca, double varIrritacao)
        {
            if (JogoFinalizado) return;

            // Checa derrota imediata se a audiência cair abaixo do limite aceitável
            if (GerenciadorDeAudiencia.Instance != null)
            {
                if (GerenciadorDeAudiencia.Instance.AudienciaAtual <= LimiteAudienciaMinima)
                {
                    Log.Print($"[GerenciadorDeFinais] Audiência caiu abaixo de {LimiteAudienciaMinima}%. Disparando demissão!");
                    GatilharFinal(TipoFinal.DemissaoAudienciaBaixa);
                }
            }
        }

        private void OnFlagNarrativaAtivada(FlagNarrativa flag)
        {
            if (JogoFinalizado) return;

            // Mapeamento de gatilhos diretos de término via Flags Narrativas
            switch (flag)
            {
                case FlagNarrativa.RevoltaPopularIniciandoGovernoBateAPorta:
                    // Exemplo de condição especial se necessário
                    break;
            }
        }

        #endregion

        #region Logica Principal de Decisao dos Finais

        /// <summary>
        /// Avalia as métricas globais e o dia atual para determinar se o jogo deve ser encerrado automaticamente.
        /// </summary>
        public void VerificarCondicoesDeFimDeJogo(int diaAtual)
        {
            Log.Print($"[GerenciadorDeFinais] Verificando condições de fim de jogo no Dia {diaAtual}...");
            if (JogoFinalizado) return;

            if (GerenciadorDeAudiencia.Instance == null)
            {
                Log.PrintErr("[GerenciadorDeFinais] GerenciadorDeAudiencia não foi encontrado!");
                return;
            }

            // 1. Checa se atingiu ou passou o último dia estipulado para a campanha
            if (diaAtual > DiaFinalDaCampanha)
            {
                Log.Print($"[GerenciadorDeFinais] Dia final da campanha ({DiaFinalDaCampanha}) atingido. Avaliando clima social para determinar final...");
                EstadoClimaSocial clima = GerenciadorDeAudiencia.Instance.ObterEstadoClimaSocial();
                DeterminarFinalAoConcluirCampanha(clima);
            }
        }

        private void DeterminarFinalAoConcluirCampanha(EstadoClimaSocial clima)
        {
            var aud = GerenciadorDeAudiencia.Instance;

            switch (clima)
            {
                case EstadoClimaSocial.DominadoPeloGoverno:
                    GatilharFinal(TipoFinal.VitoriaGoverno);
                    break;

                case EstadoClimaSocial.RevoltaPopular:
                    GatilharFinal(TipoFinal.VitoriaResistencia);
                    break;

                case EstadoClimaSocial.AudienciaBaixa:
                    GatilharFinal(TipoFinal.DemissaoAudienciaBaixa);
                    break;

                case EstadoClimaSocial.TensaoEquilibrada:
                default:
                    // Se não tiver clima social extremo, decide pelo balanço das confiabilidades
                    if (aud.ConfiabilidadeGoverno >= 60.0)
                    {
                        GatilharFinal(TipoFinal.VitoriaGoverno);
                    }
                    else if (aud.ConfiabilidadeResistencia >= 60.0)
                    {
                        GatilharFinal(TipoFinal.VitoriaResistencia);
                    }
                    else
                    {
                        GatilharFinal(TipoFinal.SobrevivenciaNeutro);
                    }
                    break;
            }
        }

        /// <summary>
        /// Dispara o encerramento do jogo com o tipo de final especificado, salvando o progresso e notificando os ouvintes.
        /// </summary>
        public void GatilharFinal(TipoFinal final)
        {
            if (JogoFinalizado) return;

            FinalAtual = final;
            Log.Print($"[GerenciadorDeFinais] === FIM DE JOGO ALCANÇADO: {final} ===");

            // Salva o estado final no save ativo se disponível
            if (GerenciadorDeSave.Instance != null)
            {
                GerenciadorDeSave.Instance.ColetarDadosDosGerenciadores();
                if (GerenciadorDeSave.Instance.SaveAtual != null)
                {
                    GerenciadorDeSave.Instance.SalvarJogo("SLOT_1", GerenciadorDeSave.Instance.SaveAtual);
                }
            }

            // Notifica UI, Cutscenes ou Gerenciadores de Cena
            FinalAlcancado?.Invoke(final);
        }

        /// <summary>
        /// Reseta o estado do finalizador (utilizado ao iniciar um Novo Jogo).
        /// </summary>
        public void ResetarFinais()
        {
            FinalAtual = TipoFinal.Nenhum;
            Log.Print("[GerenciadorDeFinais] Estado de finais resetado.");
        }

        #endregion
    }
}