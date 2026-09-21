using Godot;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Players;

namespace KazamiYuka.Scripts.Nodes;

public sealed partial class NMultiplayerSunlightCounter : Node
{
    private Player? _player;
    private Control? _container;
    private MegaLabel? _label;
    private bool _isListening;

    public void Initialize(Player player, Control container, MegaLabel label)
    {
        _player = player;
        _container = container;
        _label = label;

        if (IsInsideTree())
        {
            StartListening();
        }
    }

    public override void _Ready()
    {
        StartListening();
    }

    public override void _ExitTree()
    {
        if (_isListening)
        {
            SunlightManager.SunlightChangedForPlayer -= OnSunlightChangedForPlayer;
            _isListening = false;
        }

        base._ExitTree();
    }

    private void StartListening()
    {
        if (_isListening || _player == null || _container == null || _label == null)
        {
            return;
        }

        SunlightManager.SunlightChangedForPlayer += OnSunlightChangedForPlayer;
        _isListening = true;
        UpdateDisplay(SunlightManager.GetSunlight(_player));
    }

    private void OnSunlightChangedForPlayer(Player player, int _, int newValue)
    {
        if (player == _player)
        {
            UpdateDisplay(newValue);
        }
    }

    private void UpdateDisplay(int sunlight)
    {
        if (_container == null || _label == null)
        {
            return;
        }

        _container.Visible = sunlight > 0;
        _label.SetTextAutoSize(sunlight.ToString());
    }
}
