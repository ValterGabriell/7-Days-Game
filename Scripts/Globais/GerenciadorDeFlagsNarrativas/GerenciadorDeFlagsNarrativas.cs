using Flags;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace fiveyears3.Scripts.Globais;

public partial class GerenciadorDeFlagsNarrativas : Node
{
    public static GerenciadorDeFlagsNarrativas Instance { get; private set; }

    private const string CAMINHO_JSON = @"res://scripts/globais/gerenciadordeflagsnarrativas/QuandoSaoAtivadas.json";

    public HashSet<FlagNarrativa> FlagsNarrativasAtivas { get; private set; } = new HashSet<FlagNarrativa>();
    public HashSet<FlagsCondicionais> FlagsCondicionaisAtivas { get; private set; } = new HashSet<FlagsCondicionais>();

    private List<RegraFlagNarrativa> _regrasConfiguradas = new List<RegraFlagNarrativa>();

    public event Action<FlagNarrativa> OnFlagAtivada;

    public override void _EnterTree()
    {
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }

        Instance = this;
    }

    public override void _Ready()
    {
        CarregarConfiguracaoJSON();
    }

    private void CarregarConfiguracaoJSON()
    {
        try
        {
            if (FileAccess.FileExists(CAMINHO_JSON))
            {
                using var arquivo = FileAccess.Open(CAMINHO_JSON, FileAccess.ModeFlags.Read);
                if (arquivo == null)
                {
                    Log.PrintErr($"[Flags] Falha ao abrir JSON: {CAMINHO_JSON} | Erro: {FileAccess.GetOpenError()}");
                    return;
                }

                string jsonString = arquivo.GetAsText();
                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() },
                    PropertyNameCaseInsensitive = true
                };

                var configuracao = JsonSerializer.Deserialize<RaizConfiguracaoFlags>(jsonString, options);
                if (configuracao?.FlagsNarrativas != null)
                {
                    _regrasConfiguradas = configuracao.FlagsNarrativas;
                    Log.Print($"[Flags] Configuração carregada com sucesso. {_regrasConfiguradas.Count} regras registradas.");
                }
            }
            else
            {
                Log.PrintErr($"[Flags] Arquivo JSON não encontrado no caminho: {CAMINHO_JSON}");
            }
        }
        catch (Exception ex)
        {
            Log.PrintErr($"[Flags] Erro ao carregar/desserializar o JSON: {ex.Message}");
        }
    }

    public void AtivarFlagCondicional(FlagsCondicionais flagCondicional)
    {
        if (FlagsCondicionaisAtivas.Contains(flagCondicional)) return;
        if (FlagsCondicionaisAtivas.Add(flagCondicional))
        {
            Log.Print($"[Flags] Flag Condicional ativada: {flagCondicional}");
            VerificarGatilhosDeFlagsNarrativas();
        }
    }

    private void VerificarGatilhosDeFlagsNarrativas()
    {
        foreach (var regra in _regrasConfiguradas)
        {
            if (TemFlagNarrativa(regra.Id)) continue;

            bool todasCondicoesAtendidas = regra.Gatilho.FlagsCondicionaisRequeridas
                .All(condicao => FlagsCondicionaisAtivas.Contains(condicao));

            if (todasCondicoesAtendidas)
            {
                AgendarAtivacaoFlagNarrativa(regra);
            }
        }
    }

    private async void AgendarAtivacaoFlagNarrativa(RegraFlagNarrativa regra)
    {
        float atraso = regra.Gatilho.AtrasoEmSegundos;
        Log.Print($"[Flags] Condições atendidas para {regra.Id}. Disparando em {atraso} segundos...");

        if (atraso > 0)
        {
            Log.Print($"[Flags] Aguardando {atraso} segundos antes de ativar a flag {regra.Id}.");
            await ToSignal(GetTree().CreateTimer(atraso), SceneTreeTimer.SignalName.Timeout);
        }

        AtivarFlagNarrativa(regra.Id);
    }

    public void AtivarFlagNarrativa(FlagNarrativa flag)
    {
        if (FlagsNarrativasAtivas.Add(flag))
        {
            Log.Print($"[Flags] Flag Narrativa ativada: {flag}");
            OnFlagAtivada?.Invoke(flag);
        }
    }

    public void RemoverFlagNarrativa(FlagNarrativa flag)
    {
        if (FlagsNarrativasAtivas.Remove(flag))
        {
            Log.Print($"[Flags] Flag Narrativa removida: {flag}");
        }
    }

    private bool TemFlagNarrativa(FlagNarrativa flag)
    {
        return FlagsNarrativasAtivas.Contains(flag);
    }

}