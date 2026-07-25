using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Personagens.Principal;

public partial class PersonagemPrincipal : CharacterBody3D
{
    [ExportCategory("Configurações da Câmera")]
    [Export] public Node3D CameraPivot;         
    [Export] public float SensibilidadeDoMouse = 0.002f;
    [Export] public float SensibilidadeDoControle = 0.03f;
    private float _cameraRotationX = 0.0f;

    private void MoveCameraComMouse(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            RotacionarCamera(mouseMotion.Relative.X * SensibilidadeDoMouse, mouseMotion.Relative.Y * SensibilidadeDoMouse);
        }
    }

    private void MoveCameraComControle()
    {
        Vector2 inputDir = Input.GetVector("camera_left", "camera_right", "camera_up", "camera_down");
        if (inputDir != Vector2.Zero)
        {
            RotacionarCamera(inputDir.X * SensibilidadeDoControle, inputDir.Y * SensibilidadeDoControle);
        }
    }

    private void RotacionarCamera(float deltaX, float deltaY)
    {
        RotateY(-deltaX);
        _cameraRotationX -= deltaY;
        _cameraRotationX = Mathf.Clamp(_cameraRotationX, Mathf.DegToRad(-89.0f), Mathf.DegToRad(89.0f));

        if (CameraPivot != null)
        {
            Vector3 camRot = CameraPivot.Rotation;
            camRot.X = _cameraRotationX;
            CameraPivot.Rotation = camRot;
        }
    }
}
