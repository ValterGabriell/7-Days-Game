using fiveyears3.Scripts.Globais;
using fiveyears3.Scripts.Utilidades;
using Godot;
using Scripts.SaveSystem;
using System.Linq;

public partial class Impressora : StaticBody3D, IItemInteracao
{
    [Export]
    private PaginaFeedback? _paginaGoverno;

    [Export]
    private PaginaFeedback? _paginaResistencia;

    [Export]
    private Camera3D? _camera;

    [Export]
    private float _velocidadeMovimento = 0.5f;

    [Export]
    private float _velocidadeZoom = 0.5f;

    [Export]
    private float _zoomMinimo = 1.5f;

    [Export]
    private float _zoomMaximo = 5.0f;

    [Export]
    private float _distanciaInicial = 3.0f;

    private bool _modoLeitura;
    private float _distanciaCamera;

    public void Interagir()
    {
        Log.Print("Interagindo com a impressora");

        FocandoNaImpressora();
    }

    public void FocandoNaImpressora()
    {
        DiaConcluidoSave? diaAnterior = ObterDiaAnterior();

        if (diaAnterior == null)
            return;

        AtualizarPaginas(diaAnterior);
        AtivarModoLeitura();
    }

    public override void _Process(double delta)
    {
        if (!_modoLeitura || _camera == null)
            return;

        ProcessarMovimentoCamera((float)delta);
        ProcessarZoom((float)delta);
    }

    private DiaConcluidoSave? ObterDiaAnterior()
    {
        DadosSave? save = GerenciadorDeSave.Instance?.SaveAtual;

        if (save == null)
            return null;

        int diaAnterior =
            GerenciadorPassagemDoTempo.Instance.DiaAtual - 1;

        if (diaAnterior < 1)
            return null;

        return save.HistoricoDiasConcluidos
            .FirstOrDefault(dia => dia.Dia == diaAnterior);
    }

    private void AtualizarPaginas(DiaConcluidoSave dia)
    {
        _paginaGoverno?.Exibir(ObterConteudoGoverno(dia));
        _paginaResistencia?.Exibir(ObterConteudoResistencia(dia));
    }

    private string ObterConteudoGoverno(DiaConcluidoSave dia)
    {
        return string.Join(
            "\n\n",
            dia.NoticiasEscolhas
                .Select(noticia =>
                    noticia.ImpressoresGeradasNoDiaSeguinte?
                        .Governo?
                        .Falas)
                .Where(fala => !string.IsNullOrWhiteSpace(fala))
        );
    }

    private string ObterConteudoResistencia(DiaConcluidoSave dia)
    {
        return string.Join(
            "\n\n",
            dia.NoticiasEscolhas
                .Select(noticia =>
                    noticia.ImpressoresGeradasNoDiaSeguinte?
                        .Resistencia?
                        .Falas)
                .Where(fala => !string.IsNullOrWhiteSpace(fala))
        );
    }

    private void AtivarModoLeitura()
    {
        if (_camera == null)
            return;

        _modoLeitura = true;
        _distanciaCamera = _distanciaInicial;

        _camera.Current = true;
    }

    private void ProcessarMovimentoCamera(float delta)
    {
        Vector3 movimento = Vector3.Zero;

        if (Input.IsActionPressed("camera_left"))
            movimento.X -= 1.0f;

        if (Input.IsActionPressed("camera_right"))
            movimento.X += 1.0f;

        if (Input.IsActionPressed("camera_up"))
            movimento.Y += 1.0f;

        if (Input.IsActionPressed("camera_down"))
            movimento.Y -= 1.0f;

        if (movimento == Vector3.Zero)
            return;

        Vector3 deslocamento =
            _camera!.GlobalBasis.X * movimento.X +
            _camera.GlobalBasis.Y * movimento.Y;

        _camera.GlobalPosition +=
            deslocamento * _velocidadeMovimento * delta;
    }

    private void ProcessarZoom(float delta)
    {
        float zoom = 0.0f;

        if (Input.IsActionPressed("camera_zoom_in"))
            zoom -= 1.0f;

        if (Input.IsActionPressed("camera_zoom_out"))
            zoom += 1.0f;

        if (Mathf.IsZeroApprox(zoom))
            return;

        _distanciaCamera = Mathf.Clamp(
            _distanciaCamera +
            zoom * _velocidadeZoom * delta,
            _zoomMinimo,
            _zoomMaximo
        );

        Vector3 frente = -_camera!.GlobalBasis.Z;

        _camera.GlobalPosition +=
            frente * zoom * _velocidadeZoom * delta;
    }
}