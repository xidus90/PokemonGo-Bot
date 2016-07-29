using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using bhelper;

namespace PokemonGo.RocketAPI.GUI
{
    internal class Program
    {
        public static async void Execute(Hero _hero)
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
                    await _hero.Client.DoPtcLogin(_hero.ClientSettings.Username, _hero.ClientSettings.Password);
                else if (_hero.ClientSettings.AuthType == AuthType.Google)
                    await _hero.Client.DoGoogleLogin(_hero.ClientSettings.Username, _hero.ClientSettings.Password);
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, $"[{DateTime.Now.ToString("HH:mm:ss")}] " + Language.GetPhrases()["bot_loggedin"]);
                await _hero.Client.SetServer();
                var profile = await _hero.Client.GetProfile();
                var inventory = await _hero.Client.GetInventory();
                var pokemons = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.Pokemon).Where(p => p != null && p?.PokemonId > 0);
                var stats = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData.PlayerStats).ToArray();
                var items = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.Item).Where(p => p != null);
                foreach (var v in stats)
                    if (v != null)
                    {
                        _hero.TotalKmWalked = v.KmWalked;
                        await _hero.Client.GetLevelUpRewards(v.Level);
                    }
                bLogic.Info.PrintStartUp(_hero, profile);

                if (_hero.ClientSettings.EvolveAllGivenPokemons)
                    await bLogic.Pokemon.EvolveAllGivenPokemons(_hero, pokemons);
                if (_hero.ClientSettings.Recycler)
                    _hero.Client.RecycleItems(_hero.Client, items);
                if (_hero.ClientSettings.UseLuckyEggMode == "always")
                    _hero.Client.UseLuckyEgg(_hero.Client, inventory);
                _hero.Client.UseIncense(_hero.Client, inventory);

                await Task.Delay(1000);
                //time for some gui updates
                bLogic.Info.PrintLevel(_hero, inventory);

                await bLogic.Pokemon.ExecuteFarmingPokestopsAndPokemons(_hero, inventory);
                _hero.ClientSettings.DefaultLatitude = Client.GetLatitude(true);
                _hero.ClientSettings.DefaultLongitude = Client.GetLongitude(true);
                await Task.Delay(1000);
                ExecuteWithoutStackOverflow(_hero);
            }
            catch (TaskCanceledException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "Task Canceled Exception - Restarting: " + crap.Message); Execute(_hero); }
            catch (UriFormatException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "System URI Format Exception - Restarting: " + crap.Message); Execute(_hero); }
            catch (ArgumentOutOfRangeException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "ArgumentOutOfRangeException - Restarting: " + crap.Message); Execute(_hero); }
            catch (ArgumentNullException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "Argument Null Refference - Restarting: " + crap.Message); Execute(_hero); }
            catch (NullReferenceException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "Null Refference - Restarting: " + crap.Message); Execute(_hero); }
            catch (AccountNotVerifiedException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, "ACCOUNT NOT VERIFIED - WONT WORK - "); }
            catch (System.IO.FileNotFoundException) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, $" Use an existing language!"); }
            catch (SoftbannedException) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, $" Softbanned! Please wait while I unban you!"); await Game.Unban(_hero); ExecuteWithoutStackOverflow(_hero); }
            catch (Exception crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, "Not Handled Exception: " + crap.Message); Execute(_hero); }
        }

        public static async void ExecuteWithoutStackOverflow(Hero _hero)
        {
            if (!_hero.AllowedToRun)
            {
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.Yellow, $"[{DateTime.Now.ToString("HH:mm:ss")}] " + Language.GetPhrases()["bot_stopping"]);
                return;
            }
            try
            {
                var profile = await _hero.Client.GetProfile();
                var inventory = await _hero.Client.GetInventory();
                var pokemons = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.Pokemon).Where(p => p != null && p?.PokemonId > 0);
                var stats = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData.PlayerStats).ToArray();
                var items = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.Item).Where(p => p != null);
                foreach (var v in stats)
                    if (v != null)
                    {
                        _hero.TotalKmWalked = v.KmWalked;
                        await _hero.Client.GetLevelUpRewards(v.Level);
                    }
                bLogic.Info.PrintStartUp(_hero, profile);

                if (_hero.ClientSettings.EvolveAllGivenPokemons)
                    await bLogic.Pokemon.EvolveAllGivenPokemons(_hero, pokemons);

                await Task.Delay(1000);

                await bLogic.Pokemon.ExecuteFarmingPokestopsAndPokemons(_hero, inventory);
                _hero.ClientSettings.DefaultLatitude = Client.GetLatitude(true);
                _hero.ClientSettings.DefaultLongitude = Client.GetLongitude(true);
                await Task.Delay(1000);
                ExecuteWithoutStackOverflow(_hero);
            }
            catch (TaskCanceledException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "Task Canceled Exception - Restarting: " + crap.Message); Execute(_hero); }
            catch (UriFormatException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "System URI Format Exception - Restarting: " + crap.Message); Execute(_hero); }
            catch (ArgumentOutOfRangeException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "ArgumentOutOfRangeException - Restarting: " + crap.Message); Execute(_hero); }
            catch (ArgumentNullException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "Argument Null Refference - Restarting: " + crap.Message); Execute(_hero); }
            catch (NullReferenceException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "Null Refference - Restarting: " + crap.Message); Execute(_hero); }
            catch (AccountNotVerifiedException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, "ACCOUNT NOT VERIFIED - WONT WORK - "); }
            catch (System.IO.FileNotFoundException) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, $" Use an existing language!"); }
            catch (SoftbannedException) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, $" Softbanned! Please wait while I unban you!"); await Game.Unban(_hero); ExecuteWithoutStackOverflow(_hero); }
            catch (Exception crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, "Not Handled Exception: " + crap.Message); Execute(_hero); }
        }

        /// <summary>
        /// Updating our Main form with the newest info
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static async Task UpdateFormTitle(Hero _hero)
        {
            var inventory = await _hero.Client.GetInventory();
            var stats = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PlayerStats).ToArray();
            var profile = await _hero.Client.GetProfile();
            foreach (var playerStatistic in stats)
                if (playerStatistic != null)
                {
                    int XpDiff = bhelper.Game.GetXpDiff(playerStatistic.Level);
                    MainWindow.main.SetMainFormTitle = string.Format(profile.Profile.Username + " | LEVEL: {0:0} - ({1:0}) | SD: {2:0} | XP/H: {3:0} | POKE/H: {4:0}", playerStatistic.Level, string.Format("{0:#,##0}", (playerStatistic.Experience - playerStatistic.PrevLevelXp - XpDiff)) + "/" + string.Format("{0:#,##0}", (playerStatistic.NextLevelXp - playerStatistic.PrevLevelXp - XpDiff)), string.Format("{0:#,##0}", profile.Profile.Currency.ToArray()[1].Amount), string.Format("{0:#,##0}", Math.Round(bLogic.Pokemon.TotalExperience / bhelper.Main.GetRuntime(_hero.TimeStarted))), Math.Round(bLogic.Pokemon.TotalPokemon / bhelper.Main.GetRuntime(_hero.TimeStarted)) + " | " + (DateTime.Now - _hero.TimeStarted).ToString(@"dd\.hh\:mm\:ss"));
                }
            await Task.Delay(1000);
            UpdateFormTitle(_hero);
        }
        public static async Task UpdateGUIStats(Hero _hero)
        {
            MainWindow.main.dataGrid_clear();
            GeneratedCode.GetInventoryResponse inventory = await _hero.Client.GetInventory();
            GeneratedCode.PokemonData[] pokemons = inventory.InventoryDelta.InventoryItems
                .Select(i => i.InventoryItemData?.Pokemon)
                .Where(p => p != null && p?.PokemonId > 0)
                .ToArray();
            GeneratedCode.Item[] items = inventory.InventoryDelta.InventoryItems
                .Select(i => i.InventoryItemData?.Item)
                .Where(p => p != null && p?.Item_ > 0)
                .ToArray();
            foreach (GeneratedCode.PokemonData pokemon in pokemons)
            {
                MainWindow.main.dataGrid_pokemon_add(
                    new dataGrid_pokemon_class()
                    {
                        Name = pokemon.PokemonId.ToString(),
                        CP = pokemon.Cp,
                        Perfection = Math.Round(bLogic.Pokemon.Perfect(pokemon), 2)
                    });
            }
            foreach (GeneratedCode.Item item in items)
            {
                MainWindow.main.dataGrid_backpack_add(
                    new dataGrid_backpack_class()
                    {
                        Item = item.Item_.ToString(),
                        Amount = item.Count
                    });
            }
            Task.Delay(60000);
            UpdateGUIStats(_hero);
        }
    }
}
