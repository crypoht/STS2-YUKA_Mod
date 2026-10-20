using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class OldMagicCannon_Yuka : KazamiYukaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(13m, ValueProp.Move)
    ];

    public OldMagicCannon_Yuka() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies, true)
    {
    }

    public override int MaxUpgradeLevel => 514114514;

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (base.CombatState == null)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(base.CombatState)
            .Execute(context);
    }

    protected override void OnUpgrade()
    {
        decimal multiplier = CurrentUpgradeLevel < 5
            ? 0.3m
            : CurrentUpgradeLevel < 10
                ? 0.22m
                : 0.15m;

        decimal increase = Math.Floor(DynamicVars.Damage.BaseValue * multiplier);
        DynamicVars.Damage.UpgradeValueBy(Math.Max(1m, increase));
    }
}
