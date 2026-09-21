using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using KazamiYuka.Scripts;
using KazamiYuka.Scripts.Powers;
using STS2RitsuLib.Keywords;
using KazamiYuka.Scripts.Keywords;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class ScatteredLight_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxConsume", 4m),     
        new DynamicVar("GainPerConsume", 4m)  
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [KazamiYukaKeywords.FierceSun];

    public ScatteredLight_Yuka() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
        if (player == null) return;

        int maxConsume = (int)DynamicVars["MaxConsume"].BaseValue;

        int consumedSunlight = base.ConsumeUpToMaxSunlight(player, maxConsume, cardPlay);

        if (consumedSunlight > 0)
        {
            await PowerCmd.Apply<ScatteredLightPower>(
                choiceContext,
                player.Creature,
                consumedSunlight, 
                player.Creature,
                this,
                false
            );
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
