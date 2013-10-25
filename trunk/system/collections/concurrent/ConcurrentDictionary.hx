package system.collections.concurrent;
import system.CsRef.CsRef;
import system.NotImplementedException;

class ConcurrentDictionary<K, V>
{

	public function new() 
	{
		
	}

	public function Clear():Void
	{
		throw new NotImplementedException();
	}
	
	public function TryGetValue(key:K, out:CsRef<V>):Bool
	{
		return throw new NotImplementedException();
	}
	
	public function GetOrAdd_TKey_TValue(key:K, val:V):V
	{
		return throw new NotImplementedException();
	}
	public function GetOrAdd(key:K, valueFactory:K->V):V
	{
		return throw new NotImplementedException();
	}
	
	public function AddOrUpdate_TKey_TValue_Func(key:K, val:V, updateValueFactory:K->V->V):V
	{
		return throw new NotImplementedException();
	}
	
	public function TryRemove(key:K, out:CsRef<V>):Bool
	{
		return throw new NotImplementedException();
	}
	
	public var Keys(get, never):Array<K>;
	public function get_Keys():Array<K>
	{
		return throw new NotImplementedException();
	}
	
}