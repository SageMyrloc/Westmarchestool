// Fetch all public characters and filter for Dead (status = 3)
async function loadGraveyardCharacters() {
    try {
        const response = await fetch(`${API_BASE_URL}/api/Characters/public`);

        if (!response.ok) {
            throw new Error('Failed to fetch characters');
        }

        const characters = await response.json();

        // Filter for Dead characters (status = 3)
        const deadCharacters = characters.filter(char => char.status === 3);

        displayCharacters(deadCharacters);

    } catch (error) {
        console.error('Error loading characters:', error);
        document.getElementById('character-grid').innerHTML =
            '<p class="text-warning">Error loading characters</p>';
    }
}

function displayCharacters(characters) {
    const grid = document.getElementById('character-grid');

    if (characters.length === 0) {
        grid.innerHTML = '<p class="text-muted">No characters in the graveyard... yet.</p>';
        return;
    }

    grid.innerHTML = characters.map((char, index) => `
        <div class="${index === 0 ? 'col-md-2 offset-md-2' : 'col-md-2'} mb-4">
            <character-portrait-card
                name="${char.name}"
                level="${char.level}"
                character-class="${char.class}"
                portrait-url="${API_BASE_URL}/api/Characters/${char.id}/portrait"
                status-badge="Deceased"
            ></character-portrait-card>
        </div>
    `).join('');
}

// Load characters when page loads
loadGraveyardCharacters();