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
    apiKey: "AIzaSyAIXe3XvrWSJcuRR0ImSHtBBIJe-W8xrl0",
    authDomain: "dental-care-3388d.firebaseapp.com",
    projectId: "dental-care-3388d",
    storageBucket: "dental-care-3388d.appspot.com",
    messagingSenderId: "554400776601",
    appId: "1:554400776601:web:450c6bb275b0b646ebd308",
    measurementId: "G-LJWE3WXSM5"
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);
const storage = getStorage(app);

// Function to upload file and get URL
document.getElementById("uploadForm").addEventListener("submit", (event) => {
    event.preventDefault(); // Prevent default form submission

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

                        const imagePreview = document.getElementById("imagePreview");
                        imagePreview.src = url;

                        // Submit the form
                        document.getElementById('uploadForm').submit();
                    })
                    .catch((error) => {
                        console.error("Error getting download URL:", error);
                    });
            })
            .catch((error) => {
                console.error("Error uploading file:", error);
            });
    } else {
        // Submit the form if no file is selected
        document.getElementById('uploadForm').submit();
    }
});

// DISPLAY THE IMAGE PREVIEW
document.getElementById("imageInput").addEventListener("change", (e) => {
    const file = e.target.files[0];
    if (file) {
        const imageUrl = URL.createObjectURL(file);
        document.getElementById("imagePreview").src = imageUrl;
    }
});

// Example function to save the image URL to your database
function saveImageUrlToDatabase(url) {
    console.log("Image URL saved to database:", url);
}
