package system.collections.generic;

class KeyNotFoundException extends Exception {
	public function new(s:String = null) {
		super(s == null ? "KeyNotFoundException" : s);
	}
}
