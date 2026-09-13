using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using KazamiYuka.Scripts.Cards;

namespace KazamiYuka.Scripts;

[RegisterCard(typeof(KazamiYukaCardPool))]
// 👇 注意这里：在 ModCardTemplate 后面加上 , ICustomTargetValidator
public class Photosynthesis_Yuka : KazamiYukaCard, ICustomTargetValidator
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HealVar(5)
    ];

    public Photosynthesis_Yuka() : base(energyCost, type, rarity, KazamiYukaTargetTypes.Sunflower, shouldShowInCardLibrary)
    {
    }

    // 👇 接口要求的方法：在这里写上你专属的过滤逻辑！
    public bool IsValidCustomTarget(Creature target)
    {
        // 只有目标是向日葵时，才亮绿灯！
        return target.Monster is KazamiYukaSunflower;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;

        // 因为前端已经过滤过了，这里直接放心大胆地用
        if (target != null && target.Monster is KazamiYukaSunflower specificFlower)
        {
            await SunflowerManager.HealSingle(choiceContext,specificFlower.Creature, DynamicVars.Heal.BaseValue);
            
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Heal.UpgradeValueBy(3);
    }
}
