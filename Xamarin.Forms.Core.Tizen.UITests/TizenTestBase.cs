using System;
using System.Diagnostics;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Tizen;
using OpenQA.Selenium.Remote;
using Xamarin.UITest;

namespace Xamarin.Forms.Core.UITests
{
	public class TizenTestBase
	{
		//protected const string TizenApplicationDriverUrl = "http://127.0.0.1:4723/wd/hub";
		protected const string TizenApplicationDriverUrl = "http://10.113.111.156:4723/wd/hub";
		protected static TizenDriver<TizenElement> Session;

		public static IApp ConfigureApp()
		{
			if (Session == null)
			{
				//TODO
				DesiredCapabilities appCapabilities = new DesiredCapabilities();

				appCapabilities.SetCapability("platformName", "Tizen");
				//TM1
				//appCapabilities.SetCapability("deviceName", "0000d84200006200");
				//For Emul
				appCapabilities.SetCapability("deviceName", "emulator-26101");

				appCapabilities.SetCapability("appPackage", "ControlGallery.Tizen");
				//appCapabilities.SetCapability("app", "ControlGallery.Tizen-1.0.0.tpk");
				//appCapabilities.SetCapability("reboot", "true");

				Session = new TizenDriver<TizenElement>(new Uri(TizenApplicationDriverUrl), appCapabilities);
				Assert.IsNotNull(Session);
				//Session.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
				Reset();
			}

			return new TizenDriverApp(Session);
		}

		internal static void HandleAppClosed(Exception ex)
		{
			if (ex is InvalidOperationException && ex.Message == "Currently selected window has been closed")
			{
				Session = null;
			}
		}

		public static void Reset()
		{
			try
			{
				Debug.WriteLine($">>>>> TizenTestBase Reset");
				Session?.Keyboard?.PressKey(Keys.Escape);
			}
			catch (Exception ex)
			{
				HandleAppClosed(ex);
				Debug.WriteLine($">>>>> TizenTestBase ConfigureApp 49: {ex}");
				throw;
			}
		}
	}
}
