package system.web;
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
		// StringTools.urlEncode(s) calls encodeURIComponent, which can throw on characters 55296 through 57343.  So instead filter those out
		var i = 0;
		while (i < s.length) {
			var c = s.charCodeAt(i);
			if (c >= 55296 && c <= 57343) {
				var r = s.substr(0, i);
				i++;
				while (i < s.length) {
					var c = s.charCodeAt(i);
					if (c < 55296 || c > 57343)
						r += s.charAt(i);
					i++;
				}
				return StringTools.urlEncode(r);
			}
			i++;
		}
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
}