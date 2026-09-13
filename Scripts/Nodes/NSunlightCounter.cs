using Godot;
using MegaCrit.Sts2.addons.mega_text;

namespace KazamiYuka.Scripts.Nodes;
[ScriptPath("res://Scripts/Nodes/NSunlightCounter.cs")]
public partial class NSunlightCounter : Control
{
	private RichTextLabel _label = null!;

	public override void _Ready()
	{
		_label = GetNode<RichTextLabel>("%CountLabel");
		SunlightManager.SunlightChanged += OnSunlightChanged;
		
		UpdateText(SunlightManager.CurrentSunlight);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		SunlightManager.SunlightChanged -= OnSunlightChanged;
	}

	private void OnSunlightChanged(int oldVal, int newVal)
	{
		UpdateText(newVal);
	}

	private void UpdateText(int currentSunlight)
	{
		if (_label != null)
		{
			string color = currentSunlight == 0 ? "red" : "yellow";
			_label.Text = $"[center][color={color}]{currentSunlight}[/color][/center]";
		}
		
		this.Visible = currentSunlight > 0;
	}
}
