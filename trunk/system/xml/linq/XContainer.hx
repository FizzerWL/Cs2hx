package system.xml.linq;
import system.linq.Linq;

class XContainer extends XObject
{
	public var _x:Xml; //ideally protected, but haxe doesn't have that.
	
	public function new(name:String = null)
	{
		//If we're created without supplying a name, _x will be null. It's assumed that in this case, we're being constructed by another type in system.xml.linq, and _x will be assigned immediately after we're constructed.  This isn't perfect since in C# you can construct a type and assign its name later, but it'll do.
		if (name != null)
			_x = Xml.createElement(name); 
	}

	public function SetAttributeValue(attrName:String, attrValue:String):Void
	{
		_x.set(attrName, attrValue);
	}
	
	public function Add(element:XElement):Void
	{
		_x.addChild(element._x);
	}
	
	public function Element(name:String):XElement
	{
		for (x in _x.elementsNamed(name))
		{
			var r = new XElement();
			r._x = x;
			return r;
		}
		
		return null;
	}
	
	public function Elements():Array<XElement>
	{
		var r = new Array<XElement>();
		for (x in _x.elements())
		{
			var e = new XElement();
			e._x = x;
			r.push(e);
		}
		
		return r;
	}
	
	public function Attribute(name:String):XAttribute
	{
		var a = _x.get(name);
		if (a == null)
			return null;
		return new XAttribute(a);
	}
	
	public function Attributes():Array<XAttribute>
	{
		var ret = new Array<XAttribute>();
		
		for (a in _x.attributes())
			ret.push(new XAttribute(a));
		return ret;
	}
	
	public function toString():String
	{
		return _x.toString();
	}
	
	public var Value(get, set):String;
	
	public function get_Value():String
	{
		return _x.nodeValue;
	}
	public function set_Value(s:String):String
	{
		_x.nodeValue = s;
		return s;
	}

}