package system.data;

class ConnectionState
{

	public function new() 
	{
		
	}
	
	public static inline var Closed:Int = 0;
	public static inline var Open:Int = 1;
	public static inline var Connecting:Int = 2;
	public static inline var Executing:Int = 4;
	public static inline var Fetching:Int = 8;
	public static inline var Broken:Int = 16;
}