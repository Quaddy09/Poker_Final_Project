
// import stuff
const express = require('express');
const path = require('path')
const {readFile} = require('fs').promises;
const { v4: uuidv4 } = require('uuid');

const {parse, stringify} = require('flatted');

const app = express();
const bodyParser = require('body-parser')

const urlencodedParser = bodyParser.urlencoded({ extended: false })
const multer = require('multer');
const upload = multer();
const session = require('express-session');
const cookieParser = require('cookie-parser');

const {Table} = require('./table.js');
const {Stack} = require('./stack.js');
const {Deck} = require('./deck.js');
const {Chip} = require('./chip.js');
const {Card} = require('./card.js');

// configure app
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true })); 
app.use(upload.array());
app.use(cookieParser());
app.use(session({secret: "key1"}));

app.use(express.static('/views/scripts'))

app.set('view engine', 'pug');
app.set('views',path.join( __dirname, 'views' ));


// users and tables
let users = {};
const tables = {};

// set up initial defulat table and users for testing
users[uuidv4()] = { id: 'sam', password: 'coble', authlevel :'1', tables: ['1'],  };
users[uuidv4()] = {id:'user',password:'pass', tables: ['1'],};
users[uuidv4()] = {id:'user2',password:'pass', tables:['1'],};

const testTable = new Table({
	name: 'Test Table',
	password: 'tablepass',
});
testTable.sitDown( 'sam' );
testTable.sitDown( 'user' );
testTable.sitDown( 'user2' );

tables[uuidv4()] = testTable;




const getContentType = extension => {
	switch(extension){
		case '.map':
		case '.js':
			return 'application/javascript';
		case '.css':
			return 'text/css';
		case '.jpg':
			return 'image/jpg';
		case '.png':
			return 'image/png';
		case '.mp3':
			return 'audio/mpeg';
		case '.ico':
			return 'image/vnd';
		case '':
			return  'text/html';
		default:
			throw `bad file extension: ${extension}`;
	}
}


app.use( express.static( __dirname + '/Poker_Final_Project' ));



app.get('/login', async (req, res) => {
	res.render( 'login' );
	// console.log( "sent login page" );
});

app.get('/signup', async (req, res) => {
	res.render( 'signup' );
	// console.log( "sent signup page" );
});


function checkSignIn(req, res, next){
	if(req.session.user){
		next();     //If session exists, proceed to page
	} else {
		var err = new Error("Not logged in!");
		// console.log(req.session.user);
		next(err);  //Error, trying to access unauthorized page!
	}
}


app.get('/home', checkSignIn, function(req, res){

	res.render('home', {id: req.session.user.id})

});

app.post('/home', checkSignIn, (req, res) => {

	// console.log( req.session.user );
	res.send({id: req.session.user.id, authlevel: req.session.user.authlevel });


});

app.post('/logout', checkSignIn, (req, res) => {

	req.session.destroy();
	res.send( {redirect: "/login" } );

})




app.get('/scripts/*', async (req, res) => {
	try{
		switch(req.url){
			default:
				try{
					res.header('Content-Type', getContentType( path.extname(req.url) ) );
					res.send( await readFile( path.join( __dirname, 'views', req.url ) ) );
				}
				catch(err){
					res.header('Content-Type', 'text/html');
					res.send('404 Fie Not Found');
					throw "404 file not found";
				}
		}
	}
	catch(err){
		console.log(err);
	}
});


app.post( '/signup', async (req, res) => {

	if( !req.body.id || !req.body.password ) {

		res.status( 400 );
		// console.log(req.body);
		res.send( "Invalid Credentials\n" );

	} else {

		alreadyExists = false;

		Object.values(users).filter(function(user){
			if(user.id === req.body.id){
				alreadyExists = true;
				res.send(
					{
						message: "User Already Exists! Login or choose another user id",

					});
			}
		});

		if( !alreadyExists ){

			const newUser = {
				id: req.body.id, 
				password: req.body.password, 
				tables: [],
			};
			users[uuidv4] = newUser;

			req.session.user = newUser;
			res.send( {'redirect':'/home'} );
		}
	}
});


app.post('/login', function(req, res){
	if(!req.body.id || !req.body.password){
		res.render('login', {message: "Please enter both id and password"});
	} else {
		exists = false;
		Object.values(users).filter(function(user){
			if(user.id === req.body.id && user.password === req.body.password){
				exists = true;
				req.session.user = user;
				res.send({'redirect':'/home'});
			}
		});
		if( !exists ) {
			res.send({'redirect':'./login'});
      	}
	}
});

app.use('/home', function(err, req, res, next){
	if( err ) {
		// console.log('not logged in');

	}

   //User should be authenticated! Redirect him to log in.
   res.redirect('/login');
});



app.post('/home/gototable', checkSignIn, (req, res) => {

	if( tables[req.body.tableId] && 
		tables[req.body.tableId].password === req.body.tablePassword) {

		if( !req.session.user.tables.includes(req.body.tableId)) {
			req.session.user.tables.push(req.body.tableId);
		}

		res.status(200).header('application/json').send({
			redirect: '/table/' + req.body.tableId,
		});
	} else {
		res.status(403).header('application/json').send({
			err: 'Invalid Table ID or Password',
		});
	}

});

app.get('/table/*', checkSignIn, (req, res) => {
	const tableId = req.url.split(path.sep)[req.url.split(path.sep).length-1];
	
	if( tables[tableId] ) {

		if( req.session.user.tables.includes(tableId) ) {
			res.render( 'table' );
		} else {
			console.log('get-player not authorized');
			res.status(403).end();
		}
	} else {
		console.log('get-table doesn\'t exist' );
		res.status(404).end();
	}
});

app.post('/table/*/getstate', checkSignIn, (req, res) => {

	const tableId = req.url.split(path.sep)[req.url.split(path.sep).length - 2];

	if( tables[tableId] && req.session.user.tables.includes(tableId)) {
		// console.log(tables[tableId])
		

		res.header('application/json').status(200).send(stringify(tables[tableId]));



	} else if( tables[tableId] ){
		console.log('player not authorized');
		res.status(403).end();
	} else {
		console.log('table doesn\'t exist');
		res.status(404).end();
	}


});



app.use('/table/*', function(err, req, res, next){
	if( err.message != 'Not logged in!' ){
		console.log('te: ', err);
	}
	if( err ) {
		// console.log('not logged in');

	}

   //User should be authenticated! Redirect him to log in.
   res.redirect('/home');
});


app.listen(process.env.PORT || 3002, () => console.log(`App Available`));

