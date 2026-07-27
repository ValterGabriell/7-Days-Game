using Godot;
using System;

namespace fiveyears3.Scripts.UI
{
    public partial class MenuConfiguracaoUI : CanvasLayer
    {
        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Always;
            Visible = false;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                if (keyEvent.Keycode == Key.P)
                {
                    AlternarMenu();
                }
            }
        }

        private void AlternarMenu()
        {
            Visible = !Visible;
            GetTree().Paused = Visible;

            if (Visible)
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
            else
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
        }
    }
}