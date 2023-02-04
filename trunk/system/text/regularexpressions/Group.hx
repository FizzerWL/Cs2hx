package system.text.regularexpressions;

class Group
{

	public function new(v:String) 
	{
		this.Value = v;
	}
	
	public var Value:String;
	public var Success = true;
	
	public function toString():String
	{
		return Value;
	}
	
}