using System;
using System.Linq;
using bhelper;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.GeneratedCode;

namespace bLogic
{
    public static class Info
    {
        public static bool StartUp(Hero hero, GetPlayerResponse profileResponse)
        {
            try
            {
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.Yellow, "+-------------- account info ---------------+");
                if (hero.ClientSettings.AuthType == AuthType.Ptc)
                {
                    bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray,
                        " Account Name: " + hero.ClientSettings.PtcUsername);
                }
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, " Hero Name: " + profileResponse.Profile.Username);
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, " Team: " + profileResponse.Profile.Team);
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, " Stardust: " + profileResponse.Profile.Currency.ToArray()[1].Amount);
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, " Distance traveled: " + String.Format("{0:0.00} km", hero.TotalKmWalked));
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, " Latitude: " + hero.ClientSettings.DefaultLatitude);
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, " Longitude: " + hero.ClientSettings.DefaultLongitude);
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.Yellow, "+--------------------------------------------+");
            }
            catch (Exception crap)
            {
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.Yellow, "Info.StartUp Exception: " + crap.Message);
                return false;
            }

            return true;
        } 
    }
}