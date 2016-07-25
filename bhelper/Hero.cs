using System;
using PokemonGo.RocketAPI;

namespace bhelper
{
    public struct Hero
    {
        public Client Client;
        public int Currentlevel { get; set; }
        public int TotalExperience { get; set; }
        public int TotalPokemon { get; set; }
        public double TotalKmWalked { get; set; }
        public DateTime TimeStarted { get; set; }

        public Hero(Client client)
        {
            Client = client;
            Currentlevel = 1;
            TotalExperience = 1;
            TotalPokemon = 1;
            TotalKmWalked = 1;
            TimeStarted = DateTime.Now;
        }
    }
}