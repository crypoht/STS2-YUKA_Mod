using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Rooms;
using KazamiYuka.Scripts.Powers;
using Godot;

namespace KazamiYuka.Scripts.Relics;

[RegisterRelic(typeof(KazamiYukaRelicPool))]
public class OldSleepingCap_Yuka : ModRelicTemplate
{
    public override bool ShowCounter => true;

    public override int DisplayAmount =>
        SunflowerManager.GetRelicXiangYangApplicationsRemaining(base.Owner);

    public void RefreshSunflowerCounter()
    {
        InvokeDisplayAmountChanged();
    }

    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://mods/KazamiYuka/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"res://mods/KazamiYuka/images/relics/{GetType().Name}.png",
        BigIconPath: $"res://mods/KazamiYuka/images/relics/{GetType().Name}.png"
    );

    private int _sunlightGainedThisTurn = 0;
    private bool _isSubscribed = false;

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom)
        {
            base.Flash();
            
            if (base.Owner != null && SunlightManager.IsYukaPlayer(base.Owner))
            {
                SunlightManager.GainSunlight(base.Owner, 5);
                
                _sunlightGainedThisTurn = 0;
                
                if (!_isSubscribed)
                {
                    SunlightManager.SunlightGained += OnSunlightGained;
                    _isSubscribed = true;
                }
            }
        }
    }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(base.Owner.Creature) || base.Owner == null)
        {
            return;
        }

        var player = base.Owner as Player;
        if (player == null || !SunlightManager.IsYukaPlayer(player))
        {
            return;
        }

        _sunlightGainedThisTurn = 0;
    }

    public override async Task BeforeSideTurnEndEarly(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(base.Owner.Creature) || base.Owner == null)
        {
            return;
        }

        var player = base.Owner as Player;
        if (player == null || !SunlightManager.IsYukaPlayer(player))
        {
            return;
        }

        if (_sunlightGainedThisTurn <= 0)
        {
            return;
        }

        var sunflowers = SunflowerManager.GetSunflowers(player);
        if (sunflowers.Count > 0)
        {
            base.Flash();
            
            foreach (var sunflower in sunflowers)
            {
                if (sunflower.IsAlive)
                {
                    await CreatureCmd.Heal(sunflower, _sunlightGainedThisTurn, true);
                }
            }
        }
    }

    private void OnSunlightGained(Player player, int amount)
    {
        if (player == base.Owner && base.Owner != null && SunlightManager.IsYukaPlayer(player))
        {
            _sunlightGainedThisTurn += amount;
        }
    }
}
