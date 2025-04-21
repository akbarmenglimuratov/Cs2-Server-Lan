using BasicFaceitServer.GameStates;
using BasicFaceitServer.Utils;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.Events;

public class GameListener(BasicFaceitServer core)
{
    private readonly GameController _gameController = core.GameController;
    
    private Dictionary<string, List<CCSPlayerController>> _spectators = new Dictionary<string, List<CCSPlayerController>>();
    
    public void Load()
    {
        core.RegisterListener<Listeners.OnServerHibernationUpdate>(OnServerHibernationUpdate);
        core.RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
        core.RegisterListener<Listeners.OnTick>(OnTick);
    }

    private void OnTick()
    {
        if (core.ShowBombTimer)
            Utilities
                .GetPlayers()
                .Where(player => player is { IsValid: true, Team: CsTeam.Spectator })
                .ToList()
                .ForEach(OnTickPrintBombTimer);
    }
    
    private void OnTickPrintBombTimer(CCSPlayerController spectator)
    {
        var timerColor = $"<font class='fontSize-m' color='green'>{_gameController.BombTimer}</font>";
        if (_gameController.C4Time <= 10.0f)
            timerColor = $"<font class='fontSize-m' color='red'>{_gameController.BombTimer}</font>";
        
        spectator.PrintToCenterHtml($"<font color='white'>Бомба жарылыуына: {timerColor} сек. </font>");
    }
    
    private void OnServerHibernationUpdate(bool isHibernating)
    {
        if (!isHibernating) return;

        MyLogger.Info($"[OnServerHibernationUpdate]: Hibernating. Reset game state");
        core.GamePhase = GamePhase.Sleeping;
    }

    private void OnMapEnd()
    {
        core.GamePhase = GamePhase.Sleeping;
    }
}