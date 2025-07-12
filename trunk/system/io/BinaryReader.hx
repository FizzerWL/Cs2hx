package system.io;

import haxe.io.Eof;
import haxe.io.BytesInput;
import haxe.io.Bytes;
import system.text.StringBuilder;
import system.Exception;
import system.text.UTF8Encoding;

#if flash
import flash.utils.ByteArray;	
import flash.utils.Endian;
#end

class BinaryReader
{
	#if flash
	private var arr:ByteArray;
	#else
	private var reader:BytesInput;
	#end
	
	public function new(readFrom:Stream)
	{
		#if flash
		arr = readFrom.ToArray().getData();
		arr.position = 0;
		arr.endian = Endian.LITTLE_ENDIAN;
		#else
		reader = new BytesInput(readFrom.ToArray());
		//reader.bigEndian = false;  //commented since it's already the default
		#end
	}
	
	private function Read7BitEncodedInt():Int
	{
		var num3:Int;
		var num:Int = 0;
		var num2:Int = 0;
		do
		{
			if (num2 == 0x23)
				throw new Exception("Bad format");
				
			num3 = this.ReadByte();
			num |= (num3 & 0x7f) << num2;
			num2 += 7;
		}
		while ((num3 & 0x80) != 0);
		return num;
	}
	
	public function ReadBytes(num:Int):Bytes
	{
		try {
		var r:Bytes = Bytes.alloc(num);
		
		var i:Int = 0;
		while (i < num)
			r.set(i++, this.ReadByteInternal());

		return r;
		}
		catch (e:Eof) {
			throw new EndOfStreamException();
		}
	}
	
	public function ReadInt64():Float
	{
		//getBigInt64 isn't supported on enough browsers yet as of 2020, so we implement this manually
		var low = ReadUInt32();
		var high = ReadUInt32();
		
		var lowUnsigned = low >= 0 ? low : 4294967296 + low;
		
		return high * 4294967296 + lowUnsigned;
		
	}
	public function ReadInt32():Int
	{
		try {
		#if flash
		return arr.readInt();
		#else
		return reader.readInt32();
		#end
		}
		catch (e:Eof) {
			throw new EndOfStreamException();
		}
	}
	public function ReadUInt16():Int
	{
		try {
		#if flash
		return arr.readUnsignedShort();
		#else
		return reader.readUInt16();
		#end
		}
		catch (e:Eof) {
			throw new EndOfStreamException();
		}
	}
	public function ReadInt16():Int
	{
		try {
		#if flash
		return arr.readShort();
		#else
		return reader.readInt16();
		#end
		}
		catch (e:Eof) {
			throw new EndOfStreamException();
		}
	}
	public function ReadUInt32():Int //should be uint
	{
		try {
		#if flash
		return arr.readUnsignedInt();
		#else
		return reader.readInt32();
		#end
		}
		catch (e:Eof) {
			throw new EndOfStreamException();
		}
	}
	public function ReadString():String
	{
		try {
		var bytes:Int = Read7BitEncodedInt();
		
		#if flash
		
		return arr.readUTFBytes(bytes);

		#else
		
		var arr = ReadBytes(bytes);
		return arr.getString(0, arr.length);
		
		#end
		}
		catch (e:Eof) {
			throw new EndOfStreamException();
		}
	}
	public function ReadBoolean():Bool
	{
		try {
		#if flash
		return arr.readBoolean();
		#else
		return reader.readByte() != 0;
		#end
		}
		catch (e:Eof) {
			throw new EndOfStreamException();
		}
	}
	public function ReadDouble():Float
	{
		try {
		#if flash
		return arr.readDouble();
		#else
		return reader.readDouble();
		#end
		}
		catch (e:Eof) {
			throw new EndOfStreamException();
		}
	}
	public function ReadSingle():Float
	{
		try {
		#if flash
		return arr.readFloat();
		#else
		return reader.readFloat();
		#end
		}
		catch (e:Eof) {
			throw new EndOfStreamException();
		}
	}
	public function ReadByte():Int {
		try {
			return ReadByteInternal();
		}
		catch (e:Eof) {
			throw new EndOfStreamException();
		}
	}
	public function ReadByteInternal():Int
	{
		#if flash
		return arr.readUnsignedByte();
		#else
		return reader.readByte();
		#end
	}
	
	public function Dispose():Void
	{
	}
}
