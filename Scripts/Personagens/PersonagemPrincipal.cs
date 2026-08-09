using Godot;
using System;

namespace Scripts.Personagens.Principal;

// Personagem principal - Movimento do Corpo
public partial class PersonagemPrincipal : CharacterBody3D
{
    public enum EstadoJogador { Normal, Sentado, InteragindoAntena, InteragindoPinball }
    public enum SubEstadoMesa { Nenhum, Computador, Radio, Telefone }
    private const string ESCADA_NOME = "Escada";

    public EstadoJogador EstadoAtual { get; private set; } = EstadoJogador.Normal;
    public SubEstadoMesa SubEstadoAtual { get; private set; } = SubEstadoMesa.Nenhum;

    public const float Speed = 5.0f;
    public const float JumpVelocity = 4.5f;

    private float _tempoRestanteEscada = 0.0f;
    private const float TEMPO_COYOTE_ESCADA = 0.5f;

    public void AlternarEstado(EstadoJogador novoEstado)
    {
        Log.Print($"Alternando estado do jogador de {EstadoAtual} para {novoEstado}");
        EstadoAtual = novoEstado;

        if (JogadorEstaInteragindoComAlgumaCoisa())
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
        if (JogadorEstaInteragindoComAlgumaCoisa()) 
            return;

        MoveCameraComMouse(@event);
        TentouInteragirComAlgoIterativo(@event);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (JogadorEstaInteragindoComAlgumaCoisa())
            return;

        Vector3 velocity = Velocity;
        MoveCameraComControle();

        if (EstaNaEscada((float)delta))
        {
            ProcessaMovimentoQuandoForEscada(ref velocity);
        }
        else
        {
            if (!IsOnFloor())
            {
                velocity += GetGravity() * (float)delta;
            }

            if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
            {
                velocity.Y = JumpVelocity;
            }

            Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

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
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    private bool JogadorEstaInteragindoComAlgumaCoisa()
    {
        return EstadoAtual == EstadoJogador.Sentado || EstadoAtual == EstadoJogador.InteragindoAntena || EstadoAtual == EstadoJogador.InteragindoPinball;
    }

    private bool EstaNaEscada(float delta)
    {
        bool colidindoComEscada = RaycastDeIteracao != null
            && RaycastDeIteracao.IsColliding()
            && RaycastDeIteracao.GetCollider() is Node collider
            && collider.Name == ESCADA_NOME;

        if (colidindoComEscada)
        {
            _tempoRestanteEscada = TEMPO_COYOTE_ESCADA;
            return true;
        }

        if (_tempoRestanteEscada > 0.0f)
        {
            _tempoRestanteEscada -= delta;
            return true;
        }

        return false;
    }

    private void ProcessaMovimentoQuandoForEscada(ref Vector3 velocity)
    {
        float direcaoVertical = Input.GetActionStrength("ui_up") - Input.GetActionStrength("ui_down");

        velocity.Y = direcaoVertical * Speed;
        velocity.X = 0;
        velocity.Z = 0;
    }
}