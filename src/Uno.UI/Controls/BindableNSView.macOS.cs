﻿using Uno.Collections;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uno.Extensions;
using Uno.UI.DataBinding;
using System.Runtime.CompilerServices;
using System.Drawing;
using Uno.Disposables;
using Windows.UI.Xaml;
using System.ComponentModel;
using Windows.UI.Xaml.Media;
using AppKit;
using System.Collections;

namespace Uno.UI.Controls
{
	public partial class BindableNSView : NSView, INotifyPropertyChanged, DependencyObject, IShadowChildrenProvider, IEnumerable
	{
		private List<NSView> _shadowChildren = new List<NSView>();

		IReadOnlyList<NSView> IShadowChildrenProvider.ChildrenShadow => _shadowChildren;

		internal IReadOnlyList<NSView> ChildrenShadow => _shadowChildren;

		public override void DidAddSubview(NSView NSView)
		{
			base.DidAddSubview(NSView);

			// Reference the list as we don't know where 
			// the items has been added other than by getting the complete list.
			// Subviews materializes a new array at every call, which makes it safe to
			// reference.
			_shadowChildren = Subviews.ToList();
		}

		internal List<NSView>.Enumerator GetChildrenEnumerator() => _shadowChildren.GetEnumerator();

		public override void WillRemoveSubview(NSView NSView)
		{
			base.WillRemoveSubview(NSView);

			var position = _shadowChildren.IndexOf(NSView, ReferenceEqualityComparer<NSView>.Default);

			if(position != -1)
			{
				_shadowChildren.RemoveAt(position);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public BindableNSView()
		{
			Initialize();
		}

		public BindableNSView(IntPtr handle)
			: base(handle)
		{
			Initialize();
		}

		public BindableNSView(RectangleF frame)
			: base(frame)
		{
			Initialize();
		}

		private void Initialize()
		{
			InitializeBinder();
		}

		/// <summary>
		/// Moves a view from one position to another position, without unloading it.
		/// </summary>
		/// <param name="oldIndex">The old index of the item</param>
		/// <param name="newIndex">The new index of the item</param>
		/// <remarks>
		/// The trick for this method is to move the child from one position to the other
		/// without calling RemoveView and AddView. In this context, the only way to do this is
		/// to call BringSubviewToFront, which is the only available method on ViewGroup that manipulates 
		/// the index of a view, even if it does not allow for specifying an index.
		/// </remarks>
		internal void MoveViewTo(int oldIndex, int newIndex)
		{
			var newShadow = _shadowChildren.ToList();

			var view = newShadow[oldIndex];

			newShadow.RemoveAt(oldIndex);
			newShadow.Insert(newIndex, view);

			var reorderIndex = Math.Min(oldIndex, newIndex);

			for (int i = reorderIndex; i < newShadow.Count; i++)
			{
				// TODO: Use AddSubview with foremost
				// BringSubviewToFront(newShadow[i]);
			}

			_shadowChildren = newShadow.ToList();
		}

		protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public IEnumerator GetEnumerator() => Subviews.GetEnumerator();
	}
}
