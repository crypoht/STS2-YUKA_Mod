using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.Ui.ExtraCornerAmountLabels;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class VioletPower : ModPowerTemplate, IPowerExtraIconAmountLabelsProvider
{
    private const int TriggerInterval = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Turns", TriggerInterval),
        new PowerVar<StrengthPower>(1m)
    ];

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    protected override object InitInternalData() => new Data();

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/VioletPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/VioletPower.png"
    );

    public IReadOnlyList<ExtraIconAmountLabelSlot> GetPowerExtraIconAmountLabelSlots()
    {
        return [
            ExtraIconAmountLabelSlot.At(
                ExtraIconAmountLabelCorner.BottomRight,
                GetInternalData<Data>().TurnsRemaining.ToString(),
                Colors.White,
                Colors.Black)
        ];
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        return dealer == base.Owner && base.Owner.Monster is KazamiYukaSunflower
            ? 0m
            : 1m;
    }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != base.Owner.Side || !base.Owner.IsAlive || base.Owner.PetOwner == null)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        data.TurnsRemaining--;
        if (data.TurnsRemaining > 0)
        {
            base.InvokeDisplayAmountChanged();
            return;
        }

        List<Creature> livingEnemies = combatState.Enemies.Where(enemy => enemy.IsAlive).ToList();
        if (livingEnemies.Count > 0)
        {
            Creature? target = base.Owner.PetOwner.RunState.Rng.CombatCardGeneration.NextItem(livingEnemies);
            if (target != null)
            {
                base.Flash();
                await PowerCmd.Apply<StrengthPower>(
                    new ThrowingPlayerChoiceContext(),
                    target,
                    -DynamicVars["StrengthPower"].BaseValue,
                    base.Owner,
                    null,
                    false);
            }
        }

        data.TurnsRemaining = TriggerInterval;
        base.InvokeDisplayAmountChanged();
    }

    private sealed class Data
    {
        public int TurnsRemaining = TriggerInterval;
    }
}
