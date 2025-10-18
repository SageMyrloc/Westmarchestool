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

        this.render(portraitUrl, characterName, characterClass, level);
    }

    render(portraitUrl, name, characterClass, level) {
        const characterId = this.getAttribute('character-id') || '';

        this.innerHTML = `
        <div class="character-portrait-card ${characterId ? 'clickable' : ''}" data-character-id="${characterId}">
            <div class="portrait-frame">
                <!-- Character portrait (sits behind frame) -->
                <div class="portrait-image" style="background-image: url('${portraitUrl || '/assets/images/placeholder-portrait.png'}');">
                </div>
                
                <!-- Ornate frame overlay -->
                <img src="/assets/images/character-frame.png" class="frame-overlay" alt="Character Frame">
                
                <!-- Character info at bottom -->
                <div class="character-info-overlay">
                    <h3 class="character-name">${name}</h3>
                    <p class="character-details">${characterClass} ${level}</p>
                </div>
            </div>
        </div>
    `;

        // Add click event if character ID exists
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