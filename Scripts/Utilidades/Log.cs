using Godot;
using System;

public static class Log
{
    public static void Print(string message)
    {
        GD.Print($"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    public static void PrintErr(string message)
    {
        GD.PrintErr($"[{DateTime.Now:HH:mm:ss}] {message}");
    }
}