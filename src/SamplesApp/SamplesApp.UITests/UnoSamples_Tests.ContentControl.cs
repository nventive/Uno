﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Uno.UITest.Helpers;
using Uno.UITest.Helpers.Queries;

namespace SamplesApp.UITests
{
	[TestFixture]
	public partial class UnoSamples_Tests : SampleControlUITestBase
	{
		[Test]
		public void ContentPresenter_Template()
		{
			Run("Uno.UI.Samples.Content.UITests.ContentPresenter.ContentPresenter_Template");


			var tb1 = _app.Marked("innerText");
			Assert.AreEqual("ContentPresenter:  SampleControl.Presentation.SampleChooserViewModel", tb1.GetDependencyPropertyValue("Text").ToString());
			var tb2 = _app.Marked("innerText2");
			Assert.AreEqual("ContentControl:  SampleControl.Presentation.SampleChooserViewModel", tb2.GetDependencyPropertyValue("Text").ToString());

			_app.Tap(c => c.Text("Click me"));

			var tb3 = _app.Marked("innerText");
			Assert.AreEqual("ContentPresenter:  42", tb3.GetDependencyPropertyValue("Text").ToString());
			var tb4 = _app.Marked("innerText2");
			Assert.AreEqual("ContentControl:  42", tb4.GetDependencyPropertyValue("Text").ToString());

			_app.Tap(c => c.Text("Click me"));

			Assert.IsFalse(_app.Marked("innerText").HasResults());
			Assert.IsFalse(_app.Marked("innerText2").HasResults());
		}

		[Test]
		public void ContentPresenter_Changing_ContentTemplate()
		{
			Run("Uno.UI.Samples.Content.UITests.ContentPresenter.ContentPresenter_Changing_ContentTemplate");

			Assert.IsFalse(_app.Marked("ContentViewBorder").HasResults());

			_app.Tap(c => c.Marked("ToggleTemplateButton"));

			Assert.IsTrue(_app.Marked("ContentViewBorder").HasResults());

		}

		[Test]
		public void ContentControl_Changing_ContentTemplate()
		{
			Run("Uno.UI.Samples.Content.UITests.ContentControlTestsControl.ContentControl_Changing_ContentTemplate");

			Assert.IsFalse(_app.Marked("ContentViewBorder").HasResults());

			_app.Tap(c => c.Marked("ToggleTemplateButton"));

			Assert.IsTrue(_app.Marked("ContentViewBorder").HasResults());

		}
	}
}
