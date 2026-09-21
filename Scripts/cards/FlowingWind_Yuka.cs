using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using KazamiYuka.Scripts.DynamicVars;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class FlowingWind_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4m, ValueProp.Move),
        new SunlightCostVar(1m)
        ];
    public FlowingWind_Yuka() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true) { }

    public static async Task OnAnySunflowerHealthChanged(
        PlayerChoiceContext context,
        Creature sunflower,
        FlowerHealthEvent eventType,
        decimal amount)
    {
        if (eventType != FlowerHealthEvent.MaxHpIncrease ||
            sunflower.PetOwner == null)
        {
            return;
        }

        Player owner = sunflower.PetOwner;
        List<CardModel> cards = new();
        cards.AddRange(PileType.Draw.GetPile(owner).Cards);
        cards.AddRange(PileType.Discard.GetPile(owner).Cards);
        cards.AddRange(PileType.Play.GetPile(owner).Cards);

        foreach (CardModel card in cards)
        {
            if (card is not FlowingWind_Yuka flowingWind ||
                flowingWind.Owner != owner ||
                flowingWind.Pile == null ||
                flowingWind.Pile.Type is PileType.Hand or PileType.Exhaust)
            {
                continue;
            }

            await CardPileCmd.Add(flowingWind, PileType.Hand, CardPilePosition.Bottom, null, false);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Target != null)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, play).Targeting(play.Target).Execute(context);
        }
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}
