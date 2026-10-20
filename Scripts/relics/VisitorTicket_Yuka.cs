using System.Threading.Tasks;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Relics;

[RegisterRelic(typeof(KazamiYukaRelicPool))]
public sealed class VisitorTicket_Yuka : ModRelicTemplate
{
    private bool _usedThisCombat;
    private bool _subscribed;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/relics/VisitorTicket_Yuka.png",
        IconOutlinePath: "res://mods/KazamiYuka/images/relics/VisitorTicket_Yuka.png",
        BigIconPath: "res://mods/KazamiYuka/images/relics/VisitorTicket_Yuka.png"
    );

    public override Task BeforeCombatStart()
    {
        _usedThisCombat = false;
        if (!_subscribed)
        {
            SunlightManager.SunlightSpent += OnSunlightSpent;
            _subscribed = true;
        }

        return base.BeforeCombatStart();
    }

    private void OnSunlightSpent(Player player, int amount)
    {
        if (_usedThisCombat || amount <= 0 || player != base.Owner)
        {
            return;
        }

        _usedThisCombat = true;
        base.Flash();
        SunlightManager.GainSunlight(player, amount);
    }
}
