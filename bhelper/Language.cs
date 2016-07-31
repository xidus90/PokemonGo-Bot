using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace bhelper
{
    public class Language
    {
        public static string LanguageFile { get; set; }

        public static Dictionary<string, string> dicPokemon = new Dictionary<string, string>();
        public static Dictionary<string, string> dicPhrases = new Dictionary<string, string>();

        private static Dictionary<string, string> GetLanguageDictionary(string lang, string type)
        {
            XDocument doc = XDocument.Load("Languages/" + lang + ".xml");
            var dic = doc.Root.Elements(type)
                   .ToDictionary(c => (string)c.Attribute("key").Value,
                                 c => (string)c.Attribute("value").Value);

            return dic;
        }

        public static void LoadLanguageFile(string lang)
        {
            try
            {
                dicPokemon = GetLanguageDictionary(lang, "pokemon");
                dicPhrases = GetLanguageDictionary(lang, "phrase");
                LanguageFile = lang;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Dictionary<string, string> GetPokemon()
        {
            return dicPokemon;
        }

        public static Dictionary<string, string> GetPhrases()
        {
            return dicPhrases;
        }
    }
}