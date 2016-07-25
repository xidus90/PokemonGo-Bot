using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllEnum;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.GeneratedCode;

namespace bLogic
{
    public class Pokemon
    {

        public static async Task CheckEggsHatched(Client client, double distanceWalkedTotal)
        {
            try
            {
                var inventory = await client.GetInventory();
                var eggkmwalked = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.EggIncubators.EggIncubator).ToArray();
                foreach (var v in eggkmwalked)
                    if (v != null)
                        if (v.TargetKmWalked > distanceWalkedTotal)
                            bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkYellow, "One of your eggs is hatched");
            }
            catch (Exception) { }
        }

        public static async Task ExecuteCatchAllNearbyPokemons(Client client, ISettings clientSettings)
        {
            var mapObjects = await client.GetMapObjects();
            
            var pokemons = mapObjects.MapCells.SelectMany(i => i.CatchablePokemons);

            var inventory2 = await client.GetInventory();
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
                if (clientSettings.Language == "german")
                    pokemonName = Convert.ToString((PokemonId_german)(int)pokemon.PokemonId);
                else
                    pokemonName = Convert.ToString(pokemon.PokemonId);

                if (!CatchOnlyThesePokemon.Contains(pokemon.PokemonId) && clientSettings.CatchOnlySpecific)
                {
                    bhelper.Main.ColoredConsoleWrite(ConsoleColor.DarkYellow, $"[{DateTime.Now.ToString("HH:mm:ss")}] We didnt try to catch {pokemonName} because it is filtered");
                    return;
                }
                var update = await client.UpdatePlayerLocation(pokemon.Latitude, pokemon.Longitude);
                var encounterPokemonResponse = await client.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnpointId);
                var pokemonCP = encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp;
                CatchPokemonResponse caughtPokemonResponse;
                do
                {
                    if (clientSettings.RazzBerryMode == "cp")
                        if (pokemonCP > clientSettings.RazzBerrySetting)
                            await client.UseRazzBerry(client, pokemon.EncounterId, pokemon.SpawnpointId);
                    if (clientSettings.RazzBerryMode == "probability")
                        if (encounterPokemonResponse.CaptureProbability.CaptureProbability_.First() < clientSettings.RazzBerrySetting)
                            await client.UseRazzBerry(client, pokemon.EncounterId, pokemon.SpawnpointId);
                    caughtPokemonResponse = await client.CatchPokemon(pokemon.EncounterId, pokemon.SpawnpointId, pokemon.Latitude, pokemon.Longitude, MiscEnums.Item.ITEM_POKE_BALL, pokemonCP); ; //note: reverted from settings because this should not be part of settings but part of logic
                } while (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed || caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape);
                if (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess)
                {
                    bhelper.Main.ColoredConsoleWrite(ConsoleColor.Green, $"[{DateTime.Now.ToString("HH:mm:ss")}] We caught a {pokemonName} with {encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp} CP");
                    foreach (int xp in caughtPokemonResponse.Scores.Xp)
                        TotalExperience += xp;
                    TotalPokemon += 1;
                }
                else
                    bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, $"[{DateTime.Now.ToString("HH:mm:ss")}] {pokemonName} with {encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp} CP got away..");

                if (clientSettings.TransferType == "leaveStrongest")
                    await TransferAllButStrongestUnwantedPokemon(client);
                else if (clientSettings.TransferType == "all")
                    await TransferAllGivenPokemons(client, pokemons2);
                else if (clientSettings.TransferType == "duplicate")
                    await TransferDuplicatePokemon(client);
                else if (clientSettings.TransferType == "cp")
                    await TransferAllWeakPokemon(client, clientSettings.TransferCPThreshold);

                await Task.Delay(3000);
            }
        }
    }
}
