using Godot;
using System;
using fiveyears3.Scripts.Utilidades;
using Scripts.Personagens.Principal;

public partial class Pinball : StaticBody3D, IItemInteracao
{
    [ExportCategory("Referências do Pinball")]
    [Export] public Node3D FlipperEsquerdo { get; set; }
    [Export] public Node3D FlipperDireito { get; set; }
    [Export] public RigidBody3D Bola { get; set; }
    [Export] public Camera3D CameraPinball { get; set; }
    [Export] public Marker3D PontoDeSpawDaBola { get; set; }

    [ExportCategory("Configurações Física/Mecânicas")]
    [Export] public float VelocidadeInicialBola = 10.0f; // Usar velocidade direta em vez de impulso
    [Export] public float AnguloRotacaoFlipper = 45.0f;
    [Export] public float VelocidadeRotacaoFlipper = 20.0f;
    [Export] public Vector3 DirecaoImpulso = new Vector3(0, 0, -1);

    [Export] public PersonagemPrincipal Jogador { get; set; }

    private bool _emJogo = false;
    private bool _solicitarDisparoBola = false;
    private bool _monitorarBola = false;

    private float _rotacaoOriginalEsqY;
    private float _rotacaoOriginalDirY;

    public override void _Ready()
    {
        if (CameraPinball != null)
        {
            CameraPinball.Current = false;
        }

        if (FlipperEsquerdo != null) _rotacaoOriginalEsqY = FlipperEsquerdo.RotationDegrees.Y;
        if (FlipperDireito != null) _rotacaoOriginalDirY = FlipperDireito.RotationDegrees.Y;

        // Garante que a bola comece em estado estático se existir
        PrepararBolaNoSpawn();
    }

    public void Interagir()
    {
        if (_emJogo) return;

        if (Jogador != null)
        {
            EntrarNoPinball();
        }
    }

    private void EntrarNoPinball()
    {
        _emJogo = true;
        Jogador.AlternarEstado(PersonagemPrincipal.EstadoJogador.InteragindoPinball);

        if (CameraPinball != null)
        {
            CameraPinball.MakeCurrent();
        }

        PrepararBolaNoSpawn();
        Log.Print("[Pinball] Jogador entrou no minigame do Pinball.");
    }

    private void SairDoPinball()
    {
        _emJogo = false;
        _monitorarBola = false;

        if (Jogador != null && Jogador.CameraJogador != null)
        {
            Jogador.CameraJogador.MakeCurrent();
            Jogador.AlternarEstado(PersonagemPrincipal.EstadoJogador.Normal);
        }

        Log.Print("[Pinball] Jogador saiu do minigame do Pinball.");
    }

    public override void _Process(double delta)
    {
        if (!_emJogo) return;

        if (Input.IsActionJustPressed("ui_cancel"))
        {
            SairDoPinball();
            return;
        }

        ProcessarFlippers((float)delta);

        if (Input.IsActionJustPressed("ui_accept"))
        {
            _solicitarDisparoBola = true;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_emJogo) return;

        if (_solicitarDisparoBola)
        {
            _solicitarDisparoBola = false;
            ExecutarDisparoDaBola();
        }

        // Rastreio para descobrir para onde a bola está indo quando "sumir"
        if (_monitorarBola && Bola != null)
        {
            Log.Print($"[Debug Bola] Posição Global: {Bola.GlobalPosition} | Velocidade: {Bola.LinearVelocity}");

            // Se cair demais no eixo Y (caindo da mesa), reseta ela
            if (Bola.GlobalPosition.Y < -10.0f)
            {
                Log.PrintErr("[Pinball Error] A bola caiu do mapa/mesa! Verifique as colisões do chão do Pinball.");
                _monitorarBola = false;
                PrepararBolaNoSpawn();
            }
        }
    }

    private void PrepararBolaNoSpawn()
    {
        if (Bola == null || PontoDeSpawDaBola == null) return;

        // 1. Congela a bola
        Bola.Freeze = true;
        Bola.FreezeMode = RigidBody3D.FreezeModeEnum.Kinematic;

        // 2. Iguala a Transform Global (garante a posição E rotação exatas do Marker)
        Transform3D t = Bola.GlobalTransform;
        t.Origin = PontoDeSpawDaBola.GlobalPosition;
        Bola.GlobalTransform = t;

        // 3. Reseta forças
        Bola.LinearVelocity = Vector3.Zero;
        Bola.AngularVelocity = Vector3.Zero;
    }

    private void ExecutarDisparoDaBola()
    {
        if (Bola == null || PontoDeSpawDaBola == null) return;

        // 1. Garante o alinhamento correto no momento do disparo
        Transform3D t = Bola.GlobalTransform;
        t.Origin = PontoDeSpawDaBola.GlobalPosition;
        Bola.GlobalTransform = t;

        // 2. Ignora a colisão com o Pinball pai para a física não repelir
        Bola.AddCollisionExceptionWith(this);

        // 3. Descongela
        Bola.Freeze = false;
        Bola.Sleeping = false;

        // 4. Aplica a velocidade de disparo
        Vector3 vetorVelocidade = DirecaoImpulso.Normalized() * VelocidadeInicialBola;
        Bola.LinearVelocity = vetorVelocidade;

        _monitorarBola = true;
        Log.Print($"[Pinball] Bola disparada com velocidade: {vetorVelocidade}");
    }

    private void ProcessarFlippers(float delta)
    {
        if (FlipperEsquerdo != null)
        {
            bool pressionouEsq = Input.IsActionPressed("interagir_flipper_01");
            float alvoEsq = _rotacaoOriginalEsqY + (pressionouEsq ? AnguloRotacaoFlipper : 0.0f);

            Vector3 rot = FlipperEsquerdo.RotationDegrees;
            rot.Y = Mathf.MoveToward(rot.Y, alvoEsq, VelocidadeRotacaoFlipper * delta * 100.0f);
            FlipperEsquerdo.RotationDegrees = rot;
        }

        if (FlipperDireito != null)
        {
            bool pressionouDir = Input.IsActionPressed("interagir_flipper_02");
            float alvoDir = _rotacaoOriginalDirY - (pressionouDir ? AnguloRotacaoFlipper : 0.0f);

            Vector3 rot = FlipperDireito.RotationDegrees;
            rot.Y = Mathf.MoveToward(rot.Y, alvoDir, VelocidadeRotacaoFlipper * delta * 100.0f);
            FlipperDireito.RotationDegrees = rot;
        }
    }

    
}