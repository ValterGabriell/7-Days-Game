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

        /// <summary>
        /// Prepara o SaveAtual puxando os dados em memória dos gerenciadores antes de serializar o JSON.
        /// </summary>
        public void ColetarDadosDosGerenciadores()
        {
            if (SaveAtual == null) SaveAtual = new DadosSave();

            // 1. Puxa dados do GerenciadorDeAudiencia
            if (GerenciadorDeAudiencia.Instance != null)
            {
                var aud = GerenciadorDeAudiencia.Instance;

                // Raiz
                SaveAtual.AudienciaAtualGlobal = aud.AudienciaAtual;
                SaveAtual.EsperancaAtualGlobal = aud.EsperancaAtual;
                SaveAtual.IrritacaoAtualGlobal = aud.IrritacaoAtual;

                // Dentro da Reputacao do EstadoJogadorSave
                SaveAtual.EstadoAtualDoJogador.Reputacao.AudienciaPopular = (float)aud.AudienciaAtual;
                SaveAtual.EstadoAtualDoJogador.Reputacao.EsperancaPopulacional = (float)aud.EsperancaAtual;
                SaveAtual.EstadoAtualDoJogador.Reputacao.IrritacaoPopulacional = (float)aud.IrritacaoAtual;
            }

            // 2. Puxa dados do GerenciadorDeConfiabilidade
            if (GerenciadorDeConfiabilidade.Instance != null)
            {
                var conf = GerenciadorDeConfiabilidade.Instance;

                // Raiz
                SaveAtual.DeltaLealdadeGovernoGlobal = conf.DeltaLealdadeGovernoGeral;
                SaveAtual.DeltaConfiancaResistenciaGlobal = conf.DeltaConfiancaResistenciaGeral;
                SaveAtual.DeltaAudienciaGlobal = conf.DeltaAudienciaGeral;

                // Dentro da Reputacao do EstadoJogadorSave
                SaveAtual.EstadoAtualDoJogador.Reputacao.LealdadeGoverno = conf.DeltaLealdadeGovernoGeral;
                SaveAtual.EstadoAtualDoJogador.Reputacao.ConfiancaResistencia = conf.DeltaConfiancaResistenciaGeral;
            }

            // 3. Puxa flags narrativas/condicionais para histórico do save
            if (GerenciadorDeFlagsNarrativas.Instance != null)
            {
                SaveAtual.EstadoAtualDoJogador.FlagsHistoricas = GerenciadorDeFlagsNarrativas.Instance.ExportarFlagsHistoricasParaSave();
            }
        }

        /// <summary>
        /// Aplica os dados do SaveAtual de volta nos gerenciadores logo após carregar o arquivo JSON.
        /// </summary>
        public void AplicarDadosNosGerenciadores()
        {
            if (SaveAtual == null) return;

            if (GerenciadorPassagemDoTempo.Instance != null)
            {
                GerenciadorPassagemDoTempo.Instance.CarregarDiaAtual(SaveAtual.EstadoAtualDoJogador.DiaAtual);
            }

            // Restaura no GerenciadorDeAudiencia
            if (GerenciadorDeAudiencia.Instance != null)
            {
                GerenciadorDeAudiencia.Instance.CarregarEstado(
                    SaveAtual.AudienciaAtualGlobal,
                    SaveAtual.EsperancaAtualGlobal,
                    SaveAtual.IrritacaoAtualGlobal
                );
            }

            // Restaura no GerenciadorDeConfiabilidade
            if (GerenciadorDeConfiabilidade.Instance != null)
            {
                GerenciadorDeConfiabilidade.Instance.CarregarEstadoGeral(
                    SaveAtual.DeltaLealdadeGovernoGlobal,
                    SaveAtual.DeltaConfiancaResistenciaGlobal,
                    SaveAtual.DeltaAudienciaGlobal
                );
            }

            if (GerenciadorDeFlagsNarrativas.Instance != null)
            {
                GerenciadorDeFlagsNarrativas.Instance.CarregarFlagsHistoricasDoSave(SaveAtual.EstadoAtualDoJogador.FlagsHistoricas);
            }
        }


        #region Métodos de Persistência (Salvar / Carregar)

        /// <summary>
        /// Salva o estado atual (ou o informado) no arquivo especificado pelo saveId.
        /// </summary>
        public bool SalvarJogo(string saveId, DadosSave dadosParaSalvar)
        {
            // Coleta o estado atual dos Singletons antes de salvar
            ColetarDadosDosGerenciadores();

            DadosSave dados = dadosParaSalvar ?? SaveAtual;
            dados.DataEHoraSave = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string caminhoCompleto = ObterCaminhoDoArquivo(saveId);

            try
            {
                string jsonString = JsonSerializer.Serialize(dados, _jsonOptions);
                File.WriteAllText(caminhoCompleto, jsonString);

                Log.Print($"[GerenciadorDeSave] Jogo salvo com sucesso em: {caminhoCompleto}");
                return true;
            }
            catch (Exception ex)
            {
                Log.PrintErr($"[GerenciadorDeSave] Erro ao salvar o jogo no slot '{saveId}': {ex.Message}");
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

            if (!ExisteSave(id)) return false;

            try
            {
                string jsonString = await File.ReadAllTextAsync(caminhoCompleto);
                DadosSave dadosCarregados = JsonSerializer.Deserialize<DadosSave>(jsonString, _jsonOptions);

                if (dadosCarregados != null)
                {
                    SaveAtual = dadosCarregados;

                    // Aplica os dados do JSON carregado nos gerenciadores em memória
                    AplicarDadosNosGerenciadores();

                    Log.Print($"[GerenciadorDeSave] Jogo carregado com sucesso do slot: '{id}'");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.PrintErr($"[GerenciadorDeSave] Erro ao carregar o save '{id}': {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Reseta os dados carregados na memória para um novo jogo.
        /// </summary>
        public void NovoJogo(string saveId = null)
        {
            string id = string.IsNullOrWhiteSpace(saveId) ? SaveIdPadrao : saveId;

            DeletarSave(id);

            SaveAtual = DadosSave.CriarNovoSave();
            VariavelAuxiliarQueVaiGuardarAsEscolhasFeitasEmUmDeterminadoDia.Clear();

            if (GerenciadorPassagemDoTempo.Instance != null)
            {
                GerenciadorPassagemDoTempo.Instance.ResetarTempo();
            }

            AplicarDadosNosGerenciadores();

            Log.Print($"[GerenciadorDeSave] Novo jogo inicializado. Save antigo '{id}' removido.");
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
                    Log.Print($"[GerenciadorDeSave] Save '{saveId}' removido com sucesso.");
                    return true;
                }
                catch (Exception ex)
                {
                    Log.PrintErr($"[GerenciadorDeSave] Falha ao deletar save '{saveId}': {ex.Message}");
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