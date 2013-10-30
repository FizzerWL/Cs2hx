package system.text;
import system.NotImplementedException;


class StringBuilder
{
	private var buffer:String;
	
	public function new(initial:Dynamic = null)
	{
		if (initial == null)
		{
			buffer = "";
			return;
		}

		if (Std.is(initial, String))
			buffer = initial;
		else
			buffer = "";
	}
	
	public var Length(get_Length, never):Int;
	
	public function get_Length():Int
	{
		return buffer.length;
	}
	
	public function Insert_Int32_String(location:Int, ins:String):Void
	{
		buffer = buffer.substr(0, location) + ins + buffer.substr(location);
	}
	
	public inline function toString():String
	{
		return buffer;  
	}
	
	public inline function Append(append:String):Void
	{
		buffer += append;
	}
	
	public inline function InsertChar(location:Int, char:Int):Void
	{
		Insert_Int32_String(location, String.fromCharCode(char));
	}
	
	public inline function Append_Char(char:Int):Void
	{
		Append(String.fromCharCode(char));
	}
	
	public inline function AppendLine(append:String = ""):Void
	{
		Append(append);
		Append("\n");
	}
	
	public inline function Append_Int32(i:Int):Void
	{
		Append(Std.string(i));
	}
	
	public inline function Append_String_Int32_Int32(append:String, startAt:Int, len:Int):Void
	{
		Append(append.substr(startAt, len));
	}
	
	public function Remove(startIndex:Int, length:Int):Void
	{
		throw new NotImplementedException();
	}
	
	public function GetValue_Int32(index:Int):Int
	{
		return buffer.charCodeAt(index);
	}
	
	public function Clear():Void
	{
		buffer = "";
	}
}
