using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class FlowerGratitudePower : ModPowerTemplate
{
    private const decimal TriggerCount = 4m;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/FlowerGratitudePower_small.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/FlowerGratitudePower.png"
    );

    public async Task RegisterSunflowerHealthChange(PlayerChoiceContext choiceContext)
    {
        if (base.Owner.Player == null || base.Amount <= 0m)
        {
            return;
        }

        if (base.Amount > 1m)
        {
            await PowerCmd.Apply<FlowerGratitudePower>(
                choiceContext ?? new ThrowingPlayerChoiceContext(),
                base.Owner,
                -1m,
                base.Owner,
                null,
                false
            );
            return;
        }

        base.Flash();
        PlayerChoiceContext drawContext = choiceContext ?? new ThrowingPlayerChoiceContext();
        await CardPileCmd.Draw(drawContext, 1m, base.Owner.Player, false);
        await PowerCmd.Apply<FlowerGratitudePower>(
            drawContext,
            base.Owner,
            TriggerCount - base.Amount,
            base.Owner,
            null,
            false
        );
    }

    public static async Task OnAnySunflowerHealthChanged(PlayerChoiceContext ctx, Creature sunflower, FlowerHealthEvent eventType, decimal amount)
    {
        if (eventType != FlowerHealthEvent.Heal && eventType != FlowerHealthEvent.MaxHpIncrease)
        {
            return;
        }

        foreach (var player in SunflowerManager.GetCombatPlayers(sunflower))
        {
            var power = player.Creature.GetPower<FlowerGratitudePower>();
            if (power != null)
            {
                await power.RegisterSunflowerHealthChange(ctx);
            }
        }
    }
}
