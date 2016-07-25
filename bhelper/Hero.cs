using System;
using PokemonGo.RocketAPI;

namespace bhelper
{
    public struct Hero
    {
        public ISettings ClientSettings { get; set; }
        public Client Client { get; set; }
        public int Currentlevel { get; set; }
        public int TotalExperience { get; set; }
        public int TotalPokemon { get; set; }
        public double TotalKmWalked { get; set; }
        public DateTime TimeStarted { get; set; }
        public bool AllowedToRun { get; set; }

        public Hero(Client client)
        {
            Client = client;
            ClientSettings = new Settings();

            Currentlevel = -1;
            TotalExperience = 0;
            TotalPokemon = 0;
            TotalKmWalked = 0;
            TimeStarted = DateTime.Now;
            AllowedToRun = true;

        }
    }
}