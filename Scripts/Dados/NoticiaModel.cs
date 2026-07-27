using System.Collections.Generic;


public enum AcaoEditorial
{
    ORIGINAL, 
    OMITIR,   
    MENTIR,   
    DISTORCER     
}
public class NoticiaModel
{
    public string Id { get; set; }
    public string Remetente { get; set; } 
    public string TituloOriginal { get; set; }
    public string TextoOriginal { get; set; }
    

    public Dictionary<AcaoEditorial, VariacaoNoticia> Variacoes { get; set; } = new();
    public AcaoEditorial EscolhaJogador { get; set; } = AcaoEditorial.ORIGINAL;

}