using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class UmbrellaTechnique_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [SunflowerDamageVar(3m)];

    public UmbrellaTechnique_Yuka() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner;
        if (player == null || cardPlay.Target == null) return;

        Creature? target = cardPlay.Target;
        foreach (Creature sunflower in SunflowerManager.GetSunflowers(player).Where(flower => flower.IsAlive))
        {
            target = GetSunflowerDamageTarget(target);
            if (target == null)
            {
                break;
            }

            await CreatureCmd.Damage(
                choiceContext,
                target,
                DynamicVars["SunflowerDamage"].BaseValue,
                ValueProp.Move,
                sunflower,
                this,
                cardPlay
            );
        }

        CardPile discardPile = PileType.Discard.GetPile(player);
        if (discardPile.Cards.Count <= 0)
        {
            return;
        }

        IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromCombatPile(
            choiceContext,
            discardPile,
            player,
            new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1)
        );
        CardModel? selectedCard = selectedCards.FirstOrDefault();
        if (selectedCard != null)
        {
            await CardPileCmd.Add(selectedCard, PileType.Hand, CardPilePosition.Bottom, null, false);
        }
    }

    protected override void OnUpgrade() => DynamicVars["SunflowerDamage"].UpgradeValueBy(1m);
}
