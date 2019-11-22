package system.text;
import system.NotImplementedException;


class StringBuilder
{
	private var buffer:String;
	public var Capacity:Int; //unused
	
	public function new(initial:Dynamic = null)
	{
		if (initial == null)
			buffer = "";
		else if (Std.is(initial, String))
			buffer = initial;
		else
			buffer = "";
	}
	
	public var Length(get_Length, never):Int;
	
	public function get_Length():Int
	{
		return buffer.length;
	}
	
	public function Insert_Int32_Char(location:Int, c:Int):Void
	{
		Insert_Int32_String(location, String.fromCharCode(c));
	}
	
	public function Insert_Int32_String(location:Int, ins:String):Void
	{
		buffer = buffer.substr(0, location) + ins + buffer.substr(location);
	}
	
	public inline function toString():String
	{
		return buffer;  
	}
	public inline function Append_String(append:String):StringBuilder
	{
		Append(append);
		return this;
	}
	
	public inline function Append(append:String):StringBuilder
	{
		if (append != null)
			buffer += append;
		return this;
	}
	
	public inline function Append_Double(d:Float):StringBuilder
	{
		buffer += Std.string(d);
		return this;
	}
	
	public inline function InsertChar(location:Int, char:Int):Void
	{
		Insert_Int32_String(location, String.fromCharCode(char));
	}
	
	public inline function Append_Char(char:Int):StringBuilder
	{
		Append(String.fromCharCode(char));
		return this;
	}
	public inline function Append_Char_Int32(char:Int, repeatCount:Int):Void
	{
		for (i in 0...repeatCount)
			Append_Char(char);
	}
	
	public inline function AppendLine(append:String = ""):StringBuilder
	{
		Append(append);
		Append("\n");
		return this;
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
