using fiveyears3.Scripts.Utilidades;
using Flags;
using Godot;
using System;

namespace fiveyears3.Scripts.Globais;

public partial class GerenciadorDeEventoAleatorio : Node
{
    public enum TipoEventoAleatorio { Nenhum, AntenaQuebrada }
    public TipoEventoAleatorio TipoEventoAleatorioAtual { get; private set; } = TipoEventoAleatorio.Nenhum;

    private IEventoAleatorio _eventoAtivo;
    public static GerenciadorDeEventoAleatorio Instance { get; private set; }

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        if (GerenciadorDeFlagsNarrativas.Instance != null)
        {
            GerenciadorDeFlagsNarrativas.Instance.OnFlagAtivada += ProcessarDisparoDeEvento;
        }
    }

    public override void _ExitTree()
    {
        if (GerenciadorDeFlagsNarrativas.Instance != null)
        {
            GerenciadorDeFlagsNarrativas.Instance.OnFlagAtivada -= ProcessarDisparoDeEvento;
        }
    }

    private void ProcessarDisparoDeEvento(FlagNarrativa flag)
    {
        GD.Print($"[GerenciadorDeEventoAleatorio] Flag narrativa ativada: {flag}");
        switch (flag)
        {
            case FlagNarrativa.AntenaQuebrada_01:
                DispararEvento(TipoEventoAleatorio.AntenaQuebrada, new AntenaQuebrada());
                break;
        }
    }

    private void DispararEvento(TipoEventoAleatorio tipo, IEventoAleatorio instanciaEvento)
    {
        GD.Print($"[GerenciadorDeEventoAleatorio] Disparando evento: {tipo}");
        _eventoAtivo?.FinalizarEvento();
        TipoEventoAleatorioAtual = tipo;
        _eventoAtivo = instanciaEvento;
        _eventoAtivo.IniciarEvento();
    }

    public void ConcluirEventoAtual()
    {
        if (_eventoAtivo != null)
        {
            _eventoAtivo.FinalizarEvento();
            _eventoAtivo = null;
            TipoEventoAleatorioAtual = TipoEventoAleatorio.Nenhum;
        }
    }
}

public interface IEventoAleatorio
{
    void IniciarEvento();
    void FinalizarEvento();
}

#region Eventos Aleatorios

public class AntenaQuebrada : IEventoAleatorio
{
    private const string CAMINHO_ANTENA = "ConfiguracaoGlobal/Antena";
    private Antena _antena;

    public void IniciarEvento()
    {
        GD.Print($"[STRATEGY - AntenaQuebrada] Buscando nó no caminho: {CAMINHO_ANTENA}");

        var arvore = (SceneTree)Engine.GetMainLoop();
        var cenaAtual = arvore.CurrentScene;

        if (cenaAtual != null)
        {
            _antena = cenaAtual.GetNodeOrNull<Antena>(CAMINHO_ANTENA);

            if (GodotObject.IsInstanceValid(_antena))
            {
                GD.Print("[STRATEGY - AntenaQuebrada] Antena localizada e quebrada com sucesso!");
                _antena.QuebrarAntena();
            }
            else
            {
                GD.PrintErr($"[STRATEGY - AntenaQuebrada] Nó não encontrado no caminho fixo: {CAMINHO_ANTENA}");
            }
        }
    }

    public void FinalizarEvento()
    {
        GD.Print("[STRATEGY - AntenaQuebrada] Finalizando evento e consertando a antena.");

        if (GodotObject.IsInstanceValid(_antena))
        {
            _antena.ConsertarAntena();
            _antena = null;
        }
    }
}

#endregion