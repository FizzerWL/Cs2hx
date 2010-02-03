package system;


class Enumerable
{
	public static function Range(start:Int, count:Int):Array<Int>
	{
		var ret = new Array<Int>();
		for (i in start...count)
			ret[i - start] = i;
		return ret;
	}
}
