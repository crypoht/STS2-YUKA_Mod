using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using KazamiYuka.Scripts.Keywords;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class TyrantObliteration_Yuka : KazamiYukaCard
{
    private CardPlay? _activeDamagePlay;
    private decimal _activeDamageMultiplier = 1m;

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3m, ValueProp.Move),
        new DynamicVar("MaxConsume", 3m),
        new DynamicVar("SunlightPerDouble", 1m),
        new PowerVar<StrengthPower>(1m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [KazamiYukaKeywords.FierceSun];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(null)
    ];

    public TyrantObliteration_Yuka() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (cardSource != this || Owner == null ||
            dealer != Owner.Creature || !props.IsPoweredAttack())
        {
            return 1m;
        }

        if (_activeDamagePlay != null &&
            (cardPlay == null || cardPlay == _activeDamagePlay))
        {
            return _activeDamageMultiplier;
        }

        if (cardPlay != null)
        {
            return 1m;
        }

        int maxConsume = (int)DynamicVars["MaxConsume"].BaseValue;
        int sunlight = IsSunlightFree
            ? maxConsume
            : Math.Min(SunlightManager.GetSunlight(Owner), maxConsume);
        return GetDamageMultiplier(sunlight);
    }

    private decimal GetDamageMultiplier(int sunlight)
    {
        int doublingCount = sunlight / (int)DynamicVars["SunlightPerDouble"].BaseValue;
        return (decimal)Math.Pow(2, doublingCount);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
        if (player == null) return;
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        int maxConsume = (int)DynamicVars["MaxConsume"].BaseValue;

        int consumedSunlight = base.ConsumeUpToMaxSunlight(player, maxConsume, cardPlay);
        CardPlay? previousPlay = _activeDamagePlay;
        decimal previousMultiplier = _activeDamageMultiplier;
        _activeDamagePlay = cardPlay;
        _activeDamageMultiplier = GetDamageMultiplier(consumedSunlight);
        try
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash", null, null)
                .Execute(choiceContext);
        }
        finally
        {
            _activeDamagePlay = previousPlay;
            _activeDamageMultiplier = previousMultiplier;
        }

        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            player.Creature,
            DynamicVars["StrengthPower"].BaseValue,
            player.Creature,
            this,
            false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
    }
}
