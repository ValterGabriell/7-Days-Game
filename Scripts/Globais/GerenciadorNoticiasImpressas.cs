using Godot;
using System;
using System.Collections.Generic;

namespace fiveyears3.Scripts.Globais
{
    public partial class GerenciadorNoticiasImpressas : Node
    {
        public static GerenciadorNoticiasImpressas Instance { get; private set; }

        public event Action<NoticiaModel, VariacaoNoticia> NoticiaImpressa;
        public event Action<NoticiaModel> NoticiaTransmitida;

        public List<NoticiaModel> NoticiasImpressasDoDia { get; private set; } = new();
        public List<NoticiaModel> NoticiasTransmitidasDoDia { get; private set; } = new();

        public override void _EnterTree()
        {
            if (Instance == null)
            {
                Instance = this;
                return;
            }

            QueueFree();
        }

        public override void _Ready()
        {
            if (GerenciadorPassagemDoTempo.Instance == null) return;
            GerenciadorPassagemDoTempo.Instance.DiaAlterado += OnDiaAlterado;
        }

        private void OnDiaAlterado(int novoDia)
        {
            NoticiasImpressasDoDia.Clear();
            NoticiasTransmitidasDoDia.Clear();
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

        public bool TransmitirNoticiaNoRadio(NoticiaModel noticia)
        {
            bool invalida = noticia == null || !NoticiasImpressasDoDia.Contains(noticia);
            if (invalida) return false;

            NoticiasImpressasDoDia.Remove(noticia);
            NoticiasTransmitidasDoDia.Add(noticia);

            NoticiaTransmitida?.Invoke(noticia);
            return true;
        }
    }
}