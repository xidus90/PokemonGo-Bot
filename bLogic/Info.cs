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
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, " PTC Name: " + hero.ClientSettings.Username);
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
    }
}