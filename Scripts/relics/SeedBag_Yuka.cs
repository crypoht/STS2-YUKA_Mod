using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Relics;

[RegisterRelic(typeof(KazamiYukaRelicPool))]
public sealed class SeedBag_Yuka : ModRelicTemplate
{
    private int _charges = 1;

    public override bool ShowCounter => true;

    public override int DisplayAmount => _charges;

    public override RelicRarity Rarity => RelicRarity.Common;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/relics/SeedBag_Yuka_outline.png",
        IconOutlinePath: "res://mods/KazamiYuka/images/relics/SeedBag_Yuka.png",
        BigIconPath: "res://mods/KazamiYuka/images/relics/SeedBag_Yuka.png"
    );

    public override Task BeforeCombatStart()
    {
        _charges = 1;
        InvokeDisplayAmountChanged();
        return base.BeforeCombatStart();
    }

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (_charges <= 0 ||
            base.Owner == null ||
            side != base.Owner.Creature.Side ||
            !participants.Contains(base.Owner.Creature))
        {
            return;
        }

        _charges = 0;
        InvokeDisplayAmountChanged();
        base.Flash();
        await SunflowerManager.Summon(base.Owner, 4m);
    }
}
