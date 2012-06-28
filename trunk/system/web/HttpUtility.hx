package system.web;

class HttpUtility 
{
	public static inline function HtmlEncode(s:String):String
	{
		return StringTools.htmlEscape(s);
	}
	
	public static inline function HtmlDecode(s:String):String
	{
		return StringTools.htmlUnescape(s);
	}
	
	public static inline function UrlEncode(s:String):String
	{
		return StringTools.urlEncode(s);
	}
	
	public static inline function UrlDecode(s:String):String
	{
		return StringTools.urlDecode(s);
	}
}