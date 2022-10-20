package system;

/**
 * Caution!  Haxe doesn't support weak references, so this is implemented as a normal reference. You must take care not to leak memory by using this as a weak reference.
 */
class WeakReference<T>
{
	public var Target:T;
	public var IsAlive = true;

	public function new(obj:T)
	{
		this.Target = obj;
	}

	public function TryGetTarget(result:CsRef<T>):Bool
	{
		result.Value = Target;
		return true;
	}
}