using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers; 
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class InfiniteSpiralPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/InfiniteSpiralPower_small.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/InfiniteSpiralPower.png"
    );

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 👇 修复 1 (CS0019)：将卡牌持有者的 Creature 提取出来，与能力的 Owner(也是 Creature) 进行比对
        if (cardPlay.Card.Owner.Creature == base.Owner)
        {
            var sunflowers = SunflowerManager.GetSunflowers(base.Owner.Player!);
            if (sunflowers.Count == 0) return;

            var lowestHpEnemy = base.CombatState.Enemies
                .Where(e => e.IsAlive)
                .OrderBy(e => e.CurrentHp)
                .FirstOrDefault();

            if (lowestHpEnemy != null)
            {
                foreach (var sf in sunflowers)
                {
                    if (sf.IsAlive)
                    {
                        decimal damage = sf.MaxHp;
                        await CreatureCmd.Damage(choiceContext, lowestHpEnemy, damage, ValueProp.Unpowered, sf);
                    }
                }
            }
        }
    }

    public override async Task BeforeSideTurnEndEarly(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == base.Owner.Side)
        {
            // 👇 修复 2 (CS1061)：在 Power 里，base.Owner 本身就是 Creature 类型，不需要再加 .Creature
            if (base.Owner.IsAlive)
            {
                // 把玩家的 Creature 对象包装成列表传给灾厄
                List<Creature> doomedPlayer = new List<Creature> { base.Owner };
                
                await DoomPower.DoomKill(doomedPlayer);
            }
        }
    }
}
