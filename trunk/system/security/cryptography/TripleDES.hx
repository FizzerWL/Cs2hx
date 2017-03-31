package system.security.cryptography;
import haxe.io.Bytes;
import system.NotImplementedException;

class TripleDES
{

	public function new() 
	{
		
	}
	
	public static function Create():TripleDES
	{
		return throw new NotImplementedException();
	}
	
	public var IV:Bytes;
	public var Key:Bytes;
	
	public function CreateEncryptor():ICryptoTransform
	{
		return throw new NotImplementedException();
	}
	public function CreateDecryptor():ICryptoTransform
	{
		return throw new NotImplementedException();
	}
	
}