using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class CompressionTechniquePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/CompressionTechniquePower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/CompressionTechniquePower.png"
    );

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != base.Owner.Player)
        {
            return;
        }

        int count = (int)base.Amount;
        var candidates = PileType.Hand.GetPile(player).Cards
            .Where(card =>
                card.Type != CardType.Attack &&
                !SunflowerCardTags.IsSunflowerCard(card) &&
                !SqueezeManager.IsSqueezed(card))
            .ToList();

        foreach (CardModel card in candidates
            .OrderBy(_ => player.RunState.Rng.CombatCardSelection.NextInt(0, int.MaxValue))
            .Take(count))
        {
            card.RemoveKeyword(CardKeyword.Unplayable);
            CardCmd.ApplyKeyword(card, KazamiYukaKeywords.Squeeze);
        }

        base.Flash();
        await Task.CompletedTask;
    }
}
