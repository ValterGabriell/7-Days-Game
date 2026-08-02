using Godot;
using System;
using System.Collections.Generic;

namespace fiveyears3.Scripts.Globais
{
    public partial class GerenciadorNoticiasImpressas : Node
    {
        private enum EstadoTransmissaoNoticia
        {
            PodeTransmitir,
            NaoPodeTransmitirPoisEstaEmTransmissao
        }
        public static GerenciadorNoticiasImpressas Instance { get; private set; }

        public event Action<NoticiaModel, VariacaoNoticia> NoticiaImpressa;
        public event Action<NoticiaModel> NoticiaRemovidaDaFila;
        public event Action<MusicaModel> MusicaEnviadaNoRadio;
        public event Action<MusicaModel> MusicaRemovidaDaFila;
        public event Action<NoticiaModel> NoticiaTransmitida;
        public event Action<MusicaModel> MusicaTransmitida;

        public event Action<NoticiaModel> NoticiaFinalizadaTransmissao;
        public event Action<MusicaModel> MusicaFinalizadaTransmissao;

        public List<NoticiaModel> NoticiasImpressasDoDia { get; private set; } = new();
        public List<NoticiaModel> NoticiasTransmitidasDoDia { get; private set; } = new();
        public List<MusicaModel> MusicasEnviadasDoDia { get; private set; } = new();
        public List<MusicaModel> MusicasTransmitidasDoDia { get; private set; } = new();
        public NoticiaModel NoticiaEmTransmissao { get; private set; } = null;
        public MusicaModel MusicaEmTransmissao { get; private set; } = null;
        private EstadoTransmissaoNoticia EstadoAtualTransmissao { get; set; } = EstadoTransmissaoNoticia.PodeTransmitir;
        public bool PodeIniciarTransmissao => EstadoAtualTransmissao == EstadoTransmissaoNoticia.PodeTransmitir;


        public override void _EnterTree()
        {
            if (Instance == null)
            {
                Instance = this;
                return;
            }

            QueueFree();
        }

        public void NotificarFinalizacaoTransmissao(NoticiaModel noticia)
        {
            NoticiaFinalizadaTransmissao?.Invoke(noticia);
        }

        public void NotificarFinalizacaoTransmissaoMusica(MusicaModel musica)
        {
            MusicaFinalizadaTransmissao?.Invoke(musica);
        }

        public override void _Ready()
        {
            if (GerenciadorPassagemDoTempo.Instance == null) return;
            GerenciadorPassagemDoTempo.Instance.DiaAlterado += OnDiaAlterado;
            NoticiaFinalizadaTransmissao += OnNoticiaFinalizadaTransmissao;
            MusicaFinalizadaTransmissao += OnMusicaFinalizadaTransmissao;
        }

    

        private void OnDiaAlterado(int novoDia)
        {
            NoticiasImpressasDoDia.Clear();
            NoticiasTransmitidasDoDia.Clear();
            MusicasEnviadasDoDia.Clear();
            MusicasTransmitidasDoDia.Clear();
            NoticiaEmTransmissao = null;
            MusicaEmTransmissao = null;
            EstadoAtualTransmissao = EstadoTransmissaoNoticia.PodeTransmitir;
        }

        public bool ImprimirNoticia(NoticiaModel noticia)
        {
            if (noticia == null) return false;

            bool jaExiste = NoticiasImpressasDoDia.Exists(n => n.Id == noticia.Id) || NoticiasTransmitidasDoDia.Exists(n => n.Id == noticia.Id);
            if (jaExiste) return false;

            NoticiasImpressasDoDia.Add(noticia);
            GD.Print($"[GerenciadorNoticiasImpressas]Notícia impressa: {noticia.TituloOriginal}");
            GD.Print($"[GerenciadorNoticiasImpressas]Noticias impressas do dia: {NoticiasImpressasDoDia.Count}");

            VariacaoNoticia variacaoUsada = null;
            bool temVariacoes = noticia.Variacoes != null;
            if (temVariacoes)
            {
                noticia.Variacoes.TryGetValue(noticia.EscolhaJogador, out variacaoUsada);
            }

            NoticiaImpressa?.Invoke(noticia, variacaoUsada);
            return true;
        }

        public void TransmitirNoticiaNoRadio(NoticiaModel noticia)
        {
            bool invalida = noticia == null
                || !NoticiasImpressasDoDia.Contains(noticia)
                || !PodeIniciarTransmissao;

            if (invalida) return;
            NoticiasImpressasDoDia.Remove(noticia);

            NoticiaEmTransmissao = noticia;
            MusicaEmTransmissao = null;
            EstadoAtualTransmissao = EstadoTransmissaoNoticia.NaoPodeTransmitirPoisEstaEmTransmissao;
            NoticiaTransmitida?.Invoke(noticia);
            GerenciadorDeNoticias.Instance.AtualizarValoresDeNoticiasQueForamTransmitidasNoDiaAtual();
        }

        public bool DesfazerImpressaoNoticia(NoticiaModel noticia)
        {
            if (noticia == null) return false;

            bool emTransmissao = NoticiaEmTransmissao?.Id == noticia.Id;
            bool jaTransmitida = NoticiasTransmitidasDoDia.Exists(n => n.Id == noticia.Id);
            if (emTransmissao || jaTransmitida) return false;

            int removidas = NoticiasImpressasDoDia.RemoveAll(n => n.Id == noticia.Id);
            if (removidas <= 0) return false;

            NoticiaRemovidaDaFila?.Invoke(noticia);
            return true;
        }

        public bool EnviarMusicaParaRadio(MusicaModel musica)
        {
            if (musica == null) return false;

            bool jaEnviada = MusicasEnviadasDoDia.Exists(m => m.Id == musica.Id) || MusicasTransmitidasDoDia.Exists(m => m.Id == musica.Id);
            if (jaEnviada) return false;

            MusicasEnviadasDoDia.Add(musica);
            MusicaEnviadaNoRadio?.Invoke(musica);
            return true;
        }

        public bool DesfazerEnvioMusicaParaRadio(MusicaModel musica)
        {
            if (musica == null) return false;

            bool emTransmissao = MusicaEmTransmissao?.Id == musica.Id;
            bool jaTransmitida = MusicasTransmitidasDoDia.Exists(m => m.Id == musica.Id);
            if (emTransmissao || jaTransmitida) return false;

            int removidas = MusicasEnviadasDoDia.RemoveAll(m => m.Id == musica.Id);
            if (removidas <= 0) return false;

            MusicaRemovidaDaFila?.Invoke(musica);
            return true;
        }

        public void TransmitirMusicaNoRadio(MusicaModel musica)
        {
            bool invalida = musica == null
                || !MusicasEnviadasDoDia.Contains(musica)
                || !PodeIniciarTransmissao;

            if (invalida) return;

            MusicasEnviadasDoDia.Remove(musica);
            MusicaEmTransmissao = musica;
            NoticiaEmTransmissao = null;
            EstadoAtualTransmissao = EstadoTransmissaoNoticia.NaoPodeTransmitirPoisEstaEmTransmissao;
            MusicaTransmitida?.Invoke(musica);
            GerenciadorDeNoticias.Instance.AtualizarValoresDeMusicasQueForamTransmitidasNoDiaAtual();
        }

        private void OnNoticiaFinalizadaTransmissao(NoticiaModel model)
        {
            if (model == null) return;

            GD.Print($"[GerenciadorNoticiasImpressas]Notícia finalizada: {model.TituloOriginal}");
            EstadoAtualTransmissao = EstadoTransmissaoNoticia.PodeTransmitir;
            NoticiasTransmitidasDoDia.Add(model);
            NoticiaEmTransmissao = null;
            GerenciadorDeConfiabilidade.Instance?.ProcessarImpactoNoticia(model);
        }

        private void OnMusicaFinalizadaTransmissao(MusicaModel model)
        {
            if (model == null) return;

            GD.Print($"[GerenciadorNoticiasImpressas]Música finalizada: {model.Titulo}");
            EstadoAtualTransmissao = EstadoTransmissaoNoticia.PodeTransmitir;
            MusicasTransmitidasDoDia.Add(model);
            MusicaEmTransmissao = null;
        }
    }
}