package system.xml.linq;

class XDocument extends XContainer
{
	public var Root:XElement;
	
	public function new()
	{
		super(null);
	}
	
	public static function Parse(str:String):XDocument
	{
		var d = new XDocument();
		d._x = Xml.parse(str);
		d.Root = new XElement();
		d.Root._x = d._x.firstElement();
		return d;
	}

	public override function Add(x:XElement):Void
	{
		Root = x;
		_x = x._x;
	}
	
}