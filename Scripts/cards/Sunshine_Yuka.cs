using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using KazamiYuka.Scripts;
using STS2RitsuLib.Keywords;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
[RegisterCharacterStarterCard(typeof(KazamiYukaCharacter), 1)]
[RegisterArchaicToothTranscendence(typeof(Fantasy_Yuka))]
public class Sunshine_Yuka : KazamiYukaCard, ICustomTargetValidator
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxSunlight", 5m)
    ];

    public Sunshine_Yuka() : base(1, CardType.Attack, CardRarity.Basic, KazamiYukaTargetTypes.Sunflower, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [KazamiYukaKeywords.FierceSun];

    public bool IsValidCustomTarget(Creature target)
    {
        return target.Monster is KazamiYukaSunflower;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
        var targetFlower = cardPlay.Target;
        var combatState = player?.Creature?.CombatState;

        if (player == null || targetFlower == null || combatState == null) return;

        if (targetFlower.Monster is KazamiYukaSunflower)
        {
            int maxToConsume = (int)DynamicVars["MaxSunlight"].BaseValue;
            int consumedSunlight = base.ConsumeUpToMaxSunlight(player, maxToConsume, cardPlay);

            if (consumedSunlight > 0)
            {
                await SunflowerManager.GainMaxHpSingle(choiceContext, targetFlower, consumedSunlight);
            }

            var lowestHpEnemy = combatState.Enemies
                .Where(e => e.IsAlive)
                .OrderBy(e => e.CurrentHp)
                .FirstOrDefault();

            if (lowestHpEnemy != null)
            {
                await CreatureCmd.Damage(
                    choiceContext,
                    lowestHpEnemy,
                    targetFlower.CurrentHp,
                    ValueProp.Move,
                    targetFlower,
                    this,
                    cardPlay
                );
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxSunlight"].UpgradeValueBy(2m);
    }
}
