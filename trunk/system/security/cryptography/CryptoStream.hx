package system.security.cryptography;
import haxe.io.Bytes;
import system.io.Stream;
import system.NotImplementedException;

class CryptoStream extends Stream
{

	public function new(stream:Stream, encryptor:ICryptoTransform, streamMode:Int) 
	{
		super(null);
		throw new NotImplementedException();
	}
	
}