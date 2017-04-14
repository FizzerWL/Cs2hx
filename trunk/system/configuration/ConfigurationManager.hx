package system.configuration;
import system.collections.specialized.NameValueCollection;
import system.NotImplementedException;

class ConfigurationManager
{

	public function new() 
	{
		
	}
	
	public static var AppSettings (get, never):NameValueCollection;
	public static function get_AppSettings():NameValueCollection
	{
		return throw new NotImplementedException();
	}
	
}