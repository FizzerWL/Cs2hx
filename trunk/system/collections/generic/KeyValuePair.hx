package system.collections.generic;

class KeyValuePair<K,V>
{
	public var Key:K;
	public var Value:V;
	
	public function new(k:K = null, v:V = null)
	{
		Key = k;
		Value = v;
	}
	
	public function toString():String
	{
		return "[" + Key + "," + Value + "]";
	}
}
