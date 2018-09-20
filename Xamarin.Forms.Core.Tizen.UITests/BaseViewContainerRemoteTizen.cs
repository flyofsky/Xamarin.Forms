using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Core.UITests
{
	internal abstract partial class BaseViewContainerRemote
	{
		bool TryConvertFloat<T>(object prop, out T result)
		{
			result = default(T);

			if (prop.GetType() == typeof(string) && typeof(T) == typeof(float))
			{
				float val;
				if (float.TryParse((string)prop, out val))
				{
					result = (T)((object) val);
					return true;
				}
			}
			return false;
		}

		bool TryConvertDouble<T>(object prop, out T result)
		{
			result = default(T);

			if (prop.GetType() == typeof(string) && typeof(T) == typeof(double))
			{
				double val;
				if (double.TryParse((string)prop, out val))
				{
					result = (T)((object)val);
					return true;
				}
			}
			return false;
		}

		bool TryConvertFont<T>(object prop, out T result)
		{
			result = default(T);
			if (prop.GetType() == typeof(string) && typeof(T) == typeof(Font))
			{
				FontAttributes fontAttrs = FontAttributes.None;
				string str = (string)prop;
				if (str.Contains("FontAttributes: Bold"))
				{
					fontAttrs = FontAttributes.Bold;
				}
				else if (str.Contains("FontAttributes: Italic"))
				{
					fontAttrs = FontAttributes.None;
				}
				result = (T)((object)new Font().WithAttributes(fontAttrs));
				return true;
			}

			return false;
		}
	}
}
