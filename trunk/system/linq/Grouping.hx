package system.linq;

class Grouping<TKey, TElement>
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
	
	public inline function Values():Array<TElement> 
	{
		return vals;
	}
}
