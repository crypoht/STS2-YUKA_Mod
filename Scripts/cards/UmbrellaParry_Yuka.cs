using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.ValueProps;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class UmbrellaParry_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(9, ValueProp.Move),
        new DynamicVar("SunlightGain", 2m)
    ];


    public UmbrellaParry_Yuka() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int blockAmount = (int)DynamicVars.Block.BaseValue;
        int sunAmount = (int)DynamicVars["SunlightGain"].BaseValue;

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay, false);

        SunlightManager.GainSunlight(base.Owner, sunAmount);
    }

    protected override void OnUpgrade()
    {

        DynamicVars["SunlightGain"].UpgradeValueBy(1m);
        DynamicVars.Block.UpgradeValueBy(4);
    }
}
