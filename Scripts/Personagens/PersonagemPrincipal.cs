using Godot;
using System;

namespace Scripts.Personagens.Principal;

public partial class PersonagemPrincipal : CharacterBody3D
{
    public enum EstadoJogador { Normal, Sentado }
    public enum SubEstadoMesa { Nenhum, Computador, Radio, Telefone }

    public EstadoJogador EstadoAtual { get; private set; } = EstadoJogador.Normal;
    public SubEstadoMesa SubEstadoAtual { get; private set; } = SubEstadoMesa.Nenhum;

    public const float Speed = 5.0f;
    public const float JumpVelocity = 4.5f;

    public void AlternarEstado(EstadoJogador novoEstado)
    {
        GD.Print($"Alternando estado do jogador de {EstadoAtual} para {novoEstado}");
        EstadoAtual = novoEstado;

        if (EstadoAtual == EstadoJogador.Sentado)
        {
            Velocity = Vector3.Zero;
        }
    }

    public void DefinirSubEstadoMesa(SubEstadoMesa novoSubEstado)
    {
        SubEstadoAtual = novoSubEstado;
    }

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _Input(InputEvent @event)
    {
        if (EstadoAtual == EstadoJogador.Sentado) return;

        MoveCameraComMouse(@event);
        TentouInteragirComAlgoIterativo(@event);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (EstadoAtual == EstadoJogador.Sentado) return;

        Vector3 velocity = Velocity;
        MoveCameraComControle();

        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

        // A direção de movimento agora segue para onde a CÂMERA/PERSONAGEM está olhando
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}