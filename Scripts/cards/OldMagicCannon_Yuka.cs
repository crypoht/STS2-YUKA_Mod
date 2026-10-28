using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class OldMagicCannon_Yuka : KazamiYukaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(13m, ValueProp.Move),
        new DynamicVar("OldPerSun", 3m)
    ];


    public OldMagicCannon_Yuka() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        int sunlight = SunlightManager.GetSunlight(base.Owner);
        if (base.CombatState != null)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, play)
                .TargetingAllOpponents(base.CombatState)
                .Execute(context);
        }

        if (sunlight > 0)
        {
            await PowerCmd.Apply<OldPower>(
                context,
                base.Owner.Creature,
                sunlight * DynamicVars["OldPerSun"].BaseValue,
                base.Owner.Creature,
                this,
                false
            );
        }
    }

    protected override void OnUpgrade() 
    { 
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars["OldPerSun"].UpgradeValueBy(2m);
    }
}
