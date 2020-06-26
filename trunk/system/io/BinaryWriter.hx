package system.io;

#if flash
import flash.utils.ByteArray;
import flash.utils.Endian;
import system.Exception;
import system.NotImplementedException;
import system.text.UTF8Encoding;
#end

import haxe.io.Bytes;
import haxe.io.BytesBuffer;
import haxe.io.BytesOutput;

class BinaryWriter
{
	#if flash
	private var writer:ByteArray;
	#else
	private var writer:BytesOutput;
	#end
	
	public function new(unused:MemoryStream = null) //parameter is ignored. it's just for compatibility with C# code.  Instead get the bytes by calling BaseStream
	{
		#if flash
		writer = new ByteArray();
		writer.endian = Endian.LITTLE_ENDIAN;
		#else
		writer = new BytesOutput();
		writer.bigEndian = false;
		#end
	}
	
	public function GetBytes():Bytes
	{
		#if flash
		return Bytes.ofData(writer);
		#else
		return writer.getBytes();
		#end
	}
	
	public function Write_(data:Bytes):Void //TODO: Fix the overload resolution so this can have a better name
	{
		#if flash
		writer.writeBytes(data.getData(), 0, data.length);
		#else
		writer.writeFullBytes(data, 0, data.length);
		#end
	}
	public function Write_Int32(data:Int):Void
	{
		#if flash
		writer.writeInt(data);
		#else
		writer.writeInt32(data);
		#end
	}
	public function Write_UInt16(data:Int):Void
	{
		for (i in 0...2)
			writer.writeByte(data >> (8 * i));
	}
	public function Write_Int16(data:Int):Void
	{
		for (i in 0...2)
			writer.writeByte(data >> (8 * i));
	}
	public function Write_Int64(f:Float):Void
	{
		//setBigInt64 isn't supported on enough browsers yet as of 2020, so we implement this manually
		//https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/DataView/setBigInt64
		if (f >= 0)
		{
			var high = f % 4294967296;
			var low = f / 4294967296;
			Write_UInt32(Std.int(high));
			Write_UInt32(Std.int(low));
		}
		else
		{
			var highRaw = (-f) % 4294967296;
			var lowRaw = (-f) / 4294967296;
			var high = ~Std.int(highRaw);
			var low = ~Std.int(lowRaw);
			
			//Add one
			var c00 = (high & 0xFFFF) + 1;
            var c16 = c00 >> 16;
            c00 &= 0xFFFF;
            c16 += high >> 16;
            var c32 = c16 >> 16;
            c16 &= 0xFFFF;
            c32 += low & 0xFFFF;
            var c48 = c32 >> 16;
            c32 &= 0xFFFF;
            c48 += low >> 16;
            c48 &= 0xFFFF;
            high = (c16 << 16) | c00;
            low = (c48 << 16) | c32;
			
			
			Write_UInt32(Std.int(high));
			Write_UInt32(Std.int(low));
		}

	}
	public function Write_UInt32(data:Int):Void
	{
		#if flash
		writer.writeUnsignedInt(data);
		#else
		writer.writeByte(data & 0xFF);
		writer.writeByte((data >> 8) & 0xFF);
		writer.writeByte((data >> 16) & 0xFF);
		writer.writeByte((data >> 24) & 0xFF);
		#end
	}
	public function Write_String(data:String):Void
	{
		#if flash
		
		var b:ByteArray = new ByteArray();
		b.writeUTFBytes(data);
		
		this.Write7BitEncodedInt(b.length);
		writer.writeBytes(b);
		
		#else
		
		var bytes = Bytes.ofString(data);
		
		this.Write7BitEncodedInt(bytes.length);
		writer.writeBytes(bytes, 0, bytes.length);
		
		#end
	}
	public function Write(data:Bool):Void
	{
		#if flash
		writer.writeBoolean(data);
		#else
		writer.writeByte(data ? 1 : 0);
		#end
	}
	public function Write_Double(data:Float):Void
	{
		writer.writeDouble(data);
	}
	public function Write_Single(data:Float):Void
	{
		writer.writeFloat(data);
	}
	public function Write_Byte(data:Int):Void
	{
		writer.writeByte(data);
	}
	
	public function Write7BitEncodedInt(value:Int):Void
	{
		var num:Int = value;
		while (num >= 0x80)
		{
			this.Write_Byte(num | 0x80);
			num = num >> 7;
		}
		this.Write_Byte(num);
	}
	
	public var BaseStream (get, never):MemoryStream;
	
	public function get_BaseStream():MemoryStream
	{
		return new MemoryStream(GetBytes());
	}
}
