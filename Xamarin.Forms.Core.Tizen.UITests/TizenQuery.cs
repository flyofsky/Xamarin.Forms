using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	internal class TizenQuery
	{
		public static TizenQuery FromQuery(Func<AppQuery, AppQuery> query)
		{
			var raw = GetRawQuery(query);
			return FromRaw(raw);
		}

		public static TizenQuery FromMarked(string marked)
		{
			return new TizenQuery("*", marked, $"* '{marked}'");
		}

		public static TizenQuery FromRaw(string raw)
		{
			var match = Regex.Match(raw, @"(.*)\s(marked|text):'((.|\n)*)'");

			var controlType = match.Groups[1].Captures[0].Value;
			var marked = match.Groups[3].Captures[0].Value;

			// Just ignoring everything else for now (parent, index statements, etc)
			var result = new TizenQuery(controlType, marked, raw);
			return result;
		}

		static string GetRawQuery(Func<AppQuery, AppQuery> query = null)
		{
			if (query == null)
			{
				return string.Empty;
			}

			// When we pull out the iOS query it's got any instances of "'" escaped with "\", need to fix that up
			return query(new AppQuery(QueryPlatform.iOS)).ToString().Replace("\\'", "'");
		}

		TizenQuery(string controlType, string marked, string raw)
		{
			ControlType = controlType;
			Marked = marked;
			Raw = raw;
		}

		public string ControlType { get; }

		public string Marked { get; }

		public string Raw { get; }

		public override string ToString()
		{
			return $"{nameof(ControlType)}: {ControlType}, {nameof(Marked)}: {Marked}";
		}
	}
}