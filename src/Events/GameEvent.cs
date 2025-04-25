using BasicFaceitServer.GameStates;
using BasicFaceitServer.Utils;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.Events;

public class GameEvent(BasicFaceitServer core)
{
    private readonly GameController _gameController = core.GameController;
    private readonly GameUtils _gameUtils = core.GameUtils;
    private readonly MyHelper _helper = core.Helper;

    public void Load()
    {
        core.RegisterEventHandler<EventRoundStart>(OnRoundStart);
        core.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        core.RegisterEventHandler<EventRoundAnnounceWarmup>(OnRoundAnnounceWarmup);
        core.RegisterEventHandler<EventWarmupEnd>(OnWarmupEnd);
        core.RegisterEventHandler<EventRoundAnnounceMatchStart>(OnRoundAnnounceMatchStart);
        core.RegisterEventHandler<EventMapShutdown>(OnMapShutdown);
        core.RegisterEventHandler<EventBombPlanted>(OnEventBombPlanted);
        core.RegisterEventHandler<EventPlayerDeath>(OnEventPlayerDeath);
    }

    private HookResult OnEventPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot)
        {
            MyLogger.Debug($"Player is null or bot - {player?.IpAddress}");
            return HookResult.Continue;
        }

        if (_gameUtils.IsSleeping()) return HookResult.Continue;

        if (_gameUtils.IsMatchLive()) return HookResult.Continue;

        if (_gameUtils.IsKnife()) return HookResult.Continue;

        if (_gameUtils.IsWarmup())
        {
            _helper.RemoveGroundWeapons();
            _helper.SetPlayerAccount(player, 16000);
        }

        return HookResult.Continue;
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        MyLogger.Info("Start");

        if (_gameUtils.IsMatchLive()) return HookResult.Continue;

        var players = _helper.GetPlayers();

        if (_gameUtils.IsKnife())
        {
            MyLogger.Debug($"Knife round started. Skip team intro");

            // var gameRules = _helper.GetGameRules();
            // gameRules!.TeamIntroPeriod = false;

            foreach (var player in players)
                _helper.PreparePlayerForKnifeRound(player);

            _helper.PrintToChatAll("Пышақ роунды!!!");
        }
        else if (_gameUtils.IsMatchLive() && players.Count >= core.Config.MinPlayerToStart)
        {
            MyLogger.Debug($"Players ({players.Count}) count is below 10. Pause the match");
            _gameController.PauseMatch();
        }

        MyLogger.Info("Finish");
        return HookResult.Continue;
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        MyLogger.Info($"Start");

        if (_gameUtils.IsMatchLive()) return HookResult.Continue;

        if (_gameUtils.IsKnife())
        {
            var knifeWinner = @event.Winner == (byte)CsTeam.CounterTerrorist
                ? CsTeam.CounterTerrorist
                : CsTeam.Terrorist;
            _gameController.SetKnifeWinnerTeam(knifeWinner);
            _gameController.StartPostKnifeWarmup();
        }

        MyLogger.Info($"Finish");
        return HookResult.Continue;
    }

    private HookResult OnRoundAnnounceWarmup(EventRoundAnnounceWarmup @event, GameEventInfo info)
    {
        if (_gameUtils.IsPreWarmup()) return HookResult.Continue;

        if (_gameUtils.IsPostWarmup())
        {
            MyLogger.Debug($"Post knife warmup period started");

            var teamName1 = _gameController.Teams.Team1.Name;
            var teamName2 = _gameController.Teams.Team2.Name;
            MyLogger.Info($"Team name 1 - {teamName1}");
            MyLogger.Info($"Team name 2 - {teamName2}");

            var knifeWinner = _helper.GetKnifeWinnerTeam();
            if (knifeWinner == CsTeam.None)
                return HookResult.Continue;

            var winnerTeamName = knifeWinner == CsTeam.CounterTerrorist
                ? teamName1
                : teamName2;
            MyLogger.Debug($"Winner team name - {winnerTeamName}");

            _helper.PrintToChatAll($"{{green}}{winnerTeamName} {{white}}тəрепти таңлаң");
            _helper.PrintToChatAll("{green}!ct {white}ямаса {green}!t {white}командасын жазың");

            MyLogger.Info($"Finish");
        }

        return HookResult.Continue;
    }

    private HookResult OnWarmupEnd(EventWarmupEnd @event, GameEventInfo info)
    {
        MyLogger.Info("Start");

        if (_gameUtils.IsPreWarmup())
        {
            MyLogger.Info("Pre-knife warmup period ended");

            var players = _helper.GetPlayers();
            if (players.Count < core.Config.MinPlayerToStart)
            {
                MyLogger.Debug($"Players ({players.Count}) count is below {core.Config.MinPlayerToStart}");
                _gameController.PauseMatch();
            }

            _gameController.StartKnife();

            return HookResult.Continue;
        }

        if (_gameUtils.IsPostWarmup())
        {
            MyLogger.Info("Post knife warmup period ended");
            _gameController.StartMatch();
        }

        MyLogger.Info("End");
        return HookResult.Continue;
    }

    private HookResult OnRoundAnnounceMatchStart(EventRoundAnnounceMatchStart @event, GameEventInfo info)
    {
        MyLogger.Info("Start");

        if (_gameUtils.IsKnife())
        {
            MyLogger.Info($"Print knife round start message to each player");
            _helper.PrintToCenterAll("Пышақ роунды басланды");
        }

        if (_gameUtils.IsMatchLive())
        {
            MyLogger.Info($"Print Good luck message");
            _helper.PrintToChatAll("Ҳаммеге аўмет!!!");
        }

        MyLogger.Info("End");
        return HookResult.Continue;
    }

    private HookResult OnMapShutdown(EventMapShutdown @event, GameEventInfo info)
    {
        Server.PrintToChatAll(@event.EventName);
        return HookResult.Continue;
    }

    private HookResult OnEventBombPlanted(EventBombPlanted @event, GameEventInfo info)
    {
        MyLogger.Info("Start");

        info.DontBroadcast = true;
        _helper.PrintToCenterAlertAll("Бомба койылды. Жарылыўына 40 секунд бар");

        MyLogger.Info("End");
        return HookResult.Continue;
    }
}