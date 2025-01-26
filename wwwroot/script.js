// JavaScript for handling user interactions

// DOM Elements
const loginSection = document.getElementById("login-section");
const signupSection = document.getElementById("signup-section");
const mainPage = document.getElementById("main-page");
const signupButton = document.getElementById("signup-button");
const backToLoginButton = document.getElementById("back-to-login");
const loginForm = document.getElementById("login-form");
const signupForm = document.getElementById("signup-form");
const userRoleSpan = document.getElementById("user-role");
const studentSection = document.getElementById("student-section");
const teacherSection = document.getElementById("teacher-section");
const logoutButton = document.getElementById("logout");
const viewResultsButton = document.getElementById("view-results");
const teacherResults = document.getElementById("teacher-results");
const getScoreButton = document.getElementById("get-score");
const resultsDiv = document.getElementById("results");
const uploadProgress = document.getElementById("upload-progress");


// Simulated user data storage
let users = [];
let currentUser = null;

// Show sign-up section
signupButton.addEventListener("click", () => {
    loginSection.style.display = "none";
    signupSection.style.display = "block";
});

// Back to login
backToLoginButton.addEventListener("click", () => {
    signupSection.style.display = "none";
    loginSection.style.display = "block";
});

signupForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    const newUsername = document.getElementById("new-username").value;
    const newPassword = document.getElementById("new-password").value;
    const role = document.getElementById("role").value;

    try {
        const response = await fetch("https://teamproject1app.azurewebsites.net/api/signup", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email: newUsername, passwordHash: newPassword })
        });

        if (response.ok) {
            alert("Sign-up successful! Please log in.");
            signupSection.style.display = "none";
            loginSection.style.display = "block";
        } else {
            const error = await response.text();
            alert(`Error: ${error}`);
        }
    } catch (error) {
        console.error("Sign-up error:", error);
        alert("An error occurred during sign-up.");
    }
});

const uploadVideo = async () => {
    const fileInput = document.getElementById('video-upload');
    const file = fileInput.files[0];

    if (!file) {
        alert('Please select a video file.');
        return;
    }

    // Get the SAS token from your backend (Azure Function or API)
    const response = await fetch('https://teamproject1app.azurewebsites.net/api/GetSasToken');
    if (!response.ok) {
        alert('Failed to get SAS token.');
        return;
    }

    const sasToken = await response.text();

    // Upload the file to the blob storage container
    const blobUrl = `https://teamproject1.blob.core.windows.net/uploads/${file.name}${sasToken}`;
    const uploadResponse = await fetch(blobUrl, {
        method: 'PUT',
        headers: { 'x-ms-blob-type': 'BlockBlob' },
        body: file
    });

    if (uploadResponse.ok) {
        alert('Video uploaded successfully!');
    } else {
        alert('Failed to upload video.');
    }
};

document.getElementById('upload-button').addEventListener('click', uploadVideo);


// Handle login form submission
loginForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    const username = document.getElementById("username").value;
    const password = document.getElementById("password").value;

    try {
        const response = await fetch("/login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ username, password }),
        });

        if (response.ok) {
            const data = await response.json();
            currentUser = data.user;
            loginSection.style.display = "none";
            mainPage.style.display = "block";
            userRoleSpan.textContent = currentUser.role;

            if (currentUser.role === "student") {
                studentSection.style.display = "block";
                teacherSection.style.display = "none";
            } else {
                teacherSection.style.display = "block";
                studentSection.style.display = "block"; // Teachers can also upload videos
            }
        } else {
            const error = await response.json();
            alert(`Login failed: ${error.message}`);
        }
    } catch (error) {
        console.error("Login error:", error);
        alert("An error occurred during login.");
    }
});

// Logout
logoutButton.addEventListener("click", () => {
    currentUser = null;
    mainPage.style.display = "none";
    loginSection.style.display = "block";
});

// Handle "Get Score" button for students
getScoreButton.addEventListener("click", async () => {
    const fileInput = document.getElementById("video-upload");
    const file = fileInput.files[0];

    if (!file) {
        alert("Please upload a video file.");
        return;
    }

    uploadProgress.style.display = "block";
    uploadProgress.textContent = "Uploading video...";

    const formData = new FormData();
    formData.append("file", file);

    try {
        const response = await fetch("/upload", {
            method: "POST",
            body: formData,
        });

        uploadProgress.style.display = "none";

        if (response.ok) {
            const result = await response.json();
            resultsDiv.innerHTML = `<h3>Evaluation Result:</h3><pre>${JSON.stringify(result, null, 2)}</pre>`;
        } else {
            const error = await response.json();
            alert(`Error: ${error.error}`);
        }
    } catch (error) {
        console.error("Error uploading video:", error);
        uploadProgress.style.display = "none";
        alert("An error occurred while uploading the video.");
    }
});

// Handle "View All Results" button for teachers
viewResultsButton.addEventListener("click", async () => {
    if (currentUser.role === "teacher") {
        try {
            const response = await fetch("/get_results");
            if (response.ok) {
                const data = await response.json();
                teacherResults.innerHTML = `<h3>All Student Results:</h3><pre>${JSON.stringify(data, null, 2)}</pre>`;
            } else {
                alert("Error fetching results.");
            }
        } catch (error) {
            console.error("Error fetching results:", error);
        }
    } else {
        alert("Error: Only teachers are allowed to view this section.");
    }
});

// Update sport disciplines for student and teacher sections
const studentSportSelect = document.getElementById("sport");
const teacherSportSelect = document.getElementById("sport-teacher");

const disciplines = [
    "Sprint Starts",
    "Shot Put",
    "High Jump",
    "Hurdles",
    "Long Jump",
    "Discus Throw",
    "Javelin",
    "Relay Receiver Performance"
];

disciplines.forEach((discipline) => {
    const studentOption = document.createElement("option");
    studentOption.value = discipline.toLowerCase().replace(/\s+/g, "-");
    studentOption.textContent = discipline;
    studentSportSelect.appendChild(studentOption);

    const teacherOption = document.createElement("option");
    teacherOption.value = discipline.toLowerCase().replace(/\s+/g, "-");
    teacherOption.textContent = discipline;
    teacherSportSelect.appendChild(teacherOption);
});
