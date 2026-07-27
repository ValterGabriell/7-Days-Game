using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace fiveyears3.Scripts.Globais
{
    public partial class GerenciadorDeNoticias : Node
    {
        public static GerenciadorDeNoticias Instance { get; private set; }

        [Signal]
        public delegate void NoticiasCarregadasEventHandler();

        public List<NoticiaModel> NoticiasDoDia { get; private set; } = new List<NoticiaModel>();

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

            CarregarNoticiasDoDia();
        }

        private void OnDiaAlterado(int novoDia)
        {
            CarregarNoticiasDoDia();
        }

        public void CarregarNoticiasDoDia()
        {
            int dia = GerenciadorPassagemDoTempo.Instance != null
                ? GerenciadorPassagemDoTempo.Instance.DiaAtual
                : 1;

            string nomePastaDia = $"dia_{dia:D2}";

            string caminhoRelativo = $"res://scripts/dados/jsons/{nomePastaDia}/Noticias.json";
            string caminhoAbsoluto = ProjectSettings.GlobalizePath(caminhoRelativo);

            if (!File.Exists(caminhoAbsoluto))
            {
                caminhoAbsoluto = $@"C:\DEV\PROJETOSPESSOAIS\FIVE-YEARS-3\scripts\dados\jsons\{nomePastaDia}\Noticias.json";
            }

            if (File.Exists(caminhoAbsoluto))
            {
                string jsonString = File.ReadAllText(caminhoAbsoluto);

                var opcoes = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                };

                List<NoticiaModel> listaDeserializada = JsonSerializer.Deserialize<List<NoticiaModel>>(jsonString, opcoes);

                if (listaDeserializada != null)
                {
                    NoticiasDoDia = listaDeserializada;
                }
                else
                {
                    NoticiasDoDia.Clear();
                }
            }
            else
            {
                NoticiasDoDia.Clear();
            }

            EmitSignal(SignalName.NoticiasCarregadas);
        }
    }
}