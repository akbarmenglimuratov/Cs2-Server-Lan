using System.Text.RegularExpressions;
using BasicFaceitServer.Configs;
using BasicFaceitServer.GameStates;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace BasicFaceitServer.Utils;

public class MyHelper(BasicFaceitServer core)
{
    private static readonly string[] WeaponsList =
    {
        "weapon_ak47", "weapon_aug", "weapon_awp", "weapon_bizon", "weapon_cz75a", "weapon_deagle", "weapon_elite",
        "weapon_famas", "weapon_fiveseven", "weapon_g3sg1", "weapon_galilar",
        "weapon_glock", "weapon_hkp2000", "weapon_m249", "weapon_m4a1", "weapon_m4a1_silencer", "weapon_mac10",
        "weapon_mag7", "weapon_mp5sd", "weapon_mp7", "weapon_mp9", "weapon_negev",
        "weapon_nova", "weapon_p250", "weapon_p90", "weapon_revolver", "weapon_sawedoff", "weapon_scar20",
        "weapon_sg556", "weapon_ssg08", "weapon_tec9", "weapon_ump45", "weapon_usp_silencer", "weapon_xm1014",
        "weapon_decoy", "weapon_flashbang", "weapon_hegrenade", "weapon_incgrenade", "weapon_molotov",
        "weapon_smokegrenade", "item_defuser", "item_cutters", "weapon_knife"
    };

    public void PrintToChat(CCSPlayerController player, string message)
    {
        var coloredText = $"{{green}}[{core.Config.Host}]{{white}}: {message}";
        player.PrintToChat(GetColoredText(coloredText));
    }

    public void PrintToCenterHtmlAll(string message)
    {
        var players = GetPlayers();
        foreach (var player in players)
            Server.NextFrame(() => { player.PrintToCenterHtml(message, 10); });
    }

    public void PrintToCenter(CCSPlayerController player, string message, float delay = 0.0f)
    {
        if (delay > 0.0f)
            core.AddTimer(delay, () => player.PrintToCenter(message));
        else
            player.PrintToCenter(message);
    }

    public void PrintToChatAll(string message)
    {
        var coloredText = $"{{green}}[{core.Config.Host}]{{white}}: {message}";
        Server.PrintToChatAll(GetColoredText(coloredText));
    }

    public void PrintToCenterAll(string message)
    {
        var players = GetPlayers();
        foreach (var player in players)
            player.PrintToCenter(message);
    }

    public void PrintToCenterAlertAll(string message)
    {
        var players = GetPlayers(includeSpec: true);
        foreach (var player in players)
        {
            Server.NextFrame(() => { player.PrintToCenterAlert(message); });
        }
    }

    public bool CheckIpInParticipantsList(string playerIpAddress)
    {
        MyLogger.Info($"Check if player in participants list - {playerIpAddress}");
        if (playerIpAddress == "") return false;

        var configs = core.Config;

        var playerIp = playerIpAddress.Split(":")[0];

        var isParticipant = configs.Cabins.Any(c =>
            configs.LiveGame.Any(l => c.Id == l.CabinId)
            && c.IpAddresses.Contains(playerIp)
        );
        MyLogger.Info($"The participant - {isParticipant}");
        return isParticipant;
    }

    public List<CCSPlayerController> GetPlayers(CsTeam? includeTeam = null, bool includeSpec = false)
    {
        MyLogger.Info("Get players (CT, T)");

        var playerList = Utilities
            .FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller")
            .Where(player =>
                player is { IsValid: true, IsBot: false }
                && (includeTeam == null || player.Team == includeTeam)
                && player.Team != CsTeam.None
                && (includeSpec || player.Team != CsTeam.Spectator)
            )
            .ToList();

        return playerList;
    }

    public CsTeam GetPlayerTeam(CCSPlayerController player)
    {
        MyLogger.Info("Get client team (CT, T or Spectator)");
        var configs = core.Config;
        var playerIp = player.IpAddress?.Split(":")[0];

        if (string.IsNullOrEmpty(playerIp))
            return CsTeam.Spectator;

        var cabin = configs.Cabins.FirstOrDefault(c => c.IpAddresses.Contains(playerIp));
        if (cabin == null)
            return CsTeam.Spectator;

        var liveTeam = configs.LiveGame.FirstOrDefault(t => t.CabinId == cabin.Id);
        if (liveTeam == null)
            return CsTeam.Spectator;

        return liveTeam.DefaultTeam switch
        {
            "CT" => CsTeam.CounterTerrorist,
            "T" => CsTeam.Terrorist,
            _ => CsTeam.Spectator
        };
    }

    public void PlayerJoinTeam(CCSPlayerController player, CsTeam playerTeam)
    {
        MyLogger.Info($"Player team - {playerTeam.ToString()}");

        core.AddTimer(0.1f, () =>
        {
            player.ChangeTeam(CsTeam.Spectator);

            if (playerTeam == CsTeam.Spectator) return;

            player.Respawn();
            core.AddTimer(0.1f, () => { player.ChangeTeam(playerTeam); });
        });
    }

    public void PreparePlayerForKnifeRound(CCSPlayerController player)
    {
        RemovePlayerWeapon(player);
        SetPlayerAccount(player, 0);
        GivePlayerArmor(player);
        GivePlayerKnife(player);
    }

    private void RemoveWeapon(CCSPlayerController player, string weaponName)
    {
        MyLogger.Info("Weapon design name: " + weaponName);
        var weaponServices = player.PlayerPawn.Value?.WeaponServices;

        if (weaponServices == null)
            return;

        var matchedWeapon = weaponServices.MyWeapons
            .FirstOrDefault(w => w.IsValid && w.Value != null && w.Value.DesignerName == weaponName);

        try
        {
            if (matchedWeapon?.IsValid != true) return;
            weaponServices.ActiveWeapon.Raw = matchedWeapon.Raw;

            var weaponEntity = weaponServices.ActiveWeapon.Value?.As<CBaseEntity>();
            if (weaponEntity == null || !weaponEntity.IsValid)
                return;

            weaponEntity.Remove();
            // player.DropActiveWeapon();
            // Server.NextFrame(() => { weaponEntity.AddEntityIOEvent("Kill", weaponEntity, null, "", 0.1f); });
        }
        catch (Exception ex)
        {
            MyLogger.Error($"Error while Refreshing Weapon via className: {ex.Message}");
        }
    }

    private void RemovePlayerWeapon(CCSPlayerController player)
    {
        if (!player.IsValid || player.PlayerPawn.Value == null) return;
        RemoveWeapon(player, "weapon_c4");
        player.RemoveWeapons();
    }

    public void RemoveGroundWeapons()
    {
        foreach (var weapons in WeaponsList)
        {
            foreach (var entity in Utilities.FindAllEntitiesByDesignerName<CBaseEntity>(weapons))
            {
                if (!entity.IsValid) continue;
                if (entity.Entity == null) continue;
                if (entity.OwnerEntity.IsValid) continue;

                Server.NextFrame(() => { entity.AddEntityIOEvent("Kill", entity, null, "", 0.1f); });
            }
        }
    }

    public void SetPlayerAccount(CCSPlayerController player, int amount)
    {
        MyLogger.Info($"Set player money to {amount}");
        var playerMoney = player.InGameMoneyServices;
        if (playerMoney is null) return;

        playerMoney.Account = amount;
        Utilities.SetStateChanged(player, "CCSPlayerController_InGameMoneyServices", "m_iAccount");
    }

    private void GivePlayerArmor(CCSPlayerController player)
    {
        MyLogger.Info("Give player armor");
        player.GiveNamedItem("item_kevlar");
    }

    private void GivePlayerKnife(CCSPlayerController player)
    {
        MyLogger.Info("Give player knife");
        var knifeDesignName = player.Team == CsTeam.CounterTerrorist
            ? "weapon_knife"
            : "weapon_knife_t";

        player.GiveNamedItem(knifeDesignName);
    }

    public CCSGameRules? GetGameRules()
    {
        return Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
            .First()
            .GameRules;
    }

    public CsTeam GetKnifeWinnerTeam()
    {
        return core.GameController.KnifeWinnerTeam;
    }

    public string GetColoredText(string message)
    {
        Dictionary<string, int> colorMap = new()
        {
            { "{default}", 1 },
            { "{white}", 1 },
            { "{darkred}", 2 },
            { "{purple}", 3 },
            { "{green}", 4 },
            { "{lightgreen}", 5 },
            { "{slimegreen}", 6 },
            { "{red}", 7 },
            { "{grey}", 8 },
            { "{yellow}", 9 },
            { "{invisible}", 10 },
            { "{lightblue}", 11 },
            { "{blue}", 12 },
            { "{lightpurple}", 13 },
            { "{pink}", 14 },
            { "{fadedred}", 15 },
            { "{gold}", 16 },
            // No more colors are mapped to CS2
        };

        const string pattern = "{(\\w+)}";
        var replaced = Regex.Replace(message, pattern, match =>
        {
            var colorCode = match.Groups[1].Value;
            return colorMap.TryGetValue("{" + colorCode + "}", out var replacement)
                ? Convert.ToChar(replacement).ToString()
                : match.Value;
        });

        return $"\u200B{replaced}";
    }
}