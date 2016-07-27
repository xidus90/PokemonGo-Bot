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
                    await _hero.Client.DoPtcLogin(_hero.ClientSettings.PtcUsername, _hero.ClientSettings.PtcPassword);
                else if (_hero.ClientSettings.AuthType == AuthType.Google)
                    await _hero.Client.DoGoogleLogin();
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, $"[{DateTime.Now.ToString("HH:mm:ss")}] " + Language.GetPhrases()["bot_loggedin"]);

                await _hero.Client.SetServer();
                var profile = await _hero.Client.GetProfile();
                var settings = await _hero.Client.GetSettings();
                var mapObjects = await _hero.Client.GetMapObjects();
                var inventory = await _hero.Client.GetInventory();
                var pokemons =
                    inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.Pokemon)
                        .Where(p => p != null && p?.PokemonId > 0);
                var stats = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData.PlayerStats).ToArray();
                foreach (var v in stats)
                    if (v != null)
                        _hero.TotalKmWalked = v.KmWalked;

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

                await Task.Delay(5000);
                //time for some gui updates
                bLogic.Info.PrintLevel(_hero);
                UpdateFormTitle(_hero);
                UpdatePokemonList(_hero);


                if (_hero.ClientSettings.EggHatchedOutput)
                    bLogic.Item.CheckEggsHatched(_hero);
                if (_hero.ClientSettings.UseLuckyEggMode == "always")
                    _hero.Client.UseLuckyEgg(_hero.Client);

                await bLogic.Pokemon.ExecuteFarmingPokestopsAndPokemons(_hero);
                _hero.ClientSettings.DefaultLatitude = Client.GetLatitude(true);
                _hero.ClientSettings.DefaultLongitude = Client.GetLongitude(true);
                await Task.Delay(1000);
                Execute(_hero);
            }
            catch (TaskCanceledException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "Task Canceled Exception - Restarting: " + crap.Message); Execute(_hero); }
            catch (UriFormatException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "System URI Format Exception - Restarting: " + crap.Message); Execute(_hero); }
            catch (ArgumentOutOfRangeException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "ArgumentOutOfRangeException - Restarting: " + crap.Message); Execute(_hero); }
            catch (ArgumentNullException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "Argument Null Refference - Restarting: " + crap.Message); Execute(_hero); }
            catch (NullReferenceException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "Null Refference - Restarting: " + crap.Message); Execute(_hero); }
            catch (AccountNotVerifiedException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, "ACCOUNT NOT VERIFIED - WONT WORK - " + crap.Message); Execute(_hero); }
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
        public static async Task UpdatePokemonList(Hero _hero)
        {
            MainWindow.main.dataGrid_pokemon_clear();
            GeneratedCode.GetInventoryResponse inventory = await _hero.Client.GetInventory();
            GeneratedCode.PokemonData[] pokemons = inventory.InventoryDelta.InventoryItems
                .Select(i => i.InventoryItemData?.Pokemon)
                .Where(p => p != null && p?.PokemonId > 0)
                .ToArray();

            foreach (GeneratedCode.PokemonData pokemon in pokemons)
            {
                MainWindow.main.dataGrid_pokemon_add( 
                    new Pokemon() {
                    Name = pokemon.PokemonId.ToString(),
                    CP = pokemon.Cp,
                    Perfection = Math.Round(bLogic.Pokemon.Perfect(pokemon), 2)
                });
            }
        }
    }
}
