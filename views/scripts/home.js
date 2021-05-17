window.onload = () => {
	hello();
};

async function hello() {

	const res = JSON.parse( await ajax('/home').post( {} ) );


	document.getElementById('intro').innerHTML =   `Hello, ${res.id}`

	if( res.authlevel > 0 ) {


		const adminData = JSON.parse( await ajax('/home/admindata').post( {} ) ).adminData;
		
		const div = document.getElementById('admin');
		
		const h3 = document.createElement('h3');
		h3.innerHTML = 'admin data';
		div.appendChild(h3);
		div.appendChild(document.createElement('div'));

		for(user of adminData) {
			const s = document.createElement('div');
			s.innerHTML = `user: ${user.id}, password: ${user.password}`
			div.appendChild( s );
		}

	}

	

}

async function logout() {

	const res = JSON.parse( await ajax('/logout').post( {} ) );
	// if( res.redirect )
	// 	window.location = res.redirect;
	
	window.location = res.redirect || winow.location;

}

async function joinTable() {
	const res = JSON.parse( await ajax('/home/gototable').post({
		"tableId": document.tablejoin["tableId"].value,
		"tablePassword": document.tablejoin["tablePass"].value,
	}));
	if( res.err ) {
		console.log(err);
	} else if( res.redirect ) {
		window.location = res.redirect;
	}
}
