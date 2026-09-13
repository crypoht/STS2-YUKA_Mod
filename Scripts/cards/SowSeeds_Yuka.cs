using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Cards;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts;

[RegisterCard(typeof(KazamiYukaCardPool))]
[RegisterCharacterStarterCard(typeof(KazamiYukaCharacter), 1)]
public class SowSeeds_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    // 👇 修改为给予4点生命值
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HealVar(4)
    ];

    public SowSeeds_Yuka() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self, true)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ModKeywordRegistry.CreateHoverTip(KazamiYukaKeywords.Grow)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SunflowerManager.Summon(Owner, DynamicVars.Heal.BaseValue);
    }

    // 👇 升级逻辑：基础费用 -1（也就是从 1费 变成 0费）
    protected override void OnUpgrade() => base.EnergyCost.UpgradeBy(-1);
}
