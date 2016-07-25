using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AllEnum;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.GeneratedCode;
using System.Net.Http;
using System.Text;
using Google.Protobuf;
using PokemonGo.RocketAPI.Helpers;
using System.IO;
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
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.Yellow, "Stopping bot cyclus now!");
                return;
            }

            try
            {
                bhelper.Main.CheckVersion(Assembly.GetExecutingAssembly().GetName());

                if (_hero.ClientSettings.AuthType == AuthType.Ptc)
                    await _hero.Client.DoPtcLogin(_hero.ClientSettings.PtcUsername, _hero.ClientSettings.PtcPassword);
                else if (_hero.ClientSettings.AuthType == AuthType.Google)
                    await _hero.Client.DoGoogleLogin();

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

                bhelper.Main.ColoredConsoleWrite(ConsoleColor.Yellow, "----------------------------");
                if (_hero.ClientSettings.AuthType == AuthType.Ptc)
                {
                    bhelper.Main.ColoredConsoleWrite(ConsoleColor.Cyan, "Login Name: " + _hero.ClientSettings.PtcUsername);
                }
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, "Latitude: " + _hero.ClientSettings.DefaultLatitude);
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, "Longitude: " + _hero.ClientSettings.DefaultLongitude);
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, "Hero Name: " + profile.Profile.Username);
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, "Team: " + profile.Profile.Team);
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, "Stardust: " + profile.Profile.Currency.ToArray()[1].Amount);
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, "Distance traveled: " + String.Format("{0:0.00} km", _hero.TotalKmWalked));
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.Yellow, "----------------------------");
                if (_hero.ClientSettings.TransferType == "leaveStrongest")
                    await bLogic.Pokemon.TransferAllButStrongestUnwantedPokemon(_hero);
                else if (_hero.ClientSettings.TransferType == "all")
                    await bLogic.Pokemon.TransferAllGivenPokemons(_hero, pokemons);
                else if (_hero.ClientSettings.TransferType == "duplicate")
                    await bLogic.Pokemon.TransferDuplicatePokemon(_hero);
                else if (_hero.ClientSettings.TransferType == "cp")
                    await bLogic.Pokemon.TransferAllWeakPokemon(_hero);
                else
                    bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGray, $"[{DateTime.Now.ToString("HH:mm:ss")}] Transfering pokemon disabled");
                if (_hero.ClientSettings.EvolveAllGivenPokemons)
                    await bLogic.Pokemon.EvolveAllGivenPokemons(_hero, pokemons);
                if (_hero.ClientSettings.Recycler)
                    _hero.Client.RecycleItems(_hero.Client);

                await Task.Delay(5000);
                //time for some gui updates
                PrintLevel(_hero.Client);
                ConsoleLevelTitle(profile.Profile.Username, _hero.Client);

                if (_hero.ClientSettings.EggHatchedOutput)
                    await bLogic.Pokemon.CheckEggsHatched(_hero);
                if (_hero.ClientSettings.UseLuckyEggMode == "always")
                    await _hero.Client.UseLuckyEgg(_hero.Client);
                await bLogic.Pokemon.ExecuteFarmingPokestopsAndPokemons(_hero);
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, $"[{DateTime.Now.ToString("HH:mm:ss")}] No nearby usefull locations found. Waiting 10 seconds.");
                await Task.Delay(10000);
                Execute();
            }
            catch (TaskCanceledException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "Task Canceled Exception - Restarting: " + crap.Message); Execute(); }
            catch (UriFormatException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "System URI Format Exception - Restarting " + crap.Message); Execute(); }
            catch (ArgumentOutOfRangeException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "ArgumentOutOfRangeException - Restarting " + crap.Message); Execute(); }
            catch (ArgumentNullException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "Argument Null Refference - Restarting " + crap.Message); Execute(); }
            catch (NullReferenceException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, "Null Refference - Restarting " + crap.Message); Execute(); }
            catch (AccountNotVerifiedException crap) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, "ACCOUNT NOT VERIFIED - WONT WORK " + crap.Message); Execute(); }
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
                    //if we are on the newest version we should be fine running the bot
                    if (bhelper.Main.CheckVersion(Assembly.GetExecutingAssembly().GetName()))
                    {
                        Program._hero.AllowedToRun = true;
                    }

                    var client = new Client(new bhelper.Settings());
                    Program._hero = new Hero(client);

                    //lets get rolling
                    Program.Execute();
                }
                catch (PtcOfflineException)
                {
                    bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, "PTC Servers are probably down OR your credentials are wrong. Try google");
                }
                catch (Exception ex)
                {
                    bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, $"[{DateTime.Now.ToString("HH:mm:ss")}] Unhandled exception: {ex}");
                }
            });
            System.Console.ReadLine();
        }
        
        

        public static async Task PrintLevel(Client client)
        {
            var inventory = await client.GetInventory();
            var stats = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PlayerStats).ToArray();
            foreach (var v in stats)
                if (v != null)
                {
                    int XpDiff = bhelper.Game.GetXpDiff(v.Level);
                    if (_hero.ClientSettings.LevelOutput == "time")
                        bhelper.Main.ColoredConsoleWrite(ConsoleColor.Yellow, $"[{DateTime.Now.ToString("HH:mm:ss")}] Current Level: " + v.Level + " (" + (v.Experience - XpDiff) + "/" + (v.NextLevelXp - XpDiff) + ")");
                    else if (_hero.ClientSettings.LevelOutput == "levelup")
                        if (_hero.Currentlevel != v.Level)
                        {
                            _hero.Currentlevel = v.Level;
                            bhelper.Main.ColoredConsoleWrite(ConsoleColor.Magenta, $"[{DateTime.Now.ToString("HH:mm:ss")}] Current Level: " + v.Level + ". XP needed for next Level: " + (v.NextLevelXp - v.Experience));
                        }
                }
            if (_hero.ClientSettings.LevelOutput == "levelup")
                await Task.Delay(1000);
            else
                await Task.Delay(_hero.ClientSettings.LevelTimeInterval * 1000);
            PrintLevel(client);
        }
        

        public static async Task ConsoleLevelTitle(string Username, Client client)
        {
            var inventory = await client.GetInventory();
            var stats = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PlayerStats).ToArray();
            var profile = await client.GetProfile();
            foreach (var v in stats)
                if (v != null)
                {
                    int XpDiff = bhelper.Game.GetXpDiff(v.Level);
                    System.Console.Title = string.Format(Username + " | Level: {0:0} - ({1:0} / {2:0}) | Stardust: {3:0}", v.Level, (v.Experience - v.PrevLevelXp - XpDiff), (v.NextLevelXp - v.PrevLevelXp - XpDiff), profile.Profile.Currency.ToArray()[1].Amount) + " | XP/Hour: " + Math.Round(_hero.TotalExperience / bhelper.Main.GetRuntime(_hero.TimeStarted)) + " | Pokemon/Hour: " + Math.Round(_hero.TotalPokemon / bhelper.Main.GetRuntime(_hero.TimeStarted));
                }
            await Task.Delay(1000);
            ConsoleLevelTitle(Username, client);
        }
        
    }
}
