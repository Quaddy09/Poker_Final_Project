
async function login() {

	
	const res = JSON.parse( await ajax( '/login' ).post(
			{
				"id" : document.form["username"].value,
				"password" : document.form["password"].value,
			}
		));

	if( res.redirect ){

		window.location = res.redirect;

	}


}