package system;

#if flash
import flash.errors.Error;
class Exception extends Error
#else
class Exception
#end
{
	#if !flash
	public var Message:String;
	#end
	
	public var StackTrace:String; //not populated.  This just exists to prevent build errors on code that accesses it.
	
	public function new(msg:String = "No message")
	{
		#if flash
		super(msg);
		#else
		this.Message = msg;
		#end
	}

	public function toString():String
	{
		#if flash
		return "Exception: " + name + ": " + message;
		#else
		return "Exception: " + Message;
		#end
	}
	
	public var InnerException:Exception;

}
