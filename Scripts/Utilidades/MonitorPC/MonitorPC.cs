using fiveyears3.Scripts.Utilidades;
using Godot;
using Scripts.Personagens.Principal;

public partial class MonitorPC : StaticBody3D, IItemInteracao
{
    [Export] public SubViewport SubViewportPC;
    [Export] public MeshInstance3D TelaMesh;
    [Export] public Camera3D CameraDoPc;
    [Export] private PersonagemPrincipal Jogador;

    [ExportGroup("Configurações do Controle")]
    [Export] public float VelocidadePonteiroControle = 800.0f;

    private StandardMaterial3D _materialTela;
    private Camera3D _cameraJogador;

    public override void _Ready()
    {
        _materialTela = new StandardMaterial3D
        {
            AlbedoTexture = SubViewportPC.GetTexture(),
            EmissionTexture = SubViewportPC.GetTexture()
        };

        TelaMesh.MaterialOverride = _materialTela;

        if (CameraDoPc != null)
            CameraDoPc.Current = false;

        Desligar();
    }

    public override void _Process(double delta)
    {
        if (!CameraDoPc.Current) return;

        MoverPonteiroComAnalogicoEsquerdo((float)delta);
    }

    public override void _Input(InputEvent @event)
    {
        if (!CameraDoPc.Current) return;

        // Pressionar o botão "interagir" ou "ui_accept" simula o clique do mouse no PC
        if (@event.IsAction("interagir") || @event.IsAction("ui_accept"))
        {
            SimularCliqueMouse(@event.IsPressed());
            GetViewport().SetInputAsHandled();
            return;
        }

        switch (@event)
        {
            case InputEventKey key when key.IsActionPressed("ui_cancel"):
            case InputEventJoypadButton joyBtn when joyBtn.IsActionPressed("ui_cancel"):
                SairDoComputador();
                GetViewport().SetInputAsHandled();
                break;

            case InputEventMouse:
                ProjetarInputNaUI(@event);
                break;
        }
    }

    public void Interagir()
    {
        if (CameraDoPc.Current) return;
        EntrarNoComputador();
    }

    private void MoverPonteiroComAnalogicoEsquerdo(float delta)
    {
        // Lendo do analógico esquerdo (direções de movimento padrão)
        Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        if (inputDir == Vector2.Zero) return;

        Viewport viewport = GetViewport();
        Vector2 mousePos = viewport.GetMousePosition();
        Vector2 novaPos = mousePos + (inputDir * VelocidadePonteiroControle * delta);

        Vector2 screenSize = viewport.GetVisibleRect().Size;
        novaPos = novaPos.Clamp(Vector2.Zero, screenSize);

        viewport.WarpMouse(novaPos);

        InputEventMouseMotion motionEvent = new()
        {
            Position = novaPos,
            GlobalPosition = novaPos,
            Relative = inputDir * VelocidadePonteiroControle * delta
        };

        ProjetarInputNaUI(motionEvent);
    }

    private void SimularCliqueMouse(bool pressionado)
    {
        Vector2 mousePos = GetViewport().GetMousePosition();

        InputEventMouseButton clickEvent = new()
        {
            ButtonIndex = MouseButton.Left,
            Pressed = pressionado,
            Position = mousePos,
            GlobalPosition = mousePos
        };

        ProjetarInputNaUI(clickEvent);
    }

    private void EntrarNoComputador()
    {
        _cameraJogador = GetViewport().GetCamera3D();
        GD.Print(Jogador);
        Jogador?.AlternarEstado(PersonagemPrincipal.EstadoJogador.NoComputador);

        SubViewportPC.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
        _materialTela.EmissionEnabled = true;
        _materialTela.AlbedoColor = Colors.White;

        CameraDoPc.Current = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        AlterarProcessamentoJogador(false);
    }

    private void SairDoComputador()
    {
        GD.Print("saindo do pc");
        if (_cameraJogador != null)
            _cameraJogador.Current = true;

        Jogador?.AlternarEstado(PersonagemPrincipal.EstadoJogador.Normal);
        Input.MouseMode = Input.MouseModeEnum.Captured;
        AlterarProcessamentoJogador(true);
    }

    private void Desligar()
    {
        SairDoComputador();

        SubViewportPC.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
        _materialTela.EmissionEnabled = false;
        _materialTela.AlbedoColor = Colors.Black;
    }

    private void ProjetarInputNaUI(InputEvent @event)
    {
        if (@event is not InputEventMouse mouseEvent) return;

        Vector2 mousePos = mouseEvent.Position;
        Vector3 rayFrom = CameraDoPc.ProjectRayOrigin(mousePos);
        Vector3 rayDir = CameraDoPc.ProjectRayNormal(mousePos);

        Transform3D gt = TelaMesh.GlobalTransform;
        Plane plane = new(gt.Basis.Z, gt.Origin);

        Vector3? intersection = plane.IntersectsRay(rayFrom, rayDir);
        if (intersection is null) return;

        Vector3 localPoint = gt.AffineInverse() * intersection.Value;
        Vector2 uv = new(localPoint.X + 0.5f, 1.0f - (localPoint.Y + 0.5f));

        if (uv.X is < 0.0f or > 1.0f || uv.Y is < 0.0f or > 1.0f) return;

        Vector2 viewportPos = uv * SubViewportPC.Size;

        InputEvent dupEvent = (InputEvent)@event.Duplicate();

        if (dupEvent is InputEventMouse dupMouse)
        {
            dupMouse.Position = viewportPos;
            dupMouse.GlobalPosition = viewportPos;
        }

        SubViewportPC.PushInput(dupEvent);
    }

    private void AlterarProcessamentoJogador(bool ativo)
    {
        Jogador?.SetPhysicsProcess(ativo);
        Jogador?.SetProcess(ativo);
    }
}