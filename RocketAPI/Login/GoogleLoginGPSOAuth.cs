using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DankMemes.GPSOAuthSharp;
using Newtonsoft.Json;
using PokemonGo.RocketAPI.Exceptions;

namespace PokemonGo.RocketAPI.Login
{
    public static class GoogleLoginGPSOAuth
    {
        public static string DoLogin(string username, string password)
        {
            GPSOAuthClient client = new GPSOAuthClient(username, password);
            Dictionary<string, string> response = client.PerformMasterLogin();

            if (response.ContainsKey("Error"))
                if (response.ContainsValue("NeedsBrowser"))
                {
                    Console.WriteLine("Your Google Account uses 2Faktor Verification\n" +
                        "You need to create a password on this site:\n" +
                        "https://security.google.com/settings/security/apppasswords" + "\n" +
                        "And use that one instead of the one you current use");
                    Console.ReadLine();


                }
                else if (response.ContainsValue("BadAuth"))
                    Console.WriteLine("Credentials wrong!");
                else
                    throw new GoogleException(response["Error"]);

            if (!response.ContainsKey("Auth"))
                throw new GoogleOfflineException();

            Dictionary<string, string> oauthResponse = client.PerformOAuth(response["Token"],
                "audience:server:client_id:848232511240-7so421jotr2609rmqakceuu1luuq0ptb.apps.googleusercontent.com",
                "com.nianticlabs.pokemongo",
                "321187995bc7cdc2b5fc91b11a96e2baa8602c62");

            if (!oauthResponse.ContainsKey("Auth"))
                throw new GoogleOfflineException();

            return oauthResponse["Auth"];
        }
    }
}
