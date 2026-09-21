using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Combat;

namespace KazamiYuka.Scripts.Nodes;
[ScriptPath("res://Scripts/Nodes/NSunlightCounter.cs")]
public partial class NSunlightCounter : Control
{
	private RichTextLabel _label = null!;
	private MegaCrit.Sts2.Core.Entities.Players.Player? _localPlayer;
	private bool _initialized = false;
	private MegaCrit.Sts2.Core.Entities.Players.Player? _boundPlayer;

	public override void _Ready()
	{
		_label = GetNode<RichTextLabel>("%CountLabel");
		GD.Print("[NSunlightCounter] _Ready called, label found: " + (_label != null));
		
		if (_boundPlayer != null)
		{
			InitializePlayer(_boundPlayer);
			return;
		}

		CallDeferred(nameof(DelayedInitialize));
	}

	public void Initialize(MegaCrit.Sts2.Core.Entities.Players.Player player)
	{
		_boundPlayer = player;
		if (_label != null)
		{
			InitializePlayer(player);
		}
	}

	private async void DelayedInitialize()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		
		if (_boundPlayer != null)
		{
			InitializePlayer(_boundPlayer);
		}
		else
		{
			InitializePlayer();
		}
	}

	private void InitializePlayer()
	{
		if (_initialized) return;
		
		var runManager = MegaCrit.Sts2.Core.Runs.RunManager.Instance;
		var runState = runManager?.DebugOnlyGetState();
		
		if (runState == null)
		{
			GD.PrintErr("[NSunlightCounter] RunState is null");
			return;
		}
		
		_localPlayer = MegaCrit.Sts2.Core.Context.LocalContext.GetMe(runState);

		if (_localPlayer == null)
		{
			GD.PrintErr("[NSunlightCounter] Failed to get local player");
			return;
		}
		
		InitializePlayer(_localPlayer);
	}

	private void InitializePlayer(MegaCrit.Sts2.Core.Entities.Players.Player player)
	{
		if (_initialized)
		{
			return;
		}

		_localPlayer = player;
		SunlightManager.SunlightChangedForPlayer += OnSunlightChangedForPlayer;

		int sunlight = SunlightManager.GetSunlight(player);
		GD.Print("[NSunlightCounter] Player initialized successfully: " + player.NetId);
		UpdateText(sunlight);
		_initialized = true;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		if (_localPlayer != null)
		{
			SunlightManager.SunlightChangedForPlayer -= OnSunlightChangedForPlayer;
		}
	}

	private void OnSunlightChangedForPlayer(MegaCrit.Sts2.Core.Entities.Players.Player player, int oldVal, int newVal)
	{
		if (_localPlayer != null && player == _localPlayer)
		{
			GD.Print($"[NSunlightCounter] Sunlight changed: {oldVal} -> {newVal}");
			UpdateText(newVal);
		}
	}

	private void UpdateText(int currentSunlight)
	{
		Visible = currentSunlight > 0;

		if (_label != null)
		{
			string color = "yellow";
			_label.Text = $"[center][color={color}]{currentSunlight}[/color][/center]";
			GD.Print($"[NSunlightCounter] Updated text to: {currentSunlight} (color: {color})");
		}
		else
		{
			GD.PrintErr("[NSunlightCounter] Label is null, cannot update text");
		}
	}
}
