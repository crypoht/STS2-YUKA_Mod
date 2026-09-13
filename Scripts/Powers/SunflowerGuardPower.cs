using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public class SunflowerGuardPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    
    public override PowerStackType StackType => PowerStackType.Single;

    // 配置你的能力图标（注意官方推荐小图 64x64，大图 256x256）
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/sunflower_guard_small.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/sunflower_guard_large.png"
    );

}