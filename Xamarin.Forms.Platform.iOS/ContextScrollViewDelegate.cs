﻿using System;
using System.Drawing;
using System.Collections.Generic;
#if __UNIFIED__
using UIKit;
using Foundation;
#else
using MonoTouch.UIKit;
using MonoTouch.Foundation;
#endif
#if __UNIFIED__
using NSAction = System.Action;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using PointF = CoreGraphics.CGPoint;

#else
using nfloat=System.Single;
using nint=System.Int32;
#endif

namespace Xamarin.Forms.Platform.iOS
{
	internal class iOS7ButtonContainer : UIView
	{
		readonly nfloat _buttonWidth;

		public iOS7ButtonContainer(nfloat buttonWidth) : base(new RectangleF((nfloat)0, (nfloat)0, (nfloat)0, (nfloat)0))
		{
			_buttonWidth = buttonWidth;
			ClipsToBounds = true;
		}

		public override void LayoutSubviews()
		{
			var width = Frame.Width;
			nfloat takenSpace = 0;

			for (var i = 0; i < Subviews.Length; i++)
			{
				var view = Subviews[i];

				var pos = Subviews.Length - i;
				nfloat x = width - _buttonWidth * pos;
				view.Frame = new RectangleF(x, (nfloat)0, (nfloat)view.Frame.Width, (nfloat)view.Frame.Height);

				takenSpace += view.Frame.Width;
			}
		}
	}

	internal class ContextScrollViewDelegate : UIScrollViewDelegate
	{
		readonly nfloat _finalButtonSize;
		UIView _backgroundView;
		List<UIButton> _buttons;
		UITapGestureRecognizer _closer;
		UIView _container;
		GlobalCloseContextGestureRecognizer _globalCloser;

		bool _isDisposed;

		UITableView _table;

		public ContextScrollViewDelegate(UIView container, List<UIButton> buttons, bool isOpen)
		{
			IsOpen = isOpen;
			_container = container;
			_buttons = buttons;

			for (var i = 0; i < buttons.Count; i++)
			{
				var b = buttons[i];
				b.Hidden = !isOpen;

				ButtonsWidth += b.Frame.Width;
				_finalButtonSize = b.Frame.Width;
			}
		}

		public nfloat ButtonsWidth { get; }

		public Action ClosedCallback { get; set; }

		public bool IsOpen { get; private set; }

		public override void DraggingStarted(UIScrollView scrollView)
		{
			if (!IsOpen)
				SetButtonsShowing(true);

			var cell = GetContextCell(scrollView);
			if (!cell.Selected)
				return;

			if (!IsOpen)
				RemoveHighlight(scrollView);
		}

		public void PrepareForDeselect(UIScrollView scrollView)
		{
			RestoreHighlight(scrollView);
		}

		public override void Scrolled(UIScrollView scrollView)
		{
			var width = _finalButtonSize;
			var count = _buttons.Count;

			if (!UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
				_container.Frame = new RectangleF((nfloat)scrollView.Frame.Width, (nfloat)0, (nfloat)scrollView.ContentOffset.X, (nfloat)scrollView.Frame.Height);
			else
			{
				var ioffset = scrollView.ContentOffset.X / (float)count;

				if (ioffset > width)
					width = ioffset + 1;

				for (var i = count - 1; i >= 0; i--)
				{
					var b = _buttons[i];
					var rect = b.Frame;
					nfloat x = scrollView.Frame.Width + (count - (i + 1)) * ioffset;
					b.Frame = new RectangleF(x, (nfloat)0, width, rect.Height);
				}
			}

			if (scrollView.ContentOffset.X == 0)
			{
				IsOpen = false;
				SetButtonsShowing(false);
				RestoreHighlight(scrollView);

				ClearCloserRecognizer(scrollView);

				if (ClosedCallback != null)
					ClosedCallback();
			}
		}

		public void Unhook(UIScrollView scrollView)
		{
			RestoreHighlight(scrollView);
			ClearCloserRecognizer(scrollView);
		}

		public override void WillEndDragging(UIScrollView scrollView, PointF velocity, ref PointF targetContentOffset)
		{
			var width = ButtonsWidth;
			var x = targetContentOffset.X;
			var parentThreshold = scrollView.Frame.Width * .4f;
			var contentThreshold = width * .8f;

			if (x >= parentThreshold || x >= contentThreshold)
			{
				IsOpen = true;
				targetContentOffset = new PointF(width, 0);
				RemoveHighlight(scrollView);

				if (_globalCloser == null)
				{
					UIView view = scrollView;
					while (view.Superview != null)
					{
						view = view.Superview;

						NSAction close = () =>
						{
							if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
								RestoreHighlight(scrollView);

							IsOpen = false;
							scrollView.SetContentOffset(new PointF(0, 0), true);

							ClearCloserRecognizer(scrollView);
						};

						var table = view as UITableView;
						if (table != null)
						{
							_table = table;
							_globalCloser = new GlobalCloseContextGestureRecognizer(scrollView, close);
							_globalCloser.ShouldRecognizeSimultaneously = (recognizer, r) => r == _table.PanGestureRecognizer;
							table.AddGestureRecognizer(_globalCloser);

							_closer = new UITapGestureRecognizer(close);
							var cell = GetContextCell(scrollView);
							cell.ContentCell.AddGestureRecognizer(_closer);
						}
					}
				}
			}
			else
			{
				ClearCloserRecognizer(scrollView);

				IsOpen = false;
				targetContentOffset = new PointF(0, 0);

				if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
					RestoreHighlight(scrollView);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			if (disposing)
			{
				ClosedCallback = null;

				_table = null;
				_backgroundView = null;
				_container = null;

				_buttons = null;
			}

			base.Dispose(disposing);
		}

		void ClearCloserRecognizer(UIScrollView scrollView)
		{
			if (_globalCloser == null)
				return;

			var cell = GetContextCell(scrollView);
			cell.ContentCell.RemoveGestureRecognizer(_closer);
			_closer.Dispose();
			_closer = null;

			_table.RemoveGestureRecognizer(_globalCloser);
			_table = null;
			_globalCloser.Dispose();
			_globalCloser = null;
		}

		ContextActionsCell GetContextCell(UIScrollView scrollView)
		{
			var view = scrollView.Superview.Superview;
			var cell = view as ContextActionsCell;
			while (view.Superview != null)
			{
				cell = view as ContextActionsCell;
				if (cell != null)
					break;

				view = view.Superview;
			}

			return cell;
		}

		void RemoveHighlight(UIScrollView scrollView)
		{
			var subviews = scrollView.Superview.Superview.Subviews;

			var count = 0;
			for (var i = 0; i < subviews.Length; i++)
			{
				var s = subviews[i];
				if (s.Frame.Height > 1)
					count++;
			}

			if (count <= 1)
				return;

			_backgroundView = subviews[0];
			_backgroundView.RemoveFromSuperview();

			var cell = GetContextCell(scrollView);
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;
		}

		void RestoreHighlight(UIScrollView scrollView)
		{
			if (_backgroundView == null)
				return;

			var cell = GetContextCell(scrollView);
			cell.SelectionStyle = UITableViewCellSelectionStyle.Default;
			cell.SetSelected(true, false);

			scrollView.Superview.Superview.InsertSubview(_backgroundView, 0);
			_backgroundView = null;
		}

		void SetButtonsShowing(bool show)
		{
			for (var i = 0; i < _buttons.Count; i++)
				_buttons[i].Hidden = !show;
		}
	}
}