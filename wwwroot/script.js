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

// Handle sign-up form submission
signupForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    const newUsername = document.getElementById("new-username").value;
    const newPassword = document.getElementById("new-password").value;
    const role = document.getElementById("role").value;

    try {
        const response = await fetch("https://athletevideo.azurewebsites.net/api/signup", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email: newUsername, passwordHash: newPassword }),
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

// Handle login form submission
loginForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    const username = document.getElementById("username").value;
    const password = document.getElementById("password").value;

    try {
        const response = await fetch("https://athletevideo.azurewebsites.net/api/login", {
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
                studentSection.style.display = "none"; // Teachers see their section only
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

// Handle video upload
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
        const response = await fetch("https://athletevideo.azurewebsites.net/api/uploadvideo", {
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

// Logout
logoutButton.addEventListener("click", () => {
    currentUser = null;
    mainPage.style.display = "none";
    loginSection.style.display = "block";
});
