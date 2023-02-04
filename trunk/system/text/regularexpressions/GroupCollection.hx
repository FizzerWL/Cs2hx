package system.text.regularexpressions;
import system.NotImplementedException;

class GroupCollection
{
	
	private var _getStr:String->Group;
	private var _getIndex:Int->Group;

	public function new(getStr:String->Group, getIndex:Int->Group) 
	{
		_getStr = getStr;
		_getIndex = getIndex;
	}
	
	public function GetValue_String(name:String):Group
	{
		return _getStr(name);
	}
	
	public function GetValue_Int32(index:Int):Group
	{
		return _getIndex(index);
	}
}