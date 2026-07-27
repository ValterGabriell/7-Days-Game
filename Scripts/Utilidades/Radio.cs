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
        GD.Print("[a]Focando no rádio");
        ListaNoticiasRadios.Visible = true;
    }

    public void DesfocandoNoRadio()
    {
        GD.Print("Desfocando no rádio");
        ListaNoticiasRadios.Visible = false;
    }
}
