using Godot;
using System.Collections.Generic;

public partial class GerenciadorDeTela : Node
{
    public static GerenciadorDeTela Instance { get; private set; }

    public readonly List<Vector2I> Resolucoes = new List<Vector2I>
    {
        new Vector2I(1920, 1080),
        new Vector2I(1600, 900),
        new Vector2I(1280, 720)
    };

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        Instance = this;


        MudarResolucao(2);
    }
    public void MudarResolucao(int indice)
    {
        if (indice >= 0 && indice < Resolucoes.Count)
        {
            Vector2I novoTamanho = Resolucoes[indice];

            DisplayServer.WindowSetSize(novoTamanho);

            Vector2I tamanhoTela = DisplayServer.ScreenGetSize();
            Vector2I posicaoCentral = (tamanhoTela - novoTamanho) / 2;
            DisplayServer.WindowSetPosition(posicaoCentral);
        }
    }

    public void DefinirTelaCheia(bool estaEmTelaCheia)
    {
        if (estaEmTelaCheia)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
        }
        else
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        }
    }
}