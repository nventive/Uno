﻿using Uno;
using Uno.Client;
using Uno.Extensions;
using Uno.Logging;
using Uno.UI;
using Windows.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Uno.Disposables;
using System.Text;
using System.Threading;
using Windows.System;
using Uno.Foundation;

using View = Windows.UI.Xaml.UIElement;

namespace Windows.UI.Xaml.Controls.Primitives
{
	public partial class ButtonBase : ContentControl
	{
		private readonly SerialDisposable _touchSubscription = new SerialDisposable();
		private readonly SerialDisposable _isEnabledSubscription = new SerialDisposable();

		protected override void OnLoaded()
		{
			base.OnLoaded();

			//Focusable = true;
			//FocusableInTouchMode = true;

			//RegisterEvents();

			//OnCanExecuteChanged();

			KeyDown += OnKeyDown;
		}

		private void OnKeyDown(object sender, KeyRoutedEventArgs keyRoutedEventArgs)
		{
			switch (keyRoutedEventArgs?.Key)
			{
				case VirtualKey.Enter:
				case VirtualKey.Execute:
				case VirtualKey.Space:
					OnClick();
					break;
			}
		}

		protected override void OnUnloaded()
		{
			base.OnUnloaded();
			_isEnabledSubscription.Disposable = null;
			_touchSubscription.Disposable = null;
		}

		/// <summary>
		/// Gets the native UI Control, if any.
		/// </summary>
		private View GetUIControl()
		{
			return
				// Check for non-templated ContentControl root (ContentPresenter bypass)
				ContentTemplateRoot

				// Finally check for templated ContentControl root
				?? TemplatedRoot;
		}


		// TODO: these event handlers should be removed when overrides are correctly called from Control
		private void OnPointerPressed(object sender, PointerRoutedEventArgs args)
		{
			OnPointerPressed(args);
		}

		private void OnPointerReleased(object sender, PointerRoutedEventArgs args)
		{
			OnPointerReleased(args);
		}

		private void OnPointerCanceled(object sender, PointerRoutedEventArgs args)
		{
			OnPointerCanceled(args);
		}

		private void OnPointerExited(object sender, PointerRoutedEventArgs args)
		{
			OnPointerExited(args);
		}

		private void OnPointerEntered(object sender, PointerRoutedEventArgs args)
		{
			OnPointerEntered(args);
		}
	}
}
