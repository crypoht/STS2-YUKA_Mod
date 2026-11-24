using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using KazamiYuka.Scripts.Keywords;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class OldPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/OldPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/OldPower.png"
    );

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (base.Owner.Player == null ||
            cardSource?.Owner != base.Owner.Player ||
            !AppliesTo(cardSource))
        {
            return 1m;
        }

        return 1m + base.Amount / 100m;
    }

    internal static bool AppliesTo(CardModel card) =>
        card.Type == CardType.Attack &&
        (card.DynamicVars.ContainsKey("SunlightCost") ||
        card.DynamicVars.ContainsKey("SunlightY") ||
        card.Keywords.Contains(KazamiYukaKeywords.FierceSun) ||
        card.Keywords.Contains(KazamiYukaKeywords.FierceSunInfinite));
}
