using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class FibonacciSunflowerDisk_Yuka : KazamiYukaCard
{
    private int _currentGrow = 1;
    private int _previousGrow;

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, KazamiYukaKeywords.Grow];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new SummonVar(CurrentGrow)
    ];

    [SavedProperty]
    public int CurrentGrow
    {
        get => _currentGrow;
        set
        {
            base.AssertMutable();
            _currentGrow = value;
            if (DynamicVars.TryGetValue("Summon", out DynamicVar? summonVar))
            {
                summonVar.BaseValue = _currentGrow;
            }
        }
    }

    [SavedProperty]
    public int PreviousGrow
    {
        get => _previousGrow;
        set
        {
            base.AssertMutable();
            _previousGrow = value;
        }
    }

    public FibonacciSunflowerDisk_Yuka()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SunflowerManager.Summon(base.Owner, DynamicVars.Summon.BaseValue);
        AdvanceFibonacci();

        if (base.DeckVersion is FibonacciSunflowerDisk_Yuka deckVersion)
        {
            deckVersion.AdvanceFibonacci();
        }
    }

    private void AdvanceFibonacci()
    {
        int nextGrow = PreviousGrow + CurrentGrow;
        PreviousGrow = CurrentGrow;
        CurrentGrow = nextGrow <= 0 ? 1 : nextGrow;
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
