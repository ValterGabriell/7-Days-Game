using Flags;
using Godot;
using Scripts.SaveSystem;
using System;
using System.Threading.Tasks;

namespace fiveyears3.Scripts.Globais
{
    public partial class GerenciadorPassagemDoTempo : Node
    {
        public enum EstadoDoDia { Parado, EmAndamento }
        public static GerenciadorPassagemDoTempo Instance { get; private set; }

        public event Action<int> DiaAlterado;
        public event Action HorarioDeTrabalhoIniciado;

        public EstadoDoDia EstadoAtual { get; private set; } = EstadoDoDia.Parado;


        public double TempoEmSilencioNoDiaAtual { get; set; } = 0.0;

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
            Log.Print($"[GerenciadorPassagemDoTempo] Avançando para o dia {DiaAtual + 1}.");
            FinalizarDiaDeTrabalho();
        }

        public void ResetarTempo()
        {
            DiaAtual = 1;
            DiaAlterado?.Invoke(DiaAtual);
        }

        public void CarregarDiaAtual(int dia)
        {
            DiaAtual = Math.Max(1, dia);
            DiaAlterado?.Invoke(DiaAtual);
        }

        public void IniciarDiaDeTrabalho()
        {
            Log.Print($"[GerenciadorPassagemDoTempo] Iniciando o dia {DiaAtual} de trabalho.");

            if (DiaAtual == 1)
            {
                GerenciadorDeAudiencia.Instance?.RegistrarImpactoAoIniciarOPrimeiroDia();
            }

            AtivarFlagDeDiaIniciado(DiaAtual);

            EstadoAtual = EstadoDoDia.EmAndamento;
            HorarioDeTrabalhoIniciado?.Invoke();
        }

        private void AtivarFlagDeDiaIniciado(int dia)
        {
            if (GerenciadorDeFlagsNarrativas.Instance == null) return;

            string nomeFlag = $"DIA_{dia}_INICIADO";

            if (Enum.TryParse(nomeFlag, out FlagsCondicionais flagDia))
            {
                GerenciadorDeFlagsNarrativas.Instance.AtivarFlagCondicional(flagDia);
                return;
            }

            Log.Print($"[GerenciadorPassagemDoTempo] Flag condicional não encontrada: {nomeFlag}");
        }
        public async void FinalizarDiaDeTrabalho()
        {
            // Fade out antes de realizar a finalização (escurece a tela)
            await FadeOutAsync();

            Log.Print($"[GerenciadorPassagemDoTempo] Finalizando o dia {DiaAtual} de trabalho.");
            EstadoAtual = EstadoDoDia.Parado;

            // 1. Guarda o dia que acabou de ser concluído antes de incrementar
            int diaConcluidoIndex = this.DiaAtual;

            // 2. Calcula os impactos acumulados no GerenciadorDeConfiabilidade
            var resumoImpactos = GerenciadorDeConfiabilidade.Instance != null
                ? GerenciadorDeConfiabilidade.Instance.GerarResumoImpactosDoDia()
                : ResumoImpactosSave.CriarNovoResumoImpactos(0f, 0f, 0f);

            // 3. Cria a estrutura do dia concluído
            var escolhasDoDia = GerenciadorDeSave.Instance.VariavelAuxiliarQueVaiGuardarAsEscolhasFeitasEmUmDeterminadoDia;

            var novoDiaConcluido = DiaConcluidoSave.CriarNovoDiaConcluido(
                diaConcluidoIndex,
                (float)TempoEmSilencioNoDiaAtual,
                escolhasDoDia,
                resumoImpactos
            );

            // 4. Obtém o Save ativo ou cria um novo se ainda não existir
            DadosSave save = GerenciadorDeSave.Instance.SaveAtual ?? DadosSave.CriarNovoSave();

            // 5. Adiciona o dia concluído ao histórico
            save.HistoricoDiasConcluidos.Add(novoDiaConcluido);

            // 6. Atualiza o estado do jogador para o próximo dia no Save
            this.DiaAtual += 1;
            save.EstadoAtualDoJogador.DiaAtual = this.DiaAtual;

            // Atualiza as reputações globais acumuladas com o delta do dia
            save.EstadoAtualDoJogador.Reputacao.LealdadeGoverno += resumoImpactos.DeltaLealdadeGoverno;
            save.EstadoAtualDoJogador.Reputacao.ConfiancaResistencia += resumoImpactos.DeltaConfiancaResistencia;
            save.EstadoAtualDoJogador.Reputacao.AudienciaPopular += resumoImpactos.DeltaAudiencia;

            // 7. Notifica a mudança de dia para o restante do jogo
            DiaAlterado?.Invoke(this.DiaAtual);

            // 8. Salva o progresso em disco (SLOT_1)
            GerenciadorDeSave.Instance.SalvarJogo("SLOT_1", save);

            // 9. Reseta o acúmulo de tempo em silêncio e deltas para o próximo dia
            TempoEmSilencioNoDiaAtual = 0;
            GerenciadorDeConfiabilidade.Instance?.ResetarDeltasDoDia();
            GerenciadorDeNoticias.Instance?.ResetarValoresDeNoticiasEMusicasQueDevemSerTransmitidasNoDia();

            Log.Print($"[GerenciadorPassagemDoTempo] Dia {DiaAtual} finalizado.");

            // Fade in para revelar o novo dia
            await FadeInAsync();
        }

        private CanvasLayer _fadeCanvas;
        private ColorRect _fadeRect;
        private const float TEMPO_FADE = 0.8f;

        private async Task FadeOutAsync()
        {
            if (_fadeCanvas == null)
            {
                _fadeCanvas = new CanvasLayer { Layer = 100 };
                _fadeRect = new ColorRect
                {
                    Color = new Color(0, 0, 0, 0),
                    MouseFilter = Control.MouseFilterEnum.Stop
                };

                _fadeRect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
                _fadeCanvas.AddChild(_fadeRect);
                AddChild(_fadeCanvas);
            }

            Tween tween = CreateTween();
            tween.TweenProperty(_fadeRect, "color:a", 1.0f, TEMPO_FADE);
            await ToSignal(tween, Tween.SignalName.Finished);
        }

        private async Task FadeInAsync()
        {
            if (_fadeRect == null) return;

            Tween tween = CreateTween();
            tween.TweenProperty(_fadeRect, "color:a", 0.0f, TEMPO_FADE);
            await ToSignal(tween, Tween.SignalName.Finished);

            _fadeCanvas?.QueueFree();
            _fadeCanvas = null;
            _fadeRect = null;
        }
    }
}