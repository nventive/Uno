﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SamplesApp.UITests.TestFramework;
using Uno.UITest;
using Uno.UITest.Helpers;
using Uno.UITest.Helpers.Queries;

namespace SamplesApp.UITests.Windows_UI_Xaml_Controls.ListViewTests
{
	[TestFixture]
	public partial class ListViewTests_Tests : SampleControlUITestBase
	{
		[Test]
		[Ignore("Not available yet")]
		public void RotatedListView_AddsToBottom()
		{
			Run("SamplesApp.Windows_UI_Xaml_Controls.ListView.RotatedListView_WithRotatedItems");

			//Rotated ListView items can't be properly tests until https://github.com/xamarin/Xamarin.Forms/issues/2496 is fixed.
		}

		[Test]
		[AutoRetry]
		public void ListView_ListViewVariableItemHeightLong_InitializesTest()
		{
			Run("SamplesApp.Windows_UI_Xaml_Controls.ListView.ListViewVariableItemHeightLong");

			_app.WaitForElement(_app.Marked("theListView"));
			var theListView = _app.Marked("theListView");

			// Assert initial state
			Assert.IsNotNull(theListView.GetDependencyPropertyValue("DataContext"));
		}

		// HorizontalListViewGrouped isn't present on WASM
		[Test]
		[AutoRetry]
		[ActivePlatforms(Platform.iOS, Platform.Android)]
		public void ListView_ListViewWithHeader_InitializesTest()
		{
			Run("SamplesApp.Windows_UI_Xaml_Controls.ListView.HorizontalListViewGrouped");

			_app.WaitForElement(_app.Marked("TargetListView"));
			var theListView = _app.Marked("TargetListView");

			// Assert initial state
			Assert.IsNotNull(theListView.GetDependencyPropertyValue("DataContext"));
		}

		[Test]
		[AutoRetry]
		[ActivePlatforms(Platform.Android)]
		public void ListView_ItemPanel_HotSwapTest()
		{
			Run("UITests.Shared.Windows_UI_Xaml_Controls.ListView.ListView_ItemsPanel_HotSwap");

			_app.WaitForElement("SampleListView");
			TabWaitAndThenScreenshot("SwapHorizontalStackPanelButton");
			TabWaitAndThenScreenshot("UpdateItemsSourceButton");
			TabWaitAndThenScreenshot("SwapVerticalItemsStackPanelButton");
			TabWaitAndThenScreenshot("UpdateItemsSourceButton");

			void TabWaitAndThenScreenshot(string buttonName)
			{
				_app.Marked(buttonName).FastTap();
				_app.Wait(TimeSpan.FromSeconds(2));
				TakeScreenshot($"ListView_ItemPanel_HotSwap - {buttonName}");
			}
		}

		[Test]
		[AutoRetry]
		public void ListView_VirtualizePanelAdaptaterIdCache()
		{
			Run("SamplesApp.Windows_UI_Xaml_Controls.ListView.ListView_VirtualizePanelAdaptaterIdCache");

			_app.FastTap("MyButton");

			var textResult = _app.Marked("TextResult");
			_app.WaitForText(textResult, "Success");

			TakeScreenshot($"ListView_VirtualizePanelAdaptaterIdCache");
		}

		[Test]
		[AutoRetry]
		[ActivePlatforms(Platform.Android)] // Fails on iOS - https://github.com/unoplatform/uno/issues/1955
		public void ListView_Header_DataContextChanged()
		{
			Run("UITests.Shared.Windows_UI_Xaml_Controls.ListView_Header_DataContextChanging");

			_app.WaitForText("MyTextBlock", "InitialText InitialDataContext");
			_app.Marked("MyButton").FastTap();
			_app.WaitForText("MyTextBlock", "InitialText InitialDataContext UpdatedDataContext");
		}

		[Test]
		[AutoRetry]
		[ActivePlatforms(Platform.iOS, Platform.Android)] // Currently fails on WASM, layout requests aren't swallowed properly
		public void Check_ListView_Swallows_Measure()
		{
			Run("UITests.Shared.Windows_UI_Xaml_Controls.ListView.ListView_With_ListViews_Count_Measure");

			_app.WaitForText("StateTextBlock", "Measured");

			TakeScreenshot("before scroll");

			var measureTextBefore = _app.GetText("MeasureCountTextBlock");
			var initialMeasureCount = int.Parse(measureTextBefore);

			_app.FastTap("ChangeViewButton");

			_app.WaitForText("ResultTextBlock", "Scrolled");

			TakeScreenshot("after scroll");

			var measureTextAfter = _app.GetText("MeasureCountTextBlock");
			var finalMeasureCount = int.Parse(measureTextAfter);
			Assert.AreEqual(initialMeasureCount, finalMeasureCount);
		}

		[Test]
		[AutoRetry]
		public void ListView_Weird_Measure_During_Arrange()
		{
			Run("UITests.Shared.Windows_UI_Xaml_Controls.ListView.ListView_Weird_Measure");

			_app.WaitForText("StatusTextBlock", "Finished");

			TakeScreenshot("after layout");

			var heightStr = _app.GetText("HeightTextBlock");
			var height = int.Parse(heightStr);

			Assert.AreEqual(224, height);
		}

		[Test]
		[AutoRetry]
		public void ListView_ObservableCollection_Unused_Space()
		{
			Run("UITests.Shared.Windows_UI_Xaml_Controls.ListView.ListView_ObservableCollection_Unused_Space");

			_app.WaitForText("StatusTextBlock", "Ready");

			TakeScreenshot("1 item");

			var heightStrBefore = _app.GetText("HeightTextBlock");
			var heightBefore = int.Parse(heightStrBefore);

			_app.FastTap("AddItemsButton");

			_app.WaitForText("StatusTextBlock", "Finished");

			TakeScreenshot("3 items");

			var heightStrAfter = _app.GetText("HeightTextBlock");
			var heightAfter = int.Parse(heightStrAfter);

			Assert.Greater(heightBefore, 0);

			Assert.AreEqual(3 * heightBefore, heightAfter);
		}

		[Test]
		[AutoRetry]
		public void ListView_SelectedItem()
		{
			Run("SamplesApp.Windows_UI_Xaml_Controls.ListView.ListView_SelectedItem");

			_app.WaitForText("_SelectedItem", "3");
			_app.WaitForText("itemsStackPanelListSelectedItem", "3");
			_app.WaitForText("stackPanelListSelectedItem", "3");

			{
				var firstItem = _app.Marked("itemsStackPanelList").Descendant().Marked("1");
				_app.FastTap(firstItem);
				_app.WaitForText("itemsStackPanelListSelectedItem", "1");
			}

			{
				var firstItem = _app.Marked("stackPanelList").Descendant().Marked("1");
				_app.FastTap(firstItem);
				_app.WaitForText("itemsStackPanelListSelectedItem", "1");
			}

			TakeScreenshot("Both Selection Changed");
		}

		[Test]
		[AutoRetry]
		public void ListView_SelectedItems()
		{
			Run("SamplesApp.Windows_UI_Xaml_Controls.ListView.ListViewSelectedItems");

			_app.FastTap("Right 0");
			_app.WaitForText("_selectedItem", "Selected item: 0");
			_app.WaitForText("SelectionChangedTextBlock", "SelectionChanged event: AddedItems=(0, ), RemovedItems=()");

			_app.FastTap("Left 0");
			_app.WaitForText("_selectedItem", "Selected item: ");
			_app.WaitForText("SelectionChangedTextBlock", "SelectionChanged event: AddedItems=(), RemovedItems=(0, )");

			_app.FastTap("Left 0");
			_app.WaitForText("_selectedItem", "Selected item: 0");
			_app.WaitForText("SelectionChangedTextBlock", "SelectionChanged event: AddedItems=(0, ), RemovedItems=()");

			_app.FastTap("Left 1");
			_app.WaitForText("_selectedItem", "Selected item: 0");
			_app.WaitForText("SelectionChangedTextBlock", "SelectionChanged event: AddedItems=(1, ), RemovedItems=()");

			_app.FastTap("Left 2");
			_app.WaitForText("_selectedItem", "Selected item: 0");
			_app.WaitForText("SelectionChangedTextBlock", "SelectionChanged event: AddedItems=(2, ), RemovedItems=()");

			_app.FastTap("Center 3");
			_app.WaitForText("_selectedItem", "Selected item: 3");
			_app.WaitForText("SelectionChangedTextBlock", "SelectionChanged event: AddedItems=(3, ), RemovedItems=(0, 1, 2, )");

			_app.FastTap("Right 0");
			_app.FastTap("Right 1");
			_app.FastTap("Right 2");
			_app.WaitForText("SelectionChangedTextBlock", "SelectionChanged event: AddedItems=(2, ), RemovedItems=()");

			_app.FastTap("ClearSelectedItemButton");

			_app.WaitForText("SelectionChangedTextBlock", "SelectionChanged event: AddedItems=(), RemovedItems=(3, 0, 1, 2, )");
		}
	}
}
