// Import necessary functions from Firebase SDK
import { initializeApp } from "https://www.gstatic.com/firebasejs/9.6.1/firebase-app.js";
import {
	getStorage,
	ref,
	uploadBytes,
	getDownloadURL,
} from "https://www.gstatic.com/firebasejs/9.6.1/firebase-storage.js";

// Firebase configuration
const firebaseConfig = {
	apiKey: "AIzaSyDC1sfSIQKXTwkyGDR27LsM3AAqsSQiogk",
	authDomain: "auth-demo-123e3.firebaseapp.com",
	projectId: "auth-demo-123e3",
	storageBucket: "auth-demo-123e3.appspot.com",
	messagingSenderId: "867903141504",
	appId: "1:867903141504:web:d8f7ca0edb80a4dc04d865",
	measurementId: "G-JEWGJ20EEC",
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);
const storage = getStorage(app);

// Function to upload file and get URL
document.getElementById("uploadForm").addEventListener("submit", (event) => {
	event.preventDefault(); // Ngăn hành động mặc định của form

	const fileInput = document.getElementById("imageInput");
	const file = fileInput.files[0];

	if (file) {
		// Create a reference to 'profile_pictures/filename'
		const storageRef = ref(storage, "profile_pictures/" + file.name);

		// Upload the file to the reference
		uploadBytes(storageRef, file)
			.then((snapshot) => {
				console.log("Uploaded a file successfully!", snapshot);

				// Get the download URL
				getDownloadURL(storageRef)
					.then((url) => {
						console.log("File available at", url);

						// Display the URL
						const imageUrlElement = document.getElementById("imageUrl");
						imageUrlElement.value = url;

						// USE TO DISPLAY THE IMAGE FROM NONE TO BLOCK
						const imageDisplay = document.getElementById("imageDisplay");
						imageDisplay.src = url;
						imageDisplay.style.display = "block";

						form.submit();

						// HÌNH NHƯ DATABASE THÊM VÀO Ở ĐÂY
						// Here you would save the URL to your database
						// saveImageUrlToDatabase(url);
					})
					.catch((error) => {
						console.error("Error getting download URL:", error);
					});
			})
			.catch((error) => {
				console.error("Error uploading file:", error);
			});
	} else {
		form.submit();
	}
});

// DISPLAY THE IMAGE PREVIEW
document.getElementById("imageInput").addEventListener("change", (e) => {
	const file = e.target.files[0];
	if (file) {
		const imageUrl = URL.createObjectURL(file);
		// USE TO DISPLAY THE IMAGE FROM NONE TO BLOCK
		document.getElementById("imagePreview").src = imageUrl;
		document.getElementById("imagePreview").style.display = "block";
	}
});

// RESET THE IMAGE PREVIEW
document.querySelector('button[type="reset"]').addEventListener("click", () => {
	document.getElementById("imagePreview").src = "";
	document.getElementById("imagePreview").style.display = "none";
});

document
	.querySelector('button[type="submit"]')
	.addEventListener("click", () => {
		document.getElementById("imagePreview").src = "";
		document.getElementById("imagePreview").style.display = "none";
	});

// Example function to save the image URL to your database
// You would replace this with your actual database saving logic
function saveImageUrlToDatabase(url) {
	// Example: saving to Firestore (assumes Firestore is set up)
	// import { getFirestore, doc, setDoc } from 'https://www.gstatic.com/firebasejs/9.6.1/firebase-firestore.js';
	// const db = getFirestore(app);
	// const userDoc = doc(db, 'users', 'user-id'); // replace 'user-id' with actual user id
	// setDoc(userDoc, { profileImageUrl: url }, { merge: true });
	console.log("Image URL saved to database:", url);
}
