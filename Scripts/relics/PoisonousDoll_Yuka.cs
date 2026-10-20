using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Relics;

[RegisterRelic(typeof(KazamiYukaRelicPool))]
public sealed class PoisonousDoll_Yuka : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/relics/PoisonousDoll_Yuka.png",
        IconOutlinePath: "res://mods/KazamiYuka/images/relics/PoisonousDoll_Yuka.png",
        BigIconPath: "res://mods/KazamiYuka/images/relics/PoisonousDoll_Yuka.png"
    );

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        return GetBonus(dealer);
    }

    private decimal GetBonus(Creature? dealer)
    {
        return dealer?.Monster is KazamiYukaSunflower && base.Owner != null ? 4m : 0m;
    }
}
