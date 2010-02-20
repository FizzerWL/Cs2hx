package system.linq;

import system.collections.generic.CSDictionary;
import system.Exception;
import system.DateTime;
import system.Cs2Hx;

class Linq
{
	public static function Take<T>(a:Array<T>, numToTake:Int):Array<T>
	{
		var ret = new Array<T>();
		for (e in a)
		{
			if (numToTake-- <= 0)
				break;
			ret.push(e);
		}
		return ret;
	}
	
	public static function Skip<T>(a:Array<T>, numToSkip:Int):Array<T>
	{
		var ret = new Array<T>();
		for (e in a)
		{
			if (numToSkip > 0)
				numToSkip--;
			else
				ret.push(e);
		}
		return ret;
	}
	
	public static function CompareTo(a:Int, b:Int):Int
	{
		return a - b; 
	}
	public static function ToDictionary < T, K, V > (a:Array < T > , getKey:T -> K, getValue:T -> V):CSDictionary<K,V>
	{
		var ret:CSDictionary<K,V> = new CSDictionary();
		
		for (i in a)
			ret.Add(getKey(i), getValue(i));
		
		return ret;
	}
	
	
	public static function Concat<T>(a:Array<T>, b:Array<T>):Array<T>
	{
		var ret = new Array<T>();
		for (val1 in a)
			ret.push(val1);
		for (val2 in b)
			ret.push(val2);
		return ret;
	}
		
	public static function Except<T>(a:Array<T>, except:Array<T>):Array<T>
	{
		//Convert except to keys
		var keys:Hash<Int> = new Hash<Int>();
		for (e in except)
			keys.set(Cs2Hx.Hash(e), 1);
			
		var ret = new Array<T>();
			
		for (val in a)
			if (!keys.exists(Cs2Hx.Hash(val)))
				ret.push(val);
		return ret;
	}
	

	public static function First<T>(a:Array<T>):T
	{
		var iterator:Iterator<T> = a.iterator();
		if (!iterator.hasNext())
			throw new Exception("First() called on empty item");
		return iterator.next();
	}
	
	
	public static function FirstWhere<T>(a:Array<T>, match:T -> Bool):T
	{
		var iterator:Iterator<T> = a.iterator();
		while (iterator.hasNext())
		{
			var i:T = iterator.next();
			if (match(i))
				return i;
		}
		throw new Exception("No matching items");
	}
	
	public static function ElementAt<T>(a:Array<T>, index:Int):T
	{
		var iterator:Iterator<T> = a.iterator();
		
		while (iterator.hasNext())
		{
			var i:T = iterator.next();
			if (index-- <= 0)
				return i;
		}
		
		throw new Exception("ElementAt out of range");
	}
	
	public static function Last<T>(a:Array<T>):T
	{
		var iterator:Iterator<T> = a.iterator();

		var i:T = null;
		while (iterator.hasNext())
			i = iterator.next();
			
		if (i == null)
			throw new Exception("No matching items");
		return i;
	}
	public static function Count<T>(a:Array<T>):Int
	{
		var iterator:Iterator<T> = a.iterator();

		var i:Int = 0;
		while (iterator.hasNext())
		{
			iterator.next();
			i++;
		}
		return i;
	}
	public static function Where<T>(a:Array<T>, match:T -> Bool):Array<T>
	{
		var iterator:Iterator<T> = a.iterator();
		var ret = new Array<T>();

		while (iterator.hasNext())
		{
			var e:T = iterator.next();
			
			if (match(e)) 
				ret.push(e);
		}
		return ret;
	}
	public static function CountWhere<T>(a:Array<T>, match:T -> Bool):Int
	{
		var iterator:Iterator<T> = a.iterator();

		var i:Int = 0;
		while (iterator.hasNext())
		{
			if (match(iterator.next()))
				i++;
		}
		return i;
	}
	
	
	public static function Intersect<T>(a:Array<T>, b:Array<T>):Array<T>
	{
		var dict:Hash<Int> = new Hash<Int>();
		for (i in a)
			dict.set(Cs2Hx.Hash(i), 1);
			
		var ret = new Array<T>();
		
		for (i in b)
			if (dict.exists(Cs2Hx.Hash(i)))
				ret.push(i);
				
		return ret;
	}
	
	public static function All(a:Array<Bool>):Bool
	{
		for (v in a)
			if (!v)
				return false;
		return true;
	}
	
	public static function AllWhere<T>(a:Array<T>, evalItem:T -> Bool):Bool
	{
		for (v in a)
			if (!evalItem(v))
				return false;
		return true;
	}
	
	public static function OfType<FROM, TO>(a:Array<FROM>, type:Class<TO>):Array<TO>
	{
		var ret = new Array<TO>();
		for (i in a)
		{
			if (Std.is(i, type))
			{
				ret.push(cast i);
			}
		}
		return ret;
	}
	
	public static function SelectMany<FROM, TO>(a:Array<FROM>, select:FROM -> Array<TO>):Array<TO>
	{
		var ret = new Array<TO>();
		for (outer in a)
			for (inner in select(outer))
				ret.push(inner);
		return ret;
	}
	
	public static function FirstOrDefault<T>(a:Array<T>, where:T -> Bool):T
	{
		for (i in a)
			if (where(i))
				return i;

		return null;
	}
	public static function Single<T>(a:Array<T>):T
	{
		var item:T = null;
		for (i in a)
		{
			if (item != null)
				throw new Exception("Multiple items");
			item = i;
		}
		if (item == null)
			throw new Exception("No items");
		return item;
	}
	public static function SingleOrDefault<T>(a:Array<T>, where:T -> Bool):T
	{
		var item:T = null;
		for (val in a)
		{
			if (!where(val))
				continue;
			
			if (item != null)
				throw new Exception("Multiple items");
			item = val;
		}
		return item;
	}
	
	public static function SingleWhere<T>(a:Array<T>, where:T -> Bool):T
	{
		var item:T = null;
		for (val in a)
		{
			if (!where(val))
				continue;
			
			if (item != null)
				throw new Exception("Multiple items");
			item = val;
		}
		if (item == null)
			throw new Exception("No items");
		return item;
	}
	public static function Select<FROM, TO>(a:Array<FROM>, func:FROM -> TO):Array<TO>
	{
		var ret = new Array<TO>();
		
		for (i in a)
			ret.push(func(i));

		return ret;
	}
	
	public static function ToHaxeList<T>(a:Array<T>):List<T>
	{
		var ret:List<T> = new List<T>();
		for (i in a)
			ret.add(i);
		return ret;
	}

	public static function ToArray<T>(a:Array<T>):Array<T>
	{
		var ret:Array<T> = new Array<T>();
		
		for (k in a)
			ret.push(k);
		return ret;
	}
	public static function ToList<T>(a:Array<T>):Array<T>
	{
		return ToArray(a);
	}
	
	public static function Any<T>(a:Array<T>):Bool
	{
		for (i in a)
			return true;
		return false;
	}
	public static function AnyWhere<T>(a:Array<T>, func:T -> Bool):Bool
	{
		for (i in a)
			if (func(i))
				return true;
		return false;
	}
	public static function Distinct<T>(a:Array<T>):Array<T>
	{
		var ret = new Array<T>();
		var hash:Hash<Int> = new Hash<Int>();
		
		for (i in a)
		{
			var h:String = Cs2Hx.Hash(i);
			if (!hash.exists(h))
			{
				hash.set(h, 1);
				ret.push(i);
			}
		}
		return ret;
	}
	public static function GroupBy<T, K>(a:Array<T>, func:T -> K):Array<Grouping<K, T>>
	{
		var dict = new Hash<Grouping<K,T>>();
		var ret = new Array < Grouping < K, T >> ();
		
		for (i in a)
		{
			var key:K = func(i);
			var s = Cs2Hx.Hash(key);
			
			if (dict.exists(s))
				dict.get(s).vals.push(i);
			else
			{
				var g = new Grouping(key, i);
				dict.set(s, g);
				ret.push(g);
			}
		}
		
		return ret;
	}
	
	public static function Max(a:Array<Float>):Float
	{
		var ret:Float = First(a);
		for (i in a)
			if (i > ret)
				ret = i;
				
		return ret;
	}
	public static function MaxInt(a:Array<Int>):Int
	{
		var ret:Int = First(a);
		for (i in a)
			if (i > ret)
				ret = i;
				
		return ret;
	}
	
	public static function MaxDate(a:Array<DateTime>):DateTime
	{
		var ret:DateTime = First(a);
		for (i in a)
			if (i.date.getTime() > ret.date.getTime())
				ret = i;
		return ret;
	}
	public static function Min(a:Array<Float>):Float
	{
		var ret:Float = First(a);
		for (i in a)
			if (i < ret)
				ret = i;
				
		return ret;
	}
	public static function MinInt(a:Array<Int>):Int
	{
		var ret:Int = First(a);
		for (i in a)
			if (i < ret)
				ret = i;
				
		return ret;
	}
		
	public static function OrderBy<T>(a:Array<T>, selector:T -> Float):Array<T>
	{
		var list:Array<T> = ToArray(a);
		list.sort(function(f:T, s:T):Int { return Std.int(selector(f) - selector(s)); } );
		return list;
	}
	public static function OrderByDescending<T>(a:Array<T>, selector:T -> Float):Array<T>
	{
		var ret:Array<T> = OrderBy(a, selector);
		ret.reverse();
		return ret;
	}
	public static function Sum(a:Array<Float>):Float
	{
		var ret:Float = 0;
		for (i in a)
			ret += i;
		return ret;
	}
}