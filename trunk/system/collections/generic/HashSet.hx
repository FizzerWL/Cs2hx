package system.collections.generic;
import system.Cs2Hx;

class HashSet<T> implements IEnumerable<T>
{
	private var store:Map<String, Int>;
	private var keys:Array<T>;
	
	public function new()
	{
		store = new Map<String, Int>();
		keys = new Array<T>();
	}
	public function Clear():Void
	{
		store = new Map<String, Int>();
		keys = new Array<T>();
	}
	
	public function Add(key:T):Bool
	{
		var s = Cs2Hx.Hash(key);
		
		if (store.exists(s))
			return false;
		
		keys.push(key);
		store.set(s, 1);
		return true;
	}
	
	public function Remove(key:T):Bool
	{
		var s = Cs2Hx.Hash(key);
		
		if (!store.exists(s))
			return false;
			
		store.remove(s);
		keys.remove(key);
		
		return true;
	}
	
	public function Contains(key:T):Bool
	{
		return store.exists(Cs2Hx.Hash(key));
	}
	
	public function GetEnumerator():Array<T>
	{
		return keys;
	}
	
	public function iterator():Iterator<T>
	{
		return keys.iterator();
	}
	
	public var Count(get, never):Int;
	public inline function get_Count():Int
	{
		return keys.length;
	}

	public function ToArray():Array<T>
	{
		return keys;
	}
	
	public function RemoveWhere(pred:T->Bool)
	{
		var i = 0;
		while (i < keys.length)
		{
			var k = keys[i];
			if (pred(k))
			{
				keys.splice(i, 1);
				store.remove(Cs2Hx.Hash(k));
			}
			else
				i++;
		}
	}
}