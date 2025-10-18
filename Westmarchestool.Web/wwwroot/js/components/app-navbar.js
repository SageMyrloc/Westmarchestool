class AppNavbar extends HTMLElement {
    constructor() {
        super();
    }

    async connectedCallback() {
        const activePage = this.getAttribute('active') || '';

        // Determine auth state based on token
        const token = localStorage.getItem('token');
        const showAuth = !token;
        
        // Check if user is admin
        let isAdmin = false;
        if (token) {
            isAdmin = await this.checkIfAdmin(token);
        }

        this.render(showAuth, activePage, isAdmin);

        setTimeout(() => {
            this.attachEventListeners();
        }, 0);
    }

    async checkIfAdmin(token) {
        try {
            const response = await fetch(`${API_BASE_URL}/api/Auth/me`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) return false;

            const user = await response.json();
            return user.roles && user.roles.includes('Admin');
        } catch (error) {
            console.error('Error checking admin status:', error);
            return false;
        }
    }

    render(showAuth, activePage, isAdmin) {
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
                        <!-- Coming soon:
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
                        ${isAdmin ? `
                        <li class="nav-item">
                            <a class="nav-link ${activePage === 'admin' ? 'active' : ''}" href="/admin.html">Admin</a>
                        </li>
                        ` : ''}
                    </ul>
                    ${showAuth ? this.renderAuthButtons() : this.renderUserMenu()}
                </div>
            </nav>
            
            ${this.renderModals()}
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

    renderModals() {
        return `
            <login-modal id="loginModal"></login-modal>
            <register-modal id="registerModal"></register-modal>
        `;
    }

    attachEventListeners() {
        const token = localStorage.getItem('token');

        if (!token) {
            // Login/Register button listeners
            const signInBtn = this.querySelector('#signInBtn');
            const registerBtn = this.querySelector('#registerBtn');

            if (signInBtn) {
                signInBtn.addEventListener('click', async () => {
                    await customElements.whenDefined('login-modal');
                    const loginModal = this.querySelector('#loginModal');
                    if (loginModal && loginModal.show) {
                        loginModal.show();
                    }
                });
            }

            if (registerBtn) {
                registerBtn.addEventListener('click', async () => {
                    await customElements.whenDefined('register-modal');
                    const registerModal = this.querySelector('#registerModal');
                    if (registerModal && registerModal.show) {
                        registerModal.show();
                    }
                });
            }

        } else {
            // Logout button listener
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