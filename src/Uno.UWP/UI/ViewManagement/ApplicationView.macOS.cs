﻿#if __MACOS__
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using AppKit;
using Uno.Extensions;
using Uno.Logging;

namespace Windows.UI.ViewManagement
{
    partial class ApplicationView
	{
		internal void SetCoreBounds(NSWindow keyWindow, Foundation.Rect windowBounds)
		{
            VisibleBounds = windowBounds;

			if(this.Log().IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
			{
				this.Log().Debug($"Updated visible bounds {VisibleBounds}");
			}

			VisibleBoundsChanged?.Invoke(this, null);
		}

		public string Title
		{
			get
			{
				VerifyKeyWindowInitialized();
				return NSApplication.SharedApplication.KeyWindow.Title;
			}
			set
			{
				VerifyKeyWindowInitialized();
				NSApplication.SharedApplication.KeyWindow.Title = value;
			}
		}

		private void VerifyKeyWindowInitialized([CallerMemberName]string propertyName = null)
		{
			if (NSApplication.SharedApplication.KeyWindow == null)
			{
				throw new InvalidOperationException($"{propertyName} API must be used after KeyWindow is set");
			}
		}
	}
}
#endif
