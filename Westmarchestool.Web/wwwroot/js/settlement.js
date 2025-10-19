// Check authentication
const token = localStorage.getItem('token');
if (!token) {
    window.location.href = '/index.html';
}

// Fetch characters with Township status (status = 2)
async function loadSettlementCharacters() {
    try {
        const response = await fetch(`${API_BASE_URL}/api/Characters/my-characters`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            throw new Error('Failed to fetch characters');
        }

        const characters = await response.json();

        // Filter for Township characters (status = 2)
        const townshipCharacters = characters.filter(char => char.status === 2);

        displayCharacters(townshipCharacters);

    } catch (error) {
        console.error('Error loading characters:', error);
        document.getElementById('character-grid').innerHTML =
            '<p class="text-warning">Error loading characters</p>';
    }
}

function displayCharacters(characters) {
    const grid = document.getElementById('character-grid');

    if (characters.length === 0) {
        grid.innerHTML = '<p class="text-muted">No characters currently in the settlement.</p>';
        return;
    }

    grid.innerHTML = characters.map((char, index) => `
        <div class="${index === 0 ? 'col-md-2 offset-md-2' : 'col-md-2'} mb-4">
            <character-portrait-card
                name="${char.name}"
                level="${char.level}"
                character-class="${char.class}"
                portrait-url="${API_BASE_URL}/api/Characters/${char.id}/portrait"
                status-badge="Retired"
            ></character-portrait-card>
        </div>
    `).join('');
}

// Load characters when page loads
loadSettlementCharacters();