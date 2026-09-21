using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using KazamiYuka.Scripts.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class SunlightAggregation_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(22m, ValueProp.Move), new SunlightCostVar(6m)];
    public SunlightAggregation_Yuka() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies, true) { }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (!TryPayFixedSunlight(base.Owner, DynamicVars["SunlightCost"].IntValue, play) || CombatState == null) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, play).TargetingAllOpponents(CombatState).Execute(context);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(6m);
}
