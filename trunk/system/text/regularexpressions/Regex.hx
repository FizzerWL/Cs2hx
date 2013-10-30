package system.text.regularexpressions;
import system.NotImplementedException;

class Regex
{

	public function new(pattern:String, options:Int = 0) 
	{
		throw new NotImplementedException();
	}
	
	public function IsMatch(input:String):Bool
	{
		return throw new NotImplementedException();
	}
	
	public function Match(input:String):Match
	{
		return throw new NotImplementedException();
	}

	public static function Replace_String_String_MatchEvaluator(input:String, pattern:String, eval:Match->String):String
	{
		return throw new NotImplementedException();
	}
	
	public static function Replace(input:String, pattern:String, replaceWith:String):String
	{
		return throw new NotImplementedException();
	}
}