class AppNavbar extends HTMLElement {
    constructor() {
        super();
    }

    connectedCallback() {
        // Get attributes or use defaults
        const showAuth = this.getAttribute('show-auth') !== 'false'; // default true
        const activePage = this.getAttribute('active') || '';

        this.render(showAuth, activePage);
        this.attachEventListeners();
    }

    render(showAuth, activePage) {
        this.innerHTML = `
            <nav class="navbar navbar-expand-lg navbar-dark bg-black border-bottom border-secondary px-4">
                <a class="navbar-brand me-4" href="/">
                    <div class="home-button">TB</div>
                </a>
                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                    <span class="navbar-toggler-icon"></span>
                </button>
                <div class="collapse navbar-collapse" id="navbarNav">
                    <ul class="navbar-nav me-auto">
                        <li class="nav-item">
                            <a class="nav-link ${activePage === 'home' ? 'active' : ''}" href="/">Home</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link ${activePage === 'characters' ? 'active' : ''}" href="/characters.html">Characters</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link ${activePage === 'sessions' ? 'active' : ''}" href="/sessions.html">Sessions</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link ${activePage === 'shops' ? 'active' : ''}" href="/shops.html">Shops</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link ${activePage === 'map' ? 'active' : ''}" href="/map.html">Map</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link ${activePage === 'archives' ? 'active' : ''}" href="/archives.html">Archives</a>
                        </li>
                    </ul>
                    ${showAuth ? this.renderAuthButtons() : this.renderUserMenu()}
                </div>
            </nav>
        `;
    }

    renderAuthButtons() {
        return `
            <div class="d-flex gap-2">
                <button class="btn btn-outline-warning btn-sm" id="signInBtn">Sign In</button>
                <button class="btn btn-warning btn-sm" id="registerBtn">Register</button>
            </div>
        `;
    }

    renderUserMenu() {
        const username = localStorage.getItem('username') || 'User';
        return `
            <div class="d-flex gap-2 align-items-center">
                <span class="text-warning">Welcome, ${username}</span>
                <button class="btn btn-outline-warning btn-sm" id="logoutBtn">Logout</button>
            </div>
        `;
    }

    attachEventListeners() {
        // Check if user is logged in
        const token = localStorage.getItem('token');

        if (!token) {
            // Attach login/register listeners
            const signInBtn = this.querySelector('#signInBtn');
            const registerBtn = this.querySelector('#registerBtn');

            if (signInBtn) {
                signInBtn.addEventListener('click', () => {
                    this.dispatchEvent(new CustomEvent('show-login', { bubbles: true }));
                });
            }

            if (registerBtn) {
                registerBtn.addEventListener('click', () => {
                    this.dispatchEvent(new CustomEvent('show-register', { bubbles: true }));
                });
            }
        } else {
            // Attach logout listener
            const logoutBtn = this.querySelector('#logoutBtn');
            if (logoutBtn) {
                logoutBtn.addEventListener('click', () => {
                    localStorage.removeItem('token');
                    localStorage.removeItem('username');
                    localStorage.removeItem('roles');
                    window.location.href = '/';
                });
            }
        }
    }
}

customElements.define('app-navbar', AppNavbar);