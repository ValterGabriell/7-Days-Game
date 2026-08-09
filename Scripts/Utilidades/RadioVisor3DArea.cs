using Godot;

public partial class RadioVisor3DArea : StaticBody3D
{
    [Export] public MeshInstance3D TelaMesh;
    [Export] public SubViewport SubViewportNode;

    private StandardMaterial3D _materialTela;

    public override void _Ready()
    {
        if (TelaMesh == null)
            GD.PushWarning("[RadioVisor3DArea] TelaMesh não configurada.");

        if (SubViewportNode == null)
            GD.PushWarning("[RadioVisor3DArea] SubViewportNode não configurado.");

        if (TelaMesh == null || SubViewportNode == null)
            return;

        _materialTela = new StandardMaterial3D
        {
            AlbedoTexture = SubViewportNode.GetTexture(),
            EmissionEnabled = true,
            EmissionTexture = SubViewportNode.GetTexture(),
            AlbedoColor = Colors.White
        };

        TelaMesh.MaterialOverride = _materialTela;
        SubViewportNode.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
        SubViewportNode.GuiDisableInput = false;
        SubViewportNode.HandleInputLocally = true;

        Log.Print($"[RadioVisor3DArea] Ready | SubViewport Size={SubViewportNode.Size} | HandleInputLocally={SubViewportNode.HandleInputLocally} | GuiDisableInput={SubViewportNode.GuiDisableInput}");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventMouse mouseEvent)
            return;

        Camera3D camera = GetViewport().GetCamera3D();

        if (camera == null)
        {
            return;
        }

        Vector3 origem = camera.ProjectRayOrigin(mouseEvent.Position);
        Vector3 direcao = camera.ProjectRayNormal(mouseEvent.Position);

        var space = GetWorld3D().DirectSpaceState;

        var query = PhysicsRayQueryParameters3D.Create(
            origem,
            origem + direcao * 1000
        );

        var result = space.IntersectRay(query);

        if (result.Count == 0)
        {
            return;
        }

        var collider = result["collider"].AsGodotObject();
        if (collider != this && collider != TelaMesh)
        {
            if (@event is InputEventMouseButton botaoIgnorado && botaoIgnorado.Pressed)
            {
                string nomeCollider = collider is Node nodeCollider ? nodeCollider.Name : "(sem nome)";
                Log.Print($"[RadioVisor3DArea] Clique ignorado: collider={collider?.GetType().Name} nome={nomeCollider}");
            }
            return;
        }

        Vector3 worldPosColisao = (Vector3)result["position"];

        Vector3 normalPlanoTela = TelaMesh.GlobalTransform.Basis.Y.Normalized();
        Plane planoTela = new(normalPlanoTela, TelaMesh.GlobalTransform.Origin);
        Vector3? intersecaoPlano = planoTela.IntersectsRay(origem, direcao);
        if (intersecaoPlano == null)
        {
            return;
        }

        Vector3 worldPosTela = intersecaoPlano.Value;

        Vector3 localPos = TelaMesh.ToLocal(worldPosTela);

        Aabb aabb = TelaMesh.Mesh.GetAabb();

        float u = (localPos.X - aabb.Position.X) / aabb.Size.X;
        float v = (localPos.Z - aabb.Position.Z) / aabb.Size.Z;

        if (u < 0 || u > 1 || v < 0 || v > 1)
        {
            if (@event is InputEventMouseButton botaoForaUv && botaoForaUv.Pressed)
            {
                Log.Print($"[RadioVisor3DArea] Clique fora UV: u={u:0.000}, v={v:0.000}, localPos={localPos}, colisao={worldPosColisao}, tela={worldPosTela}");
            }
            return;
        }

        Vector2 viewportPos = new(
            u * SubViewportNode.Size.X,
            v * SubViewportNode.Size.Y
        );


        if (@event is InputEventMouseButton button)
        {
            var clone = (InputEventMouseButton)button.Duplicate();
            clone.Position = viewportPos;
            clone.GlobalPosition = viewportPos;
            clone.Canceled = false;

            SubViewportNode.PushInput(clone, true);
            Log.Print($"[RadioVisor3DArea] MouseButton {(button.Pressed ? "DOWN" : "UP")} btn={button.ButtonIndex} tela={mouseEvent.Position} uv=({u:0.000},{v:0.000}) viewport={viewportPos} colisao={worldPosColisao} telaPlano={worldPosTela}");

            if (button.Pressed)
            {
                GetViewport().SetInputAsHandled();
            }
        }
        else if (@event is InputEventMouseMotion motion)
        {
            var clone = (InputEventMouseMotion)motion.Duplicate();
            clone.Position = viewportPos;
            clone.GlobalPosition = viewportPos;
            clone.Relative = Vector2.Zero;

            SubViewportNode.PushInput(clone, true);
        }
    }
}