using Godot;
using System;
using System.Collections.Generic;
using Scripts.SaveSystem;

namespace fiveyears3.Scripts.Globais
{
    public partial class GerenciadorDeConfiabilidade : Node
    {
        public static GerenciadorDeConfiabilidade Instance { get; private set; }

        // Acumuladores de impacto do dia atual
        public float DeltaLealdadeGovernoDia { get; private set; } = 0.0f;
        public float DeltaConfiancaResistenciaDia { get; private set; } = 0.0f;
        public float DeltaAudienciaDia { get; private set; } = 0.0f;

        public override void _EnterTree()
        {
            if (Instance == null)
            {
                Instance = this;
                return;
            }
            QueueFree();
        }

        public override void _Ready()
        {
            if (GerenciadorPassagemDoTempo.Instance != null)
            {
                GerenciadorPassagemDoTempo.Instance.DiaAlterado += OnDiaAlterado;
            }
        }

        private void OnDiaAlterado(int novoDia)
        {
            ResetarDeltasDoDia();
        }

        /// <summary>
        /// Zera os deltas acumulados para o início de um novo dia.
        /// </summary>
        public void ResetarDeltasDoDia()
        {
            DeltaLealdadeGovernoDia = 0.0f;
            DeltaConfiancaResistenciaDia = 0.0f;
            DeltaAudienciaDia = 0.0f;
        }

        /// <summary>
        /// Registra o impacto individual de uma notícia transmitida baseando-se na escolha do jogador.
        /// </summary>
        public void ProcessarImpactoNoticia(NoticiaModel noticia)
        {
            if (noticia == null || noticia.Variacoes == null) return;

            if (noticia.Variacoes.TryGetValue(noticia.EscolhaJogador, out VariacaoNoticia variacaoUsada) && variacaoUsada?.Impacto != null)
            {
                var impacto = variacaoUsada.Impacto;

                float deltaLealdade = (float)impacto.VariacaoEsperanca;
                float deltaResistencia = (float)impacto.VariacaoIrritacao;
                float deltaAudiencia = (float)impacto.AudienciaGanha;

                DeltaLealdadeGovernoDia += deltaLealdade;
                DeltaConfiancaResistenciaDia += deltaResistencia;
                DeltaAudienciaDia += deltaAudiencia;

                GerenciadorDeAudiencia.Instance?.RegistrarImpactoNoticia(
                    impacto.VariacaoEsperanca,
                    impacto.VariacaoIrritacao,
                    impacto.AudienciaGanha
                );

                GD.Print($"[GerenciadorDeConfiabilidade] Impacto notícia '{noticia.Id}' registrado. Escolha: {noticia.EscolhaJogador}");
            }
        }

        /// <summary>
        /// Calcula o resumo de impactos do dia com base em todas as notícias transmitidas.
        /// Útil no final do dia antes de gravar no Save.
        /// </summary>
        public ResumoImpactosSave GerarResumoImpactosDoDia()
        {
            return ResumoImpactosSave.CriarNovoResumoImpactos(
                DeltaLealdadeGovernoDia,
                DeltaConfiancaResistenciaDia,
                DeltaAudienciaDia
            );
        }

        /// <summary>
        /// Gera o resumo recalculando diretamente a lista de notícias transmitidas.
        /// </summary>
        public ResumoImpactosSave GerarResumoImpactosDoDia(List<NoticiaModel> noticiasTransmitidas)
        {
            float lealdade = 0f;
            float resistencia = 0f;
            float audiencia = 0f;

            if (noticiasTransmitidas != null)
            {
                foreach (var noticia in noticiasTransmitidas)
                {

                    if (noticia.Variacoes != null && noticia.Variacoes.TryGetValue(noticia.EscolhaJogador, out var variacao) && variacao?.Impacto != null)
                    {
                        lealdade += (float)variacao.Impacto.VariacaoEsperanca;
                        resistencia += (float)variacao.Impacto.VariacaoIrritacao;
                        audiencia += (float)variacao.Impacto.AudienciaGanha;
                    }
                }
            }

            return ResumoImpactosSave.CriarNovoResumoImpactos(lealdade, resistencia, audiencia);
        }
    }
}