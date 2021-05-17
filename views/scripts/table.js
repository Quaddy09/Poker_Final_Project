
const SCREEN_WIDTH = 900;
const SCREEN_HEIGHT = SCREEN_WIDTH;

let tableState = {

}
let canvas;
let selfPlayer;
let selfPos;
let moves;
const potentialMoves = ['fold', 'check', 'call', 'bet', 'raise','deal'];

window.onload = function() {
	getState();
}

function setup() {

	canvas = createCanvas(SCREEN_WIDTH, SCREEN_HEIGHT);
	textAlign(CENTER);
	rectMode(CENTER);



}

function draw() {

	if( _.isEmpty( tableState ) ) {
		return;
	}


	getState();

	drawTable();
	updateButtons();

}

function updateButtons() {
	const divActions = document.getElementById('actions');	

	potentialMoves.forEach( cur => {
		const element = document.getElementById(cur+'button');
		if(moves && moves.includes(cur)) {
			if(element === null) {
				divActions.innerHTML += `<button id="${cur}button" onclick="doAction('${cur}')">${cur}</button>`;
			}
		} else {
			if(element) {
				element.remove();
			}
		}
	});
	if(tableState.currentPosition != selfPos) {
		if(document.action['amount']){
			document.action['amount'].remove();
		} 
	} else {
		if(!!document.action['amount']) {

		} else {
			const input = document.createElement('INPUT');
			input.setAttribute('type', 'text');
			input.setAttribute('name', 'amount');

			document.action.appendChild(input);

		}
	}
}
function displayCard( card, x, y, w, h, ang ) {


	const getSuitShape = suit => ({s:'♠️',c:'♣️',h:'♥️',d:'♦️'})[suit];
	const getSuitColor = suit => ({s:[0],c:[0],h:[255,0,0],d:[255,0,0]})[suit];

	if( card == undefined ) {
		// draw back of card
		return;
	}

	let rad = 5;	


	push();
	translate(x,y);
	rotate(ang);
	
	fill( 255 );
	
	rect( 0, 0, w, h, rad );

	fill(...getSuitColor(card._suit));
	text(card._rank.toString() + getSuitShape(card._suit), 0,0);


	pop();
	fill(0);

}
function doAction( action ) {
	if( action == 'deal' ) {
		ajax( window.location.pathname + '/dealCards' ).post({

		}).then( res => {

		});
	} else {
		ajax(window.location.pathname + '/doAction').post({
			action: action,
			amount: document.action['amount'].value,
		}).then( res => {
			res = JSON.parse(res);
			if( res.done ) {
				console.log( action );
			} else {
				console.log( res.err );
			}
		});
	}
}
function drawTable() {



	const rad = (width < height ? width : height) / 2;

	const r0 = rad * .7; // table
	const r1 = rad * .6; // boxes
	const r2 = rad * .94; // cards

	background(200);

	push();
	translate(width/2,height/2);


	fill(50,200,100);
	ellipse(0,0,2*r0);

	fill(0);
	text( tableState.name, 0, -100 );

	
	const getAng = (pos, max) => ((pos / max) * 2 * Math.PI) + Math.PI / 2;
	for(let i = 0; i < tableState.players.length; i++) {

		const ang = getAng((i-selfPos) % tableState.players.length, tableState.players.length - 1);

		drawPlayerBox(
			r1*cos(ang), 
			r1*sin(ang), 
			tableState.players[i], 
			i == tableState.currentPosition,
			( pl => {
				let id;
				try {
					id = pl.id;
				} catch(e) {
					return false;
				}
				return tableState.winners ? tableState.winners.reduce( (acc,cur) => acc + (cur.id == id), 0 )
											: false;
			})(tableState.players[i]),
		);

		// hole cards
		try {
			displayCard(
				tableState.players[i].holeCards[0],
				r2*cos(ang)-15,
				r2*sin(ang) - 10,
				55,
				77,
				Math.PI / -8,
			);
			displayCard(
				tableState.players[i].holeCards[1],
				r2*cos(ang)+15,
				r2*sin(ang),
				55,
				77,
				Math.PI / 8,
			);
		} catch(e) {

		}


	}

	
	// community carrds
	if( tableState.deck.length !== 0 ) {

		// pot amount
		text( 
			tableState.pots[0].amount + 
				(tableState.currentBet?tableState.currentBet:0),
			0, 
			-rad / 4,
		);
		for(let i = 0; i < 5; i++ ) {
			
			displayCard(
				tableState.communityCards[i], 
				(i-2)*40,
				0,
				55,
				77
			);
		}
	}

	pop();

}
function drawPlayerBox(x, y, player, isTurn, isWinner) {
	if(player===null)return;
	if(isTurn) fill(200,200,100);
	else if(isWinner) fill(255,200,200);
	else fill(100,150,200);
	
	rect(x, y, 100, 50);
	fill(0);
	text(player.id, x, y - 12);
	text(player.stackSize, x, y + 13);
}

async function getState() {

	ajax(window.location.pathname + '/getself').post({

	}).then( res => {
		try{
			if( res.startsWith('<')){
				console.log(res);
			}
		} catch(e) {

		}
		selfPlayer = Flatted.parse( res ).player;
		selfPos = Flatted.parse( res ).index;
		tableState = selfPlayer.table;
		ajax(window.location.pathname + '/getActions').post({
			pos: selfPos,
		}).then( res => {
			moves = JSON.parse(res).legalMoves;
		});
	});

	// ajax(window.location.pathname + '/getstate').post({

	// }).then( res => {
	// 	// console.log(res);
	// 	tableState = Flatted.parse( res );
		
	// });

	

}
function goHome() {
	window.location = '/home';
}