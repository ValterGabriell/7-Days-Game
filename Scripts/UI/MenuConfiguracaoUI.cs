using Godot;
using System;

namespace fiveyears3.Scripts.UI
{
    public partial class MenuConfiguracaoUI : CanvasLayer
    {
        private bool _estadoPausaAnterior;
        private Input.MouseModeEnum _modoMouseAnterior = Input.MouseModeEnum.Captured;

        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Always;
            Visible = false;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                if (keyEvent.Keycode == Key.F10)
                {
                    AlternarMenu();
                }
            }
        }

        private void AlternarMenu()
        {
            if (Visible)
            {
                FecharMenu();
            }
            else
            {
                AbrirMenu();
            }
        }

        private void AbrirMenu()
        {
            _estadoPausaAnterior = GetTree().Paused;
            _modoMouseAnterior = Input.MouseMode;

            Visible = true;
            GetTree().Paused = true;
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }

        private void FecharMenu()
        {
            Visible = false;
            GetTree().Paused = _estadoPausaAnterior;
            Input.MouseMode = _modoMouseAnterior;
        }
    }
}