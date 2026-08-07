using System.Text.Json.Serialization;

public class VariacaoNoticia
{
    public string TituloAlterado { get; set; }
    public string TextoParaLer { get; set; }
    public ImpactoSocial Impacto { get; set; }
    public FeedbackImpressoes ImpressoresGeradasNoDiaSeguinte { get; set; } = new FeedbackImpressoes();
    public VariacaoNoticia() { }

    [JsonConstructor]
    public VariacaoNoticia(string tituloAlterado, string textoParaLer, ImpactoSocial impacto)
    {
        TituloAlterado = tituloAlterado;
        TextoParaLer = textoParaLer;
        Impacto = impacto;
    }


    public class FeedbackImpressoes
    {
        public MensagemImpressora Governo { get; set; }
        public MensagemImpressora Resistencia { get; set; }
    }

    public class MensagemImpressora
    {
        public string Falas { get; set; }
    }
}