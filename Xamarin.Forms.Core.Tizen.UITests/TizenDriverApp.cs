using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Appium.Tizen;
using OpenQA.Selenium.Remote;
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Queries.Tokens;

namespace Xamarin.Forms.Core.UITests
{
	public class TizenDriverApp : IApp
	{
		public const string AppMainPageId = "ControlGalleryMainPage";

		readonly Dictionary<string, string> _controlNameToTag = new Dictionary<string, string>
		{
			{ "button", "ControlType.Button" }
		};

		readonly TizenDriver<TizenElement> _session;

		readonly Dictionary<string, string> _translatePropertyAccessor = new Dictionary<string, string>
		{
			{ "getAlpha", "Opacity" },
			{ "isIndeterminate", "IsRunning" },
			{ "getRotation", "Rotation" },
			{ "getRotationX", "RotationX" },
			{ "getRotationY", "RotationY" },
			{ "getScaleX", "Scale" },
			{ "getScaleY", "Scale" },
			{ "isEnabled", "IsEnabled" },
			{ "getBackground", "BorderColor" },
			{ "getTypeface", "Font" },
			{ "isBold", "Font" },
			{ "getText", "Text" },
		};

		TizenElement _viewPort;

		TizenElement _window;

		public TizenDriverApp(TizenDriver<TizenElement> session)
		{
			_session = session;
			TestServer = new TizenTestServer(_session);
		}

		public void Back()
		{
			_session.Navigate().Back();
			Thread.Sleep(2000);
		}

		public void ClearText(Func<AppQuery, AppQuery> query)
		{
			QueryTizen(query).First().Clear();
		}

		public void ClearText(Func<AppQuery, AppWebQuery> query)
		{
			throw new NotImplementedException();
		}

		public void ClearText(string marked)
		{
			QueryTizen(marked).First().Clear();
		}

		public void ClearText()
		{
			throw new NotImplementedException();
		}

		public IDevice Device { get; }

		public void DismissKeyboard()
		{
			throw new NotImplementedException();
		}

		public void DoubleTap(Func<AppQuery, AppQuery> query)
		{
			DoubleTap(TizenQuery.FromQuery(query));
		}

		public void DoubleTap(string marked)
		{
			DoubleTap(TizenQuery.FromMarked(marked));
		}

		public void DoubleTapCoordinates(float x, float y)
		{
			DoubleTap(null, x, y);
		}

		public void DragAndDrop(Func<AppQuery, AppQuery> from, Func<AppQuery, AppQuery> to)
		{
			throw new NotImplementedException();
		}

		public void DragAndDrop(string from, string to)
		{
			throw new NotImplementedException();
		}

		public void DragCoordinates(float fromX, float fromY, float toX, float toY)
		{
			TouchAction action = new TouchAction(_session);
			action.Press(fromX, fromY);
			action.MoveTo(toX, toY);
			action.Release();
			action.Perform();
			Thread.Sleep(2000);
		}

		public void EnterText(string text)
		{
			throw new NotImplementedException();
		}

		public void EnterText(string marked, string text)
		{
			TizenElement element = QueryTizen(marked).First();
			element.Clear();
			element.SendKeys(text);
			Thread.Sleep(1000);
		}

		public void EnterText(Func<AppQuery, AppQuery> query, string text)
		{
			TizenElement element = QueryTizen(query).First();
			element.Clear();
			element.SendKeys(text);
			Thread.Sleep(1000);
		}

		public void EnterText(Func<AppQuery, AppWebQuery> query, string text)
		{
			throw new NotImplementedException();
		}

		public AppResult[] Flash(Func<AppQuery, AppQuery> query = null)
		{
			throw new NotImplementedException();
		}

		public AppResult[] Flash(string marked)
		{
			throw new NotImplementedException();
		}

		public object Invoke(string methodName, object argument = null)
		{
			throw new NotImplementedException();
		}

		public object Invoke(string methodName, object[] arguments)
		{
			throw new NotImplementedException();
		}

		public void PinchToZoomIn(Func<AppQuery, AppQuery> query, TimeSpan? duration = null)
		{
			throw new NotImplementedException();
		}

		public void PinchToZoomIn(string marked, TimeSpan? duration = null)
		{
			throw new NotImplementedException();
		}

		public void PinchToZoomInCoordinates(float x, float y, TimeSpan? duration)
		{
			throw new NotImplementedException();
		}

		public void PinchToZoomOut(Func<AppQuery, AppQuery> query, TimeSpan? duration = null)
		{
			throw new NotImplementedException();
		}

		public void PinchToZoomOut(string marked, TimeSpan? duration = null)
		{
			throw new NotImplementedException();
		}

		public void PinchToZoomOutCoordinates(float x, float y, TimeSpan? duration)
		{
			throw new NotImplementedException();
		}

		public void PressEnter()
		{
			throw new NotImplementedException();
		}

		public void PressVolumeDown()
		{
			throw new NotImplementedException();
		}

		public void PressVolumeUp()
		{
			throw new NotImplementedException();
		}

		public AppPrintHelper Print { get; }

		public AppResult[] Query(Func<AppQuery, AppQuery> query = null)
		{
			ReadOnlyCollection<TizenElement> elements = QueryTizen(TizenQuery.FromQuery(query));
			return elements.Select(ToAppResult).ToArray();
		}

		public AppResult[] Query(string marked)
		{
			ReadOnlyCollection<TizenElement> elements = QueryTizen(marked);
			return elements.Select(ToAppResult).ToArray();
		}

		public AppWebResult[] Query(Func<AppQuery, AppWebQuery> query)
		{
			throw new NotImplementedException();
		}

		public T[] Query<T>(Func<AppQuery, AppTypedSelector<T>> query)
		{
			AppTypedSelector<T> appTypedSelector = query(new AppQuery(QueryPlatform.iOS));

			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
			Type selectorType = appTypedSelector.GetType();
			PropertyInfo tokensProperty = selectorType.GetProperties(bindingFlags)
				.First(t => t.PropertyType == typeof(IQueryToken[]));

			var tokens = (IQueryToken[])tokensProperty.GetValue(appTypedSelector);

			string selector = tokens[0].ToQueryString(QueryPlatform.iOS);
			string invoke = tokens[1].ToCodeString();

			TizenQuery tizenQuery = TizenQuery.FromRaw(selector);
			string attribute = _translatePropertyAccessor[invoke.Substring(8).Replace("\")", "")];

			ReadOnlyCollection<TizenElement> elements = QueryTizen(tizenQuery);

			return elements.Select(e => (T)Convert.ChangeType(e.GetAttribute(attribute), typeof(T))).ToArray();
		}

		public string[] Query(Func<AppQuery, InvokeJSAppQuery> query)
		{
			throw new NotImplementedException();
		}

		public void Repl()
		{
			throw new NotImplementedException();
		}

		public FileInfo Screenshot(string title)
		{
			string filename = $"{title}.png";

			Screenshot screenshot = _session.GetScreenshot();
			screenshot.SaveAsFile(filename, ScreenshotImageFormat.Png);
			return new FileInfo(filename);
		}

		public void ScrollDown(Func<AppQuery, AppQuery> withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto,
			double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true)
		{
			if (withinQuery == null)
			{
				Scroll(null, true);
				return;
			}

			TizenQuery tizenQuery = TizenQuery.FromQuery(withinQuery);
			Scroll(tizenQuery, true);
		}

		public void ScrollDown(string withinMarked, ScrollStrategy strategy = ScrollStrategy.Auto,
			double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true)
		{
			TizenQuery tizenQuery = TizenQuery.FromMarked(withinMarked);
			Scroll(tizenQuery, true);
		}

		public void ScrollDownTo(string toMarked, string withinMarked = null, ScrollStrategy strategy = ScrollStrategy.Auto,
			double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			ScrollTo(TizenQuery.FromMarked(toMarked), withinMarked == null ? null : TizenQuery.FromMarked(withinMarked), timeout);
		}

		public void ScrollDownTo(Func<AppQuery, AppWebQuery> toQuery, string withinMarked,
			ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollDownTo(Func<AppQuery, AppQuery> toQuery, Func<AppQuery, AppQuery> withinQuery = null,
			ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			ScrollTo(TizenQuery.FromQuery(toQuery), withinQuery == null ? null : TizenQuery.FromQuery(withinQuery), timeout);
		}

		public void ScrollDownTo(Func<AppQuery, AppWebQuery> toQuery, Func<AppQuery, AppQuery> withinQuery = null,
			ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollTo(string toMarked, string withinMarked = null, ScrollStrategy strategy = ScrollStrategy.Auto,
			double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollUp(Func<AppQuery, AppQuery> query = null, ScrollStrategy strategy = ScrollStrategy.Auto,
			double swipePercentage = 0.67, int swipeSpeed = 500,
			bool withInertia = true)
		{
			if (query == null)
			{
				Scroll(null, false);
				return;
			}

			TizenQuery tizenQuery = TizenQuery.FromQuery(query);
			Scroll(tizenQuery, false);
		}

		public void ScrollUp(string withinMarked, ScrollStrategy strategy = ScrollStrategy.Auto,
			double swipePercentage = 0.67, int swipeSpeed = 500,
			bool withInertia = true)
		{
			TizenQuery tizenQuery = TizenQuery.FromMarked(withinMarked);
			Scroll(tizenQuery, false);
		}

		public void ScrollUpTo(string toMarked, string withinMarked = null, ScrollStrategy strategy = ScrollStrategy.Auto,
			double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			ScrollTo(TizenQuery.FromMarked(toMarked), withinMarked == null ? null : TizenQuery.FromMarked(withinMarked), timeout,
				down: false);
		}

		public void ScrollUpTo(Func<AppQuery, AppWebQuery> toQuery, string withinMarked,
			ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollUpTo(Func<AppQuery, AppQuery> toQuery, Func<AppQuery, AppQuery> withinQuery = null,
			ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			ScrollTo(TizenQuery.FromQuery(toQuery), withinQuery == null ? null : TizenQuery.FromQuery(withinQuery), timeout,
				down: false);
		}

		public void ScrollUpTo(Func<AppQuery, AppWebQuery> toQuery, Func<AppQuery, AppQuery> withinQuery = null,
			ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void SetOrientationLandscape()
		{
			throw new NotImplementedException();
		}

		public void SetOrientationPortrait()
		{
			throw new NotImplementedException();
		}

		public void SetSliderValue(string marked, double value)
		{
			throw new NotImplementedException();
		}

		public void SetSliderValue(Func<AppQuery, AppQuery> query, double value)
		{
			throw new NotImplementedException();
		}

		public void SwipeLeft()
		{
			throw new NotImplementedException();
		}

		public void SwipeLeftToRight(double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeLeftToRight(string marked, double swipePercentage = 0.67, int swipeSpeed = 500,
			bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeLeftToRight(Func<AppQuery, AppQuery> query, double swipePercentage = 0.67, int swipeSpeed = 500,
			bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeLeftToRight(Func<AppQuery, AppWebQuery> query, double swipePercentage = 0.67, int swipeSpeed = 500,
			bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeRight()
		{
			throw new NotImplementedException();
		}

		public void SwipeRightToLeft(double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeRightToLeft(string marked, double swipePercentage = 0.67, int swipeSpeed = 500,
			bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeRightToLeft(Func<AppQuery, AppQuery> query, double swipePercentage = 0.67, int swipeSpeed = 500,
			bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeRightToLeft(Func<AppQuery, AppWebQuery> query, double swipePercentage = 0.67, int swipeSpeed = 500,
			bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void Tap(Func<AppQuery, AppQuery> query)
		{
			TizenQuery tizenQuery = TizenQuery.FromQuery(query);
			Tap(tizenQuery);
		}

		public void Tap(string marked)
		{
			TizenQuery tizenQuery = TizenQuery.FromMarked(marked);
			Tap(tizenQuery);
		}

		public void Tap(Func<AppQuery, AppWebQuery> query)
		{
			throw new NotImplementedException();
		}

		public void TapCoordinates(float x, float y)
		{
			TouchAction touch = new TouchAction(_session);
			touch.Tap(x, y);
			touch.Perform();
		}

		public ITestServer TestServer { get; }

		public void TouchAndHold(Func<AppQuery, AppQuery> query)
		{
			TizenQuery tizenQuery = TizenQuery.FromQuery(query);
			LongTap(tizenQuery);
		}

		public void TouchAndHold(string marked)
		{
			TizenQuery tizenQuery = TizenQuery.FromMarked(marked);
			LongTap(tizenQuery);
		}

		public void TouchAndHoldCoordinates(float x, float y)
		{
			TouchAction touch = new TouchAction(_session);
			touch.Press(x,y);
			touch.Wait(2000);
			touch.Release();
			touch.Perform();
		}

		public void WaitFor(Func<bool> predicate, string timeoutMessage = "Timed out waiting...", TimeSpan? timeout = null,
			TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			throw new NotImplementedException();
		}

		public AppResult[] WaitForElement(Func<AppQuery, AppQuery> query,
			string timeoutMessage = "Timed out waiting for element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Func<ReadOnlyCollection<TizenElement>> result = () => QueryTizen(query);
			return WaitForAtLeastOne(result, timeoutMessage, timeout, retryFrequency).Select(ToAppResult).ToArray();
		}

		public AppResult[] WaitForElement(string marked, string timeoutMessage = "Timed out waiting for element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Func<ReadOnlyCollection<TizenElement>> result = () => QueryTizen(marked);
			return WaitForAtLeastOne(result, timeoutMessage, timeout, retryFrequency).Select(ToAppResult).ToArray();
		}

		public AppWebResult[] WaitForElement(Func<AppQuery, AppWebQuery> query,
			string timeoutMessage = "Timed out waiting for element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			throw new NotImplementedException();
		}

		public void WaitForNoElement(Func<AppQuery, AppQuery> query,
			string timeoutMessage = "Timed out waiting for no element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Func<ReadOnlyCollection<TizenElement>> result = () => QueryTizen(query);
			WaitForNone(result, timeoutMessage, timeout, retryFrequency);
		}

		public void WaitForNoElement(string marked, string timeoutMessage = "Timed out waiting for no element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Func<ReadOnlyCollection<TizenElement>> result = () => QueryTizen(marked);
			WaitForNone(result, timeoutMessage, timeout, retryFrequency);
		}

		public void WaitForNoElement(Func<AppQuery, AppWebQuery> query,
			string timeoutMessage = "Timed out waiting for no element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			throw new NotImplementedException();
		}

		void DoubleTap(TizenQuery query, float x = 0, float y = 0)
		{
			TouchAction touch = new TouchAction(_session);
			if (query != null)
			{
				TizenElement element = FindFirstElement(query);
				if (element == null)
				{
					return;
				}
				touch.Tap(element, null, null, 2);
			}
			else if(x != 0 && y != 0)
			{
				touch.Tap(x, y, 2);
			}
			else
			{
				return;
			}
			touch.Perform();
			Thread.Sleep(1000);
		}

		TizenElement FindFirstElement(TizenQuery query)
		{
			Func<ReadOnlyCollection<TizenElement>> fquery = () => QueryTizen(query);
			string timeoutMessage = $"Timed out waiting for element: {query.Raw}";

			ReadOnlyCollection<TizenElement> results = WaitForAtLeastOne(fquery, timeoutMessage);
			TizenElement element = results.FirstOrDefault();

			return element;
		}

		ReadOnlyCollection<TizenElement> QueryTizen(TizenQuery query)
		{
			ReadOnlyCollection<TizenElement> resultByAccessibilityId = _session.FindElementsByAccessibilityId(query.Marked);

			IEnumerable<TizenElement> result;

			if (resultByAccessibilityId.Count > 0)
			{
				result = resultByAccessibilityId;
			}
			else
			{
				result = _session.FindElementsByName(query.Marked);
			}

			return new ReadOnlyCollection<TizenElement>(result.ToList());
		}

		ReadOnlyCollection<TizenElement> QueryTizen(string marked)
		{
			TizenQuery tizenQuery = TizenQuery.FromMarked(marked);
			return QueryTizen(tizenQuery);
		}

		ReadOnlyCollection<TizenElement> QueryTizen(Func<AppQuery, AppQuery> query)
		{
			TizenQuery tizenQuery = TizenQuery.FromQuery(query);
			return QueryTizen(tizenQuery);
		}

		void Scroll(TizenQuery query, bool down)
		{
			RemoteTouchScreen remoteTouchScreen = new RemoteTouchScreen(_session);
			int ySpeed = -50;
			if (!down)
			{
				ySpeed = 50;
			}
			remoteTouchScreen.Flick(0, ySpeed);
			Thread.Sleep(2000);
		}

		void ScrollTo(TizenQuery toQuery, TizenQuery withinQuery, TimeSpan? timeout = null, bool down = true)
		{
			timeout = timeout ?? TimeSpan.FromSeconds(5);
			DateTime start = DateTime.Now;

			while (true)
			{
				Func<ReadOnlyCollection<TizenElement>> result = () => QueryTizen(toQuery);
				TimeSpan iterationTimeout = TimeSpan.FromMilliseconds(0);
				TimeSpan retryFrequency = TimeSpan.FromMilliseconds(0);

				try
				{
					ReadOnlyCollection<TizenElement> found = WaitForAtLeastOne(result, timeoutMessage: null,
						timeout: iterationTimeout, retryFrequency: retryFrequency);

					if (found.Count > 0)
					{
						// Success
						return;
					}
				}
				catch (TimeoutException ex)
				{
					// Haven't found it yet, keep scrolling
				}

				long elapsed = DateTime.Now.Subtract(start).Ticks;
				if (elapsed >= timeout.Value.Ticks)
				{
					throw new TimeoutException($"Timed out scrolling to {toQuery}");
				}

				Scroll(withinQuery, down);
			}
		}

		void Tap(TizenQuery query)
		{
			TizenElement element = FindFirstElement(query);

			if (element == null)
			{
				return;
			}

			element.Click();
			Thread.Sleep(1000);
		}

		void LongTap(TizenQuery query, int waitTime = 2000)
		{
			TizenElement element = FindFirstElement(query);

			if (element == null)
			{
				return;
			}

			TouchAction touch = new TouchAction(_session);
			touch.Press(element);
			touch.Wait(waitTime);
			touch.Release();
			touch.Perform();
		}

		static AppRect ToAppRect(TizenElement TizenElement)
		{
			try
			{
				var location = TizenElement.Location;
				var size = TizenElement.Size;

				var result = new AppRect
				{
					X = location.X,
					Y = location.Y,
					Height = size.Height,
					Width = size.Width
				};

				result.CenterX = result.X + result.Width / 2;
				result.CenterY = result.Y + result.Height / 2;

				return result;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(
					$"Warning: error determining AppRect for {TizenElement}; "
					+ $"if this is a Label with a modified Text value, it might be confusing Windows automation. " +
					$"{ex}");
			}

			return null;
		}

		static AppResult ToAppResult(TizenElement TizenElement)
		{
			return new AppResult
			{
				Rect = ToAppRect(TizenElement),
				Label = TizenElement.Id, // Not entirely sure about this one
				Description = TizenElement.Text, // or this one,
				Text = TizenElement.Text, // or this one,
				Enabled = TizenElement.Enabled,
				Id = TizenElement.Id
			};
		}

		static ReadOnlyCollection<TizenElement> Wait(Func<ReadOnlyCollection<TizenElement>> query,
			Func<int, bool> satisfactory,
			string timeoutMessage = null,
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
		{
			timeout = timeout ?? TimeSpan.FromSeconds(5);
			retryFrequency = retryFrequency ?? TimeSpan.FromMilliseconds(500);
			timeoutMessage = timeoutMessage ?? "Timed out on query.";

			DateTime start = DateTime.Now;

			ReadOnlyCollection<TizenElement> result = query();

			while (!satisfactory(result.Count))
			{
				long elapsed = DateTime.Now.Subtract(start).Ticks;
				if (elapsed >= timeout.Value.Ticks)
				{
					Debug.WriteLine($">>>>> {elapsed} ticks elapsed, timeout value is {timeout.Value.Ticks}");

					throw new TimeoutException(timeoutMessage);
				}

				Task.Delay(retryFrequency.Value.Milliseconds).Wait();
				result = query();
			}

			return result;
		}

		static ReadOnlyCollection<TizenElement> WaitForAtLeastOne(Func<ReadOnlyCollection<TizenElement>> query,
			string timeoutMessage = null,
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
		{
			return Wait(query, i => i > 0, timeoutMessage, timeout, retryFrequency);
		}

		void WaitForNone(Func<ReadOnlyCollection<TizenElement>> query,
			string timeoutMessage = null,
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
		{
			Wait(query, i => i == 0, timeoutMessage, timeout, retryFrequency);
		}
	}
}
