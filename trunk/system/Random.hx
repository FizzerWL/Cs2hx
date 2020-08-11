package system;

class Random
{
	public function new()
	{
	}

	public function Next():Int
	{
		return Std.random(2147483647);
	}
	public function Next_Int32(max:Int):Int
	{
		return Std.random(max);
	}
	
	public function NextDouble():Float
	{
		return Math.random();
	}

    public function Next_Int32_Int32(minValue:Int, maxValue:Int):Int
	{
		return Std.random(maxValue - minValue) + minValue;
	}

}
