using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Uno.UI.Samples.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UITests.Windows_UI_Xaml_Shapes
{
	[Sample("Shapes", "Transform")]
	public sealed partial class Shapes_Transform : Page
	{
		public Shapes_Transform()
		{
			this.InitializeComponent();

			Loaded += OnPageLoaded;
		}

		private void OnPageLoaded(object sender, RoutedEventArgs e)
		{
			var path = new Windows.UI.Xaml.Shapes.Path
			{
				Fill = new SolidColorBrush(Colors.DeepPink),
				Stroke = new SolidColorBrush(Colors.DarkMagenta),
				StrokeThickness = 8,
				Data = new PathGeometry
				{
					Figures =
					{
						new PathFigure
						{
							IsClosed = true,
							StartPoint = new Point(25, 25),
							Segments =
							{
								new LineSegment{Point = new Point(25,125)},
								new LineSegment{Point = new Point(75,125)},
								new BezierSegment{Point1 = new Point(125,125), Point2 = new Point(125,25), Point3 = new Point(75, 25)},
							}
						}
					}
				},
				Stretch = Stretch.Uniform,
				RenderTransformOrigin = new Point(.5,.5),
				RenderTransform = new CompositeTransform
				{
					Rotation = 90
				}
			};

			DeferedPath.Children.Add(path);
		}
	}
}
