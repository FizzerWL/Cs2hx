package system.data;

class IsolationLevel
{
	public static inline var Unspecified:Int = -1;
	public static inline var Chaos:Int = 16;
	public static inline var ReadUncommitted:Int = 256;
	public static inline var ReadCommitted:Int = 4096;
	public static inline var RepeatableRead:Int = 65536;
	public static inline var Serializable:Int = 1048576;
	public static inline var Snapshot:Int = 16777216;
	
	public function new() 
	{
		
	}
	
}