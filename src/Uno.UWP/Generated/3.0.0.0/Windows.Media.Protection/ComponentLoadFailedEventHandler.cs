#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.Media.Protection
{
	#if __ANDROID__ || __IOS__ || NET46 || __WASM__ || __MACOS__
	public delegate void ComponentLoadFailedEventHandler(global::Windows.Media.Protection.MediaProtectionManager @sender, global::Windows.Media.Protection.ComponentLoadFailedEventArgs @e);
	#endif
}
