using fiveyears3.Scripts.Globais;
using fiveyears3.Scripts.Utilidades;
using Godot;
using Scripts.Personagens.Principal;

public partial class MonitorPC : StaticBody3D, IItemInteracao
{
    [ExportGroup("Componentes do Monitor")]
    [Export] public SubViewport SubViewportPC;
    [Export] public MeshInstance3D TelaMesh;
    [Export] public Camera3D CameraPC; 
    [Export] private PersonagemPrincipal Jogador;

    [ExportGroup("Configurações do Controle")]
    [Export] public float VelocidadePonteiroControle = 800.0f;

    private StandardMaterial3D _materialTela;
    private bool _focadoNoPC = false;

    public override void _Ready()
    {
        _materialTela = new StandardMaterial3D
        {
            AlbedoTexture = SubViewportPC.GetTexture(),
            EmissionTexture = SubViewportPC.GetTexture()
        };

        TelaMesh.MaterialOverride = _materialTela;
        LigarTela();
    }

    public override void _Process(double delta)
    {
        if (!PodeInteragirComPC()) return;

        MoverPonteiroComAnalogicoEsquerdo((float)delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (GerenciadorMesa.Instance != null) return;

        
        if (EstaNaVisaoGeralDoPC() && @event.IsActionPressed("interagir"))
        {
            AtivarFocoPC();
            GetViewport().SetInputAsHandled();
            return;
        }

        
        if (_focadoNoPC && @event.IsActionPressed("ui_cancel"))
        {
            DesativarFocoPC();
            GetViewport().SetInputAsHandled();
            return;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!PodeInteragirComPC()) return;

        if (@event.IsAction("interagir"))
        {
            SimularCliqueMouse(@event.IsPressed());
            GetViewport().SetInputAsHandled();
            return;
        }

        switch (@event)
        {
            case InputEventMouse:
                ProjetarInputNaUI(@event);
                break;
        }
    }

    public void Interagir()
    {
        if (EstaNaVisaoGeralDoPC() && !_focadoNoPC)
        {
            AtivarFocoPC();
        }
    }

    public void AtivarFocoPC()
    {
        _focadoNoPC = true;
        if (CameraPC != null) CameraPC.Current = true;
        GD.Print("Foco aproximado no PC ativado.");
    }

    public void DesativarFocoPC()
    {
        _focadoNoPC = false;
        
        if (GerenciadorMesa.Instance?.CameraMesa != null)
        {
            GerenciadorMesa.Instance.CameraMesa.Current = true;
        }
        GD.Print("Retornou para a câmera geral da mesa.");
    }

    private bool EstaNaVisaoGeralDoPC()
    {
        return Jogador != null
            && Jogador.EstadoAtual == PersonagemPrincipal.EstadoJogador.Sentado
            && GerenciadorMesa.Instance?.EquipamentoAtual == GerenciadorMesa.EquipamentoMesa.Computador;
    }

    private bool EstaNoFocoDoPcPelaMesa()
    {
        return Jogador != null
            && Jogador.EstadoAtual == PersonagemPrincipal.EstadoJogador.Sentado
            && GerenciadorMesa.Instance?.EstaFocadoNoEquipamento == true
            && GerenciadorMesa.Instance.EquipamentoAtual == GerenciadorMesa.EquipamentoMesa.Computador;
    }

    private bool PodeInteragirComPC()
    {
        return EstaNoFocoDoPcPelaMesa() || (EstaNaVisaoGeralDoPC() && _focadoNoPC);
    }

    private void LigarTela()
    {
        SubViewportPC.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
        _materialTela.EmissionEnabled = true;
        _materialTela.AlbedoColor = Colors.White;
        GerenciadorDeNoticias.Instance?.CarregarNoticiasDoDia();
    }

    private void MoverPonteiroComAnalogicoEsquerdo(float delta)
    {
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

    private void ProjetarInputNaUI(InputEvent @event)
    {
        if (@event is not InputEventMouse mouseEvent) return;

        
        Camera3D cameraAtual = GetViewport().GetCamera3D();
        if (cameraAtual == null) return;

        Vector2 mousePos = mouseEvent.Position;
        Vector3 rayFrom = cameraAtual.ProjectRayOrigin(mousePos);
        Vector3 rayDir = cameraAtual.ProjectRayNormal(mousePos);

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
}