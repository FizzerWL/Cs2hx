package system.text.regularexpressions;

class Match
{
	public var Success:Bool;
	public var Groups:GroupCollection;

	public function new(success:Bool, g:GroupCollection) 
	{
		this.Success = success;
		this.Groups = g;
	}
	
}