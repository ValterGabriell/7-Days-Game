using System.Collections.Generic;

public enum TipoBlocoRotina
{
    NOTICIA,
    MUSICA
}

public class BlocoRotinaModel
{
    public int OrdemSugerida { get; set; }
    public TipoBlocoRotina Tipo { get; set; }
    public string NoticiaIdRelacionada { get; set; }
    public string DescricaoObrigacao { get; set; }
    public bool Obrigatorio { get; set; }
}

public class RotinaDiaModel
{
    public int Dia { get; set; }
    public string NotaGoverno { get; set; }
    public List<BlocoRotinaModel> Programacao { get; set; } = new();
}