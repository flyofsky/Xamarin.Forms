using Xamarin.Forms.Platform.Tizen;
using Xamarin.Forms.Controls;
using ElmSharp;
using Tizen.Appium;

namespace Xamarin.Forms.ControlGallery.Tizen
{
	class MainApplication : FormsApplication
	{
		internal static EvasObject NativeParent { get; private set; }
		protected override void OnCreate()
		{
			base.OnCreate();
			NativeParent = MainWindow;
			LoadApplication(new App());
		}

		static void Main(string[] args)
		{
			var app = new MainApplication();
			FormsMaps.Init("HERE", "write-your-API-key-here");
			global::Xamarin.Forms.Platform.Tizen.Forms.Init(app);
			TizenAppium.StartService(app);
			app.Run(args);
		}
	}
}
