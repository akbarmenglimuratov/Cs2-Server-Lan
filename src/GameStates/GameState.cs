namespace BasicFaceitServer.GameStates;

public enum GamePhase
{
    PreKnifeWarmup,
    PostKnifeWarmup,
    Knife,
    MatchLive,
    Sleeping
}

public enum MatchState
{
    Paused,
    Live
}

public class TeamData(int id, string name)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
}

public class Teams
{
    public TeamData Team1 { get; set; } = new TeamData(1, "CounterTerrorist");
    public TeamData Team2 { get; set; } = new TeamData(2, "Terrorist");
}