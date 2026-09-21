using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using KazamiYuka.Scripts;
using KazamiYuka.Scripts.Interfaces; 
using STS2RitsuLib.Keywords;
using KazamiYuka.Scripts.Keywords;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class Shading_Yuka : KazamiYukaCard, IVariableSunlightCard
{
    public Shading_Yuka() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [KazamiYukaKeywords.FierceSun];

    public int GetMaxSunlightConsume() => (int)DynamicVars["MaxSunConsume"].BaseValue;

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new BlockVar(6m, ValueProp.Move), 
        new DynamicVar("MaxSunConsume", 2m) 
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
        if (player == null) return;

        int actualSpent = this.ConsumeVariableSunlight(player, cardPlay);

        if (actualSpent > 0)
        {
            for (int i = 0; i < actualSpent; i++)
            {
                await CreatureCmd.GainBlock(player.Creature, base.DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay, false);
                
                await Cmd.Wait(0.1f, false);
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Block.UpgradeValueBy(2m);
    }
}
