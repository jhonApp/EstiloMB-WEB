using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EstiloMB.MVC
{
    public class Localization
    {
        private static IList<Dictionary> _dictionaries = new List<Dictionary>();

        IHttpContextAccessor _httpContextAcessor;
        private string _culture;

        public Localization(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAcessor = httpContextAccessor;
            _culture = httpContextAccessor.HttpContext.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
        }

        public void SetCulture(string culture)
        {
            _httpContextAcessor.HttpContext.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName, culture,
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
        }

        public string this[string value]
        {
            get { return GetString(value); }
        }

        public string GetString(string value)
        {
            Dictionary dictionary = _dictionaries.FirstOrDefault(d => d.Culture == _culture);
            return dictionary != null ? dictionary.GetValue(value) : value;
        }

        public string GetCulture()
        {
            return _culture;
        }

        public static void AddDictionary(string culture, Dictionary<string, string> dictionary)
        {
            Dictionary dicionario = _dictionaries.FirstOrDefault(e => e.Culture == culture);
            if (dicionario != null)
            {
                dicionario.AddRange(dictionary);
            }
            else
            {
                dicionario = new Dictionary(culture);
                dicionario.AddRange(dictionary);
                _dictionaries.Add(dicionario);
            }
        }
    }
}