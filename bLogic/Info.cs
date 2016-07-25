using System;
using System.Linq;
using System.Threading.Tasks;
using bhelper;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.GeneratedCode;

namespace bLogic
{
    public static class Info
    {
        public static bool PrintStartUp(Hero hero, GetPlayerResponse profileResponse)
        {
            try
            {
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.Yellow, "+-------------- account info ---------------+");
                if (hero.ClientSettings.AuthType == AuthType.Ptc)
                {
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, " PTC Name: " + hero.ClientSettings.PtcUsername);
                }
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, " User Name: " + profileResponse.Profile.Username);
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, " Team: " + profileResponse.Profile.Team);
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, " Stardust: " + profileResponse.Profile.Currency.ToArray()[1].Amount);
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, " Distance traveled: " + String.Format("{0:0.00} km", hero.TotalKmWalked));
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, " Latitude: " + String.Format("{0:0.00} degree", hero.ClientSettings.DefaultLatitude));
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, " Longitude: " + String.Format("{0:0.00} degree", hero.ClientSettings.DefaultLongitude));
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.Yellow, "+--------------------------------------------+");
            }
            catch (Exception crap)
            {
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.Yellow, "Info.StartUpPrint Exception: " + crap.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Print a level related event to RichTextBox or console log
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static async Task PrintLevel(Hero hero)
        {
            var inventory = await hero.Client.GetInventory();
            var stats = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PlayerStats).ToArray();
            foreach (var v in stats)
                if (v != null)
                {
                    int XpDiff = bhelper.Game.GetXpDiff(v.Level);
                    if (hero.ClientSettings.LevelOutput == "time")
                        bhelper.Main.ColoredConsoleWrite(ConsoleColor.Yellow, $"[{DateTime.Now.ToString("HH:mm:ss")}] Current Level: " + v.Level + " (" + (v.Experience - XpDiff) + "/" + (v.NextLevelXp - XpDiff) + ")");
                    else if (hero.ClientSettings.LevelOutput == "levelup")
                        if (hero.Currentlevel != v.Level)
                        {
                            hero.Currentlevel = v.Level;
                            bhelper.Main.ColoredConsoleWrite(ConsoleColor.Magenta, $"[{DateTime.Now.ToString("HH:mm:ss")}] Current Level: " + v.Level + ". XP needed for next Level: " + (v.NextLevelXp - v.Experience));
                        }
                }
            if (hero.ClientSettings.LevelOutput == "levelup")
                await Task.Delay(1000);
            else
                await Task.Delay(hero.ClientSettings.LevelTimeInterval * 1000);
        }
    }
}