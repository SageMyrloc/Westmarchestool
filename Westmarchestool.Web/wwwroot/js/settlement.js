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

    // Display characters - we'll add the actual display code next
    grid.innerHTML = characters.map(char => `
        <div class="col-md-3 mb-4">
            <p>${char.name}</p>
        </div>
    `).join('');
}

// Load characters when page loads
loadSettlementCharacters();