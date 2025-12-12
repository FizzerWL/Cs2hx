package system.web;
import js.Browser;
import system.collections.specialized.NameValueCollection;
import system.NotImplementedException;

class HttpUtility 
{
	public static inline function HtmlEncode(s:String):String
	{
		if (s == null)
			return "";
		else
			return StringTools.htmlEscape(s);
	}
	
	public static inline function HtmlDecode(s:String):String
	{
		return StringTools.htmlUnescape(s);
	}
	
	public static function UrlEncode(s:String):String
	{
		var safe = Reflect.field(Browser.window, 'encodeURIComponentSafe');
		if (safe != null)
			return safe(s);
		else
			return StringTools.urlEncode(s);
	}
	
	public static inline function UrlDecode(s:String):String
	{
		return StringTools.urlDecode(s);
	}
	
	public static function ParseQueryString(query:String):NameValueCollection
	{
		return throw new NotImplementedException();
	}
	
	public static function HtmlAttributeEncode(s:String):String
	{
		if (s == null) return "";

        return s
            .split("&").join("&amp;")
            .split("\"").join("&quot;")
            .split("'").join("&#39;")
            .split("<").join("&lt;")
            .split(">").join("&gt;");
	}
}