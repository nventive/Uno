#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.Services.Store
{
	#if __ANDROID__ || false || NET461 || __WASM__ || __MACOS__
	[global::Uno.NotImplemented]
	#endif
	public  partial class StoreProductQueryResult 
	{
		#if __ANDROID__ || false || NET461 || __WASM__ || __MACOS__
		[global::Uno.NotImplemented]
		public  global::System.Exception ExtendedError
		{
			get
			{
				throw new global::System.NotImplementedException("The member Exception StoreProductQueryResult.ExtendedError is not implemented in Uno.");
			}
		}
		#endif
		#if __ANDROID__ || false || NET461 || __WASM__ || __MACOS__
		[global::Uno.NotImplemented]
		public  global::System.Collections.Generic.IReadOnlyDictionary<string, global::Windows.Services.Store.StoreProduct> Products
		{
			get
			{
				throw new global::System.NotImplementedException("The member IReadOnlyDictionary<string, StoreProduct> StoreProductQueryResult.Products is not implemented in Uno.");
			}
		}
		#endif
		// Forced skipping of method Windows.Services.Store.StoreProductQueryResult.Products.get
		// Forced skipping of method Windows.Services.Store.StoreProductQueryResult.ExtendedError.get
	}
}
