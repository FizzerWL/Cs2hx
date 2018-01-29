package system.linq;
import system.collections.generic.IEnumerable;

//Haxe doesn't support properties in interfaces properly, so we just cheat and make IGrouping a class.
class IGrouping<TKey, TElement> implements IEnumerable<TElement>
{
	public function new(key:TKey, initialItem:TElement)
	{
		this.Key = key;
		this.vals = new Array<TElement>();
		vals.push(initialItem);
	}
	
	public var Key:TKey;
	public var vals:Array<TElement>;
	
	public function iterator():Iterator<TElement>
	{
		return vals.iterator();
	}
	
	public inline function GetEnumerator():Array<TElement> 
	{
		return vals;
	}
}
