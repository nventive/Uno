﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows.Storage.Streams
{
	public partial class DataReader : IDataReader, IDisposable
	{
		private readonly IBuffer _buffer = null;
		private uint _position = 0;

		public ByteOrder ByteOrder { get; set; }
		public UnicodeEncoding UnicodeEncoding { get; set; }

		public uint UnconsumedBufferLength
		{
			get => _buffer.Length - _position;
		}

		private void CheckPosition(uint size)
		{
			if (_buffer.Length >= (_position + size))
			{
				return;
			}

			// "internal index" , not parameter error
			throw new IndexOutOfRangeException("Windows.Storage.Streams.DataReader - reading past EOF");
		}

		public byte ReadByte()
		{
			CheckPosition(1);
			byte value = _buffer.Data[_position];
			_position++;
			return value;
		}

		public void ReadBytes(byte[] value)
		{
			if (value is null)
			{
				throw new ArgumentNullException("Windows.Storage.Streams.DataReader.ReadBytes(byte[]) called with null argument");
			}

			CheckPosition((uint)value.Length);
			for(int i=0; i< value.Length; i++)
			{
				value[i] = _buffer.Data[_position];
				_position++;
			}
		}

		public IBuffer ReadBuffer(uint length)
		{
			CheckPosition(length);

			var buffer = new InMemoryBuffer((int)length);
			for (int i = 0; i < length; i++)
			{
				buffer.Data[i] = _buffer.Data[_position];
				_position++;
			}
			return buffer;
		}
		public bool ReadBoolean()
		{
			byte value = ReadByte();
			if (value == 0)
			{
				return false;
			}
			return true;        // although DataWriter uses 0/1, we can also interpret all non-zero values as true;
		}

		private byte[] ReadBytes(int size)
		{
			CheckPosition((uint)size);

			byte[] buffer = new byte[size];
			for (int i = 0; i < size; i++)
			{
				buffer[i] = _buffer.Data[_position];
				_position++;
			}

			// maybe we should reverse array (if platform endianess is different than requested)
			if ((ByteOrder == ByteOrder.LittleEndian && !BitConverter.IsLittleEndian)
				|| (ByteOrder == ByteOrder.BigEndian && BitConverter.IsLittleEndian))
			{
				Array.Reverse(buffer);
			}

			return buffer;
		}

		public Guid ReadGuid()
		{
			Int32 u32 = ReadInt32();
			Int16 u16a = ReadInt16();
			Int16 u16b = ReadInt16();
			byte[] u64 = ReadBytes(8);

			var value = new Guid(u32, u16a, u16b, u64);
			return value;
		}
		public short ReadInt16() => BitConverter.ToInt16(ReadBytes(2), 0);
		public int ReadInt32() => BitConverter.ToInt32(ReadBytes(4), 0);
		public long ReadInt64() => BitConverter.ToInt64(ReadBytes(8), 0);
		public ushort ReadUInt16() => BitConverter.ToUInt16(ReadBytes(2), 0);
		public uint ReadUInt32() => BitConverter.ToUInt32(ReadBytes(4), 0);
		public ulong ReadUInt64() => BitConverter.ToUInt64(ReadBytes(8), 0);
		public float ReadSingle()
		{// yes, it is endianness dependant
			return BitConverter.ToSingle(ReadBytes(4), 0);
		}
		public double ReadDouble()
		{// yes, it is endianness dependant
			return BitConverter.ToDouble(ReadBytes(8), 0);
		}

		public string ReadString(uint codeUnitCount)
		{
			// although docs says that input parameter is "codeUnitCount", sample
			// https://docs.microsoft.com/en-us/uwp/api/Windows.Storage.Streams.DataReader?view=winrt-19041
			// shows that it is BYTE count, not CODEUNIT count.
			// codepoint in UTF-8 can be encoded in anything from 1 to 6 bytes.

			CheckPosition(codeUnitCount);

			string result;
			switch (UnicodeEncoding)
			{
				case UnicodeEncoding.Utf16LE:
					result = Encoding.Unicode.GetString(_buffer.Data, (int)_position, (int)codeUnitCount);
					break;
				case UnicodeEncoding.Utf16BE:
					result = Encoding.BigEndianUnicode.GetString(_buffer.Data, (int)_position, (int)codeUnitCount);
					break;
				default:
					result = Encoding.UTF8.GetString(_buffer.Data, (int)_position, (int)codeUnitCount);
					break;
			}
			_position += codeUnitCount;
			return result;
		}

		public DateTimeOffset ReadDateTime()
		{
			long ticks = ReadInt64();
			var date = new DateTime(1601, 1, 1, 0, 0, 0).ToLocalTime();
			date = date.AddTicks(ticks);

			return date;
		}
		public TimeSpan ReadTimeSpan() => TimeSpan.FromTicks(ReadInt64());
		public IBuffer DetachBuffer() => _buffer;
		public void Dispose()
		{
			// for currently implemented ctor, DataReader(IBuffer), nothing to do,
			// but we don't want unimplemented exception for this method.
		}

		// unimplemented another constructor: with Streams.IInputStream as input parameter
		internal DataReader(IBuffer buffer)
		{
			_buffer = buffer;
		}

		public static DataReader FromBuffer(IBuffer buffer) => new DataReader(buffer);
	}

}
