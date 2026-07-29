using fiveyears3.Scripts.Utilidades;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Personagens.Principal;

//personagem principal iteracao
public partial class PersonagemPrincipal : CharacterBody3D
{
    [ExportCategory("Configuracao de Iteração")]
    [Export] public RayCast3D RaycastDeIteracao;
    
    public void TentouInteragirComAlgoIterativo(InputEvent @event)
    {
        if (@event.IsActionPressed("interagir") && RaycastDeIteracao.IsColliding())
        {
            var objetoColidido = RaycastDeIteracao.GetCollider();
            if (objetoColidido is IItemInteracao itemInteracao)
            {
                itemInteracao.Interagir();
            }
        }
    }
}
