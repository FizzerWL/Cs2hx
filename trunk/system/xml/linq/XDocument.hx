package system.xml.linq;
import system.io.File;
import system.NotImplementedException;
import system.xml.XmlException;

class XDocument extends XContainer
{
	public var Root:XElement;
	
	public function new(root:XElement = null)
	{
		super(null);
		
		this.Root = root;
	}
	
	public static function Parse(str:String):XDocument
	{
		if (str == null)
			throw new Exception("XDocument.Parse str is null");

		var d = new XDocument();
		try
		{
			d._x = Xml.parse(str);
		}
		catch (d:Dynamic)
		{
			throw new XmlException(Std.string(d));
		}
		d.Root = new XElement();
		d.Root._x = d._x.firstElement();
		return d;
	}

	public static function Parse_String_LoadOptions(str:String, opts:Int):XDocument {
		return Parse(str); //TODO: Obey options
	}
	
	public static function Load(path:String):XDocument
	{
		return Parse(File.ReadAllText(path));
	}

	public override function Add(x:Dynamic):Void
	{
		if (Std.isOfType(x, XElement))
		{
			Root = x;
			_x = x._x;
		}
	}
	
	public function DescendantNodes():Array<XNode>
	{
		return throw new NotImplementedException();
	}
	
	override public function toString(options:Int = 0):String 
	{
		var ret = this.Root.toString();
		
		//TEMPORARY:  Verify it's valid xml, and throw if not.  Second param is "strict"
		//haxe.xml.Parser.parse(ret, true);
		
		return ret;
		
	}
}