package system.xml.linq;
import system.NotImplementedException;

class XName
{

	public var LocalName:String;
	
	public function new(str:String) 
	{
		this.LocalName = str;
	}

	public function toString():String
	{
		return LocalName;
	}
}