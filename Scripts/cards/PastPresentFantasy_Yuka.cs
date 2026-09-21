using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class PastPresentFantasy_Yuka : KazamiYukaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("SunlightPerEnergy", 1m)];
    public PastPresentFantasy_Yuka() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true) { }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        await PowerCmd.Apply<EnergySunPower>(context, base.Owner.Creature, DynamicVars["SunlightPerEnergy"].BaseValue, base.Owner.Creature, this, false);
    }

    protected override void OnUpgrade() => DynamicVars["SunlightPerEnergy"].UpgradeValueBy(1m);
}
