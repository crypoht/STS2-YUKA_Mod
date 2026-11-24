using KazamiYuka.Scripts.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace KazamiYuka.Scripts.Relics;

[RegisterRelic(typeof(EventRelicPool))]
public sealed class DaisyRelic_Yuka : YukaAncientRelic
{
    public override bool HasUponPickupEffect => true;
    protected override bool IncludeEnergyHoverTip => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        HoverTipFactory.FromCardWithCardHoverTips<Daisy_Yuka>();

    public override async Task AfterObtained()
    {
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(
            Owner.RunState.CreateCard<Daisy_Yuka>(Owner), PileType.Deck));
    }

    public override decimal ModifyMaxEnergy(Player player, decimal amount) =>
        player == Owner ? amount + 1m : amount;
}
