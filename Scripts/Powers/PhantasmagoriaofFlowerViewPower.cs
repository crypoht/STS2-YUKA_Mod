using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class PhantasmagoriaofFlowerViewPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/PhantasmagoriaofFlowerViewPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/PhantasmagoriaofFlowerViewPower.png"
    );
}
