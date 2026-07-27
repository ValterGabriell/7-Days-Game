using Godot;
using System;
using System.Collections.Generic;

namespace fiveyears3.Scripts.Globais
{
    public partial class GerenciadorNoticiasImpressas : Node
    {
        public static GerenciadorNoticiasImpressas Instance { get; private set; }

        public event Action<NoticiaModel, VariacaoNoticia> NoticiaImpressa;

        public List<NoticiaModel> NoticiasImpressasDoDia { get; private set; } = new();

        public override void _EnterTree()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                QueueFree();
            }
        }

        public override void _Ready()
        {
            if (GerenciadorPassagemDoTempo.Instance != null)
            {
                GerenciadorPassagemDoTempo.Instance.DiaAlterado += OnDiaAlterado;
            }
        }

        private void OnDiaAlterado(int novoDia)
        {
            NoticiasImpressasDoDia.Clear();
        }

        public bool ImprimirNoticia(NoticiaModel noticia)
        {
            if (noticia == null) return false;

            if (NoticiasImpressasDoDia.Exists(n => n.Id == noticia.Id))
            {
                return false;
            }

            NoticiasImpressasDoDia.Add(noticia);

            VariacaoNoticia variacaoUsada = null;
            if (noticia.Variacoes != null && noticia.Variacoes.TryGetValue(noticia.EscolhaJogador, out var variacao))
            {
                variacaoUsada = variacao;
            }

            NoticiaImpressa?.Invoke(noticia, variacaoUsada);
            return true;
        }
    }
}