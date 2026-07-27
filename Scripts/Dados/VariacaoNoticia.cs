using System.Text.Json.Serialization;

public class VariacaoNoticia
{
    public string TituloAlterado { get; set; }
    public string TextoParaLer { get; set; }
    public ImpactoSocial Impacto { get; set; }

    public VariacaoNoticia() { }

    [JsonConstructor]
    public VariacaoNoticia(string tituloAlterado, string textoParaLer, ImpactoSocial impacto)
    {
        TituloAlterado = tituloAlterado;
        TextoParaLer = textoParaLer;
        Impacto = impacto;
    }
}