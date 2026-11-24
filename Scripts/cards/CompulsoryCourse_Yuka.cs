using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class CompulsoryCourse_Yuka : KazamiYukaCard
{
    private const string KoishiCharacterId = "KOMEIJIKOISHI-KOISHI_CHARACTER";

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(null)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(15m, ValueProp.Move),
        new PowerVar<StrengthPower>(4m)
    ];

    public CompulsoryCourse_Yuka()
        : base(0, CardType.Attack, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        List<Creature> targets = base.CombatState?.Creatures
            .Where(creature =>
                creature != base.Owner.Creature &&
                creature.Player?.Character.Id.Entry != KoishiCharacterId &&
                creature.IsHittable)
            .ToList() ?? [];

        if (targets.Count == 0)
        {
            return;
        }

        IEnumerable<DamageResult> results = await CreatureCmd.Damage(
            choiceContext,
            targets,
            DynamicVars.Damage.BaseValue,
            ValueProp.Move,
            base.Owner.Creature,
            this,
            cardPlay);

        int unblockedHits = results.Count(result => result.UnblockedDamage > 0);
        if (unblockedHits > 0)
        {
            await PowerCmd.Apply<StrengthPower>(
                choiceContext,
                base.Owner.Creature,
                DynamicVars["StrengthPower"].BaseValue * unblockedHits,
                base.Owner.Creature,
                this,
                false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
    }
}
