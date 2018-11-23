using System;
using OpenQA.Selenium.Appium.Tizen;
using Xamarin.UITest;

namespace Xamarin.Forms.Core.UITests
{
	internal class TizenTestServer : ITestServer
	{
		readonly TizenDriver<TizenElement> _session;

		public TizenTestServer(TizenDriver<TizenElement> session)
		{
			_session = session;
		}

		public string Post(string endpoint, object arguments = null)
		{
			throw new NotImplementedException();
		}

		public string Put(string endpoint, byte[] data)
		{
			throw new NotImplementedException();
		}

		public string Get(string endpoint)
		{
			if (endpoint == "version")
			{
				try
				{
					return _session.CurrentWindowHandle;
				}
				catch (Exception exception)
				{
					TizenTestBase.HandleAppClosed(exception);
					throw;
				}
			}

			return endpoint;
		}
	}
}
