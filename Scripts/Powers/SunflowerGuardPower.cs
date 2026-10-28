using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public class SunflowerGuardPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    
    public override PowerStackType StackType => PowerStackType.Single;

    protected override object InitInternalData() => new Data();

    public override LocString Description
    {
        get
        {
            if (!IsMutable || Owner.CombatState == null)
            {
                return base.Description;
            }

            var players = Owner.CombatState.Players
                .Where(player => player.Character is not KazamiYukaCharacter && Protects(player))
                .Select(player => PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, player.NetId))
                .ToList();
            if (players.Count == 0)
            {
                return base.Description;
            }

            var description = new LocString("powers", Id.Entry + ".descriptionWithOtherPlayers");
            var separator = new LocString("powers", Id.Entry + ".playerSeparator").GetFormattedText();
            description.Add("PlayerNames", string.Join(separator, players));
            return description;
        }
    }

    public bool Protects(Player player)
    {
        return player.Character is KazamiYukaCharacter ||
            Applier?.Player == player ||
            GetInternalData<Data>().ApplierIds.Contains(player.NetId);
    }

    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power == this && amount > 0m && applier?.Player is { } player)
        {
            GetInternalData<Data>().ApplierIds.Add(player.NetId);
        }

        return Task.CompletedTask;
    }

    private sealed class Data
    {
        public HashSet<ulong> ApplierIds { get; } = [];
    }
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/sunflower_guard_small.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/sunflower_guard_large.png"
    );

}
