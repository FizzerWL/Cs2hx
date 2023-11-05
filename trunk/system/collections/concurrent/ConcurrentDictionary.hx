package system.collections.concurrent;
import system.collections.generic.Dictionary;
import system.CsRef.CsRef;

class ConcurrentDictionary<K, V>
{
	//the haxe targets we support are single threaded, so wrapping normal dictionary is fine
	var _dict:Dictionary<K,V>;

	public function new() 
	{
		_dict = new Dictionary<K,V>();
	}

	public function Clear():Void
	{
		_dict.Clear();
	}
	
	public function TryGetValue(key:K, out:CsRef<V>):Bool
	{
		if (!_dict.ContainsKey(key))
			return false;
		out.Value = _dict.GetValue_TKey(key);
		return true;
	}
	
	public function GetOrAdd_TKey_TValue(key:K, val:V):V
	{
		if (_dict.ContainsKey(key))
			return _dict.GetValue_TKey(key);
		_dict.Add(key, val);
		return val;
	}
	public function GetOrAdd_TKey_Func(key:K, valueFactory:K->V):V
	{
		if (_dict.ContainsKey(key))
			return _dict.GetValue_TKey(key);
		var val = valueFactory(key);
		_dict.Add(key, val);
		return val;
	}
	
	public function AddOrUpdate_TKey_TValue_Func(key:K, val:V, updateValueFactory:K->V->V):V
	{
		if (_dict.ContainsKey(key))
			return updateValueFactory(key, _dict.GetValue_TKey(key));
		_dict.Add(key, val);
		return val;
	}
	
	public function TryRemove(key:K, out:CsRef<V>):Bool
	{
		if (!_dict.ContainsKey(key))
			return false;
			
		out.Value = _dict.GetValue_TKey(key);
		_dict.Remove(key);
		return true;
	}
	
	public var Keys(get, never):Array<K>;
	public function get_Keys():Array<K>
	{
		return _dict.Keys;
	}
	
}