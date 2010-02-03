package system;

class RandomAS
{
	public function new()
	{
	}
	
	public function Next(max:Int):Int
	{
		return Std.random(max);
	}
	
	public function NextDouble():Float
	{
		return Math.random();
	}

}
