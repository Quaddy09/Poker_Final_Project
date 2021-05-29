class Stack {
	constructor( params ) {
		this.stack = params.stack || [];
		this.pos = params.pos || {x: 0, y: 0};
	}
}

exports.Stack = Stack;