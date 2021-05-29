class Deck {
	constructor( params ) {
		this.deck = params.deck || [];
		this.pos = params.pos || {x: 0, y: 0};
		this.rotation = params.rotation || 0;
	}
}

exports.Deck = Deck;