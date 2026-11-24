using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Cards;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public class CompressionTechniquePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected virtual bool UpgradesFertilizer => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/CompressionTechniquePower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/CompressionTechniquePower.png"
    );

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != base.Owner.Player || !Owner.IsAlive || Amount <= 0m ||
            !PileType.Hand.GetPile(player).Cards.Any(card => card.IsTransformable))
        {
            return;
        }

        var selectedCards = await CardSelectCmd.FromHand(
            choiceContext,
            player,
            new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, (int)Amount),
            card => card.IsTransformable,
            this);

        foreach (CardModel card in selectedCards.ToList())
        {
            CardModel fertilizer = CombatState!.CreateCard<Fertilizer_Yuka>(player);
            if (UpgradesFertilizer)
            {
                CardCmd.Upgrade(fertilizer, CardPreviewStyle.HorizontalLayout);
            }

            await CardCmd.Transform(card, fertilizer, CardPreviewStyle.HorizontalLayout);
        }

        base.Flash();
    }
}

[RegisterPower]
public sealed class CompressionTechniqueUpgradedPower : CompressionTechniquePower
{
    protected override bool UpgradesFertilizer => true;
}
