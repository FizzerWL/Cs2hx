package system.text;


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
	
	public var length(get_Length, never):Int;
	
	public function get_Length():Int
	{
		return buffer.length;
	}
	
	public function Insert(location:Int, ins:String):Void
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
		Insert(location, String.fromCharCode(char));
	}
	
	public inline function AppendChar(char:Int):Void
	{
		Append(String.fromCharCode(char));
	}
	
	public inline function AppendLine(append:String):Void
	{
		Append(append);
		Append("\n");
	}
}
