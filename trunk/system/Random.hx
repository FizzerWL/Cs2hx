package system;

class Random
{
	public function new()
	{
	}
	
	public function Next_Int32(max:Int):Int
	{
		return Std.random(max);
	}
	
	public function NextDouble():Float
	{
		return Math.random();
	}

}
