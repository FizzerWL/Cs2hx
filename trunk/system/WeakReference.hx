package system;

/**
 * Caution!  Haxe doesn't support weak references, so this is implemented as a normal reference. You must take care not to leak memory by using this as a weak reference.
 */
class WeakReference
{
	public var Target:Dynamic;
	public var IsAlive = true;

	public function new(obj:Dynamic)
	{
		this.Target = obj;
	}
	
}