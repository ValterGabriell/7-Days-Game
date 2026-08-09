using Godot;

public partial class PaginaFeedback : StaticBody3D
{
    [Export]
    private Label? _conteudo;

    public void Exibir(string conteudo)
    {
        if (_conteudo == null)
            return;

        _conteudo.Text = conteudo;
    }
}