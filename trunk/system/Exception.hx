package system;

#if flash
import flash.errors.Error;
class Exception extends Error
#else
class Exception
#end
{
	public var Message:String;
	
	public var StackTrace:String; //not populated.  This just exists to prevent build errors on code that accesses it.
	
	public function new(msg:String = "No message", innerException:Exception = null)
	{
		#if flash
		super(msg);
		#end
		this.Message = msg;
		
		this.InnerException = innerException;
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
