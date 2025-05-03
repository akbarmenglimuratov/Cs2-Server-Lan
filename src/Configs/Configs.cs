using System.Net;
using System.Text.Json.Serialization;

namespace BasicFaceitServer.Configs;

public class Team(int id, string name)
{
    [JsonPropertyName("id")] public int Id { get; set; } = id;
    [JsonPropertyName("name")] public string Name { get; set; } = name;
}

public class Cabin(int id, string[] ipAddresses)
{
    [JsonPropertyName("id")] public int Id { get; set; } = id;
    [JsonPropertyName("ip_addresses")] public string[] IpAddresses { get; set; } = ipAddresses;
}

public class LiveTeam(int cabinId, string defaultTeam)
{
    [JsonPropertyName("cabin_id")] public int CabinId { get; init; } = cabinId;
    [JsonPropertyName("default_team")] public string DefaultTeam { get; init; } = defaultTeam;
}

public class MyConfigs
{
    [JsonPropertyName("host")] public string Host { get; set; } = "Kings";

    [JsonPropertyName("live_game")]
    public LiveTeam[] LiveGame { get; init; } =
    [
        new(1, "CT"),
        new(2, "T")
    ];

    [JsonPropertyName("maps")]
    public string[] Maps { get; init; } =
    [
        "de_mirage",
        "de_inferno",
        "de_dust2"
    ];

    [JsonPropertyName("cabins")]
    public Cabin[] Cabins { get; set; } =
    [
        new Cabin(1, [IPAddress.Any.ToString(), IPAddress.Any.ToString()])
    ];

    [JsonPropertyName("pre_warmup_time")] public int PreWarmupTime { get; set; } = 420;
    [JsonPropertyName("post_warmup_time")] public int PostWarmupTime { get; set; } = 60;

    [JsonPropertyName("min_player_to_start")]
    public int MinPlayerToStart { get; set; } = 10;
}