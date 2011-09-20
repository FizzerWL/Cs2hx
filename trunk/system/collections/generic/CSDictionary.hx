package system.collections.generic;

import system.collections.generic.KeyValuePair;
import system.Cs2Hx;
import system.Exception;

class CSDictionary<K, V>
{
	private var store:Hash<V>;
	private var keys:Array<K>;
	
	public function new(unused:Dynamic = null)
	{
		store = new Hash<V>();
		keys = new Array<K>();
	}
	
	public function Add(key:K, value:V):Void
	{
		if (ContainsKey(key))
			throw new Exception("Key already exists: " + key);
		store.set(Cs2Hx.Hash(key), value);
		keys.push(key);
	}
	
	public function GetValue(key:K):V
	{
		var h = Cs2Hx.Hash(key);
		
		if (!store.exists(h))
			throw new Exception("Key does not exist: " + h);
		return store.get(h);
	}
	
	public function ContainsKey(key:K):Bool
	{
		return store.exists(Cs2Hx.Hash(key));
	}
	
	public function Remove(key:K):Bool
	{
		if (!ContainsKey(key))
			return false;
			
		store.remove(Cs2Hx.Hash(key));
		keys.remove(key);
		return true;
	}
	
	public function SetValue(key:K, val:V):Void
	{
		var s:String = Cs2Hx.Hash(key);
		
		if (!store.exists(s))
			keys.push(key);
		store.set(s, val);
	}
	
	public var Keys(get_Keys, never):Array<K>;
	public function get_Keys():Array<K>
	{
		return keys;
	}
	
	public function TryGetValue(key:K):V
	{
		var s:String = Cs2Hx.Hash(key);
		if (store.exists(s))
			return store.get(s); 
		else
			return null;
	}
	
	public var Values(getValues, never):Array<V>;
	public function getValues():Array<V>
	{
		var ret = new Array<V>();
		var i:Int = 0;
		for (e in store)
			ret[i++] = e;
		return ret;
	}
	
	public var length(get_length, never):Int;
	public function get_length():Int
	{
		return keys.length;
	}
	
	public function KeyValues():Array<KeyValuePair<K, V>>
	{
		var ret = new Array < KeyValuePair < K, V >> ();
		for (k in keys)
			ret.push(new KeyValuePair(k, store.get(Cs2Hx.Hash(k))));
		return ret;
	}
	
	public function Clear():Void
	{
		keys = new Array<K>();
		store = new Hash<V>();
	}
	
	public function Clone():CSDictionary<K,V>
	{
		var ret = new CSDictionary<K,V>();
		for (k in keys)
			ret.Add(k, store.get(Cs2Hx.Hash(k)));
		return ret;
	}
	
	public function iterator():Iterator<KeyValuePair<K,V>>
	{
		var ret = new List < KeyValuePair < K, V >> ();
		for (i in keys)
			ret.add(new KeyValuePair(i, store.get(Cs2Hx.Hash(i))));
		return ret.iterator();
	}
}
