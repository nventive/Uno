﻿using System;
using Microsoft.Extensions.Logging;
using Uno;
using Uno.Extensions;
using Uno.UI.Xaml;
using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls
{
	public sealed partial class XamlControlsResources : ResourceDictionary
	{
		private const ControlsResourcesVersion MaxSupportedResourcesVersion = ControlsResourcesVersion.Version2;

		private static bool _isUsingResourcesVersion2 = false;

		public XamlControlsResources()
		{
#if !__NETSTD_REFERENCE__

			// Perform manually what the SourceGenerator is doing during app.xaml.cs InitializeComponent.
			// Using explicit registration allows for the styles to be linked out when not used
			Uno.UI.FluentTheme.GlobalStaticResources.Initialize();
			Uno.UI.FluentTheme.GlobalStaticResources.RegisterDefaultStyles();
			Uno.UI.FluentTheme.GlobalStaticResources.RegisterResourceDictionariesBySource();
#endif

			UpdateSource();
		}

		private void UpdateSource()
		{
			var requestedVersion = ControlsResourcesVersion;
			if (ControlsResourcesVersion > MaxSupportedResourcesVersion)
			{
				if (this.Log().IsEnabled(LogLevel.Warning))
				{
					this.Log().LogWarning($"" +
						$"WinUI resources version {ControlsResourcesVersion} is not supported " +
						$"in Uno Platform yet. Falling back to {MaxSupportedResourcesVersion} styles.");
				}
				requestedVersion = MaxSupportedResourcesVersion;
			}

			Source = new Uri(XamlFilePathHelper.AppXIdentifier + XamlFilePathHelper.GetWinUIThemeResourceUrl((int)requestedVersion));

			_isUsingResourcesVersion2 = requestedVersion == ControlsResourcesVersion.Version2;
		}

		[NotImplemented]
		public static void EnsureRevealLights(UIElement element) { }

		[NotImplemented]
		public bool UseCompactResources
		{
			get => (bool)GetValue(UseCompactResourcesProperty);
			set => SetValue(UseCompactResourcesProperty, value);
		}

		[NotImplemented]
		public static DependencyProperty UseCompactResourcesProperty { get; } =
			DependencyProperty.Register(nameof(UseCompactResources), typeof(bool), typeof(XamlControlsResources), new PropertyMetadata(false));

		public ControlsResourcesVersion ControlsResourcesVersion
		{
			get => (ControlsResourcesVersion)GetValue(ControlsResourcesVersionProperty);
			set => SetValue(ControlsResourcesVersionProperty, value);
		}

		public static DependencyProperty ControlsResourcesVersionProperty { get; } =
			DependencyProperty.Register(nameof(ControlsResourcesVersion), typeof(ControlsResourcesVersion), typeof(XamlControlsResources), new PropertyMetadata(ControlsResourcesVersion.Version1, OnControlsResourcesVersionChanged));

		private static void OnControlsResourcesVersionChanged(DependencyObject owner, DependencyPropertyChangedEventArgs args)
		{
			var resources = owner as XamlControlsResources;
			resources?.UpdateSource();
		}

		[NotImplemented]
		internal static bool IsUsingResourcesVersion2() => _isUsingResourcesVersion2;
	}
}
