using Godot;
using fiveyears3.Scripts.Utilidades;

public class DadosCarta
{
    public string Titulo;
    public string Texto;
}

public partial class CartaPorta : StaticBody3D, IItemInteracao
{
    [ExportGroup("UI da Carta")]
    [Export] private RichTextLabel TextoCarta;
    [Export] private CanvasLayer CanvasCarta;
    [Export] private Button CartaLidaBtn;
    [Export] private AudioStreamPlayer3D AudioCarta;

    private bool _jogadorInteragiuComACarta = false;

    public void ConfigurarCarta(DadosCarta dados)
    {
        if (TextoCarta == null)
            return;

        TextoCarta.Text =
            $"[center][b]{dados.Titulo}[/b][/center]\n\n" +
            dados.Texto;
    }

    public void Interagir()
    {
        if (_jogadorInteragiuComACarta)
            return;

        _jogadorInteragiuComACarta = true;
        CanvasCarta.Visible = true;
        CartaLidaBtn.Pressed += () =>
        {
            CanvasCarta.Visible = false;
            QueueFree();
        };
    }
}