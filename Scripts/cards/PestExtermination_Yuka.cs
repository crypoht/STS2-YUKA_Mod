using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class PestExtermination_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3m, ValueProp.Move),
        new DynamicVar("HitCount", 3m),
        new DynamicVar("HitsPerGrowth", 2m),
        new DynamicVar("MaxHpGain", 1m)
    ];

    public PestExtermination_Yuka()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (base.CombatState == null)
        {
            return;
        }

        int hitCount = DynamicVars["HitCount"].IntValue;
        int hitsPerGrowth = DynamicVars["HitsPerGrowth"].IntValue;
        int completedHits = 0;

        for (int hit = 0; hit < hitCount; hit++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .TargetingAllOpponents(base.CombatState)
                .Execute(choiceContext);

            completedHits++;
            if (completedHits % hitsPerGrowth == 0)
            {
                await SunflowerManager.GainMaxHpAll(
                    choiceContext,
                    base.Owner,
                    DynamicVars["MaxHpGain"].BaseValue
                );
            }
        }
    }

    protected override void OnUpgrade() => DynamicVars["HitCount"].UpgradeValueBy(1m);
}
