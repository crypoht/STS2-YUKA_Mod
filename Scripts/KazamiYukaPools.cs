using Godot;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace KazamiYuka.Scripts;

public class KazamiYukaCardPool : TypeListCardPoolModel
{
    public override string Title => "KazamiYuka";
    public override string EnergyColorName => "KazamiYukaColor";

    public override string? TextEnergyIconPath => "res://mods/KazamiYuka/images/energy_kazamiyuka.png";
    public override string? BigEnergyIconPath => "res://mods/KazamiYuka/images/energy_kazamiyuka_big.png";

    // 使用 #00cc00 绿色主题
    public override Color DeckEntryCardColor => new(0f, 0.8f, 0f);
    public override Color EnergyOutlineColor => new(0.654f, 0.333f, 0.345f);

    private static readonly Material? _poolFrameMaterial = MaterialUtils.CreateReplaceHueShaderMaterial(0f, 0.8f, 0f);
    public override Material? PoolFrameMaterial => _poolFrameMaterial;

    public override bool IsColorless => false;
}

public class KazamiYukaRelicPool : TypeListRelicPoolModel
{
    public override string? TextEnergyIconPath => "res://mods/KazamiYuka/images/energy_kazamiyuka.png";
    public override string? BigEnergyIconPath => "res://mods/KazamiYuka/images/energy_kazamiyuka_big.png";

    public override string EnergyColorName => "KazamiYukaColor";
}

public class KazamiYukaPotionPool : TypeListPotionPoolModel
{
    public override string? TextEnergyIconPath => "res://mods/KazamiYuka/images/energy_kazamiyuka.png";
    public override string? BigEnergyIconPath => "res://mods/KazamiYuka/images/energy_kazamiyuka_big.png";

    public override string EnergyColorName => "KazamiYukaColor";
}
