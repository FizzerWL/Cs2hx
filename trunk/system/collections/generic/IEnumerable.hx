package system.collections.generic;

interface IEnumerable<T>
{
	public function GetEnumerator():Array<T>;
}