using fiveyears3.Scripts.Utilidades;
using Godot;
using System;
using System.Collections.Generic;
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

    private EstadoCalendarioRotinasUiVisivel _estadoAtual = EstadoCalendarioRotinasUiVisivel.Oculto;
    private List<RotinaDiaModel> _todasAsRotinas = new();
    private const string CAMINHO_JSON = "res://Scripts/Dados/JSONS/rotinas_semanais.json";

    public override void _Ready()
    {
        base._Ready();
        CarregarRotinasDoJson();
    }

    public void Interagir()
    {
        if (_estadoAtual == EstadoCalendarioRotinasUiVisivel.Oculto)
        {
            ExibirRotinaDia1();
            CanvasLayer.Visible = true;
            _estadoAtual = EstadoCalendarioRotinasUiVisivel.Visivel;
        }
        else
        {
            CanvasLayer.Visible = false;
            _estadoAtual = EstadoCalendarioRotinasUiVisivel.Oculto;
        }
    }

    private void CarregarRotinasDoJson()
    {
        if (!FileAccess.FileExists(CAMINHO_JSON))
        {
            GD.PrintErr($"[CalendarioRotinas] Arquivo JSON não encontrado em: {CAMINHO_JSON}");
            return;
        }

        using var file = FileAccess.Open(CAMINHO_JSON, FileAccess.ModeFlags.Read);
        string jsonTexto = file.GetAsText();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter());

        try
        {
            _todasAsRotinas = JsonSerializer.Deserialize<List<RotinaDiaModel>>(jsonTexto, options);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[CalendarioRotinas] Erro ao desserializar JSON: {ex.Message}");
        }
    }

    private void ExibirRotinaDia1()
    {
        if (ListaRotinas == null) return;

        ListaRotinas.Clear();

        // Busca especificamente o Dia 1
        RotinaDiaModel rotinaDia1 = _todasAsRotinas.Find(r => r.Dia == 1);

        if (rotinaDia1 == null) return;

        foreach (var bloco in rotinaDia1.Programacao)
        {
            string tagObrigatorio = bloco.Obrigatorio ? "*" : "";
            string textoItem = $"{bloco.OrdemSugerida}. [{bloco.Tipo}] {bloco.DescricaoObrigacao} {tagObrigatorio}";

            ListaRotinas.AddItem(textoItem);
        }
    }
}