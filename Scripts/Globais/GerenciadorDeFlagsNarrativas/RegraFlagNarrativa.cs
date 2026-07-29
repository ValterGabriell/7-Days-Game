using System.Collections.Generic;
using System.Text.Json.Serialization;
using Flags;

namespace fiveyears3.Scripts.Globais;

public class RegraFlagNarrativa
{
    [JsonPropertyName("id")]
    public FlagNarrativa Id { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; }

    [JsonPropertyName("descricao")]
    public string Descricao { get; set; }

    [JsonPropertyName("gatilhoDeAtivacao")]
    public GatilhoAtivacao Gatilho { get; set; }
}

public class GatilhoAtivacao
{
    [JsonPropertyName("flagsCondicionaisRequeridas")]
    public List<FlagsCondicionais> FlagsCondicionaisRequeridas { get; set; }

    [JsonPropertyName("atrasoEmSegundos")]
    public float AtrasoEmSegundos { get; set; }

    [JsonPropertyName("eventoGerado")]
    public string EventoGerado { get; set; }

    [JsonPropertyName("alvoNaCena")]
    public string AlvoNaCena { get; set; }
}

public class RaizConfiguracaoFlags
{
    [JsonPropertyName("flagsNarrativas")]
    public List<RegraFlagNarrativa> FlagsNarrativas { get; set; }
}