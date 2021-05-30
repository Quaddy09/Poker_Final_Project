class Table {

	constructor( params ) {
		this.name = params.name || 'My Table';
		this.password = params.password || 'password';
		this.seats = params.seats || 10;
		this.players = params.players || new Array(this.seats);
		this.elements = params.elements || {};
	}
	sitDown( player, seat ) {
		if( seat != undefined && seat < seats) {
			this.players[seat] = player;
		} else {
			this.players[this.players.findIndex(p => p==undefined)] = player;
		}
	}

}

exports.Table = Table;