//
// Copyright (C) 2010 Novell Inc. http://novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace Uno.Xaml
{
	internal class XamlSubtreeReader : XamlReader
	{
		internal XamlSubtreeReader (XamlReader source)
		{
			_source = source;
		}

		private readonly XamlReader _source;

		public override bool IsEof {
			get { return _started && (_nest == 0 || _source.IsEof); }
		}
		public override XamlMember Member {
			get { return _started ? _source.Member : null; }
		}
		
		public override NamespaceDeclaration Namespace {
			get { return _started ? _source.Namespace : null; }
		}
		
		public override XamlNodeType NodeType {
			get { return _started ? _source.NodeType : XamlNodeType.None; }
		}
		
		public override XamlSchemaContext SchemaContext {
			get { return _source.SchemaContext; }
		}
		
		public override XamlType Type {
			get { return _started ? _source.Type : null; }
		}
		
		public override object Value {
			get { return _started ? _source.Value : null; }
		}
		
		protected override void Dispose (bool disposing)
		{
			while (_nest > 0)
			{
				if (!Read ())
				{
					break;
				}
			}

			base.Dispose (disposing);
		}

		private bool _started;
		private int _nest;
		
		public override bool Read ()
		{
			if (_started) {
				if (_nest == 0) {
					_source.Read ();
					return false; // already consumed
				}
				if (!_source.Read ())
				{
					return false;
				}
			}
			else
			{
				_started = true;
			}

			switch (_source.NodeType) {
			case XamlNodeType.StartObject:
			case XamlNodeType.GetObject:
			case XamlNodeType.StartMember:
				_nest++;
				break;
			case XamlNodeType.EndObject:
			case XamlNodeType.EndMember:
				_nest--;
				break;
			}
			return true;
		}
	}
}
