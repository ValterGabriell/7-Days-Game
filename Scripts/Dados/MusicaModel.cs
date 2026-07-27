using System.Text.Json.Serialization;

public enum EfeitoEmocionalMusica
{
    ANIMAR,
    DESANIMAR,
    ESPERANCAR,
    ACALMAR,
    REVOLTAR
}

public class ImpactoMusicaModel
{
    public float VariacaoEsperanca { get; set; }
    public float VariacaoIrritacao { get; set; }
    public float AudienciaGanha { get; set; }
}

public class MusicaModel
{
    public string Id { get; set; }
    public string Titulo { get; set; }
    public string Artista { get; set; }
    public string Genero { get; set; }
    public string CaminhoAudio { get; set; }
    public int DuracaoSegundos { get; set; }
    public EfeitoEmocionalMusica EfeitoDominante { get; set; }
    public string DescricaoEfeito { get; set; }
    public ImpactoMusicaModel Impacto { get; set; } = new();
}