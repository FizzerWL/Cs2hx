package system.io;

import flash.Error;
import flash.utils.ByteArray;
import flash.utils.Endian;

class BinaryWriter
{
	public var arr:ByteArray;
	
	public function new()
	{
		arr = new ByteArray();
		arr.endian = Endian.LITTLE_ENDIAN;
	}
	
	public function WriteInt64(data:Float):Void
	{
		throw new Error("WriteInt64 TODO");
		//for (i in 0...8)
		//	arr.writeByte(data >> (8 * i));
	}
	public function WriteByteArray(data:ByteArray):Void
	{
		arr.writeBytes(data, 0, data.length);
	}
	public function WriteInt32(data:Int):Void
	{
		arr.writeInt(data);
	}
	public function WriteUInt16(data:Int):Void
	{
		for (i in 0...2)
			arr.writeByte(data >> (8 * i));
	}
	public function WriteInt16(data:Int):Void
	{
		for (i in 0...2)
			arr.writeByte(data >> (8 * i));
	}
	public function WriteUInt32(data:Int):Void //Should be uint...
	{
		arr.writeUnsignedInt(data);
	}
	public function WriteString(data:String):Void
	{
		this.Write7BitEncodedInt(data.length);
		
		for (i in 0...data.length)
			WriteByte(data.charCodeAt(i));
	}
	public function WriteBoolean(data:Bool):Void
	{
		arr.writeBoolean(data);
	}
	public function WriteDouble(data:Float):Void
	{
		arr.writeDouble(data);
	}
	public function WriteSingle(data:Float):Void
	{
		arr.writeFloat(data);
	}
	public function WriteByte(data:Int):Void
	{
		arr.writeByte(data);
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
