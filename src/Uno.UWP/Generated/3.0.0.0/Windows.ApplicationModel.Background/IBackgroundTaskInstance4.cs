#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.ApplicationModel.Background
{
	#if __ANDROID__ || __IOS__ || NET46 || __WASM__ || __MACOS__
	[global::Uno.NotImplemented]
	#endif
	public  partial interface IBackgroundTaskInstance4 : global::Windows.ApplicationModel.Background.IBackgroundTaskInstance
	{
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__ || __MACOS__
		global::Windows.System.User User
		{
			get;
		}
		#endif
		// Forced skipping of method Windows.ApplicationModel.Background.IBackgroundTaskInstance4.User.get
	}
}
