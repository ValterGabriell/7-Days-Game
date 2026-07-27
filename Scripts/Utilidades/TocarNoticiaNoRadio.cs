using fiveyears3.Scripts.Globais;
using Godot;
using System;

public partial class TocarNoticiaNoRadio : Node
{
    [Export] public AudioStreamPlayer3D PlayerAudio;

    private NoticiaModel _noticiaAtual;
    private MusicaModel _musicaAtual;

    public override void _Ready()
    {
        if (GerenciadorNoticiasImpressas.Instance == null) return;

        GerenciadorNoticiasImpressas.Instance.NoticiaTransmitida += OnTransmitirNoticiaNoRadio;
        GerenciadorNoticiasImpressas.Instance.MusicaTransmitida += OnTransmitirMusicaNoRadio;

        if (PlayerAudio != null)
        {
            PlayerAudio.Finished += OnAudioFinished;
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
        FinalizarTransmissao();
    }

    private void OnTransmitirMusicaNoRadio(MusicaModel model)
    {
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
            return;
        }

        GD.PrintErr($"[TocarNoticiaNoRadio] Arquivo de música não encontrado no caminho: {caminhoAudio}");
        FinalizarTransmissao();
    }

    private void FinalizarTransmissao()
    {
        if (_noticiaAtual != null && GerenciadorNoticiasImpressas.Instance != null)
        {
            GD.Print($"[TocarNoticiaNoRadio] Transmissão finalizada para: {_noticiaAtual.Id}");
            GerenciadorNoticiasImpressas.Instance.NotificarFinalizacaoTransmissao(_noticiaAtual);
            _noticiaAtual = null;
        }

        if (_musicaAtual != null && GerenciadorNoticiasImpressas.Instance != null)
        {
            GD.Print($"[TocarNoticiaNoRadio] Música finalizada para: {_musicaAtual.Id}");
            GerenciadorNoticiasImpressas.Instance.NotificarFinalizacaoTransmissaoMusica(_musicaAtual);
            _musicaAtual = null;
        }
    }
}