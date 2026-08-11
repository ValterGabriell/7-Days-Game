using Godot;
using System;
using Scripts.SaveSystem;

namespace fiveyears3.Scripts.Globais
{
    public partial class GerenciadorDeConfiabilidade : Node
    {
        public static GerenciadorDeConfiabilidade Instance { get; private set; }


        public float DeltaLealdadeGovernoDia { get; private set; } = 0.0f;
        public float DeltaConfiancaResistenciaDia { get; private set; } = 0.0f;
        public float DeltaAudienciaDia { get; private set; } = 0.0f;

        public float DeltaLealdadeGovernoGeral { get; private set; } = 0.0f;
        public float DeltaConfiancaResistenciaGeral { get; private set; } = 0.0f;
        public float DeltaAudienciaGeral { get; private set; } = 0.0f;

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

            if (GerenciadorDeAudiencia.Instance != null)
            {
                // Escuta o evento unificado de alteração de métricas da audiência
                GerenciadorDeAudiencia.Instance.MetricasAlteradas += OnMetricasAlteradas;
            }
        }

        private void OnMetricasAlteradas(double varAudiencia, double varEsperanca, double varIrritacao)
        {
            float deltaAud = (float)varAudiencia;
            float deltaGov = (float)varEsperanca;
            float deltaRes = (float)varIrritacao;

            // Incrementa o acumulador diário
            DeltaAudienciaDia += deltaAud;
            DeltaLealdadeGovernoDia += deltaGov;
            DeltaConfiancaResistenciaDia += deltaRes;

            // Incrementa o acumulador geral
            DeltaAudienciaGeral += deltaAud;
            DeltaLealdadeGovernoGeral += deltaGov;
            DeltaConfiancaResistenciaGeral += deltaRes;
            ProcessarEfeitosDoClimaSocial();
        }

        private void OnDiaAlterado(int novoDia)
        {
            
            ResetarDeltasDoDia();
        }

        public void ResetarDeltasDoDia()
        {
            DeltaLealdadeGovernoDia = 0.0f;
            DeltaConfiancaResistenciaDia = 0.0f;
            DeltaAudienciaDia = 0.0f;
        }

        /// <summary>
        /// Processa regras de impacto no encerramento do dia com base no Clima Social.
        /// </summary>
        public void ProcessarEfeitosDoClimaSocial()
        {
            Log.Print("[GerenciadorDeConfiabilidade] Processando efeitos do Clima Social...");
            if (GerenciadorDeAudiencia.Instance == null) return;

            EstadoClimaSocial clima = GerenciadorDeAudiencia.Instance.ObterEstadoClimaSocial();

            switch (clima)
            {
                case EstadoClimaSocial.AudienciaBaixa:
                    // Prejudica o JOGADOR (ex: perda de alcance/recursos)
                    Log.Print("[ClimaSocial] Audiência Baixa: O Jogador perde relevância e suporte geral.");
                    break;

                case EstadoClimaSocial.DominadoPeloGoverno:
                    // Audiência alta e confiança dos Ricos/Governo alta -> Prejudica a RESISTÊNCIA
                    Log.Print("[ClimaSocial] Dominado pelo Governo: A Resistência sofre retaliações e perde força.");
                    break;

                case EstadoClimaSocial.RevoltaPopular:
                    // Audiência alta e confiança da Resistência alta -> Prejudica os RICOS/GOVERNO
                    Log.Print("[ClimaSocial] Revolta Popular: Os Ricos/Governo perdem estabilidade e controle.");
                    break;

                case EstadoClimaSocial.TensaoEquilibrada:
                    // Audiência mediana: A influência direta é ditada pura e simplesmente pelos valores vigentes
                    Log.Print("[ClimaSocial] Tensão Equilibrada: Forças em neutralidade temporária.");
                    break;
            }
        }

        public void ProcessarImpactoNoticia(NoticiaModel noticia)
        {
            if (noticia == null || noticia.Variacoes == null) return;

            if (noticia.Variacoes.TryGetValue(noticia.EscolhaJogador, out VariacaoNoticia variacaoUsada) && variacaoUsada?.Impacto != null)
            {
                var impacto = variacaoUsada.Impacto;

                // Chama o GerenciadorDeAudiencia que irá alterar os dados e disparar o evento
                GerenciadorDeAudiencia.Instance?.RegistrarImpactoNoticia(
                    impacto.VariacaoEsperanca,
                    impacto.VariacaoIrritacao,
                    impacto.AudienciaGanha
                );

                Log.Print($"[GerenciadorDeConfiabilidade] Impacto notícia '{noticia.Id}' registrado.");
            }
        }

        public void CarregarEstadoGeral(float lealdade, float resistencia, float audiencia)
        {
            DeltaLealdadeGovernoGeral = lealdade;
            DeltaConfiancaResistenciaGeral = resistencia;
            DeltaAudienciaGeral = audiencia;
        }

        public ResumoImpactosSave GerarResumoImpactosDoDia()
        {
            return ResumoImpactosSave.CriarNovoResumoImpactos(
                DeltaLealdadeGovernoDia,
                DeltaConfiancaResistenciaDia,
                DeltaAudienciaDia
            );
        }
    }
}