#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.UI.Xaml.Controls
{
	#if __ANDROID__ || __IOS__ || NET461 || __WASM__ || __SKIA__ || __NETSTD_REFERENCE__ || __MACOS__
	[global::Uno.NotImplemented]
	#endif
	public  partial class RatingItemInfo : global::Windows.UI.Xaml.DependencyObject
	{
		#if __ANDROID__ || __IOS__ || NET461 || false || false || false || __MACOS__
		[global::Uno.NotImplemented("__ANDROID__", "__IOS__", "NET461", "__MACOS__")]
		public RatingItemInfo() : base()
		{
			global::Windows.Foundation.Metadata.ApiInformation.TryRaiseNotImplemented("Windows.UI.Xaml.Controls.RatingItemInfo", "RatingItemInfo.RatingItemInfo()");
		}
		#endif
		// Forced skipping of method Windows.UI.Xaml.Controls.RatingItemInfo.RatingItemInfo()
	}
}
