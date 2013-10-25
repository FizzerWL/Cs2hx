package system;

class DBNull
{

	public function new() 
	{
		
	}
	
	private static var _inst:DBNull;
	public static var Value (get, never):DBNull;
	public static function get_Value():DBNull
	{
		if (_inst == null)
			_inst = new DBNull();
			
		return _inst;
	}
	
}