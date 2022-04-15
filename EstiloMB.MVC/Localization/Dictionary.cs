using System;
using System.Collections;
using System.Collections.Generic;

namespace EstiloMB.MVC
{
	internal class Dictionary : IEnumerable
	{
		public string Culture { get; }

		private Dictionary<string, string> _values;

		public Dictionary(string culture)
		{
			Culture = culture ?? throw new ArgumentException("The culture argument is required.");
			_values = new Dictionary<string, string>();
		}

		public void Add(string key, string value)
		{
			_values.Add(key, value);
		}

		public string GetValue(string value)
		{
			if (value == null) { return null; }

			string result = null;
			return _values.TryGetValue(value, out result) ? result : value;
		}

		public void AddRange(Dictionary dictionary)
		{
			foreach (KeyValuePair<string, string> value in dictionary._values)
			{
				_values.TryAdd(value.Key, value.Value);
			}
		}

		public void AddRange(Dictionary<string, string> dictionary)
		{
			foreach (KeyValuePair<string, string> value in dictionary)
			{
				_values.TryAdd(value.Key, value.Value);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_values).GetEnumerator();
		}
	}
}