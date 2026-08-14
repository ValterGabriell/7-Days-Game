using Godot;
using System;
using System.Threading.Tasks;
using Scripts.SaveSystem;

public partial class RadioMenu : StaticBody3D
{
    [Export] private MeshInstance3D BotaoQueControlaORadio;
    [Export] private float TempoDeRotacao = 0.3f;
    [Export] private float AnguloIniciarJogo = -45.0f;
    [Export] private float AnguloContinuarJogo = 45.0f;
    [Export] private string EixoDeRotacao = "rotation:y";

    [ExportGroup("Ui")]
    [Export] private Label LabelIniciarJogo;
    [Export] private Label LabelContinuarJogo;
    [Export] private Control PainelDeLabels;

    [ExportSubgroup("Animação UI")]
    [Export] private float DistanciaDeslocamentoUI = 100.0f;
    [Export] private float TempoAnimacaoUI = 0.25f;

    [ExportGroup("Controles")]
    [Export] private string AcaoInteragir = "interagir";

    [ExportGroup("Save System")]
    [Export] private string SlotSaveVerificacao = "slot_1";

    [ExportGroup("Transição")]
    [Export] private float TempoFade = 0.8f;

    public enum EstadoBotao
    {
        IniciarJogo,
        ContinuarJogo
    }

    public EstadoBotao EstadoAtual { get; private set; } = EstadoBotao.IniciarJogo;

    private Tween tweenAtualBotao;
    private Tween tweenAtualUI;

    private Vector2 posOriginalIniciar;
    private Vector2 posOriginalContinuar;

    private bool temSaveDisponivel = false;
    private bool estaTrocandoDeCena = false;
    private bool menuAtivo = true;

    private CanvasLayer _fadeCanvas;
    private ColorRect _fadeRect;

    public event Action JogoPreparado;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        InputRayPickable = true;
        InputEvent += OnInputEvent;

        if (LabelIniciarJogo != null) posOriginalIniciar = LabelIniciarJogo.Position;
        if (LabelContinuarJogo != null) posOriginalContinuar = LabelContinuarJogo.Position;

        VerificarDisponibilidadeDeSave();

        AplicarRotacaoInicial();
        AtualizarUI(true);
    }

    private void VerificarDisponibilidadeDeSave()
    {
        string idSlot = !string.IsNullOrEmpty(SlotSaveVerificacao)
            ? SlotSaveVerificacao
            : (GerenciadorDeSave.Instance?.SaveIdPadrao ?? "slot_1");

        if (GerenciadorDeSave.Instance != null)
        {
            temSaveDisponivel = GerenciadorDeSave.Instance.ExisteSave(idSlot);
        }
        else
        {
            temSaveDisponivel = false;
        }

        if (!temSaveDisponivel)
        {
            EstadoAtual = EstadoBotao.IniciarJogo;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!menuAtivo || !Visible) return;
        if (estaTrocandoDeCena) return;

        if (@event.IsActionPressed("ui_left"))
        {
            MudarEstado(EstadoBotao.IniciarJogo);
        }
        else if (@event.IsActionPressed("ui_right"))
        {
            if (temSaveDisponivel)
            {
                MudarEstado(EstadoBotao.ContinuarJogo);
            }
            else
            {
                GD.Print("[RadioMenu] Tentativa de navegar para Continuar bloqueada: Nenhum save encontrado.");
            }
        }
        else if (@event.IsActionPressed(AcaoInteragir))
        {
            ConfirmarEscolha();
        }
    }

    private void OnInputEvent(Node camera, InputEvent @event, Vector3 position, Vector3 normal, long shapeIdx)
    {
        if (!menuAtivo || !Visible) return;
        if (estaTrocandoDeCena) return;

        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
        {
            ConfirmarEscolha();
        }
    }

    public void MudarEstado(EstadoBotao novoEstado)
    {
        if (estaTrocandoDeCena) return;
        if (novoEstado == EstadoBotao.ContinuarJogo && !temSaveDisponivel) return;
        if (EstadoAtual == novoEstado) return;

        EstadoAtual = novoEstado;

        float anguloAlvo = (EstadoAtual == EstadoBotao.IniciarJogo) ? AnguloIniciarJogo : AnguloContinuarJogo;
        GirarBotaoPara(anguloAlvo);
        AtualizarUI(false);
    }

    public void ConfirmarEscolha()
    {
        if (!menuAtivo || !Visible) return;
        if (estaTrocandoDeCena) return;

        if (EstadoAtual == EstadoBotao.IniciarJogo)
        {
            _ = ExecutarTransicaoEIniciarNovoJogo();
        }
        else if (EstadoAtual == EstadoBotao.ContinuarJogo && temSaveDisponivel)
        {
            _ = ExecutarTransicaoEContinuarJogo();
        }
    }

    public void DefinirMenuAtivo(bool ativo)
    {
        menuAtivo = ativo;
        Visible = ativo;

        if (!ativo)
        {
            return;
        }

        VerificarDisponibilidadeDeSave();
        AplicarRotacaoInicial();
        AtualizarUI(true);
    }

    #region Ações com Transição (Fade Out -> Preparar Save -> Retornar)

    private async Task ExecutarTransicaoEIniciarNovoJogo()
    {
        estaTrocandoDeCena = true;

        // 1. Executa o Fade Out (Escurece a tela)
        await FadeOutAsync();

        // 2. Prepara os dados de Novo Jogo
        GerenciadorDeSave.Instance?.NovoJogo(SlotSaveVerificacao);

        JogoPreparado?.Invoke();
        await FadeInAsync();

        estaTrocandoDeCena = false;
    }

    private async Task ExecutarTransicaoEContinuarJogo()
    {
        estaTrocandoDeCena = true;

        // 1. Executa o Fade Out (Escurece a tela)
        await FadeOutAsync();

        // 2. Carrega os dados do Save
        if (GerenciadorDeSave.Instance != null)
        {
            await GerenciadorDeSave.Instance.CarregarJogoAsync(SlotSaveVerificacao);
        }

        JogoPreparado?.Invoke();
        await FadeInAsync();

        estaTrocandoDeCena = false;
    }

    /// <summary>
    /// Cria dinamicamente um Overlay preto na frente da tela e faz o Fade Out (transparência 0 -> 1)
    /// </summary>
    private async Task FadeOutAsync()
    {
        if (_fadeCanvas == null)
        {
            _fadeCanvas = new CanvasLayer { Layer = 100 };
            _fadeRect = new ColorRect
            {
                Color = new Color(0, 0, 0, 0),
                MouseFilter = Control.MouseFilterEnum.Stop
            };

            _fadeRect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            _fadeCanvas.AddChild(_fadeRect);
            AddChild(_fadeCanvas);
        }

        // Tween para escurecer
        Tween tweenFade = CreateTween();
        tweenFade.TweenProperty(_fadeRect, "color:a", 1.0f, TempoFade);

        // Aguarda o término da animação do Tween
        await ToSignal(tweenFade, Tween.SignalName.Finished);
    }

    private async Task FadeInAsync()
    {
        if (_fadeRect == null) return;

        Tween tweenFade = CreateTween();
        tweenFade.TweenProperty(_fadeRect, "color:a", 0.0f, TempoFade);
        await ToSignal(tweenFade, Tween.SignalName.Finished);

        _fadeCanvas?.QueueFree();
        _fadeCanvas = null;
        _fadeRect = null;
    }

    #endregion

    private void GirarBotaoPara(float anguloGraus)
    {
        if (BotaoQueControlaORadio == null) return;

        if (tweenAtualBotao != null && tweenAtualBotao.IsValid())
        {
            tweenAtualBotao.Kill();
        }

        float anguloRadianos = Mathf.DegToRad(anguloGraus);

        tweenAtualBotao = CreateTween();
        tweenAtualBotao.SetEase(Tween.EaseType.Out);
        tweenAtualBotao.SetTrans(Tween.TransitionType.Back);

        tweenAtualBotao.TweenProperty(BotaoQueControlaORadio, EixoDeRotacao, anguloRadianos, TempoDeRotacao);
    }

    private void AtualizarUI(bool instantaneo)
    {
        if (LabelIniciarJogo == null || LabelContinuarJogo == null) return;

        if (tweenAtualUI != null && tweenAtualUI.IsValid())
        {
            tweenAtualUI.Kill();
        }

        float direcaoX = (EstadoAtual == EstadoBotao.IniciarJogo) ? -DistanciaDeslocamentoUI : DistanciaDeslocamentoUI;

        Label labelAtiva = (EstadoAtual == EstadoBotao.IniciarJogo) ? LabelIniciarJogo : LabelContinuarJogo;
        Label labelInativa = (EstadoAtual == EstadoBotao.IniciarJogo) ? LabelContinuarJogo : LabelIniciarJogo;

        Vector2 posOriginalAtiva = (EstadoAtual == EstadoBotao.IniciarJogo) ? posOriginalIniciar : posOriginalContinuar;

        if (instantaneo)
        {
            labelAtiva.Visible = true;
            labelAtiva.Modulate = new Color(1, 1, 1, 1);
            labelAtiva.Position = posOriginalAtiva;

            labelInativa.Visible = false;
            labelInativa.Modulate = new Color(1, 1, 1, 0);
            return;
        }

        labelAtiva.Visible = true;
        labelInativa.Visible = true;

        Vector2 posInicioAtiva = posOriginalAtiva + new Vector2(direcaoX, 0);
        labelAtiva.Position = posInicioAtiva;
        labelAtiva.Modulate = new Color(1, 1, 1, 0);

        tweenAtualUI = CreateTween().SetParallel(true);
        tweenAtualUI.SetEase(Tween.EaseType.Out);
        tweenAtualUI.SetTrans(Tween.TransitionType.Cubic);

        tweenAtualUI.TweenProperty(labelAtiva, "position", posOriginalAtiva, TempoAnimacaoUI);
        tweenAtualUI.TweenProperty(labelAtiva, "modulate:a", 1.0f, TempoAnimacaoUI);

        Vector2 posSaidaInativa = labelInativa.Position - new Vector2(direcaoX, 0);
        tweenAtualUI.TweenProperty(labelInativa, "position", posSaidaInativa, TempoAnimacaoUI);
        tweenAtualUI.TweenProperty(labelInativa, "modulate:a", 0.0f, TempoAnimacaoUI);

        tweenAtualUI.Chain().TweenCallback(Callable.From(() =>
        {
            labelInativa.Visible = false;
        }));
    }

    private void AplicarRotacaoInicial()
    {
        if (BotaoQueControlaORadio == null) return;

        float anguloInicial = (EstadoAtual == EstadoBotao.IniciarJogo) ? AnguloIniciarJogo : AnguloContinuarJogo;

        Vector3 rot = BotaoQueControlaORadio.Rotation;

        if (EixoDeRotacao.EndsWith("y")) rot.Y = Mathf.DegToRad(anguloInicial);
        else if (EixoDeRotacao.EndsWith("x")) rot.X = Mathf.DegToRad(anguloInicial);
        else if (EixoDeRotacao.EndsWith("z")) rot.Z = Mathf.DegToRad(anguloInicial);

        BotaoQueControlaORadio.Rotation = rot;
    }
}