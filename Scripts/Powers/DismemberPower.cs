using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using KazamiYuka.Scripts.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Combat.Ui.ExtraCornerAmountLabels;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class DismemberPower : ModPowerTemplate, IPowerExtraIconAmountLabelsProvider, ITemporaryPower
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/DismemberPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/DismemberPower.png");

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.None;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public AbstractModel OriginModel => ModelDb.Card<Dismember_Yuka>();
    public PowerModel InternallyAppliedPower => ModelDb.Power<StrengthPower>();

    protected override object InitInternalData() => new Data();

    public IReadOnlyList<ExtraIconAmountLabelSlot> GetPowerExtraIconAmountLabelSlots()
    {
        return [
            ExtraIconAmountLabelSlot.At(
                ExtraIconAmountLabelCorner.BottomRight,
                GetRemainingTurns().ToString(),
                Colors.White,
                Colors.Black)
        ];
    }

    public override async Task BeforeApplied(
        Creature target,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        Data data = base.GetInternalData<Data>();
        data.RemainingTurns = cardSource is Dismember_Yuka dismemberCard
            ? (int)dismemberCard.DynamicVars["Duration"].BaseValue
            : 3;

        await PowerCmd.Apply<StrengthPower>(
            new ThrowingPlayerChoiceContext(),
            target,
            -amount,
            applier,
            cardSource,
            true);
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power == this && amount != base.Amount)
        {
            await PowerCmd.Apply<StrengthPower>(
                choiceContext,
                base.Owner,
                -amount,
                applier,
                cardSource,
                true);
        }
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(base.Owner))
        {
            return;
        }

        Data data = base.GetInternalData<Data>();
        data.RemainingTurns--;
        if (data.RemainingTurns > 0)
        {
            base.InvokeDisplayAmountChanged();
            return;
        }

        base.Flash();
        await PowerCmd.Remove(this);
        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            base.Owner,
            base.Amount,
            base.Owner,
            null,
            false);
    }

    private int GetRemainingTurns() => base.GetInternalData<Data>().RemainingTurns;

    private sealed class Data
    {
        public int RemainingTurns = 3;
    }

}
