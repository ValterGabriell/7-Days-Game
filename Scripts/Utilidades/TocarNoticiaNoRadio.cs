using fiveyears3.Scripts.Globais;
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
        //o dia ainda nao inicou, essa é a primeira transmissao do dia, iniciando ele propriamente dito
        ProcessaPrimeiraTransmissaoDoDia(model);
        if (model == null || PlayerAudio == null) return;

        _noticiaAtual = model;

        int diaAtual = GerenciadorPassagemDoTempo.Instance != null ? GerenciadorPassagemDoTempo.Instance.DiaAtual : 1;
        string diaFormatado = $"Dia_{diaAtual:D2}"; // Dia_01

        string numeroNoticia = model.Id.Replace("noticia_", ""); // "noticia_01" -> "01"
        string nomeArquivoAudio = $"n{numeroNoticia}.mp3";      // "n01.mp3"

        string caminhoAudio = $"res://Scripts/Dados/JSONS/{diaFormatado}/Audios/{model.Id}/Variacoes/{model.EscolhaJogador}/{nomeArquivoAudio}";

        GD.Print($"[TocarNoticiaNoRadio] Carregando áudio: {caminhoAudio}");

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
            GD.PrintErr($"[TocarNoticiaNoRadio] Arquivo de áudio não encontrado no caminho: {caminhoAudio}");
            FinalizarTransmissao();
        }
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

    private void OnTransmitirMusicaNoRadio(MusicaModel model)
    {
        ProcessaPrimeiraTransmissaoDoDia(model);
        if (model == null || PlayerAudio == null) return;

        _musicaAtual = model;
        _noticiaAtual = null;

        string caminhoAudio = model.CaminhoAudio;
        GD.Print($"[TocarNoticiaNoRadio] Carregando música: {caminhoAudio}");

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

        GD.PrintErr($"[TocarNoticiaNoRadio] Arquivo de música não encontrado no caminho: {caminhoAudio}");
        FinalizarTransmissao();
    }
    private bool EstaMudoNoRadioMasATransmissaoJaIniciou()
    {
        
        return this._estadoAtualDaTransmissao == EstadoDaTransmissao.Nenhuma && this._noticiasJaTransmitidas.Count > 0;
    }

    private void ProcessaPrimeiraTransmissaoDoDia(MusicaModel musicaModel)
    {
        GD.Print($"[TocarNoticiaNoRadio] GerenciadorPassagemDoTempo.Instance.EstadoAtual {GerenciadorPassagemDoTempo.Instance.EstadoAtual}");
        if (GerenciadorPassagemDoTempo.Instance.EstadoAtual == GerenciadorPassagemDoTempo.EstadoDoDia.Parado &&
                    this._estadoAtualDaTransmissao == EstadoDaTransmissao.Nenhuma)
        {
            this._estadoAtualDaTransmissao = EstadoDaTransmissao.TransmitindoNoticia;
            GerenciadorPassagemDoTempo.Instance.IniciarHorarioDeTrabalho();
        }

    }
    private void ProcessaPrimeiraTransmissaoDoDia(NoticiaModel noticiaModel)
    {
        if (GerenciadorPassagemDoTempo.Instance.EstadoAtual == GerenciadorPassagemDoTempo.EstadoDoDia.Parado &&
                    this._estadoAtualDaTransmissao == EstadoDaTransmissao.Nenhuma)
        {
            this._estadoAtualDaTransmissao = EstadoDaTransmissao.TransmitindoNoticia;
            GerenciadorPassagemDoTempo.Instance.IniciarHorarioDeTrabalho();
        }
    }


    private void FinalizarTransmissao()
    {
        if (_noticiaAtual != null && GerenciadorNoticiasImpressas.Instance != null)
        {
            GD.Print($"[TocarNoticiaNoRadio] Transmissão finalizada para: {_noticiaAtual.Id}");
            GerenciadorNoticiasImpressas.Instance.NotificarFinalizacaoTransmissao(_noticiaAtual);
            _noticiasJaTransmitidas.Add(_noticiaAtual.Id, true);
            _noticiaAtual = null;
        }

        if (_musicaAtual != null && GerenciadorNoticiasImpressas.Instance != null)
        {
            GD.Print($"[TocarNoticiaNoRadio] Música finalizada para: {_musicaAtual.Id}");
            GerenciadorNoticiasImpressas.Instance.NotificarFinalizacaoTransmissaoMusica(_musicaAtual);
            _noticiasJaTransmitidas.Add(_musicaAtual.Id, true);
            _musicaAtual = null;
        }
        this._estadoAtualDaTransmissao = EstadoDaTransmissao.Nenhuma;
        GD.Print($"[TocarNoticiaNoRadio] Transmissão finalizada. Total de transmissões hoje: {_noticiasJaTransmitidas.Count}");
        GD.Print($"[TocarNoticiaNoRadio] Estado atual da transmissão: {_estadoAtualDaTransmissao}");
    }
}