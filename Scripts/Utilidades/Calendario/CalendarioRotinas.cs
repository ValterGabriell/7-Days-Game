using fiveyears3.Scripts.Globais;
using fiveyears3.Scripts.Utilidades;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

public partial class CalendarioRotinas : StaticBody3D, IItemInteracao
{
    private enum EstadoCalendarioRotinasUiVisivel
    {
        Oculto,
        Visivel
    }

    [Export] public CanvasLayer CanvasLayer;
    [Export] public ItemList ListaRotinas;
    [Export] public Button DebugFinalizarDia;

    private EstadoCalendarioRotinasUiVisivel _estadoAtual = EstadoCalendarioRotinasUiVisivel.Oculto;
    private List<RotinaDiaModel> _todasAsRotinas = new();

    private const string CAMINHO_JSON = "res://Scripts/Dados/JSONS/rotinas_semanais.json";

    public override void _Ready()
    {
        base._Ready();
        CarregarRotinasDoJson();

        // Inscreve no evento de alteração de dia
        if (GerenciadorPassagemDoTempo.Instance != null)
        {
            GerenciadorPassagemDoTempo.Instance.DiaAlterado += OnDiaAlterado;
        }

        if (DebugFinalizarDia != null)
        {
            DebugFinalizarDia.Pressed += () =>
            {
                GerenciadorPassagemDoTempo.Instance?.AvancarDia();
            };
        }

        if (GerenciadorDeNoticias.Instance != null)
        {
            GerenciadorDeNoticias.Instance.FinalizacaoDoDiaLiberada += OnFinalizacaoDoDiaLiberada;
            GerenciadorDeNoticias.Instance.NoticiasCarregadas += OnNoticiasCarregadasDoDia;
        }
    }

    private void OnNoticiasCarregadasDoDia()
    {
        if (DebugFinalizarDia != null)
            DebugFinalizarDia.Disabled = true;
    }

    private void OnFinalizacaoDoDiaLiberada()
    {
        if (DebugFinalizarDia != null)
            DebugFinalizarDia.Disabled = false;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        if (GerenciadorPassagemDoTempo.Instance != null)
        {
            GerenciadorPassagemDoTempo.Instance.DiaAlterado -= OnDiaAlterado;
        }

        if (GerenciadorDeNoticias.Instance != null)
        {
            GerenciadorDeNoticias.Instance.FinalizacaoDoDiaLiberada -= OnFinalizacaoDoDiaLiberada;
            GerenciadorDeNoticias.Instance.NoticiasCarregadas -= OnNoticiasCarregadasDoDia;
        }
    }

    private void OnDiaAlterado(int novoDia)
    {
        if (_estadoAtual == EstadoCalendarioRotinasUiVisivel.Visivel)
        {
            ExibirRotinaDoDia(novoDia);
        }
    }

    public void Interagir()
    {
        if (_estadoAtual == EstadoCalendarioRotinasUiVisivel.Oculto)
        {
            AbrirInterface();
        }
        else
        {
            FecharInterface();
        }
    }

    private void AbrirInterface()
    {
        int diaAtual = GerenciadorPassagemDoTempo.Instance != null
            ? GerenciadorPassagemDoTempo.Instance.DiaAtual
            : 1;

        ExibirRotinaDoDia(diaAtual);

        if (CanvasLayer != null)
            CanvasLayer.Visible = true;

        _estadoAtual = EstadoCalendarioRotinasUiVisivel.Visivel;

        // LIBERA O MOUSE NA TELA
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void FecharInterface()
    {
        if (CanvasLayer != null)
            CanvasLayer.Visible = false;

        _estadoAtual = EstadoCalendarioRotinasUiVisivel.Oculto;

        // RECAPTURA E ESCONDE O MOUSE
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    private void CarregarRotinasDoJson()
    {
        if (!FileAccess.FileExists(CAMINHO_JSON))
        {
            Log.PrintErr($"[CalendarioRotinas] Arquivo JSON não encontrado em: {CAMINHO_JSON}");
            return;
        }

        using var file = FileAccess.Open(CAMINHO_JSON, FileAccess.ModeFlags.Read);
        string jsonTexto = file.GetAsText();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        // Permite converter enums mesmo com diferenças de maiúsculas/minúsculas no JSON
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));

        try
        {
            _todasAsRotinas = JsonSerializer.Deserialize<List<RotinaDiaModel>>(jsonTexto, options);
        }
        catch (Exception ex)
        {
            Log.PrintErr($"[CalendarioRotinas] Erro ao desserializar JSON: {ex.Message}");
        }
    }

    private void ExibirRotinaDoDia(int dia)
    {
        if (ListaRotinas == null) return;

        ListaRotinas.Clear();

        RotinaDiaModel rotinaDoDia = _todasAsRotinas.Find(r => r.Dia == dia);

        if (rotinaDoDia == null) return;

      
        if (GerenciadorDeNoticias.Instance != null)
        {
            GerenciadorDeNoticias.Instance.AtualizarValoresDeNoticiasEMusicasQueDevemSerTransmitidasNoDia(rotinaDoDia.Programacao);
        }
        foreach (var bloco in rotinaDoDia.Programacao)
        {
            string tagObrigatorio = bloco.Obrigatorio ? "*" : "";
            string textoItem = $"{bloco.OrdemSugerida}. [{bloco.Tipo}] {bloco.DescricaoObrigacao} {tagObrigatorio}";

            ListaRotinas.AddItem(textoItem);
        }
    }
}