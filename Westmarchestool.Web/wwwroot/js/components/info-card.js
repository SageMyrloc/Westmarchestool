class InfoCard extends HTMLElement {
    constructor() {
        super();
    }

    connectedCallback() {
        const title = this.getAttribute('title') || 'Info';
        const content = this.getAttribute('content') || 'No content available';
        const variant = this.getAttribute('variant') || 'secondary';

        this.render(title, content, variant);
    }

    render(title, content, variant) {
        this.innerHTML = `
            <div class="card bg-black border-${variant} mb-4">
                <div class="card-body">
                    <h5 class="text-warning">${title}</h5>
                    <p class="mb-0">${content}</p>
                </div>
            </div>
        `;
    }

    // Method to update content dynamically
    updateContent(title, content) {
        const titleEl = this.querySelector('h5');
        const contentEl = this.querySelector('p');
        if (titleEl) titleEl.textContent = title;
        if (contentEl) contentEl.textContent = content;
    }
}

customElements.define('info-card', InfoCard);