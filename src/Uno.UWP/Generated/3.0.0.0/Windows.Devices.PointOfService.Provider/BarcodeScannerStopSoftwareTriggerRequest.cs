#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.Devices.PointOfService.Provider
{
	#if __ANDROID__ || __IOS__ || NET46 || __WASM__ || __MACOS__
	[global::Uno.NotImplemented]
	#endif
	public  partial class BarcodeScannerStopSoftwareTriggerRequest 
	{
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__ || __MACOS__
		[global::Uno.NotImplemented]
		public  global::Windows.Foundation.IAsyncAction ReportCompletedAsync()
		{
			throw new global::System.NotImplementedException("The member IAsyncAction BarcodeScannerStopSoftwareTriggerRequest.ReportCompletedAsync() is not implemented in Uno.");
		}
		#endif
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__ || __MACOS__
		[global::Uno.NotImplemented]
		public  global::Windows.Foundation.IAsyncAction ReportFailedAsync()
		{
			throw new global::System.NotImplementedException("The member IAsyncAction BarcodeScannerStopSoftwareTriggerRequest.ReportFailedAsync() is not implemented in Uno.");
		}
		#endif
	}
}
