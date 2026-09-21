using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class InfiniteSpiral_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxSunConsume", 3m),
        new CardsVar(1),
        new EnergyVar(1),
        new DynamicVar("MaxHpGained", 1m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, KazamiYukaKeywords.FierceSun];

    public InfiniteSpiral_Yuka() : base(0, CardType.Skill, CardRarity.Rare, TargetType.None, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
        if (player == null) return;

        int maxSunlight = DynamicVars["MaxSunConsume"].IntValue;
        int availableSunlight = Math.Min(SunlightManager.GetSunlight(player), maxSunlight);
        int consumedSunlight = availableSunlight;
        if (consumedSunlight > 0)
        {
            SunlightManager.TrySpendSunlight(player, consumedSunlight);
        }

        int repeats = 1 + consumedSunlight;
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue * repeats, player, false);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue * repeats, player);
        await SunflowerManager.GainMaxHpAll(
            choiceContext,
            player,
            DynamicVars["MaxHpGained"].BaseValue * repeats
        );
        await CreatureCmd.Damage(
            choiceContext,
            player.Creature,
            repeats,
            MegaCrit.Sts2.Core.ValueProps.ValueProp.Unblockable |
            MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered,
            player.Creature,
            this,
            cardPlay
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxSunConsume"].UpgradeValueBy(1m);
    }
}
