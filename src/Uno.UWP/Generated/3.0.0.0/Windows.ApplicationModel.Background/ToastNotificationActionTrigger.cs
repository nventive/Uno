#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.ApplicationModel.Background
{
	#if __ANDROID__ || __IOS__ || NET46 || __WASM__ || __MACOS__
	[global::Uno.NotImplemented]
	#endif
	public  partial class ToastNotificationActionTrigger : global::Windows.ApplicationModel.Background.IBackgroundTrigger
	{
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__ || __MACOS__
		[global::Uno.NotImplemented]
		public ToastNotificationActionTrigger( string applicationId) 
		{
			global::Windows.Foundation.Metadata.ApiInformation.TryRaiseNotImplemented("Windows.ApplicationModel.Background.ToastNotificationActionTrigger", "ToastNotificationActionTrigger.ToastNotificationActionTrigger(string applicationId)");
		}
		#endif
		// Forced skipping of method Windows.ApplicationModel.Background.ToastNotificationActionTrigger.ToastNotificationActionTrigger(string)
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__ || __MACOS__
		[global::Uno.NotImplemented]
		public ToastNotificationActionTrigger() 
		{
			global::Windows.Foundation.Metadata.ApiInformation.TryRaiseNotImplemented("Windows.ApplicationModel.Background.ToastNotificationActionTrigger", "ToastNotificationActionTrigger.ToastNotificationActionTrigger()");
		}
		#endif
		// Forced skipping of method Windows.ApplicationModel.Background.ToastNotificationActionTrigger.ToastNotificationActionTrigger()
		// Processing: Windows.ApplicationModel.Background.IBackgroundTrigger
	}
}
