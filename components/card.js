class Card {
	constructor( params ) {
		if(!this.name || this.name.length == 3) {
			throw 'Invalid card';
		}
		this.name = params.name || 'As';
		this.suit = this.name[1];
		this.symbol = {c:'♣️', d:'♦️', h:'♥️', s:'♠️'}[this.suit];
		this.value = this.name[0];
	}
}

exports.Card = Card;