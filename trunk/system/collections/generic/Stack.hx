package system.collections.generic;

class Stack<T> implements IEnumerable<T>
{
	private var _store:Array<T>;

	public function new(size:Int = -1)
	{
		_store = new Array<T>();
	}
	
	public function Push(item:T):Void
	{
		_store.push(item);
	}
	
	public function Pop():T
	{
		return _store.pop();
	}
	
	public function Peek():T
	{
		return _store[_store.length - 1];
	}
	
	public function Clear():Void
	{
		_store = new Array<T>();
	}
	
	public var Count(get, never):Int;
	public inline function get_Count():Int
	{
		return _store.length;
	}
	
	
	public function iterator():Iterator<T>
	{
		return GetEnumerator().iterator();
	}
	
	public function GetEnumerator():Array<T>
	{
		//We must reverse it, since C# enumerates in the opposite direction of haxe
		var a = _store.copy();
		a.reverse();
		return a;
	}
	
	
	
}