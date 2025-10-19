class CharacterPortraitCard extends HTMLElement {
    constructor() {
        super();
    }

    connectedCallback() {
        const characterId = this.getAttribute('character-id');
        const portraitUrl = this.getAttribute('portrait-url') || '';
        const characterName = this.getAttribute('name') || 'Unknown';
        const characterClass = this.getAttribute('character-class') || '';
        const level = this.getAttribute('level') || '1';
        const statusBadge = this.getAttribute('status-badge') || '';

        this.render(portraitUrl, characterName, characterClass, level, statusBadge);
    }

    render(portraitUrl, name, characterClass, level, statusBadge = '') {
        const characterId = this.getAttribute('character-id') || '';

        // Always use Azure for portrait images
        let fullPortraitUrl = portraitUrl;

        if (portraitUrl && portraitUrl.includes('/api/Characters/') && portraitUrl.includes('/portrait')) {
            const azureApiUrl = 'https://westmarches-api-d4czg9a3cbb5dhfe.uksouth-01.azurewebsites.net';
            const match = portraitUrl.match(/\/api\/Characters\/(\d+)\/portrait/);
            if (match) {
                fullPortraitUrl = `${azureApiUrl}/api/Characters/${match[1]}/portrait`;
            }
        } else if (!portraitUrl) {
            fullPortraitUrl = '/assets/images/placeholder-portrait.png';
        }

        this.innerHTML = `
    <div class="character-portrait-card ${characterId ? 'clickable' : ''}" data-character-id="${characterId}">
        <div class="portrait-frame">
            <div class="portrait-image" style="background-image: url('${fullPortraitUrl}');">
            </div>
            <img src="/assets/images/character-frame.png" class="frame-overlay" alt="Character Frame">
            <div class="character-info-overlay">
                <h3 class="character-name">${name}</h3>
                <p class="character-details">${characterClass} ${level}</p>
            </div>
            ${statusBadge ? `<div class="status-badge">${statusBadge}</div>` : ''}
        </div>
    </div>
    `;

        if (characterId) {
            this.querySelector('.character-portrait-card').addEventListener('click', () => {
                window.location.href = `/character-detail.html?id=${characterId}`;
            });
        }
    }

    // Method to update character data
    updateCharacter(data) {
        const portraitDiv = this.querySelector('.portrait-image');
        const nameEl = this.querySelector('.character-name');
        const detailsEl = this.querySelector('.character-details');

        if (portraitDiv && data.portraitUrl) {
            portraitDiv.style.backgroundImage = `url('${data.portraitUrl}')`;
        }
        if (nameEl && data.name) {
            nameEl.textContent = data.name;
        }
        if (detailsEl && data.class && data.level) {
            detailsEl.textContent = `${data.class} ${data.level}`;
        }
    }
}

customElements.define('character-portrait-card', CharacterPortraitCard);