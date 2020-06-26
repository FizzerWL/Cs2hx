package system.linq;

import haxe.ds.ObjectMap;
import system.collections.generic.Dictionary;
import system.Exception;
import system.DateTime;
import system.Cs2Hx;
import system.NotImplementedException;

class Enumerable
{
	public static function Reverse<T>(a:Array<T>):Array<T>
	{
		var ret = a.copy();
		ret.reverse();
		return ret;
	}
	
	public static function Range(start:Int, count:Int):Array<Int>
	{
		var ret = new Array<Int>();
		for (i in 0...count)
			ret[i] = i + start;
		return ret;
	}

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
	public static function ToDictionary < T, K, V > (a:Array < T > , getKey:T -> K, getValue:T -> V):Dictionary<K,V>
	{
		var ret:Dictionary<K,V> = new Dictionary();
		
		for (i in a)
			ret.Add(getKey(i), getValue(i));
		
		return ret;
	}
	
	public static function ToDictionary_IEnumerable_Func_Func<T, K, V>(a:Array<T>, getKey:T -> K, getValue:T -> V):Dictionary<K,V>
	{
		var ret:Dictionary<K,V> = new Dictionary();
		
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
	
	public static function Union<T>(a:Array<T>, union:Array<T>):Array<T>
	{
		var keys = new Map<String, Int>();
		var ret = new Array<T>();

		for (val in a)
		{
			ret.push(val);
			keys.set(Cs2Hx.Hash(val), 1);
		}
		
		for (val in union)
			if (!keys.exists(Cs2Hx.Hash(val)))
				ret.push(val);
				
		return ret;
	}
		
	public static function Except<T>(a:Array<T>, except:Array<T>):Array<T>
	{
		//Convert except to keys
		var keys = new Map<String, Int>();
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
		for (e in a)
			return e;
			
		throw new Exception("First() called on empty collection");
	}
	
	public static function FirstOrDefault<T>(a:Array<T>):T
	{
		for (e in a)
			return e;
			
		return null;
	}
	
	
	public static function First_IEnumerable_FuncBoolean<T>(a:Array<T>, match:T -> Bool):T
	{
		for(i in a)
			if (match(i))
				return i;

		throw new Exception("No matching item");
	}
	
	public static function ElementAt<T>(a:Array<T>, index:Int):T
	{
		for(i in a)
			if (index-- <= 0)
				return i;
		
		throw new Exception("ElementAt out of range");
	}
	
	public static function Last<T>(a:Array<T>):T
	{
		var i:T = null;
		for (e in a)
			i = e;
			
		if (i == null)
			throw new Exception("No matching items");
		return i;
	}
	public static function LastOrDefault<T>(a:Array<T>):T
	{
		var i:T = null;
		for (e in a)
			i = e;
		return i;
	}
	
	public static function LastOrDefault_IEnumerable_FuncBoolean<T>(a:Array<T>, func:T->Bool):T
	{
		var i:T = null;
		for (e in a)
			if (func(e))
				i = e;
			
		return i;
	}
	
	public static function Count<T>(a:Array<T>):Int
	{
		var i:Int = 0;
		for (e in a)
			i++;
		return i;
	}
	public static function Where<T>(a:Array<T>, match:T -> Bool):Array<T>
	{
		var ret = new Array<T>();

		for (e in a)
			if (match(e)) 
				ret.push(e);

		return ret;
	}
	public static function Count_IEnumerable_FuncBoolean<T>(a:Array<T>, match:T -> Bool):Int
	{
		var i:Int = 0;
		for(e in a)
			if (match(e))
				i++;
		return i;
	}
	
	
	public static function Intersect<T>(a:Array<T>, b:Array<T>):Array<T>
	{
		var dict = new Map<String, Int>();
		for (i in a)
			dict.set(Cs2Hx.Hash(i), 1);
			
		var ret = new Array<T>();
		
		for (i in b)
			if (dict.exists(Cs2Hx.Hash(i)))
				ret.push(i);
				
		return ret;
	}
	
	public static function AllTrue(a:Array<Bool>):Bool //TODO: What should this be named?
	{
		for (v in a)
			if (!v)
				return false;
		return true;
	}
	
	public static function All<T>(a:Array<T>, evalItem:T -> Bool):Bool
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
	
	public static function SelectMany_IEnumerable_FuncInt32IEnumerable<FROM, TO>(a:Array<FROM>, select:FROM -> Int -> Array<TO>):Array<TO>
	{
		var ret = new Array<TO>();
		var i:Int = 0;
		for (outer in a)
			for (inner in select(outer, i++))
				ret.push(inner);
		return ret;
	}
	
	
	
	public static function FirstOrDefault_IEnumerable_FuncBoolean<T>(a:Array<T>, where:T -> Bool):T
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
	public static function SingleOrDefault_IEnumerable_FuncBoolean<T>(a:Array<T>, where:T -> Bool):T
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
	public static function SingleOrDefault<T>(a:Array<T>):T
	{
		var item:T = null;
		for (val in a)
		{
			if (item != null)
				throw new Exception("Multiple items");
			item = val;
		}
		return item;
	}
	
	
	public static function Single_IEnumerable_FuncBoolean<T>(a:Array<T>, where:T -> Bool):T
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
	
	public static function Select_IEnumerable_FuncInt32<FROM, TO>(a:Array<FROM>, func:FROM->Int->TO):Array<TO>
	{
		var ret = new Array<TO>();
		
		var i:Int = 0;
		for (e in a)
			ret.push(func(e, i++));

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
	public static inline function ToList<T>(a:Array<T>):Array<T>
	{
		return ToArray(a);
	}
	
	public static function Any<T>(a:Array<T>):Bool
	{
		for (i in a)
			return true;
		return false;
	}
	public static function Any_IEnumerable_FuncBoolean<T>(a:Array<T>, func:T -> Bool):Bool
	{
		for (i in a)
			if (func(i))
				return true;
		return false;
	}
	public static function Distinct<T>(a:Array<T>):Array<T>
	{
		var ret = new Array<T>();
		var hash = new Map<String, Int>();
		
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
	public static function GroupBy<T, K>(a:Array<T>, func:T -> K):Array<IGrouping<K, T>>
	{
		var dict = new Map<String, IGrouping<K,T>>();
		var ret = new Array < IGrouping < K, T >> ();
		
		for (i in a)
		{
			var key:K = func(i);
			var s = Cs2Hx.Hash(key);
			
			if (dict.exists(s))
				dict.get(s).vals.push(i);
			else
			{
				var g = new IGrouping(key, i);
				dict.set(s, g);
				ret.push(g);
			}
		}
		
		return ret;
	}
	
	public static function Max_Float(a:Array<Float>):Float
	{
		var ret:Float = First(a);
		for (i in a)
			if (i > ret)
				ret = i;
				
		return ret;
	}
	public static function Max(a:Array<Int>):Int
	{
		var ret:Int = First(a);
		for (i in a)
			if (i > ret)
				ret = i;
				
		return ret;
	}
	
	public static function Max_IEnumerable(a:Array<DateTime>):DateTime
	{
		var ret:DateTime = First(a);
		for (i in a)
			if (i.date.getTime() > ret.date.getTime())
				ret = i;
		return ret;
	}
	public static function Max_IEnumerableInt32(a:Array<Int>):Int
	{
		var ret:Int = First(a);
		for (i in a)
			if (i > ret)
				ret = i;
		return ret;
	}
	public static function Max_IEnumerableDouble(a:Array<Float>):Float
	{
		var ret:Float = First(a);
		for (i in a)
			if (i > ret)
				ret = i;
		return ret;
	}
	public static function Min_IEnumerableInt32(a:Array<Int>):Int
	{
		var ret:Int = First(a);
		for (i in a)
			if (i < ret)
				ret = i;
		return ret;
	}

	public static function Min_Float(a:Array<Float>):Float
	{
		var ret = First(a);
		for (i in a)
			if (i < ret)
				ret = i;
				
		return ret;
	}
	public static function Min(a:Array<Int>):Int
	{
		var ret:Int = First(a);
		for (i in a)
			if (i < ret)
				ret = i;
				
		return ret;
	}
	public static function Min_IEnumerable(a:Array<Int>):Int //alias for Min that takes bytes
	{
		return Min(a);
	}
	public static function Min_IEnumerableDouble(a:Array<Float>):Float
	{
		return Min_Float(a);
	}
	public static function Min_IEnumerable_FuncInt32<T>(a:Array<T>, func:T->Int):Int
	{
		var min = 2147483647;
		for (e in a)
		{
			var m = func(e);
			if (m < min)
				min = m;
		}
		return min;
	}
	public static function Max_IEnumerable_FuncDouble<T>(a:Array<T>, func:T->Float):Float
	{
		var max = -999900000000000000;
		for (e in a)
		{
			var m = func(e);
			if (m > max)
				max = m;
		}
		return max;
	}
	public static function Max_IEnumerable_FuncSingle<T>(a:Array<T>, func:T->Float):Float
	{
		var max = -999900000000000000;
		for (e in a)
		{
			var m = func(e);
			if (m > max)
				max = m;
		}
		return max;
	}
	public static function Min_IEnumerable_FuncSingle<T>(a:Array<T>, func:T->Float):Float
	{
		var min = 999900000000000000;
		for (e in a)
		{
			var m = func(e);
			if (m < min)
				min = m;
		}
		return min;
	}
	
	
	public static function Max_IEnumerable_FuncInt64<T>(a:Array<T>, func:T->Float):Float
	{
		var max = -999900000000000000;
		for (e in a)
		{
			var m = func(e);
			if (m > max)
				max = m;
		}
		return max;
	}
	public static function Max_IEnumerable_FuncInt32<T>(a:Array<T>, func:T->Int):Int
	{
		var max = -2147483647;
		for (e in a)
		{
			var m = func(e);
			if (m > max)
				max = m;
		}
		return max;
	}
	public static function Min_IEnumerable_Func<T>(a:Array<T>, func:T->DateTime):DateTime
	{
		var max = DateTime.MinValue;
		for (e in a)
		{
			var m = func(e);
			if (DateTime.op_GreaterThan(m, max))
				max = m;
		}
		return max;
	}

	public static function OrderBy_Float<T>(a:Array<T>, selector:T -> Float):Array<T>
	{
		var list:Array<T> = ToArray(a);
		list.sort(function(f:T, s:T):Int { return Std.int(selector(f) - selector(s)); } );
		return list;
	}
	public static function OrderBy_Int<T>(a:Array<T>, selector:T -> Int):Array<T>
	{
		var list:Array<T> = ToArray(a);
		list.sort(function(f:T, s:T):Int { return selector(f) - selector(s); } );
		return list;
	}
	
	public static function OrderBy_String<T>(a:Array<T>, selector:T -> String):Array<T>
	{
		var list:Array<T> = ToArray(a);
		list.sort(function(a:T, b:T):Int 
		{ 
			var f = selector(a);
			var s = selector(b);
            if (f == s)
                return 0;
            else if (f < s)
                return -1;
            else
                return 1;
		} );
		return list;
	}
	
	public static function ThenBy<T>(a:Array<T>, selector:T -> Float):Array<T>
	{
		return throw new NotImplementedException();
	}
	
	
	
	public static function OrderByDescending_String<T>(a:Array<T>, selector:T -> String):Array<T>
	{
		var list:Array<T> = ToArray(a);
		list.sort(function(a:T, b:T):Int 
		{ 
			var f = selector(a);
			var s = selector(b);
            if (f == s)
                return 0;
            else if (f < s)
                return 1;
            else
                return -1;
		} );
		return list;
	}
	
	public static function OrderByDescending_Float<T>(a:Array<T>, selector:T -> Float):Array<T>
	{
		var ret:Array<T> = OrderBy_Float(a, selector);
		ret.reverse();
		return ret;
	}
	public static function OrderByDescending_Int<T>(a:Array<T>, selector:T -> Int):Array<T>
	{
		var list:Array<T> = ToArray(a);
		list.sort(function(f:T, s:T):Int { return selector(s) - selector(f); } );
		return list;
	}

	public static function Sum(a:Array<Int>):Int
	{
		var ret:Int = 0;
		for (i in a)
			ret += i;
		return ret;
	}
	public static function Sum_IEnumerable_FuncInt32<T>(a:Array<T>, func:T->Int):Int
	{
		var ret = 0;
		for (i in a)
			ret += func(i);
		return ret;
	}
	
	public static function Sum_IEnumerable_FuncInt64<T>(a:Array<T>, func:T->Float):Float
	{
		var ret:Float = 0;
		for (i in a)
			ret += func(i);
		return ret;
	}
	public static function Sum_IEnumerableDouble(a:Array<Float>):Float
	{
		var ret:Float = 0;
		for (i in a)
			ret += i;
		return ret;
	}
	
	public static function Sum_IEnumerable_FuncDouble<T>(a:Array<T>, func:T->Float):Float
	{
		var ret:Float = 0;
		for (i in a)
			ret += func(i);
		return ret;
	}
	
	public static function Average_IEnumerableDouble(a:Array<Float>):Float
	{
		var sum:Float = 0;
		var count:Int = 0;
		
		for (e in a)
		{
			sum += e;
			count++;
		}
		
		return sum / count;
	}
	public static function Average_IEnumerable_FuncDouble<T>(a:Array<T>, func:T->Float):Float
	{
		var sum:Float = 0;
		var count:Int = 0;
		
		for (e in a)
		{
			sum += func(e);
			count++;
		}
		
		return sum / count;
	}
	public static inline function Average_IEnumerable_FuncSingle<T>(a:Array<T>, func:T->Float):Float
	{
		return Average_IEnumerable_FuncDouble(a, func);
	}
	
	public static function Cast<FROM,TO>(a:Array<FROM>, type:Class<TO>):Array<TO>
	{
		var ret = new Array<TO>();
		for (e in a)
			ret.push(cast e);
		return ret;
	}
	
	public static function Aggregate<T>(a:Array<T>, func:T->T->T):T
	{
		var c = a[0];
		for(i in 1...a.length)
			c = func(c, a[i]);
		return c;
	}
}