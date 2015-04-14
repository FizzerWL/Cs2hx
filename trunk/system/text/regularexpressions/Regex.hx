package system.text.regularexpressions;
import system.NotImplementedException;

class Regex
{
	var _reg:EReg;

	public function new(pattern:String, options:Int = 0) 
	{
		var opts = "g";
		
		if ((options & RegexOptions.IgnoreCase) > 0)
			opts += "i";
		if ((options & RegexOptions.Multiline) > 0)
			opts += "m";
		
		_reg = new EReg(pattern, opts);
	}
	
	public function IsMatch(input:String):Bool
	{
		return throw new NotImplementedException();
	}
	
	public function Match(input:String):Match
	{
		return throw new NotImplementedException();
	}

	public static inline function Replace_String_String_MatchEvaluator(input:String, pattern:String, eval:Match->String):String
	{
		return new Regex(pattern).Replace_String_MatchEvaluator(input, eval);
	}
	
	public static function Replace(input:String, pattern:String, replaceWith:String):String
	{
		return throw new NotImplementedException();
	}
	
	public function Replace_String_MatchEvaluator(input:String, eval:Match->String):String
	{
		#if flash
		return throw new NotImplementedException(); //Haxe's regex implementation in flash calls eval(), which produces a warning: "The method eval is no longer supported.  This functionality has been removed."  So we just don't support Flash
		#else
		return _reg.map(input, function(m) return eval(new Match(m)));
		#end
	}
	
	public static function Escape(str:String):String
	{
		return throw new NotImplementedException();
	}
}