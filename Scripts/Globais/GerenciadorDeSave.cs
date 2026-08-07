using Godot;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Scripts.SaveSystem;
using fiveyears3.Scripts.Globais;
using System.Collections.Generic;

namespace Scripts.SaveSystem
{
    public partial class GerenciadorDeSave : Node
    {
        public static GerenciadorDeSave Instance { get; private set; }

        public double TempoEmSilencioGeral { get; set; } = 0.0;


        [Export]
        public string SaveIdPadrao { get; set; } = "slot_1";

        public DadosSave SaveAtual { get; private set; } = new DadosSave();

        public List<NoticiaEscolhaSave> VariavelAuxiliarQueVaiGuardarAsEscolhasFeitasEmUmDeterminadoDia { get; private set; } = new List<NoticiaEscolhaSave>();

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                GD.PushWarning("[GerenciadorDeSave] Instância duplicada detectada! Destruindo objeto extra.");
                QueueFree();
            }

            GerenciadorNoticiasImpressas.Instance.NoticiaTransmitida += SalvarEscolhaDaNoticiaNoRadio;
        }

    
        #region Métodos de Persistência (Salvar / Carregar)

        /// <summary>
        /// Salva o estado atual (ou o informado) no arquivo especificado pelo saveId.
        /// </summary>
        public bool SalvarJogo(string saveId, DadosSave dadosParaSalvar)
        {
            DadosSave dados = dadosParaSalvar;

            // Atualiza metadados do save
            dados.DataEHoraSave = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string caminhoCompleto = ObterCaminhoDoArquivo(saveId);

            try
            {
                string jsonString = JsonSerializer.Serialize(dados, _jsonOptions);
                File.WriteAllText(caminhoCompleto, jsonString);

                GD.Print($"[GerenciadorDeSave] Jogo salvo com sucesso em: {caminhoCompleto}");
                return true;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[GerenciadorDeSave] Erro ao salvar o jogo no slot '{saveId}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Carrega os dados de um arquivo de save existente.
        /// </summary>
        public async Task<bool> CarregarJogoAsync(string saveId = null)
        {
            string id = string.IsNullOrWhiteSpace(saveId) ? SaveIdPadrao : saveId;
            string caminhoCompleto = ObterCaminhoDoArquivo(id);

            if (!ExisteSave(id))
            {
                GD.PushWarning($"[GerenciadorDeSave] Nenhum arquivo de save encontrado para o ID: {id}");
                return false;
            }

            try
            {
                string jsonString = await File.ReadAllTextAsync(caminhoCompleto);
                DadosSave dadosCarregados = JsonSerializer.Deserialize<DadosSave>(jsonString, _jsonOptions);

                if (dadosCarregados != null)
                {
                    SaveAtual = dadosCarregados;
                    GD.Print($"[GerenciadorDeSave] Jogo carregado com sucesso do slot: '{id}'");
                    return true;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[GerenciadorDeSave] Erro ao carregar o save '{id}': {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Reseta os dados carregados na memória para um novo jogo.
        /// </summary>
        public void NovoJogo()
        {
            SaveAtual = new DadosSave();
            GD.Print("[GerenciadorDeSave] Dados de novo jogo inicializados.");
        }

        private void SalvarEscolhaDaNoticiaNoRadio(NoticiaModel model)
        {
            model.Variacoes.TryGetValue(model.EscolhaJogador, out var variacaoEscolhida);
            VariavelAuxiliarQueVaiGuardarAsEscolhasFeitasEmUmDeterminadoDia.Add(new NoticiaEscolhaSave
            {
                IDNoticia = model.Id,
                VariacaoEscolhida = model.EscolhaJogador.ToString(),
                ImpressoresGeradasNoDiaSeguinte = variacaoEscolhida?.ImpressoresGeradasNoDiaSeguinte
            });
        }


        /// <summary>
        /// Deleta o arquivo de save do ID informado.
        /// </summary>
        public bool DeletarSave(string saveId)
        {
            if (string.IsNullOrWhiteSpace(saveId)) return false;

            string caminhoCompleto = ObterCaminhoDoArquivo(saveId);

            if (File.Exists(caminhoCompleto))
            {
                try
                {
                    File.Delete(caminhoCompleto);
                    GD.Print($"[GerenciadorDeSave] Save '{saveId}' removido com sucesso.");
                    return true;
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GerenciadorDeSave] Falha ao deletar save '{saveId}': {ex.Message}");
                }
            }

            return false;
        }

        /// <summary>
        /// Verifica se o arquivo de save existe.
        /// </summary>
        public bool ExisteSave(string saveId)
        {
            if (string.IsNullOrWhiteSpace(saveId)) return false;
            return File.Exists(ObterCaminhoDoArquivo(saveId));
        }

        #endregion

        #region Helpers de Caminho e Utilitários

        /// <summary>
        /// Converte um saveId simples no caminho do sistema de arquivos dentro do diretório `user://`.
        /// </summary>
        private string ObterCaminhoDoArquivo(string saveId)
        {
            // Mapeia para a pasta do Godot (ex: C:/Users/Nome/AppData/Roaming/Godot/app_userdata/SeuJogo)
            string userPath = ProjectSettings.GlobalizePath("user://");

            // Garante que o diretório base exista
            if (!Directory.Exists(userPath))
            {
                Directory.CreateDirectory(userPath);
            }

            // Garante a extensão .json
            string nomeArquivo = saveId.EndsWith(".json") ? saveId : $"{saveId}.json";

            return Path.Combine(userPath, nomeArquivo);
        }

        #endregion
    }
}