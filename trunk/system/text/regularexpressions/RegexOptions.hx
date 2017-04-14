package system.text.regularexpressions;

class RegexOptions
{

	public function new() 
	{
		
	}

	public static inline var None = 0;
	public static inline var IgnoreCase = 1;
	public static inline var Multiline = 2;
	public static inline var ExplicitCapture = 4;
	public static inline var Compiled = 8;
	public static inline var Singleline = 16;
	public static inline var IgnorePatternWhitespace = 32;
	public static inline var RightToLeft = 64;
	public static inline var ECMAScript = 256;
	public static inline var CultureInvariant = 512;
}