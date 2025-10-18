class AppNavbar extends HTMLElement {
    constructor() {
        super();
    }

    connectedCallback() {
        const activePage = this.getAttribute('active') || '';

        // Determine auth state based on token, not attribute
        const token = localStorage.getItem('token');
        const showAuth = !token; // Show auth buttons if NO token exists

        this.render(showAuth, activePage);

        setTimeout(() => {
            this.attachEventListeners();
        }, 0);
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
        console.log('attachEventListeners called');  // <-- ADD THIS
        const token = localStorage.getItem('token');
        console.log('Token:', token);  // <-- ADD THIS

        if (!token) {
            console.log('No token, showing auth buttons');  // <-- ADD THIS
    // rest of code...
            // Login/Register button listeners
            const signInBtn = this.querySelector('#signInBtn');
            const registerBtn = this.querySelector('#registerBtn');

            console.log('Attaching listeners, signInBtn:', signInBtn, 'registerBtn:', registerBtn);

            if (signInBtn) {
                signInBtn.addEventListener('click', async () => {
                    console.log('Sign In button clicked');

                    // Wait for custom element to be ready
                    await customElements.whenDefined('login-modal');
                    console.log('login-modal is defined');

                    const loginModal = this.querySelector('#loginModal');
                    console.log('loginModal element:', loginModal);
                    console.log('loginModal.show exists?', typeof loginModal.show);

                    if (loginModal && loginModal.show) {
                        console.log('Calling show()');
                        loginModal.show();
                    } else {
                        console.log('Could not call show()');
                    }
                });
            }

            if (registerBtn) {
                registerBtn.addEventListener('click', async () => {
                    console.log('Register button clicked');

                    // Wait for custom element to be ready
                    await customElements.whenDefined('register-modal');
                    console.log('register-modal is defined');

                    const registerModal = this.querySelector('#registerModal');
                    console.log('registerModal element:', registerModal);
                    console.log('registerModal.show exists?', typeof registerModal.show);

                    if (registerModal && registerModal.show) {
                        console.log('Calling show()');
                        registerModal.show();
                    } else {
                        console.log('Could not call show()');
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