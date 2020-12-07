﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace Microsoft.UI.Xaml.Controls
{
	public partial class NavigationViewItemHeader : NavigationViewItemBase
	{
		private Grid m_rootGrid = null;
		private bool m_isClosedCompact = false;
		private long m_splitViewIsPaneOpenChangedRevoker;
		private long m_splitViewDisplayModeChangedRevoker;
		private const string c_rootGrid = "NavigationViewItemHeaderRootGrid";

		public NavigationViewItemHeader()
		{
			DefaultStyleKey = typeof(NavigationViewItemHeader);
		}

		protected override void OnApplyTemplate()
		{
			// TODO: Uno specific: NavigationView may not be set yet, wait for later #4689
			if (GetNavigationView() is null)
			{
				// Postpone template application for later
				return;
			}

			var splitView = GetSplitView();
			if (splitView != null)
			{
				//TODO: MZ: Unsubscribe?
				m_splitViewIsPaneOpenChangedRevoker = splitView.RegisterPropertyChangedCallback(
					SplitView.IsPaneOpenProperty,
					OnSplitViewPropertyChanged);
				m_splitViewDisplayModeChangedRevoker = splitView.RegisterPropertyChangedCallback(
					SplitView.DisplayModeProperty,
					OnSplitViewPropertyChanged);

				UpdateIsClosedCompact();
			}

			var rootGrid = GetTemplateChild(c_rootGrid) as Grid;
			if (rootGrid != null)
			{
				m_rootGrid = rootGrid;
			}

			UpdateVisualState(false /*useTransitions*/);
			UpdateItemIndentation();

			var visual = ElementCompositionPreview.GetElementVisual(this);
			NavigationView.CreateAndAttachHeaderAnimation(visual);
		}

		private void OnSplitViewPropertyChanged(DependencyObject sender, DependencyProperty args)
		{
			if (args == SplitView.IsPaneOpenProperty ||
				args == SplitView.DisplayModeProperty)
			{
				UpdateIsClosedCompact();
			}
		}

		private void UpdateIsClosedCompact()
		{
			var splitView = GetSplitView();
			if (splitView != null)
			{
				// Check if the pane is closed and if the splitview is in either compact mode.
				m_isClosedCompact = !splitView.IsPaneOpen && (splitView.DisplayMode == SplitViewDisplayMode.CompactOverlay || splitView.DisplayMode == SplitViewDisplayMode.CompactInline);
				UpdateVisualState(true /*useTransitions*/);
			}
		}

		// TODO: can new cause issues?
		private new void UpdateVisualState(bool useTransitions)
		{
			VisualStateManager.GoToState(this, m_isClosedCompact && IsTopLevelItem ? "HeaderTextCollapsed" : "HeaderTextVisible", useTransitions);
		}

		protected override void OnNavigationViewItemBaseDepthChanged()
		{
			UpdateItemIndentation();
		}

		private void UpdateItemIndentation()
		{
			// Update item indentation based on its depth
			var rootGrid = m_rootGrid;
			if (rootGrid != null)
			{
				var oldMargin = rootGrid.Margin;
				var newLeftMargin = Depth * c_itemIndentation;
				rootGrid.Margin = new Thickness(
					(double)(newLeftMargin),
					oldMargin.Top,
					oldMargin.Right,
					oldMargin.Bottom);
			}
		}
	}
}