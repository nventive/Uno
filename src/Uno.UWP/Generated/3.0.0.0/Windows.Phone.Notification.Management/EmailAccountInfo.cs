#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.Phone.Notification.Management
{
	#if __ANDROID__ || __IOS__ || NET46 || __WASM__ || __MACOS__
	[global::Uno.NotImplemented]
	#endif
	public  partial class EmailAccountInfo 
	{
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__ || __MACOS__
		[global::Uno.NotImplemented]
		public  string DisplayName
		{
			get
			{
				throw new global::System.NotImplementedException("The member string EmailAccountInfo.DisplayName is not implemented in Uno.");
			}
		}
		#endif
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__ || __MACOS__
		[global::Uno.NotImplemented]
		public  bool IsNotificationEnabled
		{
			get
			{
				throw new global::System.NotImplementedException("The member bool EmailAccountInfo.IsNotificationEnabled is not implemented in Uno.");
			}
		}
		#endif
		// Forced skipping of method Windows.Phone.Notification.Management.EmailAccountInfo.DisplayName.get
		// Forced skipping of method Windows.Phone.Notification.Management.EmailAccountInfo.IsNotificationEnabled.get
	}
}
