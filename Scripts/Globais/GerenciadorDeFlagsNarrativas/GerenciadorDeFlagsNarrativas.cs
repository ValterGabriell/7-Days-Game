using Flags;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace fiveyears3.Scripts.Globais;

public partial class GerenciadorDeFlagsNarrativas : Node
{
    public static GerenciadorDeFlagsNarrativas Instance { get; private set; }

    private const string CAMINHO_JSON = @"C:\DEV\PROJETOSPESSOAIS\FIVE-YEARS-3\scripts\globais\gerenciadordeflagsnarrativas\QuandoSaoAtivadas.json";

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
            if (File.Exists(CAMINHO_JSON))
            {
                string jsonString = File.ReadAllText(CAMINHO_JSON);
                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() },
                    PropertyNameCaseInsensitive = true
                };

                var configuracao = JsonSerializer.Deserialize<RaizConfiguracaoFlags>(jsonString, options);
                if (configuracao?.FlagsNarrativas != null)
                {
                    _regrasConfiguradas = configuracao.FlagsNarrativas;
                    GD.Print($"[Flags] Configuração carregada com sucesso. {_regrasConfiguradas.Count} regras registradas.");
                }
            }
            else
            {
                GD.PrintErr($"[Flags] Arquivo JSON não encontrado no caminho: {CAMINHO_JSON}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Flags] Erro ao carregar/desserializar o JSON: {ex.Message}");
        }
    }

    public void AtivarFlagCondicional(FlagsCondicionais flagCondicional)
    {
        if (FlagsCondicionaisAtivas.Add(flagCondicional))
        {
            GD.Print($"[Flags] Flag Condicional ativada: {flagCondicional}");
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
        GD.Print($"[Flags] Condições atendidas para {regra.Id}. Disparando em {atraso} segundos...");

        if (atraso > 0)
        {
            GD.Print($"[Flags] Aguardando {atraso} segundos antes de ativar a flag {regra.Id}.");
            await ToSignal(GetTree().CreateTimer(atraso), SceneTreeTimer.SignalName.Timeout);
        }

        AtivarFlagNarrativa(regra.Id);
    }

    public void AtivarFlagNarrativa(FlagNarrativa flag)
    {
        if (FlagsNarrativasAtivas.Add(flag))
        {
            GD.Print($"[Flags] Flag Narrativa ativada: {flag}");
            OnFlagAtivada?.Invoke(flag);
        }
    }

    public void RemoverFlagNarrativa(FlagNarrativa flag)
    {
        if (FlagsNarrativasAtivas.Remove(flag))
        {
            GD.Print($"[Flags] Flag Narrativa removida: {flag}");
        }
    }

    private bool TemFlagNarrativa(FlagNarrativa flag)
    {
        return FlagsNarrativasAtivas.Contains(flag);
    }

    public void AdicionarFlagCondicional(FlagsCondicionais flagCondicional)
    {
        if (FlagsCondicionaisAtivas.Add(flagCondicional))
        {
            GD.Print($"[Flags] Flag Condicional adicionada: {flagCondicional}");
            VerificarGatilhosDeFlagsNarrativas();
        }
    }
}