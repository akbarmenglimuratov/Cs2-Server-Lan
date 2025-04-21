using System.Text.Json;
using BasicFaceitServer.Configs;
using BasicFaceitServer.Utils;

namespace BasicFaceitServer.Config;

public class ConfigManager(BasicFaceitServer core)
{
    private const string ConfigPath = "configs.json";

    private static MyConfigs Config { get; set; } = new();

    public MyConfigs GetConfig(string moduleDirectory)
    {
        var cfgFullPath = Path.Combine(moduleDirectory, ConfigPath);
        try
        {
            if (!File.Exists(cfgFullPath))
            {
                MyLogger.Debug("Configs file does not exists, saving defaults.");

                Config = new MyConfigs();
                File.WriteAllText(cfgFullPath, JsonSerializer.Serialize(
                    Config,
                    new JsonSerializerOptions { WriteIndented = true }
                ));

                return new MyConfigs();
            }

            var json = File.ReadAllText(cfgFullPath);
            var tmpConfig = JsonSerializer.Deserialize<MyConfigs>(json);
            if (tmpConfig == null)
            {
                MyLogger.Debug("Failed to parse config, using defaults.");
                return new MyConfigs();
            }

            MyLogger.Debug("Processed the config file, now using it");
            return tmpConfig;
        }
        catch (Exception ex)
        {
            MyLogger.Error($"Exception reading config: {ex.Message}");
            MyLogger.Debug("Using defaults...");
            return new MyConfigs();
        }
    }

    public void ValidateConfigs()
    {
        if (core.Config.Cabins is null || Config.Cabins.Length == 0)
            throw new Exception("Cabins are null or empty");
        MyLogger.Info("Cabins are loaded");

        if (core.Config.Cabins.Any(cabin => cabin.IpAddresses.Length == 0))
            throw new Exception("Cabin[i] ip_addresses is null or empty");
        MyLogger.Info("Ip addresses are loaded");

        if (core.Config.LiveGame is not { Length: 2 })
            throw new Exception("Live game must have exactly two teams.");
        MyLogger.Info("Live match is loaded");

        MyLogger.Info("Configs validation passed");
    }
}