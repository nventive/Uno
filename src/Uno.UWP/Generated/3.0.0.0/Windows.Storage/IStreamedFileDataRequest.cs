#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.Storage
{
	#if __ANDROID__ || __IOS__ || NET46 || __WASM__ || __MACOS__
	[global::Uno.NotImplemented]
	#endif
	public  partial interface IStreamedFileDataRequest 
	{
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__ || __MACOS__
		void FailAndClose( global::Windows.Storage.StreamedFileFailureMode failureMode);
		#endif
	}
}
