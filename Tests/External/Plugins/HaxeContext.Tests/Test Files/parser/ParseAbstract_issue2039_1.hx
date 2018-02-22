package;
@:forward
abstract Strings(Array<String>) from Array<String> {
	public function new() this = new CSList<String>();
}