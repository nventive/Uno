#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.UI.Xaml.Controls
{
	#if __ANDROID__ || __IOS__ || NET461 || __WASM__ || __MACOS__
	[global::Uno.NotImplemented]
	#endif
	public  partial class PickerConfirmedEventArgs : global::Windows.UI.Xaml.DependencyObject
	{
		#if false || false || NET461 || __WASM__ || __MACOS__
		[global::Uno.NotImplemented]
		public PickerConfirmedEventArgs() : base()
		{
			global::Windows.Foundation.Metadata.ApiInformation.TryRaiseNotImplemented("Windows.UI.Xaml.Controls.PickerConfirmedEventArgs", "PickerConfirmedEventArgs.PickerConfirmedEventArgs()");
		}
		#endif
		// Forced skipping of method Windows.UI.Xaml.Controls.PickerConfirmedEventArgs.PickerConfirmedEventArgs()
	}
}
