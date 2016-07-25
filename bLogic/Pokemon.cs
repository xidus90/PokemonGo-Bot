using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllEnum;
using bhelper;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.GeneratedCode;

namespace bLogic
{
    public class Pokemon
    {

        public static async Task CheckEggsHatched(Hero hero)
        {
            try
            {
                var inventory = await hero.Client.GetInventory();
                var eggkmwalked = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.EggIncubators.EggIncubator).ToArray();
                foreach (var v in eggkmwalked)
                    if (v != null)
                        if (v.TargetKmWalked > hero.TotalKmWalked)
                            bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkYellow, "One of your eggs is hatched");
            }
            catch (Exception) { }
        }

        public static async Task ExecuteCatchAllNearbyPokemons(Hero hero)
        {
            var mapObjects = await hero.Client.GetMapObjects();
            
            var pokemons = mapObjects.MapCells.SelectMany(i => i.CatchablePokemons);

            var inventory2 = await hero.Client.GetInventory();
            var pokemons2 = inventory2.InventoryDelta.InventoryItems
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
                    caughtPokemonResponse = await hero.Client.CatchPokemon(pokemon.EncounterId, pokemon.SpawnpointId, pokemon.Latitude, pokemon.Longitude, MiscEnums.Item.ITEM_POKE_BALL, pokemonCP); ; //note: reverted from settings because this should not be part of settings but part of logic
                } while (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed || caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape);
                if (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess)
                {
                    bhelper.Main.ColoredConsoleWrite(ConsoleColor.Green, $"[{DateTime.Now.ToString("HH:mm:ss")}] We caught a {pokemonName} with {encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp} CP");
                    foreach (int xp in caughtPokemonResponse.Scores.Xp)
                        hero.TotalExperience += xp;
                    hero.TotalPokemon += 1;
                }
                else
                    bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, $"[{DateTime.Now.ToString("HH:mm:ss")}] {pokemonName} with {encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp} CP got away..");

                // TODO:: TransferAllButStrongestUnwantedPokemon
               /* if (hero.ClientSettings.TransferType == "leaveStrongest")
                    await TransferAllButStrongestUnwantedPokemon(hero.Client);
                else if (hero.ClientSettings.TransferType == "all")
                    await TransferAllGivenPokemons(hero.Client, pokemons2);
                else if (hero.ClientSettings.TransferType == "duplicate")
                    await TransferDuplicatePokemon(hero.Client);
                else if (hero.ClientSettings.TransferType == "cp")
                    await TransferAllWeakPokemon(hero.Client, hero.ClientSettings.TransferCPThreshold); */

                await Task.Delay(3000);
            }
        }
    }
}
