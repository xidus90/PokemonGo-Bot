using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using bhelper;

namespace PokemonGo.RocketAPI.Console
{
    internal class Program
    {
        public static bhelper.Hero _hero;
        
        private static async void Execute()
        {
            if (!_hero.AllowedToRun)
            {
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.Yellow, $"[{DateTime.Now.ToString("HH:mm:ss")}] " + Language.GetPhrases()["bot_stopping"]);
                return;
            }

            try
            {
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, $"[{DateTime.Now.ToString("HH:mm:ss")}] " + Language.GetPhrases()["bot_authenticating"]);
                if (_hero.ClientSettings.AuthType == AuthType.Ptc)
                    await _hero.Client.DoPtcLogin(_hero.ClientSettings.PtcUsername, _hero.ClientSettings.PtcPassword);
                else if (_hero.ClientSettings.AuthType == AuthType.Google)
                    await _hero.Client.DoGoogleLogin();
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, $"[{DateTime.Now.ToString("HH:mm:ss")}] " + Language.GetPhrases()["bot_loggedin"]);

                await _hero.Client.SetServer();
                var profile = await _hero.Client.GetProfile();
                var inventory = await _hero.Client.GetInventory();
                var pokemons =
                    inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.Pokemon)
                        .Where(p => p != null && p?.PokemonId > 0);
                var stats = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData.PlayerStats).ToArray();
                foreach (var v in stats)
                    if (v != null)
                        _hero.TotalKmWalked = v.KmWalked;
                foreach (var v in stats)
                    if (v != null)
                        await _hero.Client.GetLevelUpRewards(v.Level);
                bLogic.Info.PrintStartUp(_hero, profile);
               
                if (_hero.ClientSettings.TransferType == "leaveStrongest")
                    await bLogic.Pokemon.TransferAllButStrongestUnwantedPokemon(_hero);
                else if (_hero.ClientSettings.TransferType == "all")
                    await bLogic.Pokemon.TransferAllGivenPokemons(_hero, pokemons);
                else if (_hero.ClientSettings.TransferType == "duplicate")
                    await bLogic.Pokemon.TransferDuplicatePokemon(_hero);
                else if (_hero.ClientSettings.TransferType == "cp")
                    await bLogic.Pokemon.TransferAllWeakPokemon(_hero);
                else
                    bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, $"[{DateTime.Now.ToString("HH:mm:ss")}] " + Language.GetPhrases()["pokemon_transfer_disabled"]);
                if (_hero.ClientSettings.EvolveAllGivenPokemons)
                    await bLogic.Pokemon.EvolveAllGivenPokemons(_hero, pokemons);
                if (_hero.ClientSettings.Recycler)
                    _hero.Client.RecycleItems(_hero.Client);

                await Task.Delay(1000);
                //time for some gui updates
                bLogic.Info.PrintLevel(_hero);
                RefreshConsoleTitle(profile.Profile.Username, _hero);
                

                if (_hero.ClientSettings.EggHatchedOutput)
                    bLogic.Item.CheckEggsHatched(_hero);
                if (_hero.ClientSettings.UseLuckyEggMode == "always")
                    _hero.Client.UseLuckyEgg(_hero.Client);
                await bLogic.Pokemon.ExecuteFarmingPokestopsAndPokemons(_hero);
                _hero.ClientSettings.DefaultLatitude = Client.GetLatitude(true);
                _hero.ClientSettings.DefaultLongitude = Client.GetLongitude(true);
                await Task.Delay(1000);
                Execute();
            }
            catch (TaskCanceledException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "Task Canceled Exception - Restarting: " + crap.Message); Execute(); }
            catch (UriFormatException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "System URI Format Exception - Restarting: " + crap.Message); Execute(); }
            catch (ArgumentOutOfRangeException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "ArgumentOutOfRangeException - Restarting: " + crap.Message); Execute(); }
            catch (ArgumentNullException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "Argument Null Refference - Restarting: " + crap.Message); Execute(); }
            catch (NullReferenceException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "Null Refference - Restarting: " + crap.Message); Execute(); }
            catch (AccountNotVerifiedException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, "ACCOUNT NOT VERIFIED - WONT WORK - " + crap.Message); Execute(); }
            catch (System.IO.FileNotFoundException) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, $" Use an existing language!"); }
            catch (Exception crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, "Not Handled Exception: " + crap.Message); Execute(); }
        }
        
        /// <summary>
        /// console client main entry point
        /// No start parameter possible currently
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            Task.Run(() =>
            {
                try
                {

                    var client = new Client(new bhelper.Settings());
                    Program._hero = new Hero(client);

                    if (_hero.ClientSettings.Language == "System")
                    {
                        switch (System.Globalization.CultureInfo.InstalledUICulture.Name)
                        {
                            case "de_DE":
                            case "de_AT":
                            case "de_CH":
                                {
                                    Language.LoadLanguageFile("de_DE");
                                    break;
                                }
                            default:
                                {
                                    Language.LoadLanguageFile("en_EN");
                                    break;
                                }
                        }
                        bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, $"[{DateTime.Now.ToString("HH:mm:ss")}] " + Language.GetPhrases()["detected_sys_language"] + System.Globalization.CultureInfo.InstalledUICulture.DisplayName);
                    }
                    else
                        Language.LoadLanguageFile(_hero.ClientSettings.Language);
                    bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, $"[{DateTime.Now.ToString("HH:mm:ss")}] " + Language.GetPhrases()["loaded_language"] + Language.LanguageFile);

                    //if we are on the newest version we should be fine running the bot
                    if (bhelper.Main.CheckVersion(Assembly.GetExecutingAssembly().GetName()))
                    {
                        Program._hero.AllowedToRun = true;
                    }

                    //lets get rolling
                    Program.Execute();
                }
                catch (PtcOfflineException) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, Language.GetPhrases()["PtcOfflineException"]); }
                catch (Exception ex) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, $"[{DateTime.Now.ToString("HH:mm:ss")}] {ex}"); }
            });
            System.Console.ReadLine();
        }
        
        
        /// <summary>
        /// Change the console title
        /// for much info. wow
        /// </summary>
        /// <param name="username"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static async Task RefreshConsoleTitle(string username, Hero hero)
        {
            var inventory = await hero.Client.GetInventory();
            var stats = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PlayerStats).ToArray();
            var profile = await hero.Client.GetProfile();
            foreach (var playerStatistic in stats)
                if (playerStatistic != null)
                {
                    int XpDiff = bhelper.Game.GetXpDiff(playerStatistic.Level);
                    System.Console.Title = string.Format(username + " | LEVEL: {0:0} - ({1:0}) | SD: {2:0} | XP/H: {3:0} | POKE/H: {4:0}", playerStatistic.Level, string.Format("{0:#,##0}", (playerStatistic.Experience - playerStatistic.PrevLevelXp - XpDiff)) + "/" + string.Format("{0:#,##0}", (playerStatistic.NextLevelXp - playerStatistic.PrevLevelXp - XpDiff)), string.Format("{0:#,##0}", profile.Profile.Currency.ToArray()[1].Amount), string.Format("{0:#,##0}", Math.Round(bLogic.Pokemon.TotalExperience / bhelper.Main.GetRuntime(_hero.TimeStarted))), Math.Round(bLogic.Pokemon.TotalPokemon / bhelper.Main.GetRuntime(_hero.TimeStarted)) + " | " +(DateTime.Now - _hero.TimeStarted).ToString(@"dd\.hh\:mm\:ss"));
                }
            await Task.Delay(1000);

            RefreshConsoleTitle(username, hero);
        }
    }
}
