using fiveyears3.Scripts.Utilidades;
using Godot;
using System;

public partial class Radio : StaticBody3D, IItemInteracao
{
    [Export] public ItemList ListaNoticiasRadios;
    public void Interagir()
    {
        GD.Print("Interagindo com o rádio");
    }

    public override void _Ready()
    {
        DesfocandoNoRadio();
    }

    public void FocandoNoRadio()
    {
        ListaNoticiasRadios.Visible = true;
    }

    public void DesfocandoNoRadio()
    {
        ListaNoticiasRadios.Visible = false;
    }
}
