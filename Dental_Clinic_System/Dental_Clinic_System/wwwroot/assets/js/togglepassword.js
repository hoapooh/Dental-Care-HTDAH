function togglePasswordVisibility(passwordGroup) {
	var passwordInput = document.querySelector(
		"." + passwordGroup + " .passwordInput"
	);
	var eyeIcon = document.querySelector(
		"." + passwordGroup + " .eyeIcon"
	);
	var eyeSlashIcon = document.querySelector(
		"." + passwordGroup + " .eyeSlashIcon"
	);

	if (passwordInput.type === "password") {
		passwordInput.type = "text";
		eyeIcon.style.display = "none";
		eyeSlashIcon.style.display = "inline-block";
	} else {
		passwordInput.type = "password";
		eyeIcon.style.display = "inline-block";
		eyeSlashIcon.style.display = "none";
	}
}