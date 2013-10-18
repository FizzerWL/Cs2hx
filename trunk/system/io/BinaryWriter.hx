package system.io;

#if flash
import flash.utils.ByteArray;
import flash.utils.Endian;
import system.Exception;
import system.NotImplementedException;
import system.text.UTF8Encoding;
#end

import haxe.io.Bytes;
import haxe.io.BytesOutput;

class BinaryWriter
{
	#if flash
	private var writer:ByteArray;
	#else
	private var writer:BytesOutput;
	#end
	
	public function new(streamOpt:MemoryStream = null) //parameter is ignored. it's just for compatibility with C# code.  Instead get the bytes by calling BaseStream
	{
		#if flash
		writer = new ByteArray();
		writer.endian = Endian.LITTLE_ENDIAN;
		#else
		writer = new BytesOutput();
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
	public function Write_UInt32(data:Int):Void //Should be uint...
	{
		#if flash
		writer.writeUnsignedInt(data);
		#else
		throw new NotImplementedException();
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
		throw new NotImplementedException();
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
