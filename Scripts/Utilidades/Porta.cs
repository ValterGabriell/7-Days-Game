using fiveyears3.Scripts.Utilidades;
using Godot;

public partial class Porta : StaticBody3D, IItemInteracao
{
    [ExportGroup("Porta")]
    [Export] private MeshInstance3D VisorPorta;
    [Export] public AudioStreamPlayer3D AudioBatendoPorta;

    [ExportGroup("Carta")]
    [Export] private PackedScene CenaCartaPorta;
    [Export] private Node3D PontoSpawnCarta;

    private CartaPorta _cartaAtual;
    private DadosCarta _dadosCarta;

    private bool _jogadorInteragiuComAPorta = false;
    private bool _dialogoFinalizado = false;
    private bool _cartaSpawnada = false;

    public override void _Ready()
    {
        AudioBatendoPorta.Finished += OnAudioBatendoPortaFinished;
    }

    public void Interagir()
    {
        if (_jogadorInteragiuComAPorta)
            return;

        _jogadorInteragiuComAPorta = true;

        VisorPorta.Visible = !VisorPorta.Visible;

        // Aqui você inicia o diálogo da porta.
        // Quando o diálogo terminar, chame:
        //
         FinalizarDialogoPorta();
    }

    private void OnAudioBatendoPortaFinished()
    {
        if (!_jogadorInteragiuComAPorta)
        {
            AudioBatendoPorta.Play();
        }
    }

    /// <summary>
    /// Deve ser chamado pelo sistema de diálogo
    /// quando o diálogo da porta terminar.
    /// </summary>
    public void FinalizarDialogoPorta()
    {
        if (_dialogoFinalizado)
            return;

        _dialogoFinalizado = true;

        SpawnarCarta();
    }

    /// <summary>
    /// Define qual carta será entregue quando
    /// o diálogo da porta terminar.
    /// </summary>
    public void PrepararCarta(DadosCarta dados)
    {
        _dadosCarta = dados;
    }

    private void SpawnarCarta()
    {
        if (_cartaSpawnada)
            return;

        if (_dadosCarta == null)
        {
            GD.PrintErr("[Porta] Nenhum DadosCarta foi configurado.");
            return;
        }

        if (CenaCartaPorta == null)
        {
            GD.PrintErr("[Porta] CenaCartaPorta não configurada.");
            return;
        }

        if (PontoSpawnCarta == null)
        {
            GD.PrintErr("[Porta] PontoSpawnCarta não configurado.");
            return;
        }

        CartaPorta carta = CenaCartaPorta.Instantiate<CartaPorta>();

        GetTree().CurrentScene.AddChild(carta);

        carta.GlobalTransform = PontoSpawnCarta.GlobalTransform;

        carta.ConfigurarCarta(_dadosCarta);

        _cartaAtual = carta;
        _cartaSpawnada = true;

        GD.Print("[Porta] Carta spawnada após término do diálogo.");
    }
}