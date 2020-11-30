﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Data;
using Uno.Disposables;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Uno.UI.Lottie;
using System.Linq;
using System.Json;
using Uno.Extensions;

namespace Microsoft.Toolkit.Uwp.UI.Lottie
{
	[Bindable]
	public class DynamicReloadedLottieAnimatedVisualSource : LottieVisualSourceBase, IDynamicAnimatedVisualSource
	{
		private JsonValue? _currentDocument;

		private readonly Dictionary<string, ColorBinding> _colorsBindings
			= new Dictionary<string, ColorBinding>(2);

		private UpdatedAnimation? _updateCallback;
		private string? _sourceCacheKey;

		public void SetColorProperty(string propertyName, Color? color)
		{
			if(_colorsBindings.TryGetValue(propertyName, out var existing))
			{
				existing.NextValue = color;
			}
			else
			{
				_colorsBindings[propertyName] = new ColorBinding {NextValue = color};
			}

			if (_currentDocument == null)
			{
				return; // no document to change yet
			}

			if (ApplyProperties())
			{
				NotifyCallback();
			}
		}

		protected override bool IsPayloadNeedsToBeUpdated => true;

#if NETFRAMEWORK
		public Task LoadForTests(
			IInputStream sourceJson,
			string sourceCacheKey,
			UpdatedAnimation updateCallback)
		{
			_updateCallback = updateCallback;
			return LoadAndUpdate(default, sourceCacheKey, sourceJson);
		}

		public string? GetJson()
		{
			return _currentDocument?.ToString();
		}
#endif

		protected override IDisposable? LoadAndObserveAnimationData(
			IInputStream sourceJson,
			string sourceCacheKey,
			UpdatedAnimation updateCallback)
		{
			var cts = new CancellationTokenSource();

			_updateCallback = updateCallback;

			var t = LoadAndUpdate(cts.Token, sourceCacheKey, sourceJson);

			return Disposable.Create(() =>
			{
				cts.Cancel();
				cts.Dispose();
			});
		}

		private async Task LoadAndUpdate(
			CancellationToken ct,
			string sourceCacheKey,
			IInputStream sourceJson)
		{
			_sourceCacheKey = sourceCacheKey;

			// Note: we're using Newtownsoft JSON.NET here
			// because System.Text.Json does not support changing the
			// parsed document - it's read only.

			// LOAD JSON
			JsonObject? document;
			using (var stream = sourceJson.AsStreamForRead(0))
			{
				document = JsonValue.Load(stream) as JsonObject;
			}

			if (document == null)
			{
				return;
			}

			// PARSE JSON
			ParseDocument(document);

			// APPLY PROPERTIES
			ApplyProperties();

			// NOTIFY
			NotifyCallback();
		}

		private void ParseDocument(JsonObject document)
		{
			_currentDocument = document;

			foreach (var colorBinding in _colorsBindings)
			{
				colorBinding.Value.Elements.Clear();
			}

			void ParseLayers(JsonArray layers)
			{
				if (layers == null)
				{
					return; // potentially invalid lottie file
				}

				foreach (var layer in layers)
				{
					if (layer is JsonObject l)
					{
						if (l.TryGetValue("shapes", out var shapesValue))
						{
							if (shapesValue is JsonArray shapes)
							{
								foreach (var shape in shapes)
								{
									if (shape is JsonObject s)
									{
										ParseShape(s);
									}
								}
							}
						}
					}
				}
			}

			void ParseShape(JsonObject shapeElement)
			{
				if (!shapeElement.TryGetValue("ty", out var typeValue))
				{
					return; // potentially invalid lottie file
				}
				if (typeValue.JsonType != JsonType.String)
				{
					return; // potentially invalid lottie file
				}

				var shapeType = (string)typeValue;

				if (shapeType != null && shapeType.Equals("gr"))
				{
					// That's a group

					if (!shapeElement.TryGetValue("it", out var itemsProperty)
						|| itemsProperty.JsonType != JsonType.Array)
					{
						return; // potentially invalid lottie file
					}
					if (itemsProperty is JsonArray items)
					{
						foreach (var item in items)
						{
							if (item is JsonObject s)
							{
								ParseShape(s);
							}
						}
					}

					return;
				}

				if(!shapeElement.TryGetValue("nm", out var nameProperty)
					|| nameProperty.JsonType != JsonType.String)
				{
					return; // No name
				}

				var name = (string)nameProperty;

				if (!string.IsNullOrWhiteSpace(name))
				{
					var elementBindings = PropertyBindingsParser.ParseBindings(name);
					if (elementBindings.Length > 0)
					{
						foreach (var binding in elementBindings)
						{
							if (binding.propertyName.Equals("Color", StringComparison.Ordinal))
							{
								if (_colorsBindings.TryGetValue(binding.bindingName, out var colorBinding))
								{
									colorBinding.Elements.Add(shapeElement);
								}
								else
								{
									colorBinding = new ColorBinding();
									colorBinding.Elements.Add(shapeElement);
									_colorsBindings[binding.bindingName] = colorBinding;
								}
							}
						}
					}
				}
			}

			if (document.TryGetValue("layers", out var lyrs) && lyrs is JsonArray documentLayers)
			{
				ParseLayers(documentLayers);
			}
		}

		private bool ApplyProperties()
		{
			var changed = false;
			foreach (var colorBinding in _colorsBindings)
			{
				if (!(colorBinding.Value.NextValue is {} color))
				{
					continue; // nothing to change
				}

				var colorComponents = new[] {color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f};

				foreach (var element in colorBinding.Value.Elements)
				{
					if (element.TryGetValue("c", out var cElm)
					    && cElm is JsonObject c
					    && c.TryGetValue("k", out var kElm)
					    && kElm is JsonArray k)
					{

						k.Clear();
						k.Add(colorComponents[0]);
						k.Add(colorComponents[1]);
						k.Add(colorComponents[2]);
						k.Add(colorComponents[3]);

						changed = true;
					}
				}

				colorBinding.Value.CurrentValue = colorBinding.Value.NextValue;
				colorBinding.Value.NextValue = null;
			}

			return changed;
		}

		private void NotifyCallback()
		{
			if (_updateCallback is {} callback)
			{
				var json = _currentDocument?.ToString();
				if (json is { })
				{
					var propertiesKey = _colorsBindings
						.SelectToArray(kvp => $"{kvp.Key}-{kvp.Value.CurrentValue}")
						.JoinBy("-");

					callback(json, _sourceCacheKey + "-" + propertiesKey);
				}
			}
		}


		private class ColorBinding
		{
			internal List<JsonObject> Elements { get; } = new List<JsonObject>(1);
			internal Color? CurrentValue { get; set; }
			internal Color? NextValue { get; set; }
		}
	}
}