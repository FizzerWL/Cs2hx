package system.data;

interface IDbCommand
{
	public var CommandText:String;
	public function Dispose():Void;
	public function ExecuteReader():IDataReader;
	public function ExecuteNonQuery():Int;
	
}