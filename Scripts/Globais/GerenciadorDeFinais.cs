using Godot;
using System;
using Flags;
using Scripts.SaveSystem;

namespace fiveyears3.Scripts.Globais
{
    public enum TipoFinal
    {
        Nenhum,
        RevolucaoHades,           // reputacao_hades: alta, reputacao_fdp: baixa
        OrdemCorporativa,         // reputacao_hades: baixa, reputacao_fdp: alta
        PazArmada,                // reputacao_hades: alta, reputacao_fdp: alta
        CaosTotal,                // reputacao_hades: baixa, reputacao_fdp: baixa
        DemissaoAudienciaBaixa,   // Audiência caiu abaixo do limite crítico (< 20%)
        FugaSoloEden,             // Final narrativo via Flag/Decisão direta
        FimPrematuro              // Encerramento forçado genérico
    }

    public partial class GerenciadorDeFinais : Node
    {
        public static GerenciadorDeFinais Instance { get; private set; }

        public event Action<TipoFinal> FinalAlcancado;

        public TipoFinal FinalAtual { get; private set; } = TipoFinal.Nenhum;

        public bool JogoFinalizado => FinalAtual != TipoFinal.Nenhum;

        [Export]
        public int DiaFinalDaCampanha { get; set; } = 7;

        [Export]
        public double LimiteAudienciaMinima { get; set; } = 20.0;

        [Export]
        public string CaminhoCenaCreditos { get; set; } = "res://Cenas/Utilidades/Creditos.tscn";

        [Export]
        public string NomeSlotSave { get; set; } = "SLOT_1";

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

            // Derrota por perda de audiência crítica
            if (GerenciadorDeAudiencia.Instance != null)
            {
                if (GerenciadorDeAudiencia.Instance.AudienciaAtual < LimiteAudienciaMinima)
                {
                    Log.Print($"[GerenciadorDeFinais] Audiência caiu abaixo de {LimiteAudienciaMinima}%. Disparando Demissão!");
                    GatilharFinal(TipoFinal.DemissaoAudienciaBaixa);
                }
            }
        }

        private void OnFlagNarrativaAtivada(FlagNarrativa flag)
        {
            if (JogoFinalizado) return;

            // Exemplo de Gatilhos Diretos por Flags Narrativas
            switch (flag)
            {
                case FlagNarrativa.RevoltaPopularIniciandoGovernoBateAPorta:
                    // Pode disparar um final antecipado por invadir a rádio, por exemplo
                    break;
            }
        }

        #endregion

        #region Logica Principal de Decisao dos Finais

        public void VerificarCondicoesDeFimDeJogo(int diaAtual)
        {
            if (JogoFinalizado) return;

            if (GerenciadorDeAudiencia.Instance == null)
            {
                Log.PrintErr("[GerenciadorDeFinais] GerenciadorDeAudiencia não foi encontrado!");
                return;
            }

            // Checa se encerrou o último dia de campanha
            if (diaAtual > DiaFinalDaCampanha)
            {
                Log.Print($"[GerenciadorDeFinais] Dia final da campanha ({DiaFinalDaCampanha}) atingido. Avaliando Reputações...");
                DeterminarFinalPorReputacao();
            }
        }

        /// <summary>
        /// Avalia as reputações de Hades e FDP para decidir entre os 4 Finais Principais do JSON.
        /// </summary>
        private void DeterminarFinalPorReputacao()
        {
            var aud = GerenciadorDeAudiencia.Instance;

            // Define reputação 'Alta' como >= 50
            bool hadesAlta = aud.ConfiabilidadeResistencia >= 50.0;
            bool fdpAlta = aud.ConfiabilidadeGoverno >= 50.0;

            if (hadesAlta && !fdpAlta)
            {
                GatilharFinal(TipoFinal.RevolucaoHades);
            }
            else if (!hadesAlta && fdpAlta)
            {
                GatilharFinal(TipoFinal.OrdemCorporativa);
            }
            else if (hadesAlta && fdpAlta)
            {
                GatilharFinal(TipoFinal.PazArmada);
            }
            else // !hadesAlta && !fdpAlta
            {
                GatilharFinal(TipoFinal.CaosTotal);
            }
        }

        public void GatilharFinal(TipoFinal final)
        {
            if (JogoFinalizado) return;

            FinalAtual = final;
            Log.Print($"[GerenciadorDeFinais] === FIM DE JOGO ALCANÇADO: {final} ===");

            // 1. Apaga o save do jogo finalizado
            DeletarSaveDoJogo();

            // 2. Notifica inscritos
            FinalAlcancado?.Invoke(final);

            // 3. Muda para a cena de Créditos
            CallDeferred(MethodName.TrocarParaCenaDeCreditos);
        }

        private void DeletarSaveDoJogo()
        {
            if (GerenciadorDeSave.Instance != null)
            {
                Log.Print($"[GerenciadorDeFinais] Apagando save do slot '{NomeSlotSave}' por conta do fim de jogo...");
                GerenciadorDeSave.Instance.DeletarSave(NomeSlotSave);
            }
            else
            {
                Log.PrintErr("[GerenciadorDeFinais] Não foi possível apagar o save: GerenciadorDeSave.Instance é nulo!");
            }
        }

        private void TrocarParaCenaDeCreditos()
        {
            Log.Print($"[GerenciadorDeFinais] Trocando cena para: {CaminhoCenaCreditos}");
            Error err = GetTree().ChangeSceneToFile(CaminhoCenaCreditos);

            if (err != Error.Ok)
            {
                Log.PrintErr($"[GerenciadorDeFinais] Erro ao carregar a cena de créditos ({CaminhoCenaCreditos}): {err}");
            }
        }

        public void ResetarFinais()
        {
            FinalAtual = TipoFinal.Nenhum;
            Log.Print("[GerenciadorDeFinais] Estado de finais resetado.");
        }

        #endregion
    }
}