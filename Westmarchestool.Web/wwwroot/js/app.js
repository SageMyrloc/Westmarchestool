console.log('Westmarchestool loaded successfully!');

// API configuration
const API_BASE_URL = 'https://localhost:7157';

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