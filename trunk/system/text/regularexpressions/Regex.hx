package system.text.regularexpressions;

import system.NotImplementedException;

class Regex {
	var _pattern:String;
	var _options:Int;

	public function new(pattern:String, options:Int = 0) {
		_pattern = pattern;
		_options = options;
	}

	private function MakeEReg():EReg {
		var opts = "g";

		if ((_options & RegexOptions.IgnoreCase) > 0)
			opts += "i";
		if ((_options & RegexOptions.Multiline) > 0)
			opts += "m";

		return new EReg(_pattern, opts);
	}

	public function IsMatch(input:String):Bool {
		return MakeEReg().match(input);
	}

	public function Match(input:String):Match {
		#if js
		var opts = "g";

		if ((_options & RegexOptions.IgnoreCase) > 0)
			opts += "i";
		if ((_options & RegexOptions.Multiline) > 0)
			opts += "m";

		var pattern = _pattern;
		var matchIter:Dynamic = js.Syntax.code("input.matchAll(new RegExp(pattern, opts))");
		var match:Dynamic = js.Syntax.code("[...matchIter]");

		if (match.length == 0)
			return new Match(false, null); // fail

		var match0:Dynamic = match[0];
		var g = new GroupCollection(function(s) return new Group(Reflect.field(match0.groups, s)), function(i) return new Group(match0.groups[i]));
		return new Match(true, g);
		#else
		return throw new NotImplementedException();
		#end
	}

	public static inline function Replace_String_String_MatchEvaluator(input:String, pattern:String, eval:Match->String):String {
		return new Regex(pattern).Replace_String_MatchEvaluator(input, eval);
	}

	public function Replace(input:String, replace:String):String {
		return Replace_String_MatchEvaluator(input, function(m) return replace);
	}

	public function Replace_String_MatchEvaluator(input:String, eval:Match->String):String {
		return MakeEReg().map(input, function(m) {
			var g = new GroupCollection(function(s) return throw new NotImplementedException("No match by name"),
				function(index) return new Group(m.matched(index)));

			return eval(new Match(true, g));
		});
	}

	public static function Replace_String_String_String_RegexOptions(input:String, pattern:String, replacement:String, opts:Int):String { 
		return throw new NotImplementedException();
	}
	public static function Replace_String_String_String(input:String, pattern:String, replacement:String):String { 
		return throw new NotImplementedException();
	}

	public static function Escape(str:String):String {
		return throw new NotImplementedException();
	}
}
