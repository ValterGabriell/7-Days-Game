using Godot;
using System;

public partial class UIConfiguracaoDeTela : Control
{
    [Export] private OptionButton _dropdownResolucao;
    [Export] private CheckBox _checkboxTelaCheia;

    public override void _Ready()
    {
        if (GerenciadorDeTela.Instance == null) return;

        _dropdownResolucao.Clear();
        foreach (var res in GerenciadorDeTela.Instance.Resolucoes)
        {
            _dropdownResolucao.AddItem($"{res.X} x {res.Y}");
        }

        _dropdownResolucao.ItemSelected += OnResolucaoSelecionada;
        _checkboxTelaCheia.Toggled += OnTelaCheiaAlternada;
    }

    private void OnResolucaoSelecionada(long indice)
    {
        GerenciadorDeTela.Instance.MudarResolucao((int)indice);
    }

    private void OnTelaCheiaAlternada(bool ativo)
    {
        GerenciadorDeTela.Instance.DefinirTelaCheia(ativo);
    }
}
