console.log('Westmarchestool loaded successfully!');


// Automatically detect if we're running locally or in production
const API_BASE_URL = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1'
    ? 'https://localhost:7157'  // Local API
    : 'https://westmarches-api-d4czg9a3cbb5dhfe.uksouth-01.azurewebsites.net';  // Azure API

console.log('API_BASE_URL:', API_BASE_URL); 

// Utility function for API calls (we'll use this later)
async function apiCall(endpoint, options = {}) {
    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, options);
        return await response.json();
    } catch (error) {
        console.error('API call failed:', error);
        throw error;
    }
}