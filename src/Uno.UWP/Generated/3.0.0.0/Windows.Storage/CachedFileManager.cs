#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.Storage
{
#if false
	[global::Uno.NotImplemented]
	#endif
	public partial class CachedFileManager 
	{
#if false
		[global::Uno.NotImplemented("__ANDROID__", "__IOS__", "NET461", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__")]
		public static void DeferUpdates( global::Windows.Storage.IStorageFile file)
		{
			global::Windows.Foundation.Metadata.ApiInformation.TryRaiseNotImplemented("Windows.Storage.CachedFileManager", "void CachedFileManager.DeferUpdates(IStorageFile file)");
		}
#endif
#if false
		[global::Uno.NotImplemented("__ANDROID__", "__IOS__", "NET461","__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__")]
		public static global::Windows.Foundation.IAsyncOperation<global::Windows.Storage.Provider.FileUpdateStatus> CompleteUpdatesAsync( global::Windows.Storage.IStorageFile file)
		{
			throw new global::System.NotImplementedException("The member IAsyncOperation<FileUpdateStatus> CachedFileManager.CompleteUpdatesAsync(IStorageFile file) is not implemented in Uno.");
		}
		#endif
	}
}
