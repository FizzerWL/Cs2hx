package system.xml.linq;
import system.Exception;
import system.linq.Enumerable;
import system.NotImplementedException;

class XContainer extends XNode
{
	public var _x:Xml; //ideally protected, but haxe doesn't have that.
	
	public function new(name:String = null)
	{
		super();
		//If we're created without supplying a name, _x will be null. It's assumed that in this case, we're being constructed by another type in system.xml.linq, and _x will be assigned immediately after we're constructed.  This isn't perfect since in C# you can construct a type and assign its name later, but it'll do.
		if (name != null)
			_x = Xml.createElement(name); 
	}

	public function SetAttributeValue(attrName:String, attrValue:String):Void
	{
		if (attrValue == null)
			throw new Exception("Attribute value cannot be null");
		_x.set(attrName, attrValue);
	}
	
	public function Add(obj:Dynamic):Void
	{
		if (Std.isOfType(obj, XElement))
			_x.addChild(cast(obj, XElement)._x);
		else if (Std.isOfType(obj, XAttribute))
		{
			var attr = cast(obj, XAttribute);
			SetAttributeValue(attr.Name.LocalName, attr.Value);
		}
		else
			throw new Exception("Unexpected type");
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
	
	public function Elements_XName(name:String):Array<XElement>
	{
		var r = new Array<XElement>();
		for (x in _x.elements())
		{
			if (x.nodeName == name)
			{
				var e = new XElement();
				e._x = x;
				r.push(e);
			}
		}
		
		return r;
		
	}
	
	public function Attribute(name:String):XAttribute
	{
		var a = _x.get(name);
		if (a == null)
			return null;
		return new XAttribute(name, a);
	}
	
	public function Attributes():Array<XAttribute>
	{
		var ret = new Array<XAttribute>();
		
		for (a in _x.attributes())
			ret.push(new XAttribute(a, _x.get(a)));
		return ret;
	}
	
	public function toString(options:Int = 0):String
	{
		return _x.toString();
	}
	
	private var _valueChild:Xml;
	private function CheckValueChild():Void
	{
		if (_valueChild != null)
			return;
		for (child in _x.iterator())
			if (child.nodeType == Xml.PCData)
			{
				_valueChild = child;
				return;
			}
	}
	public var Value(get, set):String;
	public function get_Value():String
	{
		CheckValueChild();
		if (_valueChild == null)
			return "";
		else
			return _valueChild.nodeValue;
	}
	public function set_Value(s:String):String
	{
		CheckValueChild();
		if (_valueChild == null)
			_x.addChild(_valueChild = Xml.createPCData(s));
		else
			_valueChild.nodeValue = s;
		return s;
	}
	
	public var Name(get, never):XName;
	public function get_Name():XName {
		return new XName(_x.nodeName);
	}
	
	public function Nodes():Array<XNode> {
		//Nodes is supposed to return XElement, XText, XComment, XProcessingInstruction, XDocumentType.  It does not return attributes.  Currently we only return elements since we never bothered implementing XText, and we don't need the others

		var ret = new Array<XNode>();

		for(e in Elements())
			ret.push(e);

		//if (this.Value != null)
		//	ret.push(new XText(this.Value));

		return ret;
	}

	override function Remove() {
		super.Remove();
		this._x.parent.removeChild(this._x);
	}


}