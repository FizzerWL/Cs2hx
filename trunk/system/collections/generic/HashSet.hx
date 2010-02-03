package system.collections.generic;
import system.Cs2Hx;

class HashSet<T>
{
	private var store:Hash<Int>;
	private var keys:Array<T>;
	
	public function new()
	{
		store = new Hash<Int>();
		keys = new Array<T>();
	}
	
	public function Add(key:T):Void
	{
		var s = Cs2Hx.Hash(key);
		
		if (store.exists(s))
			return;
		
		keys.push(key);
		store.set(s, 1);
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
	
	public function Values():Array<T>
	{
		return keys;
	}
	
	public function iterator():Iterator<T>
	{
		return keys.iterator();
	}
	
	public var length(get_length, never):Int;
	public function get_length():Int
	{
		return keys.length;
	}
}