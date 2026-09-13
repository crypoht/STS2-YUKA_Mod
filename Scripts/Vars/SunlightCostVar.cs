using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using KazamiYuka.Scripts.Cards;

namespace KazamiYuka.Scripts.DynamicVars;

public class SunlightCostVar : DynamicVar
{
    public SunlightCostVar(decimal cost) : base("SunlightCost", cost)
    {
    }

    // 👇 终极核武器：实时获取当前应该显示的阳光数
    public int GetRealTimeDisplayCost()
    {
        // _owner 是官方底层的隐藏字段，代表带着这个变量的卡牌
        if (this._owner is KazamiYukaCard yukaCard)
        {
            // 顺着网线直接问卡牌：你现在免费吗？
            if (yukaCard.IsSunlightFree)
            {
                return 0; // 只要免费，绝对返回 0！
            }
        }
        // 如果不免费，老老实实返回原本的阳光消耗
        return (int)this.BaseValue;
    }

    // 保底：让官方预览也同步这个实时数字
    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        base.PreviewValue = GetRealTimeDisplayCost();
    }

    // 彻底堵死官方转换后门
    protected override decimal GetBaseValueForIConvertible() => GetRealTimeDisplayCost();
    public override string ToString() => GetRealTimeDisplayCost().ToString();
}