using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fiveyears3.Scripts.Globais
{
    public partial class GerenciadorDeFolhasDeRelatorio : Node
    {
        public static GerenciadorDeFolhasDeRelatorio Instance { get; private set; }
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
        
        public void RegistrarFolhaDeRelatorio(string titulo, string conteudo)
        {
            GD.Print($"[GerenciadorDeFolhasDeRelatorio] Registrando folha de relatório: {titulo}");
           
        }
    }
}
