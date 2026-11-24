using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class GreatYoukaiElegance_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(11m, ValueProp.Move),
        new DynamicVar("OldPerSun", 2m)
    ];

    public GreatYoukaiElegance_Yuka() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(
            base.Owner.Creature,
            DynamicVars.Block.BaseValue,
            DynamicVars.Block.Props,
            cardPlay,
            false
        );

        int sunlight = SunlightManager.GetSunlight(base.Owner);
        if (sunlight > 0)
        {
            await PowerCmd.Apply<OldPower>(
                choiceContext,
                base.Owner.Creature,
                sunlight * DynamicVars["OldPerSun"].BaseValue,
                base.Owner.Creature,
                this,
                false
            );
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
        DynamicVars["OldPerSun"].UpgradeValueBy(1m);
    }
}
