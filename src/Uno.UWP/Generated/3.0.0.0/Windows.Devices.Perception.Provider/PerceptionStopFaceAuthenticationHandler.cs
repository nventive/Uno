#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.Devices.Perception.Provider
{
	#if __ANDROID__ || __IOS__ || NET461 || __WASM__ || __SKIA__ || __NETSTD_REFERENCE__ || __MACOS__
	public delegate void PerceptionStopFaceAuthenticationHandler(global::Windows.Devices.Perception.Provider.PerceptionFaceAuthenticationGroup sender);
	#endif
}
