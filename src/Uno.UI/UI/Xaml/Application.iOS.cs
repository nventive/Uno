﻿#if XAMARIN_IOS
using Foundation;
using System;
using UIKit;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.ApplicationModel;
using ObjCRuntime;
using Windows.Graphics.Display;

namespace Windows.UI.Xaml
{
	[Register("UnoAppDelegate")]
	public partial class Application : UIApplicationDelegate
	{
		private bool _suspended;
		internal bool IsSuspended => _suspended;

		public Application()
		{
			Current = this;
			Windows.UI.Xaml.GenericStyles.Initialize();
		}

		public Application(IntPtr handle) : base(handle)
		{
		}

		static partial void StartPartial(ApplicationInitializationCallback callback)
		{
			callback(new ApplicationInitializationCallbackParams());
		}

		public override void FinishedLaunching(UIApplication application)
		{
			OnLaunched(new LaunchActivatedEventArgs());
		}

		public override void DidEnterBackground(UIApplication application)
		{
			var operation = new SuspendingOperation(DateTime.Now.AddSeconds(10));

			Suspending?.Invoke(this, new ApplicationModel.SuspendingEventArgs(operation));

			_suspended = true;
		}

		public override void WillEnterForeground(UIApplication application)
		{
			if(_suspended)
			{
				_suspended = false;

				Resuming?.Invoke(this, null);
			}
		}

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, [Transient] UIWindow forWindow)
        {
            return DisplayInformation.AutoRotationPreferences.ToUIInterfaceOrientationMask();
        }

        /// <summary>
        /// This method enables UI Tests to get the output path
        /// of the current application, in the context of the simulator.
        /// </summary>
        /// <returns>The host path to get the container</returns>
        [Export("getApplicationDataPath")]
		[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
		public NSString GetWorkingFolder() => new NSString(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
	}
}
#endif
