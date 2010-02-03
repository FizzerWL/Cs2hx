package system.io;

import flash.utils.ByteArray;	
import flash.utils.Endian;
import flash.Error;
import system.text.StringBuilder;

class BinaryReader
{
	public var arr:ByteArray;
	
	public function new(readFrom:ByteArray)
	{
		arr = readFrom;
		arr.endian = Endian.LITTLE_ENDIAN;
	}
	
	public function Read7BitEncodedInt():Int
	{
		var num3:Int;
		var num:Int = 0;
		var num2:Int = 0;
		do
		{
			if (num2 == 0x23)
				throw new Error("Bad format");
				
			num3 = this.ReadByte();
			num |= (num3 & 0x7f) << num2;
			num2 += 7;
		}
		while ((num3 & 0x80) != 0);
		return num;
	}
	
	public function ReadBytes(num:Int):ByteArray
	{
		var ret:ByteArray = new ByteArray();
		while (num > 0)
		{
			ret.writeByte(this.ReadByte());
			num--;
		}
		return ret;
	}
	
	public function ReadInt64():Float
	{
		return Std.parseFloat(ReadString());
	}
	public function ReadInt32():Int
	{
		return arr.readInt();
	}
	public function ReadUInt16():Int
	{
		return arr.readUnsignedShort();
	}
	public function ReadInt16():Int
	{
		return arr.readShort();
	}
	public function ReadUInt32():Int //should be uint
	{
		return arr.readUnsignedInt();
	}
	public function ReadString():String
	{
		var length:Int = Read7BitEncodedInt();
		
		//trace("Read string length of " + length);
		
		if (length == 0)
			return "";
			
		var sb:StringBuilder = new StringBuilder();
		
		for (i in 0...length) 
			sb.AppendChar(this.ReadByte());
		return sb.toString();
	}
	public function ReadBoolean():Bool
	{
		var ret:Bool = arr.readBoolean();
		return ret;
	}
	public function ReadDouble():Float
	{
		return arr.readDouble();
	}
	public function ReadSingle():Float
	{
		return arr.readFloat();
	}
	public function ReadByte():Int
	{
		return arr.readUnsignedByte();
	}
}
