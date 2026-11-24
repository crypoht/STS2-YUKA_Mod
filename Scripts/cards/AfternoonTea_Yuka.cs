using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class AfternoonTea_Yuka : KazamiYukaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("SunlightPerHeal", 3m),
        new DynamicVar("HealingPerThreshold", 1m)
    ];

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    public override CardMultiplayerConstraint MultiplayerConstraint
        => CardMultiplayerConstraint.MultiplayerOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public AfternoonTea_Yuka() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player owner = base.Owner;

        await CreatureCmd.TriggerAnim(owner.Creature, "Cast", owner.Character.CastAnimDelay);

        foreach (Player otherPlayer in owner.Creature.CombatState?.Players.Where(player => player != owner) ?? Enumerable.Empty<Player>())
        {
            int sunlight = SunlightManager.GetSunlight(otherPlayer);
            SunlightManager.SpendAllSunlight(otherPlayer);

            int healing = sunlight / DynamicVars["SunlightPerHeal"].IntValue
                * DynamicVars["HealingPerThreshold"].IntValue;
            if (healing > 0)
            {
                await CreatureCmd.Heal(otherPlayer.Creature, healing, true);
            }
        }
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
