package system.io;

#if flash
import flash.utils.ByteArray;
import flash.utils.Endian;
import system.Exception;
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
	
	public function new()
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
	
	public function WriteByteArray(data:Bytes):Void
	{
		#if flash
		writer.writeBytes(data.getData(), 0, data.length);
		#else
		writer.writeFullBytes(data, 0, data.length);
		#end
	}
	public function WriteInt32(data:Int):Void
	{
		#if flash
		writer.writeInt(data);
		#else
		writer.writeInt32(haxe.Int32.ofInt(data));
		#end
	}
	public function WriteUInt16(data:Int):Void
	{
		for (i in 0...2)
			writer.writeByte(data >> (8 * i));
	}
	public function WriteInt16(data:Int):Void
	{
		for (i in 0...2)
			writer.writeByte(data >> (8 * i));
	}
	public function WriteUInt32(data:Int):Void //Should be uint...
	{
		#if flash
		writer.writeUnsignedInt(data);
		#else
		writer.writeUInt30(data);
		#end
	}
	public function WriteString(data:String):Void
	{
		#if flash
		
		var b:ByteArray = new ByteArray();
		b.writeUTFBytes(data);
		
		this.Write7BitEncodedInt(b.length);
		writer.writeBytes(b);
		
		#else
		throw new Exception("TODO");
		#end
	}
	public function WriteBoolean(data:Bool):Void
	{
		#if flash
		writer.writeBoolean(data);
		#else
		writer.writeByte(data ? 1 : 0);
		#end
	}
	public function WriteDouble(data:Float):Void
	{
		writer.writeDouble(data);
	}
	public function WriteSingle(data:Float):Void
	{
		writer.writeFloat(data);
	}
	public function WriteByte(data:Int):Void
	{
		writer.writeByte(data);
	}
	
	public function Write7BitEncodedInt(value:Int):Void
	{
		var num:Int = value;
		while (num >= 0x80)
		{
			this.WriteByte(num | 0x80);
			num = num >> 7;
		}
		this.WriteByte(num);
	}
}
