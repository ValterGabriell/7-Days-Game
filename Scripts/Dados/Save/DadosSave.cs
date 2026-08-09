using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using static VariacaoNoticia;

namespace Scripts.SaveSystem
{
    public class DadosSave
    {
        [JsonPropertyName("VersaoSave")]
        public string VersaoSave { get; set; } = "1.0.0";

        [JsonPropertyName("DataEHoraSave")]
        public string DataEHoraSave { get; set; } = "";

        [JsonPropertyName("EstadoAtualDoJogador")]
        public EstadoJogadorSave EstadoAtualDoJogador { get; set; } = new();

        [JsonPropertyName("EstatisticasGerais")]
        public EstatisticasGeraisSave EstatisticasGerais { get; set; } = new();

        [JsonPropertyName("HistoricoDiasConcluidos")]
        public List<DiaConcluidoSave> HistoricoDiasConcluidos { get; set; } = new();

        public static DadosSave CriarNovoSave()
        {
            return new DadosSave
            {
                VersaoSave = "1.0.0",
                DataEHoraSave = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                EstadoAtualDoJogador = EstadoJogadorSave.CriarNovoEstado(),
                EstatisticasGerais = EstatisticasGeraisSave.CriarNovasEstatisticas(),
                HistoricoDiasConcluidos = new List<DiaConcluidoSave>()
            };
        }

        public override string ToString()
        {
           return $"VersaoSave: {VersaoSave}, DataEHoraSave: {DataEHoraSave}, DiaAtual: {EstadoAtualDoJogador.DiaAtual}, Reputacao: [LealdadeGoverno: {EstadoAtualDoJogador.Reputacao.LealdadeGoverno}, ConfiancaResistencia: {EstadoAtualDoJogador.Reputacao.ConfiancaResistencia}, AudienciaPopular: {EstadoAtualDoJogador.Reputacao.AudienciaPopular}, EsperancaPopulacional: {EstadoAtualDoJogador.Reputacao.EsperancaPopulacional}, IrritacaoPopulacional: {EstadoAtualDoJogador.Reputacao.IrritacaoPopulacional}], AdvertenciasGoverno: {EstadoAtualDoJogador.AdvertenciasGoverno}, FlagsHistoricas: [{string.Join(", ", EstadoAtualDoJogador.FlagsHistoricas)}], TempoDeInatividadeGeral: {EstatisticasGerais.TempoDeInatividadeGeral}, TotalNoticiasTransmitidas: {EstatisticasGerais.TotalNoticiasTransmitidas}, TotalNoticiasCensuradas: {EstatisticasGerais.TotalNoticiasCensuradas}, EscolhasAcumuladas: [{string.Join(", ", EstatisticasGerais.EscolhasAcumuladas)}], HistoricoDiasConcluidosCount: {HistoricoDiasConcluidos.Count}";
        }
    }

    public class EstadoJogadorSave
    {
        [JsonPropertyName("DiaAtual")]
        public int DiaAtual { get; set; } = 1;

        [JsonPropertyName("Reputacao")]
        public ReputacaoSave Reputacao { get; set; } = new();

        [JsonPropertyName("AdvertenciasGoverno")]
        public int AdvertenciasGoverno { get; set; } = 0;

        [JsonPropertyName("FlagsHistoricas")]
        public Dictionary<string, bool> FlagsHistoricas { get; set; } = new();

        public static EstadoJogadorSave CriarNovoEstado()
        {
            return new EstadoJogadorSave
            {
                DiaAtual = 1,
                Reputacao = new ReputacaoSave(),
                AdvertenciasGoverno = 0,
                FlagsHistoricas = new Dictionary<string, bool>()
            };
        }
    }

    public class ReputacaoSave
    {
        [JsonPropertyName("LealdadeGoverno")]
        public float LealdadeGoverno { get; set; } = 50.0f;

        [JsonPropertyName("ConfiancaResistencia")]
        public float ConfiancaResistencia { get; set; } = 0.0f;

        [JsonPropertyName("AudienciaPopular")]
        public float AudienciaPopular { get; set; } = 50.0f;

        [JsonPropertyName("EsperancaPopulacional")]
        public float EsperancaPopulacional { get; set; } = 50.0f;

        [JsonPropertyName("IrritacaoPopulacional")]
        public float IrritacaoPopulacional { get; set; } = 0.0f;

        public static ReputacaoSave CriarNovaReputacao()
        {
            return new ReputacaoSave
            {
                LealdadeGoverno = 50.0f,
                ConfiancaResistencia = 0.0f,
                AudienciaPopular = 50.0f,
                EsperancaPopulacional = 50.0f,
                IrritacaoPopulacional = 0.0f
            };
        }
    }

    public class EstatisticasGeraisSave
    {
        [JsonPropertyName("TempoDeInatividadeGeral")]
        public float TempoDeInatividadeGeral { get; set; } = 0.0f;

        [JsonPropertyName("TotalNoticiasTransmitidas")]
        public int TotalNoticiasTransmitidas { get; set; } = 0;

        [JsonPropertyName("TotalNoticiasCensuradas")]
        public int TotalNoticiasCensuradas { get; set; } = 0;

        [JsonPropertyName("EscolhasAcumuladas")]
        public Dictionary<string, int> EscolhasAcumuladas { get; set; } = new()
        {
            { "ORIGINAL", 0 },
            { "OMITIR", 0 },
            { "MENTIR", 0 },
            { "DISTORCER", 0 }
        };

        public static EstatisticasGeraisSave CriarNovasEstatisticas()
        {
            return new EstatisticasGeraisSave
            {
                TempoDeInatividadeGeral = 0.0f,
                TotalNoticiasTransmitidas = 0,
                TotalNoticiasCensuradas = 0,
                EscolhasAcumuladas = new Dictionary<string, int>
                {
                    { "ORIGINAL", 0 },
                    { "OMITIR", 0 },
                    { "MENTIR", 0 },
                    { "DISTORCER", 0 }
                }
            };
        }
    }

    public class DiaConcluidoSave
    {
        [JsonPropertyName("Dia")]
        public int Dia { get; set; }

        [JsonPropertyName("TempoDeInatividadeDoDia")]
        public float TempoDeInatividadeDoDia { get; set; }

        [JsonPropertyName("ClassificacaoInatividade")]
        public string ClassificacaoInatividade { get; set; } = "EXCELENTE";

        [JsonPropertyName("CumpriturouObrigacoesDoGoverno")]
        public bool CumpriturouObrigacoesDoGoverno { get; set; }

        [JsonPropertyName("NoticiasEscolhas")]
        public List<NoticiaEscolhaSave> NoticiasEscolhas { get; set; } = new();

        [JsonPropertyName("ResumoImpactosDoDia")]
        public ResumoImpactosSave ResumoImpactosDoDia { get; set; } = new();

        public static DiaConcluidoSave CriarNovoDiaConcluido(int dia, float tempoInatividade, 
            List<NoticiaEscolhaSave> noticiasEscolhas,
            ResumoImpactosSave impactos)
        {
            return new DiaConcluidoSave
            {
                Dia = dia,
                TempoDeInatividadeDoDia = tempoInatividade,
                ClassificacaoInatividade = ClassificarInatividade(tempoInatividade),
                CumpriturouObrigacoesDoGoverno = true,
                NoticiasEscolhas = noticiasEscolhas,
                ResumoImpactosDoDia = impactos
            };
        }

        private static string ClassificarInatividade(float tempoInatividade)
        {
            if (tempoInatividade < 30.0f)
                return "EXCELENTE";
            else if (tempoInatividade < 60.0f)
                return "BOM";
            else if (tempoInatividade < 90.0f)
                return "REGULAR";
            else
                return "RUIM";
        }
    }

    public class NoticiaEscolhaSave
    {
        [JsonPropertyName("IDNoticia")]
        public string IDNoticia { get; set; } = "";

        [JsonPropertyName("VariacaoEscolhida")]
        public string VariacaoEscolhida { get; set; } = "ORIGINAL";
        
        [JsonPropertyName("ImpressoresGeradasNoDiaSeguinte")]   
        public FeedbackImpressoes ImpressoresGeradasNoDiaSeguinte { get; set; } = new FeedbackImpressoes();

      
    }

    public class ResumoImpactosSave
    {
        [JsonPropertyName("DeltaLealdadeGoverno")]
        public float DeltaLealdadeGoverno { get; set; }

        [JsonPropertyName("DeltaConfiancaResistencia")]
        public float DeltaConfiancaResistencia { get; set; }

        [JsonPropertyName("DeltaAudiencia")]
        public float DeltaAudiencia { get; set; }

        public static ResumoImpactosSave CriarNovoResumoImpactos(float deltaLealdade, float deltaConfianca, float deltaAudiencia)
        {
            return new ResumoImpactosSave
            {
                DeltaLealdadeGoverno = deltaLealdade,
                DeltaConfiancaResistencia = deltaConfianca,
                DeltaAudiencia = deltaAudiencia
            };
        }
    }
}