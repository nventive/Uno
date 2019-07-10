﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using SamplesApp.Windows_UI_Xaml_Controls.Models;
using Uno.UI.Samples.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace SamplesApp.Samples.DatePicker
{
	[SampleControlInfo("Date Picker", "Flyout", typeof(DatePickerViewModel))]
	public sealed partial class DatePickerFlyoutSample : Page
	{
		public DatePickerFlyoutSample()
		{
			this.InitializeComponent();
		}
	}
}
