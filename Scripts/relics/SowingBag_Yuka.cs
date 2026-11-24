using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace KazamiYuka.Scripts.Relics;

[RegisterRelic(typeof(EventRelicPool))]
public sealed class SowingBag_Yuka : YukaAncientRelic
{
    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromEnchantment<Sown>(1);

    public override Task AfterObtained()
    {
        Sown enchantment = ModelDb.Enchantment<Sown>();
        foreach (CardModel card in PileType.Deck.GetPile(Owner).Cards
                     .Where(enchantment.CanEnchant).TakeRandom(6, Owner.PlayerRng.Rewards))
            CardCmd.Enchant<Sown>(card, 1m);

        return Task.CompletedTask;
    }
}
