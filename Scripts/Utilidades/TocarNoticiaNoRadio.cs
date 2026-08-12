using fiveyears3.Scripts.Globais;
using Flags;
using Godot;
using Godot.Collections;
using System;

public partial class TocarNoticiaNoRadio : Node
{
    private enum EstadoDaTransmissao { Nenhuma, TransmitindoNoticia, TransmitindoMusica }
    [Export] public AudioStreamPlayer3D PlayerAudio;
    [Export] public CanvasLayer CanvasLayerRadio;
    [Export] public Label LabelRadio;

    private NoticiaModel _noticiaAtual;
    private MusicaModel _musicaAtual;
    private EstadoDaTransmissao _estadoAtualDaTransmissao = EstadoDaTransmissao.Nenhuma;
    private Dictionary<string, bool> _noticiasJaTransmitidas = new Dictionary<string, bool>();
    private const double TEMPO_DE_SILENCIO_PARA_PERDA_DE_AUDIENCIA = 5.0;
    private double tempoEmSilencio = 0.0;
    private double proximoIntervaloDePunicao = TEMPO_DE_SILENCIO_PARA_PERDA_DE_AUDIENCIA;

    private readonly Dictionary<int, FlagsCondicionais> _flagsAoIniciar = new()
{
    { 0, FlagsCondicionais.PRIMEIRA_MUSICA_DISPARADA_RADIO },
    { 1, FlagsCondicionais.SEGUNDA_MUSICA_DISPARADA_RADIO },
    { 2, FlagsCondicionais.TERCEIRA_MUSICA_DISPARADA_RADIO },
    { 3, FlagsCondicionais.QUARTA_MUSICA_DISPARADA_RADIO },
    { 4, FlagsCondicionais.QUINTA_MUSICA_DISPARADA_RADIO },
    { 5, FlagsCondicionais.SEXTA_MUSICA_DISPARADA_RADIO },
    { 6, FlagsCondicionais.SETIMA_MUSICA_DISPARADA_RADIO },
    { 7, FlagsCondicionais.OITAVA_MUSICA_DISPARADA_RADIO },
    { 8, FlagsCondicionais.NONA_MUSICA_DISPARADA_RADIO },
    { 9, FlagsCondicionais.DECIMA_MUSICA_DISPARADA_RADIO },
    { 10, FlagsCondicionais.DECIMA_PRIMEIRA_MUSICA_DISPARADA_RADIO },
    { 11, FlagsCondicionais.DECIMA_SEGUNDA_MUSICA_DISPARADA_RADIO },
    { 12, FlagsCondicionais.DECIMA_TERCEIRA_MUSICA_DISPARADA_RADIO },
    { 13, FlagsCondicionais.DECIMA_QUARTA_MUSICA_DISPARADA_RADIO },
    { 14, FlagsCondicionais.DECIMA_QUINTA_MUSICA_DISPARADA_RADIO },
    { 15, FlagsCondicionais.DECIMA_SEXTA_MUSICA_DISPARADA_RADIO },
    { 16, FlagsCondicionais.DECIMA_SETIMA_MUSICA_DISPARADA_RADIO },
    { 17, FlagsCondicionais.DECIMA_OITAVA_MUSICA_DISPARADA_RADIO },
    { 18, FlagsCondicionais.DECIMA_NONA_MUSICA_DISPARADA_RADIO },
    { 19, FlagsCondicionais.VIGESIMA_MUSICA_DISPARADA_RADIO },
    { 20, FlagsCondicionais.VIGESIMA_PRIMEIRA_MUSICA_DISPARADA_RADIO },
    { 21, FlagsCondicionais.VIGESIMA_SEGUNDA_MUSICA_DISPARADA_RADIO },
    { 22, FlagsCondicionais.VIGESIMA_TERCEIRA_MUSICA_DISPARADA_RADIO },
    { 23, FlagsCondicionais.VIGESIMA_QUARTA_MUSICA_DISPARADA_RADIO },
    { 24, FlagsCondicionais.VIGESIMA_QUINTA_MUSICA_DISPARADA_RADIO },
    { 25, FlagsCondicionais.VIGESIMA_SEXTA_MUSICA_DISPARADA_RADIO },
    { 26, FlagsCondicionais.VIGESIMA_SETIMA_MUSICA_DISPARADA_RADIO },
    { 27, FlagsCondicionais.VIGESIMA_OITAVA_MUSICA_DISPARADA_RADIO },
    { 28, FlagsCondicionais.VIGESIMA_NONA_MUSICA_DISPARADA_RADIO },
    { 29, FlagsCondicionais.TRIGESIMA_MUSICA_DISPARADA_RADIO },
};

    private readonly Dictionary<int, FlagsCondicionais> _flagsAoFinalizar = new()
{
    { 1, FlagsCondicionais.PRIMEIRA_MUSICA_TOCADA },
    { 2, FlagsCondicionais.SEGUNDA_MUSICA_TOCADA },
    { 3, FlagsCondicionais.TERCEIRA_MUSICA_TOCADA },
    { 4, FlagsCondicionais.QUARTA_MUSICA_TOCADA },
    { 5, FlagsCondicionais.QUINTA_MUSICA_TOCADA },
    { 6, FlagsCondicionais.SEXTA_MUSICA_TOCADA },
    { 7, FlagsCondicionais.SETIMA_MUSICA_TOCADA },
    { 8, FlagsCondicionais.OITAVA_MUSICA_TOCADA },
    { 9, FlagsCondicionais.NONA_MUSICA_TOCADA },
    { 10, FlagsCondicionais.DECIMA_MUSICA_TOCADA },
    { 11, FlagsCondicionais.DECIMA_PRIMEIRA_MUSICA_TOCADA },
    { 12, FlagsCondicionais.DECIMA_SEGUNDA_MUSICA_TOCADA },
    { 13, FlagsCondicionais.DECIMA_TERCEIRA_MUSICA_TOCADA },
    { 14, FlagsCondicionais.DECIMA_QUARTA_MUSICA_TOCADA },
    { 15, FlagsCondicionais.DECIMA_QUINTA_MUSICA_TOCADA },
    { 16, FlagsCondicionais.DECIMA_SEXTA_MUSICA_TOCADA },
    { 17, FlagsCondicionais.DECIMA_SETIMA_MUSICA_TOCADA },
    { 18, FlagsCondicionais.DECIMA_OITAVA_MUSICA_TOCADA },
    { 19, FlagsCondicionais.DECIMA_NONA_MUSICA_TOCADA },
    { 20, FlagsCondicionais.VIGESIMA_MUSICA_TOCADA },
    { 21, FlagsCondicionais.VIGESIMA_PRIMEIRA_MUSICA_TOCADA },
    { 22, FlagsCondicionais.VIGESIMA_SEGUNDA_MUSICA_TOCADA },
    { 23, FlagsCondicionais.VIGESIMA_TERCEIRA_MUSICA_TOCADA },
    { 24, FlagsCondicionais.VIGESIMA_QUARTA_MUSICA_TOCADA },
    { 25, FlagsCondicionais.VIGESIMA_QUINTA_MUSICA_TOCADA },
    { 26, FlagsCondicionais.VIGESIMA_SEXTA_MUSICA_TOCADA },
    { 27, FlagsCondicionais.VIGESIMA_SETIMA_MUSICA_TOCADA },
    { 28, FlagsCondicionais.VIGESIMA_OITAVA_MUSICA_TOCADA },
    { 29, FlagsCondicionais.VIGESIMA_NONA_MUSICA_TOCADA },
    { 30, FlagsCondicionais.TRIGESIMA_MUSICA_TOCADA }
};

    public override void _Ready()
    {
        CanvasLayerRadio.Visible = false;
        if (GerenciadorNoticiasImpressas.Instance == null) return;

        GerenciadorNoticiasImpressas.Instance.NoticiaTransmitida += OnTransmitirNoticiaNoRadio;
        GerenciadorNoticiasImpressas.Instance.MusicaTransmitida += OnTransmitirMusicaNoRadio;

        if (PlayerAudio != null)
        {
            PlayerAudio.Finished += OnAudioFinished;
        }
    }

    public override void _Process(double delta)
    {
        if (PlayerAudio != null && PlayerAudio.Playing && PlayerAudio.Stream != null && LabelRadio != null)
        {
            CanvasLayerRadio.Visible = true;
            double tempoAtual = PlayerAudio.GetPlaybackPosition();
            double tempoTotal = PlayerAudio.Stream.GetLength();
            double tempoRestante = tempoTotal - tempoAtual;
            ProcessaLabelQueMostraOTempoDaMusica(tempoAtual, tempoTotal, tempoRestante);
        }

        if (EstaMudoNoRadioMasATransmissaoJaIniciou())
        {
            ACadaCincoSegundosAplicaPunicaoDeAudiencia(delta);
        }
    }

    private void ACadaCincoSegundosAplicaPunicaoDeAudiencia(double delta)
    {
        tempoEmSilencio += delta;
        if (tempoEmSilencio >= proximoIntervaloDePunicao)
        {
            GerenciadorDeAudiencia.Instance?.RegistrarImpactoCasoFiqueSilencioDuranteATransmissaoJaIniciada(tempoEmSilencio);
            proximoIntervaloDePunicao += TEMPO_DE_SILENCIO_PARA_PERDA_DE_AUDIENCIA;
        }
    }

    private void ProcessaLabelQueMostraOTempoDaMusica(double tempoAtual, double tempoTotal, double tempoRestante)
    {
        LabelRadio.Text = $"{FormatarTempo(tempoAtual)} / {FormatarTempo(tempoTotal)}";

        if (tempoRestante <= 10.0)
        {
            LabelRadio.Modulate = Colors.Red;
        }
        else
        {
            LabelRadio.Modulate = Colors.White;
        }
    }

    public override void _ExitTree()
    {
        if (GerenciadorNoticiasImpressas.Instance != null)
        {
            GerenciadorNoticiasImpressas.Instance.NoticiaTransmitida -= OnTransmitirNoticiaNoRadio;
            GerenciadorNoticiasImpressas.Instance.MusicaTransmitida -= OnTransmitirMusicaNoRadio;
        }

        if (PlayerAudio != null)
        {
            PlayerAudio.Finished -= OnAudioFinished;
        }
    }

    private void OnTransmitirNoticiaNoRadio(NoticiaModel model)
    {
        ProcessaPrimeiraTransmissaoDoDia(model);
        ProcessarFlagsAoIniciarTransmissao();
        if (model == null || PlayerAudio == null) return;

        _noticiaAtual = model;
        _musicaAtual = null;

        _estadoAtualDaTransmissao = EstadoDaTransmissao.TransmitindoNoticia;
        ResetarContadoresDeSilencio();

        int diaAtual = GerenciadorPassagemDoTempo.Instance != null ? GerenciadorPassagemDoTempo.Instance.DiaAtual : 1;
        string diaFormatado = $"Dia_{diaAtual:D2}"; // Dia_01

        string numeroNoticia = model.Id.Replace("noticia_", ""); // "noticia_01" -> "01"
        string nomeArquivoAudio = $"n{numeroNoticia}.mp3";      // "n01.mp3"

        string caminhoAudio = $"res://Scripts/Dados/JSONS/{diaFormatado}/Audios/{model.Id}/Variacoes/{model.EscolhaJogador}/{nomeArquivoAudio}";

        Log.Print($"[TocarNoticiaNoRadio] Carregando áudio: {caminhoAudio}");

        if (ResourceLoader.Exists(caminhoAudio))
        {
            AudioStream stream = GD.Load<AudioStream>(caminhoAudio);
            PlayerAudio.Stream = stream;
            PlayerAudio.Play();

            if (LabelRadio != null)
            {
                LabelRadio.Modulate = Colors.White; // Garante que começa branca
                double tempoTotal = stream.GetLength();
                LabelRadio.Text = $"00:00 / {FormatarTempo(tempoTotal)}";
            }
        }
        else
        {
            Log.PrintErr($"[TocarNoticiaNoRadio] Arquivo de áudio não encontrado no caminho: {caminhoAudio}");
            FinalizarTransmissao();
        }
    }

    private void OnTransmitirMusicaNoRadio(MusicaModel model)
    {
        ProcessaPrimeiraTransmissaoDoDia(model);
        ProcessarFlagsAoIniciarTransmissao();
        if (model == null || PlayerAudio == null) return;

        _musicaAtual = model;
        _noticiaAtual = null;

        _estadoAtualDaTransmissao = EstadoDaTransmissao.TransmitindoMusica;

        ResetarContadoresDeSilencio();

        string caminhoAudio = model.CaminhoAudio;
        Log.Print($"[TocarNoticiaNoRadio] Carregando música: {caminhoAudio}");

        if (ResourceLoader.Exists(caminhoAudio))
        {
            AudioStream stream = GD.Load<AudioStream>(caminhoAudio);
            PlayerAudio.Stream = stream;
            PlayerAudio.Play();

            if (LabelRadio != null)
            {
                LabelRadio.Modulate = Colors.White; // Garante que começa branca
                double tempoTotal = stream.GetLength();
                LabelRadio.Text = $"00:00 / {FormatarTempo(tempoTotal)}";
            }

            return;
        }

        Log.PrintErr($"[TocarNoticiaNoRadio] Arquivo de música não encontrado no caminho: {caminhoAudio}");
        FinalizarTransmissao();
    }

    private void OnAudioFinished()
    {
        PlayerAudio.Stop();
        CanvasLayerRadio.Visible = false;

        if (LabelRadio != null)
        {
            LabelRadio.Modulate = Colors.White; // Reseta a cor para branco ao finalizar
        }

        FinalizarTransmissao();
    }

    private string FormatarTempo(double tempoEmSegundos)
    {
        int minutos = (int)(tempoEmSegundos / 60);
        int segundos = (int)(tempoEmSegundos % 60);
        return $"{minutos:D2}:{segundos:D2}";
    }

    private bool EstaMudoNoRadioMasATransmissaoJaIniciou()
    {
        return this._estadoAtualDaTransmissao == EstadoDaTransmissao.Nenhuma && this._noticiasJaTransmitidas.Count > 0;
    }

    private void ProcessaPrimeiraTransmissaoDoDia(MusicaModel musicaModel)
    {
        Log.Print($"[TocarNoticiaNoRadio] GerenciadorPassagemDoTempo.Instance.EstadoAtual {GerenciadorPassagemDoTempo.Instance.EstadoAtual}");
        if (GerenciadorPassagemDoTempo.Instance.EstadoAtual == GerenciadorPassagemDoTempo.EstadoDoDia.Parado &&
                    this._estadoAtualDaTransmissao == EstadoDaTransmissao.Nenhuma)
        {
            this._estadoAtualDaTransmissao = EstadoDaTransmissao.TransmitindoMusica; // FIX: ajustado para música
            GerenciadorPassagemDoTempo.Instance.IniciarDiaDeTrabalho();
        }
    }

    private void ProcessaPrimeiraTransmissaoDoDia(NoticiaModel noticiaModel)
    {
        if (GerenciadorPassagemDoTempo.Instance.EstadoAtual == GerenciadorPassagemDoTempo.EstadoDoDia.Parado &&
                    this._estadoAtualDaTransmissao == EstadoDaTransmissao.Nenhuma)
        {
            this._estadoAtualDaTransmissao = EstadoDaTransmissao.TransmitindoNoticia;
            GerenciadorPassagemDoTempo.Instance.IniciarDiaDeTrabalho();
        }
    }

    private void FinalizarTransmissao()
    {
        if (_noticiaAtual != null && GerenciadorNoticiasImpressas.Instance != null)
        {
            Log.Print($"[TocarNoticiaNoRadio] Transmissão finalizada para: {_noticiaAtual.Id}");
            GerenciadorNoticiasImpressas.Instance.NotificarFinalizacaoTransmissao(_noticiaAtual);
            _noticiasJaTransmitidas.Add(_noticiaAtual.Id, true);
            _noticiaAtual = null;
        }

        if (_musicaAtual != null && GerenciadorNoticiasImpressas.Instance != null)
        {
            Log.Print($"[TocarNoticiaNoRadio] Música finalizada para: {_musicaAtual.Id}");
            GerenciadorNoticiasImpressas.Instance.NotificarFinalizacaoTransmissaoMusica(_musicaAtual);
            _noticiasJaTransmitidas.Add(_musicaAtual.Id, true);
            _musicaAtual = null;
        }

        this._estadoAtualDaTransmissao = EstadoDaTransmissao.Nenhuma;
        ResetarContadoresDeSilencio(); // FIX: garante que o contador de silêncio recomeça limpo após o áudio terminar
        ProcessarFlagsAoFinalizarTransmissao();
        Log.Print($"[TocarNoticiaNoRadio] Transmissão finalizada. Total de transmissões hoje: {_noticiasJaTransmitidas.Count}");
        Log.Print($"[TocarNoticiaNoRadio] Estado atual da transmissão: {_estadoAtualDaTransmissao}");
    }

    private void ProcessarFlagsAoIniciarTransmissao()
    {
        if (GerenciadorDeFlagsNarrativas.Instance == null) return;

        if (_flagsAoIniciar.TryGetValue(_noticiasJaTransmitidas.Count, out var flag))
        {
            GerenciadorDeFlagsNarrativas.Instance.AtivarFlagCondicional(flag);
        }
    }

    private void ProcessarFlagsAoFinalizarTransmissao()
    {
        if (GerenciadorDeFlagsNarrativas.Instance == null) return;

        if (_flagsAoFinalizar.TryGetValue(_noticiasJaTransmitidas.Count, out var flag))
        {
            GerenciadorDeFlagsNarrativas.Instance.AtivarFlagCondicional(flag);
        }
    }

    private void ResetarContadoresDeSilencio()
    {
        tempoEmSilencio = 0.0;
        proximoIntervaloDePunicao = TEMPO_DE_SILENCIO_PARA_PERDA_DE_AUDIENCIA;
    }
}