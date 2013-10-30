package system.xml;

class XmlNodeType
{

	public function new() 
	{
		
	}

	public static inline var None = 0;
	public static inline var Element = 1;
	public static inline var Attribute = 2;
	public static inline var Text = 3;
	public static inline var CDATA = 4;
	public static inline var EntityReference = 5;
	public static inline var Entity = 6;
	public static inline var ProcessingInstruction = 7;
	public static inline var Comment = 8;
	public static inline var Document = 9;
	public static inline var DocumentType = 10;
	public static inline var DocumentFragment = 11;
	public static inline var Notation = 12;
	public static inline var Whitespace = 13;
	public static inline var SignificantWhitespace = 14;
	public static inline var EndElement = 15;
	public static inline var EndEntity = 16;
	public static inline var XmlDeclaration = 17;
}