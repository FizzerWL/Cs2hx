package system.diagnostics;
import system.NotImplementedException;

class Process
{

	public function new() 
	{
		
	}
	
	public var HasExited:Bool;
	public var ProcessName:String;
	
	public function Kill():Void 
	{
		throw new NotImplementedException();
	}
	
	public static function GetCurrentProcess():Process
	{
		return throw new NotImplementedException();
	}
	
	public static function Start_String(path:String):Process
	{
		return throw new NotImplementedException();
	}
}