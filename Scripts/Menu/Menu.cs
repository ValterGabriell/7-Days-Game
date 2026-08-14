using Godot;
using System;

public partial class Menu : Node3D
{
    public static bool JogoIniciado { get; private set; }
    public static event Action AoIniciarJogo;

    [Export] private Camera3D CameraMenu;

    
    [Export] private float DuracaoDoMovimento = 3.0f;

    
    [Export] private Tween.TransitionType TipoTransicao = Tween.TransitionType.Quad;
    [Export] private Tween.EaseType TipoSuavizacao = Tween.EaseType.InOut;

    private RadioMenu _radioMenu;
    private Camera3D _cameraAntesDoMenu;
    private bool _menuAberto;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        JogoIniciado = false;

        _radioMenu = GetNodeOrNull<RadioMenu>("RadioMenu");
        if (_radioMenu != null)
        {
            _radioMenu.JogoPreparado += OnJogoPreparado;
        }

        AbrirMenu();
    }

    public override void _ExitTree()
    {
        if (_radioMenu != null)
        {
            _radioMenu.JogoPreparado -= OnJogoPreparado;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && keyEvent.Keycode == Key.P)
        {
            if (!JogoIniciado)
            {
                GetViewport().SetInputAsHandled();
                return;
            }

            if (_menuAberto)
            {
                FecharMenu();
            }
            else
            {
                AbrirMenu();
            }

            GetViewport().SetInputAsHandled();
        }
    }

    private void OnJogoPreparado()
    {
        if (!JogoIniciado)
        {
            JogoIniciado = true;
            AoIniciarJogo?.Invoke();
        }

        FecharMenu();
    }

    private void AbrirMenu()
    {
        if (CameraMenu == null)
        {
            GD.PrintErr("Menu: CameraMenu não foi atribuída no Inspector.");
            return;
        }

        Camera3D cameraAtual = GetViewport().GetCamera3D();
        if (cameraAtual != null && cameraAtual != CameraMenu)
        {
            _cameraAntesDoMenu = cameraAtual;
        }

        Visible = true;
        _menuAberto = true;

        CameraMenu.MakeCurrent();


        if (_radioMenu != null)
        {
            _radioMenu.DefinirMenuAtivo(true);
        }

        Input.MouseMode = Input.MouseModeEnum.Visible;
        GetTree().Paused = true;
    }

    private void FecharMenu()
    {
        if (!_menuAberto) return;

        if (_radioMenu != null)
        {
            _radioMenu.DefinirMenuAtivo(false);
        }

        if (GodotObject.IsInstanceValid(_cameraAntesDoMenu))
        {
            _cameraAntesDoMenu.MakeCurrent();
        }

        _menuAberto = false;
        Visible = false;

        Input.MouseMode = Input.MouseModeEnum.Captured;
        GetTree().Paused = false;
    }

    
}