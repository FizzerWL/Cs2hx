package system.runtime.caching;
import system.collections.objectmodel.Collection.Collection;
import system.DateTime;
import system.TimeSpan;

class CacheItemPolicy
{

	public function new() 
	{
		
	}
	
	public var AbsoluteExpiration:DateTime;
	public var SlidingExpiration:TimeSpan;
	public var ChangeMonitors:Collection<ChangeMonitor>;
}