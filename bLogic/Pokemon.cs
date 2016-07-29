using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllEnum;
using bhelper;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.GeneratedCode;

namespace bLogic
{
    public class Pokemon
    {
        /// <summary>
        /// iterating through a given list of pokemon we want to evolve
        /// </summary>
        /// <param name="hero"></param>
        /// <param name="pokemonToEvolve"></param>
        /// <returns></returns>

        public static int TotalExperience = 0;
        public static int TotalPokemon = 0;
        public static async Task EvolveAllGivenPokemons(Hero hero, IEnumerable<PokemonData> pokemonToEvolve)
        {
            PokemonId[] DontEvolve = new[]
            {
                PokemonId.Missingno
            };


            foreach (var pokemon in pokemonToEvolve)
            {
                /*
                enum Holoholo.Rpc.Types.EvolvePokemonOutProto.Result {
	                UNSET = 0;
	                SUCCESS = 1;
	                FAILED_POKEMON_MISSING = 2;
	                FAILED_INSUFFICIENT_RESOURCES = 3;
	                FAILED_POKEMON_CANNOT_EVOLVE = 4;
	                FAILED_POKEMON_IS_DEPLOYED = 5;
                }
                }*/
                if (DontEvolve.Contains(pokemon.PokemonId))
                    continue;

                

                var countOfEvolvedUnits = 0;
                var xpCount = 0;

                EvolvePokemonOut evolvePokemonOutProto;
                do
                {
                    evolvePokemonOutProto = await hero.Client.EvolvePokemon(pokemon.Id);
                    //todo: someone check whether this still works

                    if (evolvePokemonOutProto.Result == 1)
                    {
                        bhelper.Main.ColoredConsoleWrite(ConsoleColor.Cyan,
                            $"[{DateTime.Now.ToString("HH:mm:ss")}] Evolved {pokemon.PokemonId} successfully for {evolvePokemonOutProto.ExpAwarded}xp");
                        TotalExperience += evolvePokemonOutProto.ExpAwarded;


                        countOfEvolvedUnits++;
                        xpCount += evolvePokemonOutProto.ExpAwarded;
                    }
                    else
                    {
                        var result = evolvePokemonOutProto.Result;
                        /*
                        ColoredConsoleWrite(ConsoleColor.White, $"Failed to evolve {pokemon.PokemonId}. " +
                                                 $"EvolvePokemonOutProto.Result was {result}");

                        ColoredConsoleWrite(ConsoleColor.White, $"Due to above error, stopping evolving {pokemon.PokemonId}");
                        */
                    }
                } while (evolvePokemonOutProto.Result == 1);
                if (countOfEvolvedUnits > 0)
                    bhelper.Main.ColoredConsoleWrite(ConsoleColor.Cyan,
                        $"[{DateTime.Now.ToString("HH:mm:ss")}] Evolved {countOfEvolvedUnits} pieces of {pokemon.PokemonId} for {xpCount}xp");

                await Task.Delay(3000);
            }
        }


        /// <summary>
        /// Catch all nearby pokemon
        /// </summary>
        /// <param name="hero"></param>
        /// <returns></returns>
        public static async Task ExecuteCatchAllNearbyPokemons(Hero hero)
        {
            var inventory = await hero.Client.GetInventory();
            var mapObjects = await hero.Client.GetMapObjects();

            var pokemons = mapObjects.MapCells.SelectMany(i => i.CatchablePokemons);

            var pokemons2 = inventory.InventoryDelta.InventoryItems
                .Select(i => i.InventoryItemData?.Pokemon)
                .Where(p => p != null && p?.PokemonId > 0)
                .ToArray();

            PokemonId[] CatchOnlyThesePokemon = new[]
            {
                PokemonId.Rattata
            };

            foreach (var pokemon in pokemons)
            {
                string pokemonName;
                if (hero.ClientSettings.Language == "german")
                    pokemonName = Convert.ToString((PokemonId_german)(int)pokemon.PokemonId);
                else
                    pokemonName = Convert.ToString(pokemon.PokemonId);

                if (!CatchOnlyThesePokemon.Contains(pokemon.PokemonId) && hero.ClientSettings.CatchOnlySpecific)
                {
                    bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkYellow, $"[{DateTime.Now.ToString("HH:mm:ss")}] We didnt try to catch {pokemonName} because it is filtered");
                    return;
                }
                var update = await hero.Client.UpdatePlayerLocation(pokemon.Latitude, pokemon.Longitude);
                var encounterPokemonResponse = await hero.Client.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnpointId);
                var pokemonCP = encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp;
                CatchPokemonResponse caughtPokemonResponse;
                do
                {
                    if (hero.ClientSettings.RazzBerryMode == "cp")
                        if (pokemonCP > hero.ClientSettings.RazzBerrySetting)
                            await hero.Client.UseRazzBerry(hero.Client, pokemon.EncounterId, pokemon.SpawnpointId);
                    if (hero.ClientSettings.RazzBerryMode == "probability")
                        if (encounterPokemonResponse.CaptureProbability.CaptureProbability_.First() < hero.ClientSettings.RazzBerrySetting)
                            await hero.Client.UseRazzBerry(hero.Client, pokemon.EncounterId, pokemon.SpawnpointId);
                    caughtPokemonResponse = await hero.Client.CatchPokemon(pokemon.EncounterId, pokemon.SpawnpointId, pokemon.Latitude, pokemon.Longitude, MiscEnums.Item.ITEM_POKE_BALL, pokemonCP);
                    if (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape)
                        Game.softbanned_detection++;
                } while (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed || caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape);
                if (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess)
                {
                    bhelper.Main.ColoredConsoleWrite(ConsoleColor.Green, $"[{DateTime.Now.ToString("HH:mm:ss")}] " + Language.GetPhrases()["pokemon_caught"].Replace("[POKEMON]", pokemonName).Replace("[CP]", Convert.ToString(encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp)));
                    foreach (int xp in caughtPokemonResponse.Scores.Xp)
                        TotalExperience += xp;
                    TotalPokemon += 1;
                    Game.softbanned_detection = 0;
                }
                else
                    bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, $"[{DateTime.Now.ToString("HH:mm:ss")}] " + Language.GetPhrases()["pokemon_away"].Replace("[POKEMON]", pokemonName).Replace("[CP]", Convert.ToString(encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp)));

                if (Game.softbanned_detection > 10)
                    throw new PokemonGo.RocketAPI.Exceptions.SoftbannedException();
                if (hero.ClientSettings.TransferType == "leaveStrongest")
                    await bLogic.Pokemon.TransferAllButStrongestUnwantedPokemon(hero);
                else if (hero.ClientSettings.TransferType == "all")
                    await bLogic.Pokemon.TransferAllGivenPokemons(hero, pokemons2);
                else if (hero.ClientSettings.TransferType == "duplicate")
                    await bLogic.Pokemon.TransferDuplicatePokemon(hero);
                else if (hero.ClientSettings.TransferType == "cp")
                    await bLogic.Pokemon.TransferAllWeakPokemon(hero);

                await Task.Delay(150);
            }
        }


        /// <summary>
        /// Transfer duplicate weak pokemon to the doctor
        /// </summary>
        /// <param name="hero"></param>
        /// <returns></returns>
        public static async Task TransferAllButStrongestUnwantedPokemon(Hero hero)
        {
            PokemonId[] unwantedPokemonTypes = new[]
            {
                PokemonId.Pidgey,
                PokemonId.Rattata,
                PokemonId.Weedle,
                PokemonId.Zubat,
                PokemonId.Caterpie,
                PokemonId.Pidgeotto,
                PokemonId.Paras,
                PokemonId.Venonat,
                PokemonId.Psyduck,
                PokemonId.Poliwag,
                PokemonId.Slowpoke,
                PokemonId.Drowzee,
                PokemonId.Gastly,
                PokemonId.Goldeen,
                PokemonId.Staryu,
                PokemonId.Magikarp,
                PokemonId.Clefairy,
                PokemonId.Eevee,
                PokemonId.Tentacool,
                PokemonId.Dratini,
                PokemonId.Ekans,
                PokemonId.Jynx,
                PokemonId.Lickitung,
                PokemonId.Spearow,
                PokemonId.NidoranFemale,
                PokemonId.NidoranMale
            };
            GetInventoryResponse inventory = await hero.Client.GetInventory();
            var pokemons = inventory.InventoryDelta.InventoryItems
                .Select(i => i.InventoryItemData?.Pokemon)
                .Where(p => p != null && p?.PokemonId > 0)
                .ToArray();

            foreach (var unwantedPokemonType in unwantedPokemonTypes)
            {
                var pokemonOfDesiredType = pokemons.Where(p => p.PokemonId == unwantedPokemonType)
                    .OrderByDescending(p => p.Cp)
                    .ToList();

                var unwantedPokemon =
                    pokemonOfDesiredType.Skip(1) // keep the strongest one for potential battle-evolving
                        .ToList();

                //ColoredConsoleWrite(ConsoleColor.White, $"[{DateTime.Now.ToString("HH:mm:ss")}] Grinding {unwantedPokemon.Count} pokemons of type {unwantedPokemonType}");
                await bLogic.Pokemon.TransferAllGivenPokemons(hero, unwantedPokemon);
            }

            //ColoredConsoleWrite(ConsoleColor.White, $"[{DateTime.Now.ToString("HH:mm:ss")}] Finished grinding all the meat");
        }
        public static async Task ExecuteFarmingPokestopsAndPokemons(Hero _hero, PokemonGo.RocketAPI.GeneratedCode.GetInventoryResponse inventory)
        {
            var mapObjects = await _hero.Client.GetMapObjects();

            var pokeStops = mapObjects.MapCells.SelectMany(i => i.Forts).Where(i => i.Type == FortType.Checkpoint && i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime());

            foreach (var pokeStop in pokeStops)
            {
                var update = await _hero.Client.UpdatePlayerLocation(pokeStop.Latitude, pokeStop.Longitude);
                var fortInfo = await _hero.Client.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                var fortSearch = await _hero.Client.SearchFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);

                StringWriter PokeStopOutput = new StringWriter();
                PokeStopOutput.Write($"[{DateTime.Now.ToString("HH:mm:ss")}] ");
                if (fortInfo.Name != string.Empty)
                    PokeStopOutput.Write("PokeStop: " + fortInfo.Name);
                if (fortSearch.ExperienceAwarded != 0)
                    PokeStopOutput.Write($", XP: {fortSearch.ExperienceAwarded}");
                if (fortSearch.GemsAwarded != 0)
                    PokeStopOutput.Write($", Gems: {fortSearch.GemsAwarded}");
                if (fortSearch.PokemonDataEgg != null)
                    PokeStopOutput.Write($", Eggs: {fortSearch.PokemonDataEgg}");
                if (bLogic.Item.GetFriendlyItemsString(fortSearch.ItemsAwarded) != string.Empty)
                    PokeStopOutput.Write($", Items: {bLogic.Item.GetFriendlyItemsString(fortSearch.ItemsAwarded)} ");
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.Cyan, PokeStopOutput.ToString());

                if (fortSearch.ExperienceAwarded > 0)
                {
                    TotalExperience += (fortSearch.ExperienceAwarded);
                    Game.softbanned_detection = 0;
                }
                else
                    Game.softbanned_detection++;
                if (Game.softbanned_detection > 10)
                    throw new PokemonGo.RocketAPI.Exceptions.SoftbannedException();
                await Task.Delay(15000);
                await ExecuteCatchAllNearbyPokemons(_hero);
            }
        }



        public static async Task TransferAllWeakPokemon(Hero hero)
        {
            //ColoredConsoleWrite(ConsoleColor.White, $"[{DateTime.Now.ToString("HH:mm:ss")}] Firing up the meat grinder");

            PokemonId[] doNotTransfer = new[] //these will not be transferred even when below the CP threshold
            { // DO NOT EMPTY THIS ARRAY
                //PokemonId.Pidgey,
                //PokemonId.Rattata,
                //PokemonId.Weedle,
                //PokemonId.Zubat,
                //PokemonId.Caterpie,
                //PokemonId.Pidgeotto,
                //PokemonId.NidoranFemale,
                //PokemonId.Paras,
                //PokemonId.Venonat,
                //PokemonId.Psyduck,
                //PokemonId.Poliwag,
                //PokemonId.Slowpoke,
                //PokemonId.Drowzee,
                //PokemonId.Gastly,
                //PokemonId.Goldeen,
                //PokemonId.Staryu,
                //PokemonId.Dratini
                PokemonId.Magikarp,
                PokemonId.Eevee
            };
            GetInventoryResponse inventory = await hero.Client.GetInventory();
            var pokemons = inventory.InventoryDelta.InventoryItems
                                .Select(i => i.InventoryItemData?.Pokemon)
                                .Where(p => p != null && p?.PokemonId > 0)
                                .ToArray();

            //foreach (var unwantedPokemonType in unwantedPokemonTypes)
            {
                List<PokemonData> pokemonToDiscard;
                if (doNotTransfer.Count() != 0)
                    pokemonToDiscard = pokemons.Where(p => !doNotTransfer.Contains(p.PokemonId) && p.Cp < hero.ClientSettings.TransferCPThreshold).OrderByDescending(p => p.Cp).ToList();
                else
                    pokemonToDiscard = pokemons.Where(p => p.Cp < hero.ClientSettings.TransferCPThreshold).OrderByDescending(p => p.Cp).ToList();


                //var unwantedPokemon = pokemonOfDesiredType.Skip(1) // keep the strongest one for potential battle-evolving
                //                                          .ToList();
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.Gray, $"[{DateTime.Now.ToString("HH:mm:ss")}] Grinding {pokemonToDiscard.Count} pokemon below {hero.ClientSettings.TransferCPThreshold} CP.");
                await TransferAllGivenPokemons(hero, pokemonToDiscard);

            }

            bhelper.Main.ColoredConsoleWrite(ConsoleColor.Gray, $"[{DateTime.Now.ToString("HH:mm:ss")}] Finished grinding all the meat");
        }

        public static async Task TransferAllGivenPokemons(Hero hero, IEnumerable<PokemonData> unwantedPokemons, float keepPerfectPokemonLimit = 80.0f)
        {
            foreach (var pokemon in unwantedPokemons)
            {
                if (Perfect(pokemon) >= keepPerfectPokemonLimit) continue;
                bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, $"[{DateTime.Now.ToString("HH:mm:ss")}] Pokemon {pokemon.PokemonId} with {pokemon.Cp} CP has IV percent less than {keepPerfectPokemonLimit}%");

                if (pokemon.Favorite == 0)
                {
                    var transferPokemonResponse = await hero.Client.TransferPokemon(pokemon.Id);        // REQUEST: TRANSFER POKEMON

                    /*
                    ReleasePokemonOutProto.Status {
                        UNSET = 0;
                        SUCCESS = 1;
                        POKEMON_DEPLOYED = 2;
                        FAILED = 3;
                        ERROR_POKEMON_IS_EGG = 4;
                    }*/
                    string pokemonName;
                    if (hero.ClientSettings.Language == "german")
                        pokemonName = Convert.ToString((PokemonId_german)(int)pokemon.PokemonId);
                    else
                        pokemonName = Convert.ToString(pokemon.PokemonId);
                    if (transferPokemonResponse.Status == 1)
                    {
                        bhelper.Main.ColoredConsoleWrite(ConsoleColor.Magenta, $"[{DateTime.Now.ToString("HH:mm:ss")}] Transferred {pokemonName} with {pokemon.Cp} CP");
                    }
                    else
                    {
                        var status = transferPokemonResponse.Status;

                        bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, $"[{DateTime.Now.ToString("HH:mm:ss")}] Somehow failed to transfer {pokemonName} with {pokemon.Cp} CP. " +
                                                 $"ReleasePokemonOutProto.Status was {status}");
                    }

                    await Task.Delay(3000);
                }
            }
        }

        public static async Task TransferDuplicatePokemon(Hero hero)
        {

            PokemonId[] Whitelist = new[] //these will not be transferred even when duplicate
            { 
                //PokemonId.Pidgey,
                //PokemonId.Rattata,
                //PokemonId.Weedle,
                //PokemonId.Zubat,
                //PokemonId.Caterpie,
                //PokemonId.Pidgeotto,
                //PokemonId.NidoranFemale,
                //PokemonId.Paras,
                //PokemonId.Venonat,
                //PokemonId.Psyduck,
                //PokemonId.Poliwag,
                //PokemonId.Slowpoke,
                //PokemonId.Drowzee,
                //PokemonId.Gastly,
                //PokemonId.Goldeen,
                //PokemonId.Staryu,
                //PokemonId.Dratini
                //PokemonId.Magikarp,
                //PokemonId.Eevee,
                //etc
                PokemonId.Missingno // dont remove
            };
            GetInventoryResponse inventory = await hero.Client.GetInventory();
            var allpokemons =
                inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.Pokemon)
                    .Where(p => p != null && p?.PokemonId > 0);

            var dupes = allpokemons.OrderBy(x => x.Cp).Select((x, i) => new { index = i, value = x })
                .GroupBy(x => x.value.PokemonId)
                .Where(x => x.Skip(1).Any());

            for (var i = 0; i < dupes.Count(); i++)
            {
                for (var j = 0; j < dupes.ElementAt(i).Count() - 1; j++)
                {
                    var dubpokemon = dupes.ElementAt(i).ElementAt(j).value;
                    if (dubpokemon.Favorite == 0 && !Whitelist.Contains(dubpokemon.PokemonId))
                    {
                        var transfer = await hero.Client.TransferPokemon(dubpokemon.Id);    // REQUEST: TRANSFER POKEMON
                        await Task.Delay(333);
                        string pokemonName;
                        if (hero.ClientSettings.Language == "german")
                            pokemonName = Convert.ToString((PokemonId_german)(int)dubpokemon.PokemonId);
                        else
                            pokemonName = Convert.ToString(dubpokemon.PokemonId);
                        bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkGreen,
                            $"[{DateTime.Now.ToString("HH:mm:ss")}] Transferred {pokemonName} with {dubpokemon.Cp} CP (Highest is {dupes.ElementAt(i).Last().value.Cp})");

                    }
                }
            }
        }


        public static float Perfect(PokemonData poke)
        {
            return ((float)(poke.IndividualAttack + poke.IndividualDefense + poke.IndividualStamina) / (3.0f * 15.0f)) * 100.0f;
        }
    }
}
