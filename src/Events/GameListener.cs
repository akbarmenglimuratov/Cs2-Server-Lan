using BasicFaceitServer.GameStates;
using BasicFaceitServer.Utils;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace BasicFaceitServer.Events;

public class GameListener(BasicFaceitServer core)
{
    public void Load()
    {
        core.RegisterListener<Listeners.OnServerHibernationUpdate>(OnServerHibernationUpdate);
        core.RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
        core.RegisterListener<Listeners.OnTick>(OnTick);
    }

    private void OnTick()
    {
        if (core.MatchBeingPlayedIn)
            core.Helper.GetPlayers()
                .ToList()
                .ForEach(OnMatchBeingPlayedIn);
    }

    private void OnMatchBeingPlayedIn(CCSPlayerController player)
    {
        var imgPath = Path.Combine(core.ModuleDirectory, "F9jJeIw3percent.png");
        string organizer =
            $"<font class='fontSize-m' color='red'>Организатор турнира</font><br><img src='{imgPath}' width='64' height='64'/>";
        player.PrintToCenterHtml($"{organizer}");
    }

    private void OnServerHibernationUpdate(bool isHibernating)
    {
        if (!isHibernating) return;

        MyLogger.Info($"[OnServerHibernationUpdate]: Hibernating. Reset game state");
        core.GamePhase = GamePhase.Sleeping;
    }

    private void OnMapEnd()
    {
        Server.NextFrame(() =>
        {
            Server.PrintToChatAll("OnMapEnd");
            core.GameController.StartNextMap();
            core.GamePhase = GamePhase.Sleeping;
        });
    }
}