console.log('Westmarchestool loaded successfully!');

// API configuration
const API_BASE_URL = 'https://westmarches-api-d4czg9a3cbb5dhfe.uksouth-01.azurewebsites.net';

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