package system.text.regularexpressions;

class Match
{

	public function new(e:EReg) 
	{
		this.Groups = new GroupCollection(e);
	}
	
	public var Groups:GroupCollection;
}