using Godot;

public partial class TesteItemList : ItemList
{
    private int _ultimoItemHover = int.MinValue;

    public override void _Ready()
    {
        ItemClicked += OnItemClicked;
        ItemSelected += OnItemSelected;
        FocusEntered += OnFocusEntered;
        FocusExited += OnFocusExited;

        GD.Print($"[TesteItemList] Ready | Size={Size} | GlobalPosition={GlobalPosition} | ItemCount={ItemCount} | MouseFilter={MouseFilter} | FocusMode={FocusMode}");
    }

    public override void _GuiInput(InputEvent @event)
    {
        base._GuiInput(@event);

        if (@event is InputEventMouseMotion motion)
        {
            int itemHover = GetItemAtPosition(motion.Position, true);
            if (itemHover != _ultimoItemHover)
            {
                _ultimoItemHover = itemHover;
                GD.Print($"[TesteItemList] Hover mudou: item={itemHover} pos={motion.Position}");
            }
        }

        if (@event is InputEventMouseButton b && b.Pressed && b.ButtonIndex == MouseButton.Left)
        {
            int itemAtPosExato = GetItemAtPosition(b.Position, true);
            int itemAtPosArea = GetItemAtPosition(b.Position, false);
            Rect2 rectPrimeiroItem = ItemCount > 0 ? GetItemRect(0, true) : new Rect2();
            GD.Print($"[TesteItemList] Clique local={b.Position} itemAtPosExato={itemAtPosExato} itemAtPosArea={itemAtPosArea} selectedItems={GetSelectedItems().Length} itemCount={ItemCount} rectItem0={rectPrimeiroItem}");
        }
    }

    private void OnItemClicked(long index, Vector2 atPosition, long mouseButtonIndex)
    {
        GD.Print($"ITEM CLICADO (Sinal nativo): {index}");
    }

    private void OnItemSelected(long index)
    {
        GD.Print($"ITEM SELECIONADO: {index}");
    }

    private void OnFocusEntered()
    {
        GD.Print("[TesteItemList] FocusEntered");
    }

    private void OnFocusExited()
    {
        GD.Print("[TesteItemList] FocusExited");
    }
}