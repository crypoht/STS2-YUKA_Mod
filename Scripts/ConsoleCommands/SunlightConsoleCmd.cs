using System.Threading.Tasks;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;

namespace KazamiYuka.Scripts.ConsoleCommands;

public sealed class SunlightConsoleCmd : AbstractConsoleCmd
{
    public override string CmdName => "sunlight";

    public override string Args => "<amount:int>";

    public override string Description => "Adds sunlight to player";

    public override bool IsNetworked => true;

    public override CmdResult Process(Player? issuingPlayer, string[] args)
    {
        if (args.Length == 0)
            return new CmdResult(false, "The first argument must be an int.");

        if (!int.TryParse(args[0], out int amount))
            return new CmdResult(false, "The first argument must be an int.");

        if (issuingPlayer == null)
            return new CmdResult(false, "This command only works during a run.");

        if (issuingPlayer.PlayerCombatState == null)
            return new CmdResult(false, "This command only works in combat.");

        if (amount < 0)
            return new CmdResult(false, "The sunlight amount cannot be negative.");

        SunlightManager.GainSunlight(issuingPlayer, amount);
        return new CmdResult(Task.CompletedTask, true, $"Added '{amount}' sunlight.");
    }
}
