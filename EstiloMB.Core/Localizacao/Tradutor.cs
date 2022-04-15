using System.Collections.Generic;
using System.Linq;

namespace EstiloMB.Core
{
    public class Tradutor
    {
        private static IList<Dicionario> Dicionarios = new List<Dicionario>();
        private string _culture;

        public Tradutor(string culture)
        {
            _culture = culture;
        }

        public string this[string text]
        {
            get
            {
                Dicionario dicionario = Dicionarios.FirstOrDefault(e => e.Culture == _culture);
                if (dicionario == null) { return text; }

                string result = null;
                return dicionario.Dictionary.TryGetValue(text, out result) ? result : text;
            }
        }

        public void Add(string culture, Dictionary<string, string> dicionario)
        {
            if (Dicionarios.Any(e => e.Culture == culture)) { return; }

            Dicionarios.Add(new Dicionario(culture, dicionario));
        }

        private class Dicionario
        {
            public string Culture { get; }
            public Dictionary<string, string> Dictionary { get; }

            public Dicionario(string culture, Dictionary<string, string> dictionary)
            {
                Culture = culture;
                Dictionary = dictionary;
            }
        }
    }
}
