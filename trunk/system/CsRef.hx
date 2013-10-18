package system;

//Used to wrap variables that are passed by "ref" or "out" in C#
class CsRef<T>
{
	public var Value:T;

	public function new(v:T) 
	{
		this.Value = v;
	}
	
	public function toString():String
	{
		return Std.string(this.Value);
	}
}