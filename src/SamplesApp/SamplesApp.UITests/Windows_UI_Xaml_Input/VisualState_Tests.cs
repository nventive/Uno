﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SamplesApp.UITests.TestFramework;
using Uno.UITest;
using Uno.UITest.Helpers;
using Uno.UITest.Helpers.Queries;

namespace SamplesApp.UITests.Windows_UI_Xaml_Input
{
	public class VisualState_Tests : SampleControlUITestBase
	{
		[Test]
		[ActivePlatforms(Platform.iOS, Platform.Android)]
		public void TestButtonReleasedOut() => TestButtonReleasedOutState(
			"MyButton",
			"CommonStates.PointerOver",
			"CommonStates.Pressed",
			"CommonStates.Normal");

		[Test]
		[ActivePlatforms(Platform.iOS, Platform.Android)]
		public void TestIndeterminateToggleButtonReleasedOut() => TestButtonReleasedOutState(
			"MyIndeterminateToggleButton",
			"CommonStates.IndeterminatePointerOver",
			"CommonStates.IndeterminatePressed",
			"CommonStates.Indeterminate");

		[Test]
		[ActivePlatforms(Platform.iOS, Platform.Android)]
		public void TestCheckedToggleButtonReleasedOut() => TestButtonReleasedOutState(
			"MyCheckedToggleButton",
			"CommonStates.CheckedPointerOver",
			"CommonStates.CheckedPressed",
			"CommonStates.Checked");

		[Test]
		[Ignore("We get an invalid 'PointerOver' on release, a fix in pending in another PR")]
		public void TestUncheckedToggleButtonReleasedOut() => TestButtonReleasedOutState(
			"MyUncheckedToggleButton",
			"CommonStates.UncheckedPointerOver",
			"CommonStates.UncheckedPressed",
			"CommonStates.Unchecked");

		[Test]
		[ActivePlatforms(Platform.iOS, Platform.Android)]
		public void TestRadioButtonReleasedOut() => TestButtonReleasedOutState(
			"MyRadioButton",
			"CommonStates.PointerOver",
			"CommonStates.Pressed",
			"CommonStates.Normal");

		[Test]
		[ActivePlatforms(Platform.iOS, Platform.Android)]
		public void TestHyperlinkButtonReleasedOut() => TestButtonReleasedOutState(
			"MyHyperlinkButton",
			"CommonStates.PointerOver",
			"CommonStates.Pressed",
			"CommonStates.Normal");

		[Test]
		[ActivePlatforms(Platform.iOS, Platform.Android)]
		public void TestIndeterminateCheckboxReleasedOut() => TestButtonReleasedOutState(
			"MyIndeterminateCheckbox",
			"CombinedStates.IndeterminatePointerOver",
			"CombinedStates.IndeterminatePressed",
			"CombinedStates.IndeterminateNormal");

		[Test]
		[ActivePlatforms(Platform.iOS, Platform.Android)]
		public void TestCheckedCheckboxReleasedOut() => TestButtonReleasedOutState(
			"MyCheckedCheckbox",
			"CombinedStates.CheckedPointerOver",
			"CombinedStates.CheckedPressed",
			"CombinedStates.CheckedNormal");

		[Test]
		[ActivePlatforms(Platform.iOS, Platform.Android)]
		public void TestUncheckedCheckboxReleasedOut() => TestButtonReleasedOutState(
			"MyUncheckedCheckbox",
			"CombinedStates.UncheckedPointerOver",
			"CombinedStates.UncheckedPressed",
			"CombinedStates.UncheckedNormal");

		[Test]
		public void TestHyperlinkReleasedOut() => TestButtonReleasedOutState(
			"MyHyperlink"); // There is no "VisualState" for Hyperlink, only a hardcoded opacity of .5 (kind-of like UWP)

		[Test]
		public void TestListViewReleasedOut()
		{
			Run("UITests.Shared.Windows_UI_Input.VisualStatesTests.ListViewItem");

			var initial = TakeScreenshot("Initial");
			var rect = _app.WaitForElement("MyListView").Single().Rect;

			// Press over and move out to release
			_app.DragCoordinates(rect.X + 10, rect.Y + 10, rect.X - 30, rect.Y);

			var final = TakeScreenshot("Final");
			AssertScreenshotsAreEqual(initial, final, rect);
		}

		[Test]
		public void TestTextBoxReleaseOut() => TestTextBoxReleasedOutState(
			"MyTextBox",
			"CommonStates.PointerOver",
			"CommonStates.Focused");

		[Test]
		public void TestTextBoxTap() => TestTextBoxTappedState(
			"MyTextBox",
			"CommonStates.PointerOver",
			"CommonStates.Focused");

		private void TestButtonReleasedOutState(string target, params string[] expectedStates)
		{
			Run("UITests.Shared.Windows_UI_Input.VisualStatesTests.Buttons");
			TestVisualTests(target, ReleaseOut, expectedStates);
		}

		private void TestTextBoxReleasedOutState(string target, params string[] expectedStates)
		{
			Run("UITests.Shared.Windows_UI_Input.VisualStatesTests.TextBox_VisualStates");
			TestVisualTests(target, ReleaseOut, expectedStates);
		}
		private void TestTextBoxTappedState(string target, params string[] expectedStates)
		{
			Run("UITests.Shared.Windows_UI_Input.VisualStatesTests.TextBox_VisualStates");
			TestVisualTests(target, Tap, expectedStates);
		}

		// Press over and move out to release
		private void ReleaseOut(IAppRect target)
			=> _app.DragCoordinates(target.X + 2, target.Y + 2, target.X, target.Y - 30);

		private void Tap(IAppRect target)
			=> _app.TapCoordinates(target.X + 2, target.Y + 2);

		private void TestVisualTests(string targetName, Action<IAppRect> act, params string[] expectedStates)
		{
			var initial = TakeScreenshot("Initial");
			var target = _app.WaitForElement(targetName).Single().Rect;

			act(target);

			var final = TakeScreenshot("Final");
			var actualStates = _app
				.Marked("VisualStatesLog")
				.GetDependencyPropertyValue<string>("Text")
				.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
				.Where(line => line.StartsWith(targetName))
				.Select(line => line.Trim().Substring(targetName.Length + 1))
				.ToArray();

			if (expectedStates?.Any() ?? false)
			{
				CollectionAssert.AreEqual(expectedStates, actualStates, StringComparer.OrdinalIgnoreCase);
			}

			// For the comparison, we compare only the location of the control (i.e. we provide the rect).
			// This is required to NOT include the visual output ot the states (on the right of the test control)
			AssertScreenshotsAreEqual(initial, final, target);
		}
	}
}
