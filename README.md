# Pokemon-Go-Rocket-API
![alt tag](https://github.com/Sen66/PokemonGo-Bot/blob/master/screenshot.jpg)

A Pokemon Go bot in C#

## Features
* PTC / Google Login
* Get Map Objects and Inventory
* Search for Gyms / Pokéstops / Spawns
* Farm Pokéstops
* Farm all Pokémon in the neighbourhood
* Evolve Pokémon
* Transfer Pokémon
* Auto-Recycle uneeded items
* Output levelups and needed XP for levelup
* Output Username, Level, Stardust, XP/hour, Pokemon/hour, Runtime in Console Title
* German/English translation
* Automatic use of Razzberries/Lucky Eggs/Incenses
* Automatic Update checker
* Automatic get levelup rewards
* Automatic unban after getting banned

## Getting Started

Go to App.config -> Edit the Settings you like -> Build and Run (CTRL+F5)

# Settings
## AuthType
* *Google* - Google login via GPSoauth2
* *Ptc* - Pokemon Trainer Club login with username/password combination

## Username
* *username* for PTC account. Email for Google account

## Password
* *password* for PTC account. Password for Google account

## GoogleRefreshToken
* *GoogleRefreshToken* - Dont touch this

## DefaultLatitude
* *12.345678* - Latitude of your location you want to use the bot in. Number between -90 and +90. Doesn't matter how many numbers stand after the comma.

## DefaultLongitude
* *123.456789* - Longitude of your location you want to use the bot in. Number between -180 and +180. Doesn't matter how many numbers stand after the comma.

## LevelOutput
* *true* - Give a purple output at every levelup
* *false* - Dont notify on levelups

## Recycler
* *false* Recycler not active.
* *true* Recycler active.

## RecycleItemsInterval
* *seconds* After X seconds it recycles items from the filter in *Settings.cs*.

## Language
* *System/en_EN/de_DE* Change bot locale.

## RazzBerryMode
* *cp* - Use RazzBerry when Pokemon is over specific CP.
* *probability* - Use RazzBerry when Pokemon catch chance is under a specific percentage.

## RazzBerrySetting
* *value* CP: Use RazzBerry when Pokemon is over this value | Probability Mode: Use Razzberry when % of catching is under this value

## TransferType
* *none* - disables transferring
* *cp* - transfers all pokemon below the CP threshold in the app.config, EXCEPT for those types specified in program.cs in TransferAllWeakPokemon
* *leaveStrongest* - transfers all but the highest CP pokemon of each type SPECIFIED IN program.cs in TransferAllButStrongestUnwantedPokemon (those that aren't specified are untouched)
* *duplicate* - same as above but for all pokemon (no need to specify type), (will not transfer favorited pokemon)
* *all* - transfers all pokemon

## TransferCPThreshold
* *CP* transfers all pokemons with less CP than this value.

## EvolveAllGivenPokemons
* *false* Evolves no pokemons.
* *true* Evolves all pokemoms.

# Credits
@Sen66
@Fannboii
@AeonLucid
@DetectiveSquirrel
@Ar1i
@drbloody
@0x1911 & mate
