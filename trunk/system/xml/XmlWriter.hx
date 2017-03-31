package system.xml;
import system.NotImplementedException;
import system.text.StringBuilder;

class XmlWriter
{

	public function new() 
	{
		
	}
	
	public function Flush():Void
	{
		throw new NotImplementedException();
	}
	
	public function WriteAttributeString(localName:String, value:String):Void
	{
		throw new NotImplementedException();
	}
	
	public function WriteStartElement(name:String):Void
	{
		throw new NotImplementedException();
	}
	
	public function WriteEndElement():Void
	{
		throw new NotImplementedException();
	}
	
	public function WriteString(text:String):Void
	{
		throw new NotImplementedException();
	}
	
	public static function Create_StringBuilder_XmlWriterSettings(sb:StringBuilder, settings:XmlWriterSettings):XmlWriter
	{
		return throw new NotImplementedException();
	}
}