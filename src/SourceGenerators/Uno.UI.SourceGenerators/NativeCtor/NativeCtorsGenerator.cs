﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno.SourceGeneration;
using Uno.UI.SourceGenerators.Helpers;

namespace Uno.UI.SourceGenerators.NativeCtor
{
	public class NativeCtorsGenerator : SourceGenerator
	{
		public override void Execute(SourceGeneratorContext context)
		{ 
			var visitor = new SerializationMethodsGenerator(context);
			visitor.Visit(context.Compilation.SourceModule);
		}

		private class SerializationMethodsGenerator : SymbolVisitor
		{ 
			private readonly SourceGeneratorContext _context; 
			private readonly Compilation _comp; 
			private readonly INamedTypeSymbol _iosViewSymbol; 
			private readonly INamedTypeSymbol _androidViewSymbol; 
			private readonly INamedTypeSymbol _intPtrSymbol; 
			private readonly INamedTypeSymbol _jniHandleOwnershipSymbol;
			private readonly INamedTypeSymbol[] _javaCtorParams;
			private readonly INamedTypeSymbol _panelSymbol;
			private int _fileIndex;

			private const string BaseClassFormat =
@"// <auto-generated>
// *************************************************************
// This file has been generated by Uno.UI (NativeCtorsGenerator)
// *************************************************************
// </auto-generated>
using System;

namespace {0}
{{
#if __IOS__
	[global::Foundation.Register]
#endif
	partial class {1}
	{{
#if {2}
		public {3}() {{ }}
#endif

#if __ANDROID__
		/// <summary>
		/// Native constructor, do not use explicitly.
		/// </summary>
		/// <remarks>
		/// Used by the Xamarin Runtime to materialize native 
		/// objects that may have been collected in the managed world.
		/// </remarks>
		public {3}(IntPtr javaReference, global::Android.Runtime.JniHandleOwnership transfer) : base (javaReference, transfer) {{ }}
#endif
#if __IOS__
		/// <summary>
		/// Native constructor, do not use explicitly.
		/// </summary>
		/// <remarks>
		/// Used by the Xamarin Runtime to materialize native 
		/// objects that may have been collected in the managed world.
		/// </remarks>
		public {3}(IntPtr handle) : base (handle) {{ }}
#endif
	}}
}}
";

			public SerializationMethodsGenerator(SourceGeneratorContext context)
			{
				_context = context;

				_comp = context.Compilation;

				_iosViewSymbol = context.Compilation.GetTypeByMetadataName("UIKit.UIView");
				_androidViewSymbol = context.Compilation.GetTypeByMetadataName("Android.Views.View");
				_intPtrSymbol = context.Compilation.GetTypeByMetadataName("System.IntPtr");
				_jniHandleOwnershipSymbol = context.Compilation.GetTypeByMetadataName("Android.Runtime.JniHandleOwnership");
				_panelSymbol = context.Compilation.GetTypeByMetadataName("Windows.UI.Xaml.Controls.Panel");
				_javaCtorParams = new[] { _intPtrSymbol, _jniHandleOwnershipSymbol };
			}

			public override void VisitNamedType(INamedTypeSymbol type)
			{
				foreach (var t in type.GetTypeMembers())
				{
					VisitNamedType(t);
				}

				ProcessType(type);
			}

			public override void VisitModule(IModuleSymbol symbol)
			{
				VisitNamespace(symbol.GlobalNamespace);
			}

			public override void VisitNamespace(INamespaceSymbol symbol)
			{
				foreach (var n in symbol.GetNamespaceMembers())
				{
					VisitNamespace(n);
				}

				foreach (var t in symbol.GetTypeMembers())
				{
					VisitNamedType(t);
				}
			}

			private void ProcessType(INamedTypeSymbol typeSymbol)
			{
				var isiOSView = typeSymbol.Is(_iosViewSymbol);
				var isAndroidView = typeSymbol.Is(_androidViewSymbol);
				var smallSymbolName = typeSymbol.ToString().Replace(typeSymbol.ContainingNamespace + ".", "");

				if (isiOSView)
				{
					var nativeCtor = typeSymbol
						.GetMethods()
						.Where(m => m.MethodKind == MethodKind.Constructor && m.Parameters.FirstOrDefault()?.Type == _intPtrSymbol)
						.FirstOrDefault();
					
					if (nativeCtor == null)
					{
						_context.AddCompilationUnit(
							HashBuilder.BuildIDFromSymbol(typeSymbol), 
							string.Format(
								BaseClassFormat, 
								typeSymbol.ContainingNamespace,
								smallSymbolName, 
								NeedsExplicitDefaultCtor(typeSymbol),
								typeSymbol.Name
							)
						);
					}
				}

				if (isAndroidView)
				{

					var nativeCtor = typeSymbol
						.GetMethods()
						.Where(m => m.MethodKind == MethodKind.Constructor && m.Parameters.Select(p => p.Type).SequenceEqual(_javaCtorParams))
						.FirstOrDefault();

					if (nativeCtor == null)
					{
						_context.AddCompilationUnit(
							HashBuilder.BuildIDFromSymbol(typeSymbol),
							string.Format(
								BaseClassFormat, 
								typeSymbol.ContainingNamespace,
								smallSymbolName, 
								NeedsExplicitDefaultCtor(typeSymbol),
								typeSymbol.Name
							)
						);
					}
				}
			}

			private bool NeedsExplicitDefaultCtor(INamedTypeSymbol typeSymbol)
			{
				var hasExplicitConstructor = typeSymbol
					.GetMethods()
					.Where(m => m.MethodKind == MethodKind.Constructor && m.Parameters.Length == 0 && !m.IsImplicitlyDeclared)
					.Any();

				var baseHasDefaultCtor = typeSymbol
					.BaseType?
					.GetMethods()
					.Where(m => m.MethodKind == MethodKind.Constructor && m.Parameters.Length == 0)
					.Any() ?? false;

				return !hasExplicitConstructor && baseHasDefaultCtor;
			}
		}
	}
}

