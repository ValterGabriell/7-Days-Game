using fiveyears3.Scripts.Utilidades;
using Godot;
using System;
using Scripts.Personagens.Principal;

public partial class Antena : StaticBody3D, IItemInteracao
{
    [Export] public Camera3D CameraAntena;
    [Export] public float VelocidadeDeRotacao = 2.0f;

    [Export] public PersonagemPrincipal _jogadorEmUso;
    private bool _estaInteragindo = false;

    public void Interagir()
    {
        if (_jogadorEmUso == null) return;

        _estaInteragindo = true;

        _jogadorEmUso.AlternarEstado(PersonagemPrincipal.EstadoJogador.InteragindoAntena);

        if (CameraAntena != null)
        {
            CameraAntena.MakeCurrent();
        }
        this._jogadorEmUso.Visible = false;
    }

    public override void _Process(double delta)
    {
        if (!_estaInteragindo) return;

        float giro = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
        if (giro != 0.0f)
        {
            RotateY(-giro * VelocidadeDeRotacao * (float)delta);
        }

        if (Input.IsActionJustPressed("ui_cancel"))
        {
            SairDaInteracao();
        }
    }

    private void SairDaInteracao()
    {
        _estaInteragindo = false;

        if (_jogadorEmUso != null)
        {
            if (_jogadorEmUso.CameraJogador != null)
            {
                _jogadorEmUso.CameraJogador.MakeCurrent();
            }

            _jogadorEmUso.AlternarEstado(PersonagemPrincipal.EstadoJogador.Normal);
            this._jogadorEmUso.Visible = true;
        }
    }
}