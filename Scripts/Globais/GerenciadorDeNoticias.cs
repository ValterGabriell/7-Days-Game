using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace fiveyears3.Scripts.Globais
{
    public partial class GerenciadorDeNoticias : Node
    {
        public static GerenciadorDeNoticias Instance { get; private set; }

 
        public event Action NoticiasCarregadas;
        public event Action FinalizacaoDoDiaLiberada; 
        public List<NoticiaModel> NoticiasDoDia { get; private set; } = new List<NoticiaModel>();
        public bool CalendarioFoiVistoHoje { get; set; } = false;
        public int NumeroDeMusicasQueDevemSerTocadasNoDia { get; set; } = 0;
        public int NUmeroDeNoticiasQueDevemSerTransmitidasNoDia { get; set; } = 0;

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

        public override void _ExitTree()
        {
            if (GerenciadorPassagemDoTempo.Instance != null)
            {
                GerenciadorPassagemDoTempo.Instance.DiaAlterado -= OnDiaAlterado;
            }
        }

        public void AtualizarValoresDeNoticiasEMusicasQueDevemSerTransmitidasNoDia(List<BlocoRotinaModel> Noticias)
        {
            if(CalendarioFoiVistoHoje)
                return;
            GD.Print($"[GerenciadorDeNoticias] Atualizando valores de notícias que devem ser transmitidas no dia. Total de notícias: {Noticias.Count}");
            this.NUmeroDeNoticiasQueDevemSerTransmitidasNoDia = Noticias.Where(n => n.Tipo == TipoBlocoRotina.NOTICIA).Count();
            this.NumeroDeMusicasQueDevemSerTocadasNoDia = Noticias.Where(n => n.Tipo == TipoBlocoRotina.MUSICA).Count();
            CalendarioFoiVistoHoje = true;
        }



        public void AtualizarValoresDeNoticiasQueForamTransmitidasNoDiaAtual()
        {
            if (this.NUmeroDeNoticiasQueDevemSerTransmitidasNoDia > 0)
            {
                this.NUmeroDeNoticiasQueDevemSerTransmitidasNoDia -= 1;
            }

            VerificarSeDeveLiberarFinalizacaoDoDia();
        }

        public void AtualizarValoresDeMusicasQueForamTransmitidasNoDiaAtual()
        {
            if (this.NumeroDeMusicasQueDevemSerTocadasNoDia > 0)
            {
                this.NumeroDeMusicasQueDevemSerTocadasNoDia -= 1;
            }

            VerificarSeDeveLiberarFinalizacaoDoDia();
        }

        private void VerificarSeDeveLiberarFinalizacaoDoDia()
        {
            GD.Print($"[GerenciadorDeNoticias] Noticias restantes: {this.NUmeroDeNoticiasQueDevemSerTransmitidasNoDia}, Musicas restantes: {this.NumeroDeMusicasQueDevemSerTocadasNoDia}");
            if (this.NUmeroDeNoticiasQueDevemSerTransmitidasNoDia <= 0 && this.NumeroDeMusicasQueDevemSerTocadasNoDia <= 0)
            {
                GD.Print("[GerenciadorDeNoticias] Todas as notícias e músicas do dia foram transmitidas. Liberando botão de encerrar o dia!");
                FinalizacaoDoDiaLiberada?.Invoke(); 
            }
        }

        public void ResetarValoresDeNoticiasEMusicasQueDevemSerTransmitidasNoDia()
        {
            this.NumeroDeMusicasQueDevemSerTocadasNoDia = 0;
            this.NumeroDeMusicasQueDevemSerTocadasNoDia =0;
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

            NoticiasCarregadas?.Invoke();
        }
    }
}