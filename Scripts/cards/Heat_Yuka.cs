using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Powers;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class Heat_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Damage", 3m)];
    public Heat_Yuka() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        foreach (var sunflower in SunflowerManager.GetSunflowers(base.Owner))
        {
            if (!sunflower.IsAlive)
            {
                continue;
            }

            await PowerCmd.Apply<HeatPower>(
                context,
                sunflower,
                DynamicVars["Damage"].BaseValue,
                base.Owner.Creature,
                this,
                false
            );
        }
    }
    protected override void OnUpgrade() => DynamicVars["Damage"].UpgradeValueBy(1m);
}
