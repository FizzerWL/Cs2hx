package system.text.regularexpressions;
import system.NotImplementedException;

class GroupCollection
{
	
	private var _e:EReg;

	public function new(e:EReg) 
	{
		_e = e;
	}
	
	public function GetValue_String(name:String):Group
	{
		return throw new NotImplementedException();
	}
	
	public function GetValue_Int32(index:Int):Group
	{
		return new Group(_e.matched(index));
	}
}