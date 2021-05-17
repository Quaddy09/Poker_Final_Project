async function signup() {

	
	const res = JSON.parse( await ajax( '/signup' ).post(
			{
				"id" : document.form["username"].value,
				"password" : document.form["password"].value,
			}
		)
	);

	if( res.redirect ) {

		window.location = res.redirect;

	} else if ( res.message ) {
		document.write( res.message );
	}




}