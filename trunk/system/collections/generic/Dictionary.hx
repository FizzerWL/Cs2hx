package system.collections.generic;

import system.collections.generic.KeyValuePair;
import system.Cs2Hx;
import system.CsRef.CsRef;
import system.Exception;

class Dictionary<K, V> implements IEnumerable<KeyValuePair<K,V>>
{
	private var store:Map<String, V>;
	private var keys:Array<K>;
	
	public function new(unused:Dynamic = null)
	{
		store = new Map<String, V>();
		keys = new Array<K>();
	}
	
	public function Add(key:K, value:V):Void
	{
		if (ContainsKey(key))
			throw new Exception("Key already exists: " + key);
		store.set(Cs2Hx.Hash(key), value);
		keys.push(key);
	}
	
	public function GetValue_TKey(key:K):V
	{
		var h = Cs2Hx.Hash(key);
		
		if (!store.exists(h))
			throw new Exception("Key does not exist: " + h);
		return store.get(h);
	}
	public inline function GetValue(key:K):V //exists for backcompat. Will never be called by CS2HX generated code.
	{
		return GetValue_TKey(key);
	}
	public inline function GetValue_Object(key:K):V
	{
		return GetValue_TKey(key);
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
	
    public function SetValue(key:K, val:V):Void //exists for backcompat. Will never be called by CS2HX generated code.
	{
		SetValue_TKey(key, val);
	}
	public function SetValue_TKey(key:K, val:V):V
	{
		var s:String = Cs2Hx.Hash(key);
		
		if (!store.exists(s))
			keys.push(key);
		store.set(s, val);
		return val;
	}
	
	public var Keys(get_Keys, never):Array<K>;
	public function get_Keys():Array<K>
	{
		return keys;
	}
	
	public function TryGetValue(key:K, out:CsRef<V>):Bool
	{
		var s:String = Cs2Hx.Hash(key);
		if (store.exists(s))
		{
			out.Value = store.get(s); 
			return true;
		}
		else
			return false;
	}
	
	public function GetValueOrNull(key:K):V
	{
		var s:String = Cs2Hx.Hash(key);
		if (store.exists(s))
			return store.get(s); 
		else
			return null;
	}

	
	public var Values(get, never):Array<V>;
	public function get_Values():Array<V>
	{
		var ret = new Array<V>();
		var i:Int = 0;
		for (k in keys)
			ret[i++] = store.get(Cs2Hx.Hash(k));
		return ret;
	}
	
	public var Count(get, never):Int;
	public inline function get_Count():Int
	{
		return keys.length;
	}
	
	public function GetEnumerator():Array<KeyValuePair<K, V>>
	{
		var ret = new Array<KeyValuePair<K, V>>();
		for (k in keys)
			ret.push(new KeyValuePair(k, store.get(Cs2Hx.Hash(k))));
		return ret;
	}
	
	public function Clear():Void
	{
		keys = new Array<K>();
		store = new Map<String, V>();
	}
	
	public function Clone():Dictionary<K,V>
	{
		var ret = new Dictionary<K,V>();
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
