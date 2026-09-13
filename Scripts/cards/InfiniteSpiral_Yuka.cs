using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using KazamiYuka.Scripts.Powers;
using KazamiYuka.Scripts;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class InfiniteSpiral_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxHpGained", 6m),
        new EnergyVar(2),         
        new CardsVar(2)          
    ];

    public InfiniteSpiral_Yuka() : base(0, CardType.Power, CardRarity.Rare, TargetType.None, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
        if (player == null) return;

        int maxHp = (int)DynamicVars["MaxHpGained"].BaseValue;
        int draw = base.DynamicVars.Cards.IntValue;
        int energy = base.DynamicVars.Energy.IntValue;


        await SunflowerManager.GainMaxHpAll(choiceContext, player, maxHp);
        await CardPileCmd.Draw(choiceContext, draw, player, false);
        await PlayerCmd.GainEnergy(energy, player);
        await PowerCmd.Apply<InfiniteSpiralPower>(choiceContext, player.Creature, 1, player.Creature, this, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxHpGained"].UpgradeValueBy(2m);
        base.DynamicVars.Cards.UpgradeValueBy(1m);
        base.DynamicVars.Energy.UpgradeValueBy(1m);
    }
}
