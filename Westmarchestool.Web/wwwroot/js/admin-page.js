// admin-page.js

// Check if user is admin
async function checkAdminAccess() {
    const token = localStorage.getItem('token');

    if (!token) {
        window.location.href = '/';
        return false;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/api/Auth/me`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            window.location.href = '/';
            return false;
        }

        const user = await response.json();

        if (!user.roles.includes('Admin')) {
            alert('Access denied. Admin only.');
            window.location.href = '/';
            return false;
        }

        return true;
    } catch (error) {
        console.error('Error checking admin access:', error);
        window.location.href = '/';
        return false;
    }
}

// Load all users
async function loadUsers() {
    const token = localStorage.getItem('token');

    try {
        const response = await fetch(`${API_BASE_URL}/api/Auth/users`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            throw new Error('Failed to load users');
        }

        const users = await response.json();
        displayUsers(users);

    } catch (error) {
        console.error('Error loading users:', error);
        document.getElementById('userList').innerHTML = `
            <p class="text-danger">Failed to load users</p>
        `;
    }
}

// Display users in table
function displayUsers(users) {
    const userList = document.getElementById('userList');

    if (users.length === 0) {
        userList.innerHTML = '<p class="text-muted">No users found</p>';
        return;
    }

    let tableHTML = `
        <table class="table table-dark table-striped">
            <thead>
                <tr>
                    <th>ID</th>
                    <th>Username</th>
                    <th>Roles</th>
                    <th>Status</th>
                    <th>Created</th>
                    <th>Actions</th>
                </tr>
            </thead>
            <tbody>
    `;

    users.forEach(user => {
        const statusBadge = user.isLocked
            ? '<span class="badge bg-danger">Locked</span>'
            : '<span class="badge bg-success">Active</span>';

        const unlockBtn = user.isLocked
            ? `<button class="btn btn-sm btn-success" onclick="unlockUser(${user.id}, '${user.username}')">Unlock</button>`
            : '';

        tableHTML += `
            <tr>
                <td>${user.id}</td>
                <td>${user.username}</td>
                <td>${user.roles.join(', ')}</td>
                <td>${statusBadge}</td>
                <td>${new Date(user.createdDate).toLocaleDateString()}</td>
                <td>
                    <button class="btn btn-sm btn-warning me-2" onclick="openResetPasswordModal(${user.id}, '${user.username}')">Reset Password</button>
                    ${unlockBtn}
                </td>
            </tr>
        `;
    });

    tableHTML += `
            </tbody>
        </table>
    `;

    userList.innerHTML = tableHTML;
}

// Open reset password modal
function openResetPasswordModal(userId, username) {
    document.getElementById('resetUserId').value = userId;
    document.getElementById('resetUsername').textContent = username;
    document.getElementById('newPassword').value = '';
    document.getElementById('resetError').classList.add('d-none');
    document.getElementById('resetSuccess').classList.add('d-none');

    const modal = new bootstrap.Modal(document.getElementById('resetPasswordModal'));
    modal.show();
}

// Reset password
document.getElementById('confirmResetBtn').addEventListener('click', async () => {
    const userId = document.getElementById('resetUserId').value;
    const newPassword = document.getElementById('newPassword').value;
    const errorDiv = document.getElementById('resetError');
    const successDiv = document.getElementById('resetSuccess');

    errorDiv.classList.add('d-none');
    successDiv.classList.add('d-none');

    if (newPassword.length < 8) {
        errorDiv.textContent = 'Password must be at least 8 characters';
        errorDiv.classList.remove('d-none');
        return;
    }

    const token = localStorage.getItem('token');

    try {
        const response = await fetch(`${API_BASE_URL}/api/Auth/reset-password`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                userId: parseInt(userId),
                newPassword: newPassword
            })
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Failed to reset password');
        }

        successDiv.textContent = 'Password reset successfully!';
        successDiv.classList.remove('d-none');

        setTimeout(() => {
            bootstrap.Modal.getInstance(document.getElementById('resetPasswordModal')).hide();
            loadUsers(); // Reload user list
        }, 1500);

    } catch (error) {
        errorDiv.textContent = error.message;
        errorDiv.classList.remove('d-none');
    }
});

// Unlock user
async function unlockUser(userId, username) {
    if (!confirm(`Unlock account for ${username}?`)) {
        return;
    }

    const token = localStorage.getItem('token');

    try {
        const response = await fetch(`${API_BASE_URL}/api/Auth/unlock/${userId}`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            throw new Error('Failed to unlock user');
        }

        alert('User unlocked successfully!');
        loadUsers(); // Reload user list

    } catch (error) {
        alert('Failed to unlock user: ' + error.message);
    }
}

// Initialize page
document.addEventListener('DOMContentLoaded', async () => {
    const isAdmin = await checkAdminAccess();
    if (isAdmin) {
        loadUsers();
    }
});