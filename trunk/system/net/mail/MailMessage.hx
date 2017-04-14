package system.net.mail;

class MailMessage
{

	public function new() 
	{
		
	}
	
	public var ReplyToList:MailAddressCollection;
	public var To:MailAddressCollection;
	public var Body:String;
	public var From:MailAddress;
	public var IsBodyHtml:Bool;
	public var Subject:String;
}