package system.text;

class Encoding
{

	public function new() 
	{
		
	}
	
	public static var UTF8 (get, never):UTF8Encoding;
	public static function get_UTF8():UTF8Encoding
	{
		return new UTF8Encoding();
	}
	
}