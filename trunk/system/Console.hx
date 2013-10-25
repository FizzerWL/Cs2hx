package system;

class Console
{

	public function new() 
	{
		
	}
	
	public static function WriteLine(s:String):Void
	{
		trace(s);
	}
	
	public static function Write_String(s:String):Void
	{
		trace(s); //note: trace implicitly adds newlines, so calls to Write() may not produce desired results.  Recommend you re-factor into calls to WriteLine if this matters to you.
	}
	
	
}