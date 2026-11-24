using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class FlowerGuardian_Yuka : KazamiYukaCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(1m, ValueProp.Move)
    ];

    public FlowerGuardian_Yuka()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.AllAllies, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var rewards = Owner.Creature.CombatState?.Players
            .Where(player => player != Owner && player.Creature.IsAlive)
            .Select(player => (Player: player, Sunlight: SunlightManager.GetSunlight(player)))
            .ToList();
        if (rewards == null)
        {
            return;
        }

        foreach (var reward in rewards)
        {
            if (reward.Sunlight <= 0 || !reward.Player.Creature.IsAlive)
            {
                continue;
            }

            await CreatureCmd.GainBlock(
                reward.Player.Creature,
                reward.Sunlight * DynamicVars.Block.BaseValue,
                DynamicVars.Block.Props,
                cardPlay,
                false);
        }
    }

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
