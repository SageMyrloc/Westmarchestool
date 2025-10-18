// characters-page.js

async function loadCharacters() {
    const token = localStorage.getItem('token');

    if (!token) {
        console.log('No token found - user not logged in');
        // TODO: Maybe redirect to home or show a message?
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/api/Characters/my-characters`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error('Failed to fetch characters');
        }

        const characters = await response.json();
        console.log('Characters:', characters);

        // TODO: Display the characters
        displayCharacters(characters);

    } catch (error) {
        console.error('Error loading characters:', error);
    }
}

// Run when page loads
document.addEventListener('DOMContentLoaded', loadCharacters);

function displayCharacters(characters) {
    const grid = document.getElementById('character-grid');

    // Clear any existing content
    grid.innerHTML = '';

    // Check if user has any characters
    if (characters.length === 0) {
        grid.innerHTML = '<div class="col-12 text-center"><p>No characters found. Create your first character!</p></div>';
        return;
    }

    // Create a card for each character
    characters.forEach((character, index) => {
        // Create the column wrapper
        const col = document.createElement('div');
        col.className = index === 0 ? 'col-md-2 offset-md-2' : 'col-md-2';

        // Create the character card
        const card = document.createElement('character-portrait-card');
        card.setAttribute('name', character.name);
        card.setAttribute('character-id', character.id);
        card.setAttribute('character-class', character.class);
        card.setAttribute('level', character.level);

        // Use API endpoint for portrait
        const portraitUrl = `${API_BASE_URL}/api/Characters/${character.id}/portrait`;
        card.setAttribute('portrait-url', portraitUrl);

        // Add card to column, column to grid
        col.appendChild(card);
        grid.appendChild(col);
    });
}