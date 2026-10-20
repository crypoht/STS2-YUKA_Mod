using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class FourSeasonsBloomPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("SpringAmount", 2m),
        new DynamicVar("SummerAmount", 4m),
        new DynamicVar("AutumnAmount", 1m),
        new DynamicVar("WinterAmount", 4m)
    ];

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/FourSeasonsBloomPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/FourSeasonsBloomPower.png"
    );

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player != base.Owner.Player || !base.Owner.IsAlive || base.Amount <= 0m)
        {
            return;
        }

        foreach (Creature sunflower in SunflowerManager.GetAllCombatSunflowers(base.Owner)
                     .Where(flower => flower.IsAlive)
                     .ToList())
        {
            switch (player.RunState.Rng.CombatCardGeneration.NextInt(4))
            {
                case 0:
                    await PowerCmd.Apply<SpringBloomPower>(
                        choiceContext,
                        sunflower,
                        DynamicVars["SpringAmount"].BaseValue,
                        base.Owner,
                        null,
                        false
                    );
                    break;
                case 1:
                    await PowerCmd.Apply<SummerBloomPower>(
                        choiceContext,
                        sunflower,
                        DynamicVars["SummerAmount"].BaseValue,
                        base.Owner,
                        null,
                        false
                    );
                    break;
                case 2:
                    await PowerCmd.Apply<AutumnBloomPower>(
                        choiceContext,
                        sunflower,
                        DynamicVars["AutumnAmount"].BaseValue,
                        base.Owner,
                        null,
                        false
                    );
                    break;
                default:
                    await PowerCmd.Apply<WinterBloomPower>(
                        choiceContext,
                        sunflower,
                        DynamicVars["WinterAmount"].BaseValue,
                        base.Owner,
                        null,
                        false
                    );
                    break;
            }
        }
    }
}
