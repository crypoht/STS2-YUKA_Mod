using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.DynamicVars;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class FlowerRage_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;
    protected override IEnumerable<MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar> CanonicalVars => [
        new MegaCrit.Sts2.Core.Localization.DynamicVars.PowerVar<VulnerablePower>(3m),
        new MegaCrit.Sts2.Core.Localization.DynamicVars.PowerVar<ParasitePower>(3m),
        new SunlightCostVar(1m)
    ];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<VulnerablePower>(null),
        HoverTipFactory.FromPower<ParasitePower>(null)
    ];
    public override IEnumerable<MegaCrit.Sts2.Core.Entities.Cards.CardKeyword> CanonicalKeywords => [MegaCrit.Sts2.Core.Entities.Cards.CardKeyword.Exhaust];
    public FlowerRage_Yuka() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies, true) { }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (!TryPayFixedSunlight(base.Owner, DynamicVars["SunlightCost"].IntValue, play))
        {
            return;
        }

        if (CombatState?.HittableEnemies is not { } enemies) return;
        await PowerCmd.Apply<VulnerablePower>(context, enemies, 3m, base.Owner.Creature, this, false);
        await PowerCmd.Apply<ParasitePower>(context, enemies, 3m, base.Owner.Creature, this, false);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
