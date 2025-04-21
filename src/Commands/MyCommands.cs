using BasicFaceitServer.GameStates;
using BasicFaceitServer.Utils;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.Commands;

public class MyCommands(BasicFaceitServer core)
{
    private readonly GameController _gameController = core.GameController;
    private readonly GameUtils _game = core.GameUtils;
    private readonly MyHelper _helper = core.Helper;
    
    public void Load()
    {
        core.AddCommand("t", "Switch team to T", OnTCommand);
        core.AddCommand("ct", "Switch team to CT", OnCTCommand);
        core.AddCommand("set_state", "Set game state", OnSetStateCommand);
        core.AddCommand("print_state", "Print game slot", OnPrintStateCommand);
        core.AddCommand("print_gr", "Print game rules", OnPrintGameRulesCommand);
    }

    private void OnPrintGameRulesCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        var gameRules = _helper.GetGameRules();
        Console.WriteLine($"GamePhase: {gameRules!.GamePhase}");
    }
    
    private void OnCTCommand(CCSPlayerController? player, CommandInfo command)
    {
        MyLogger.Info("On command execute: !ct - Start");

        if (_game.IsMatchLive()) return;

        if (player == null || !player.IsValid) return;

        var knifeWinnerTeam = _gameController.GetKnifeWinnerTeam();
        
        MyLogger.Debug($"On command execute: !ct - Player team: {player.Team}");
        if (player.Team != knifeWinnerTeam || player.Team == CsTeam.Spectator) return;
        
        var gameRules = _helper.GetGameRules();
        
        gameRules!.SwapTeamsOnRestart = player.Team == CsTeam.Terrorist;
        gameRules.WarmupPeriod = false;
        
        _gameController.StartMatch();
        
        MyLogger.Info($"On command execute: !ct - End");
    }

    private void OnTCommand(CCSPlayerController? player, CommandInfo command)
    {
        MyLogger.Info($"On command execute: !t - Start");
        
        if (_game.IsMatchLive()) return;

        if (player == null || !player.IsValid) return;

        var knifeWinnerTeam = _gameController.GetKnifeWinnerTeam();
        
        MyLogger.Debug($"On command execute: !t - Player team: {player.Team}");
        if (player.Team != knifeWinnerTeam || player.Team == CsTeam.Spectator) return;
        
        var gameRules = _helper.GetGameRules();
        
        gameRules!.SwapTeamsOnRestart = player.Team == CsTeam.CounterTerrorist;
        gameRules.WarmupPeriod = false;
        
        _gameController.StartMatch();
        
        MyLogger.Info($"On command execute: !t - End");
    }

    private void OnPrintStateCommand(CCSPlayerController? player, CommandInfo command)
    {
        Console.WriteLine($"Current game state: {_game.GetCurrentGameState()}");
    }
    
    private void OnSetStateCommand(CCSPlayerController? player, CommandInfo command)
    {
        var cmdArg = command.GetArg(1);
        switch (cmdArg)
        {
            case "prewarmup":
                _gameController.StartPreKnifeWarmup();
                break;
            case "knife":
                _gameController.StartKnife();
                Server.ExecuteCommand("mp_restartgame 1");
                break;
            case "postwarmup":
                _gameController.StartPostKnifeWarmup();
                break;
            case "live":
                _gameController.StartMatch();
                Server.ExecuteCommand("mp_restartgame 1");
                break;
            default:
                Console.WriteLine("Incorrect state");
                return;
        }
    }
}