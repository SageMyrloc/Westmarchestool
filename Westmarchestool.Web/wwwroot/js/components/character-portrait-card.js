class CharacterPortraitCard extends HTMLElement {
    constructor() {
        super();
    }

    connectedCallback() {
        const characterId = this.getAttribute('character-id');
        const portraitUrl = this.getAttribute('portrait-url') || '';
        const characterName = this.getAttribute('name') || 'Unknown';
        const characterClass = this.getAttribute('class') || '';
        const level = this.getAttribute('level') || '1';

        this.render(portraitUrl, characterName, characterClass, level);
    }

    render(portraitUrl, name, characterClass, level) {
        this.innerHTML = `
            <div class="character-portrait-card">
                <div class="portrait-frame">
                    <!-- Character portrait (sits behind frame) -->
                    <div class="portrait-image" style="background-image: url('${portraitUrl || '/assets/images/placeholder-portrait.jpg'}');">
                    </div>
                    
                    <!-- Ornate frame overlay -->
                    <img src="/images/character-frame.png" class="frame-overlay" alt="Character Frame">
                    
                    <!-- Character info at bottom -->
                    <div class="character-info-overlay">
                        <h3 class="character-name">${name}</h3>
                        <p class="character-details">${characterClass} ${level}</p>
                    </div>
                </div>
            </div>
        `;
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