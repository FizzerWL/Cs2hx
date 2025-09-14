package system.xml.linq;
import system.NotImplementedException;

class XElement extends XContainer
{
	public function new(name:String = null, content1:Dynamic = null, content2:Dynamic = null, content3:Dynamic = null, content4:Dynamic = null)
	{
		super(name);
		
		if (content1 != null) this.Add(content1);
		if (content2 != null) this.Add(content2);
		if (content3 != null) this.Add(content3);
		if (content4 != null) this.Add(content4);
	}
	
	public function GetDefaultNamespace():XNamespace
	{
		return throw new NotImplementedException();
	}
	
	public var NodeType:Int;

	
}