class Chip {
	constuctor( params ) {
		this.value = params.value || 1;
		this.rotation = params.rotation || 0;
	}
}

exports.Chip = Chip;