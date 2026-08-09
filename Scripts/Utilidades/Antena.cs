using fiveyears3.Scripts.Globais;
using fiveyears3.Scripts.Utilidades;
using Godot;
using System;
using Scripts.Personagens.Principal;

public partial class Antena : StaticBody3D, IItemInteracao
{
    [Export] public Camera3D CameraAntena;
    [Export] public float VelocidadeDeRotacao = 2.0f;
    [Export] public PersonagemPrincipal _jogadorEmUso;
    [Export] public AudioStreamPlayer AudioAntenaQuebrando;
    [Export] public AudioStreamPlayer AudioAntenaConcertando;

    [ExportCategory("Sintonia da Antena")]
    [Export] public float ToleranciaSintoniaGraus = 3.0f; // Margem de erro aceitável (ex: +- 3 graus)

    private bool _estaInteragindo = false;
    private bool _estaQuebrada = false;

    private float _anguloAlvoRad = 0.0f; // Ângulo correto da sintonia perfeita

    public void Interagir()
    {
        if (_jogadorEmUso == null) return;

        _estaInteragindo = true;
        _jogadorEmUso.AlternarEstado(PersonagemPrincipal.EstadoJogador.InteragindoAntena);

        if (CameraAntena != null)
        {
            CameraAntena.MakeCurrent();
        }

        _jogadorEmUso.Visible = false;
    }

    public override void _Process(double delta)
    {
        if (!_estaInteragindo) return;

        // Giro manual da antena
        float giro = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
        if (giro != 0.0f)
        {
            RotateY(-giro * VelocidadeDeRotacao * (float)delta);

            // Se estiver quebrada, verifica se o jogador encontrou o ponto de sintonia
            if (_estaQuebrada)
            {
                VerificarSintonia();
            }
        }

        if (Input.IsActionJustPressed("ui_cancel"))
        {
            SairDaInteracao();
        }
    }

    public void QuebrarAntena()
    {
        _estaQuebrada = true;
        AudioAntenaQuebrando.Play();

        // Salva a rotação Y correta antes de desalinhar
        _anguloAlvoRad = Rotation.Y;

        // Desalinha a antena aleatoriamente entre 60 e 180 graus (ou um valor fixo de desalinhamento)
        float desalinhamento = (float)GD.RandRange(Mathf.DegToRad(60.0f), Mathf.DegToRad(180.0f));
        RotateY(desalinhamento);

        Log.Print($"[Antena] A antena foi quebrada e desalinhada em {Mathf.RadToDeg(desalinhamento):F1}°!");
    }

    private void VerificarSintonia()
    {
        // Normaliza a diferença de ângulo para a menor distância entre -PI e PI
        float diferencaAngulo = Mathf.Abs(Mathf.AngleDifference(Rotation.Y, _anguloAlvoRad));
        float toleranciaRad = Mathf.DegToRad(ToleranciaSintoniaGraus);

        if (diferencaAngulo <= toleranciaRad)
        {
            ConsertarAntena();
        }
    }

    public void ConsertarAntena()
    {
        if (!_estaQuebrada) return;

        AudioAntenaConcertando.Play();
        _estaQuebrada = false;

        // Trava no ângulo perfeito exato para não ficar levemente torta
        Vector3 rot = Rotation;
        rot.Y = _anguloAlvoRad;
        Rotation = rot;

        Log.Print("[Antena] Sintonia perfeita encontrada! Antena consertada.");

        if (GerenciadorDeEventoAleatorio.Instance != null)
        {
            GerenciadorDeEventoAleatorio.Instance.ConcluirEventoAtual();
        }

        SairDaInteracao();
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
            _jogadorEmUso.Visible = true;
        }
    }
}