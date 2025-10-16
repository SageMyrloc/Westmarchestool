class NextSessionCard extends HTMLElement {
    constructor() {
        super();
    }

    connectedCallback() {
        this.render();
        this.loadSession();
    }

    render() {
        this.innerHTML = `
            <div class="card bg-black border-secondary">
                <div class="card-body">
                    <h4 class="text-warning">Next Session</h4>
                    <div id="sessionContent">
                        <p class="text-center text-muted">Loading...</p>
                    </div>
                </div>
            </div>
        `;
    }

    async loadSession() {
        const contentDiv = this.querySelector('#sessionContent');

        try {
            // TODO: Replace with actual API call when sessions endpoint exists
            // const response = await fetch(`${API_BASE_URL}/api/sessions/upcoming`);
            // const session = await response.json();

            // Placeholder data for now
            const session = {
                title: "The Whispering Depths",
                description: "A terrifying adventure for characters of 6th to 8th level.",
                date: "October 25, 2025",
                gm: "Martin",
                signedUp: 3,
                maxPlayers: 6
            };

            contentDiv.innerHTML = `
                <h5 class="text-warning">${session.title}</h5>
                <p class="mb-2">${session.description}</p>
                <div class="mt-3">
                    <small class="d-block text-muted">Date: ${session.date}</small>
                    <small class="d-block text-muted">GM: ${session.gm}</small>
                    <small class="d-block text-warning mt-2">${session.signedUp}/${session.maxPlayers} players</small>
                </div>
            `;
        } catch (error) {
            contentDiv.innerHTML = `
                <p class="text-muted">No upcoming sessions</p>
            `;
        }
    }

    // Method to refresh session data
    refresh() {
        this.loadSession();
    }
}

customElements.define('next-session-card', NextSessionCard);