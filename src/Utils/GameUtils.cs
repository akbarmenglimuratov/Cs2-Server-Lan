using BasicFaceitServer.GameStates;

namespace BasicFaceitServer.Utils;

public class GameUtils(BasicFaceitServer core)
{
    private readonly MyHelper _helper = core.Helper;
    
    public bool IsPreWarmup()
    {
        return core.GamePhase == GamePhase.PreKnifeWarmup;
    }
    
    public bool IsPostWarmup()
    {
        return core.GamePhase == GamePhase.PostKnifeWarmup;
    }
    
    public bool IsKnife()
    {
        return core.GamePhase == GamePhase.Knife;
    }
    
    public bool IsMatchLive()
    {
        return core.GamePhase == GamePhase.MatchLive;
    }
    
    public bool IsSleeping()
    {
        return core.GamePhase == GamePhase.Sleeping;
    }

    public GamePhase GetCurrentGameState()
    {
        return core.GamePhase;
    }

    public bool IsWarmup()
    {
        var gameRules = _helper.GetGameRules();
        return gameRules!.WarmupPeriod;
    }

    public bool IsPaused()
    {
        return core.MatchState == MatchState.Paused;
    }

    public bool IsFreezePeriod()
    {
        var gameRules = _helper.GetGameRules();
        return gameRules!.FreezePeriod;
    }
}