#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.Services.Store
{
	#if __ANDROID__ || __IOS__ || NET461 || __WASM__ || __MACOS__
	[global::Uno.NotImplemented]
	#endif
	public  partial class StoreAcquireLicenseResult 
	{
		#if __ANDROID__ || __IOS__ || NET461 || __WASM__ || __MACOS__
		[global::Uno.NotImplemented]
		public  global::System.Exception ExtendedError
		{
			get
			{
				throw new global::System.NotImplementedException("The member Exception StoreAcquireLicenseResult.ExtendedError is not implemented in Uno.");
			}
		}
		#endif
		#if __ANDROID__ || __IOS__ || NET461 || __WASM__ || __MACOS__
		[global::Uno.NotImplemented]
		public  global::Windows.Services.Store.StorePackageLicense StorePackageLicense
		{
			get
			{
				throw new global::System.NotImplementedException("The member StorePackageLicense StoreAcquireLicenseResult.StorePackageLicense is not implemented in Uno.");
			}
		}
		#endif
		// Forced skipping of method Windows.Services.Store.StoreAcquireLicenseResult.StorePackageLicense.get
		// Forced skipping of method Windows.Services.Store.StoreAcquireLicenseResult.ExtendedError.get
	}
}
