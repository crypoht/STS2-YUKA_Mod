using Godot;
using System.Threading.Tasks;

namespace KazamiYuka.Scripts.Nodes;

/// <summary>
/// 向日葵召唤位置选择器。
/// 在战斗中显示一个跟随鼠标的光标，让玩家选择召唤位置。
/// </summary>
public partial class SunflowerPositionPicker : Node2D
{
    private TaskCompletionSource<Vector2?> _tcs = new();
    private Sprite2D _cursor = null!;
    private Label _hintLabel = null!;

    // 有效范围（相对于玩家位置的偏移）
    public Vector2 MinOffset { get; set; } = new(-150f, -350f);
    public Vector2 MaxOffset { get; set; } = new(350f, 100f);
    public Vector2 PlayerPosition { get; set; }

    public override void _Ready()
    {
        ZIndex = 100;
        ZAsRelative = false;

        // 光标：用现有的向日葵图片，半透明缩小
        _cursor = new Sprite2D();
        _cursor.Texture = GD.Load<Texture2D>("res://mods/KazamiYuka/images/SunFlower.png");
        _cursor.Scale = new Vector2(0.4f, 0.4f);
        AddChild(_cursor);

        // 提示文字
        _hintLabel = new Label();
        _hintLabel.Text = "[左键]确认  [右键]取消";
        _hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _hintLabel.Position = new Vector2(-80, -50);
        _hintLabel.AddThemeFontSizeOverride("font_size", 14);
        AddChild(_hintLabel);
    }

    public override void _Process(double delta)
    {
        Vector2 mousePos = GetGlobalMousePosition();
        _cursor.GlobalPosition = mousePos;
        _hintLabel.GlobalPosition = mousePos + new Vector2(-80, -50);

        // 根据是否在有效范围内变色
        Vector2 offset = mousePos - PlayerPosition;
        bool inRange = offset.X >= MinOffset.X && offset.X <= MaxOffset.X
                    && offset.Y >= MinOffset.Y && offset.Y <= MaxOffset.Y;

        _cursor.Modulate = inRange
            ? new Color(0.5f, 1f, 0.5f, 0.8f)   // 绿色：范围内
            : new Color(1f, 0.3f, 0.3f, 0.8f);  // 红色：超出范围
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.IsPressed())
        {
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                // 左键确认：将鼠标位置限制在有效范围内
                Vector2 pos = GetGlobalMousePosition();
                Vector2 offset = pos - PlayerPosition;
                offset.X = Mathf.Clamp(offset.X, MinOffset.X, MaxOffset.X);
                offset.Y = Mathf.Clamp(offset.Y, MinOffset.Y, MaxOffset.Y);
                _tcs.TrySetResult(PlayerPosition + offset);
                GetViewport().SetInputAsHandled();
            }
            else if (mouseButton.ButtonIndex == MouseButton.Right)
            {
                // 右键取消
                _tcs.TrySetResult(null);
                GetViewport().SetInputAsHandled();
            }
        }
    }

    /// <summary>
    /// 异步等待玩家选择位置。返回 null 表示取消。
    /// </summary>
    public Task<Vector2?> WaitForPosition() => _tcs.Task;
}
