package system.xml;
import system.Exception;

class XmlException extends Exception
{

	public function new(msg:String = "XML Parse error") 
	{
		super(msg);
	}
	
}