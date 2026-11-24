using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using KazamiYuka.Scripts.Cards;
using MegaCrit.Sts2.Core.Models;
namespace KazamiYuka.Scripts.DynamicVars;

public class SunlightCostVar : DynamicVar
{
    public SunlightCostVar(decimal cost) : base("SunlightCost", cost)
    {
    }

    public int GetRealTimeDisplayCost()
    {
        if (_owner is KazamiYukaCard card && KazamiYukaCard.IsEnergyCostTemporarilyFree(card))
        {
            return 0;
        }

        return (int)this.BaseValue;
    }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        base.PreviewValue = GetRealTimeDisplayCost();
    }

    public override string ToString() => GetRealTimeDisplayCost().ToString();
}
