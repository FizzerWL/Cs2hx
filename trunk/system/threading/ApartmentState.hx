package system.threading;

class ApartmentState
{

	public function new() 
	{
		
	}
	
	public static inline var STA = 0;
	public static inline var MTA = 1;
	public static inline var Unknown = 2;
}