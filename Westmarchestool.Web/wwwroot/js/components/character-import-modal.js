class CharacterImportModal extends HTMLElement {
    connectedCallback() {
        this.render();
        this.attachEventListeners();
    }

    render() {
        this.innerHTML = `
            <!-- Character Import Modal -->
            <div class="modal fade" id="characterImportModal" tabindex="-1">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content bg-dark text-light">
                        <div class="modal-header border-secondary">
                            <h5 class="modal-title" style="color: #d4af37;">Import Character</h5>
                            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <form id="characterImportForm">
                                <div class="mb-3">
                                    <label for="pathbuilderJson" class="form-label">Pathbuilder JSON</label>
                                    <textarea 
                                        class="form-control bg-dark text-light border-secondary" 
                                        id="pathbuilderJson" 
                                        rows="10" 
                                        placeholder="Paste your Pathbuilder 2e JSON here..."
                                        required>
                                    </textarea>
                                    <small class="text-muted">Export your character from Pathbuilder 2e and paste the JSON here</small>
                                </div>
                                <div class="mb-3">
                                    <label for="portraitFile" class="form-label">Character Portrait (Optional)</label>
                                    <input
                                        type="file"
                                        class="form-control bg-dark text-light border-secondary"
                                        id="portraitFile"
                                        accept="image/jpeg,image/jpg,image/png,image/webp">
                                    <small class="text-muted">Max 2MB, max 1024x1024 pixels. Formats: JPG, PNG, WEBP</small>
                                </div>
                                <div id="importError" class="alert alert-danger d-none"></div>
                                <div id="importSuccess" class="alert alert-success d-none"></div>
                            </form>
                        </div>
                        <div class="modal-footer border-secondary">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                            <button type="button" class="btn btn-primary" id="importCharacterBtn">Import Character</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    attachEventListeners() {
        const importBtn = this.querySelector('#importCharacterBtn');
        importBtn.addEventListener('click', () => this.handleImport());
    }

    async handleImport() {
        const jsonTextarea = this.querySelector('#pathbuilderJson');
        const portraitFileInput = this.querySelector('#portraitFile');
        const errorDiv = this.querySelector('#importError');
        const successDiv = this.querySelector('#importSuccess');

        // Clear previous messages
        errorDiv.classList.add('d-none');
        successDiv.classList.add('d-none');

        // Validate JSON
        let pathbuilderJson;
        try {
            pathbuilderJson = JSON.parse(jsonTextarea.value);
        } catch (e) {
            errorDiv.textContent = 'Invalid JSON format. Please check your Pathbuilder export.';
            errorDiv.classList.remove('d-none');
            return;
        }

        // Get token
        const token = localStorage.getItem('token');
        if (!token) {
            errorDiv.textContent = 'You must be logged in to import a character.';
            errorDiv.classList.remove('d-none');
            return;
        }

        let portraitUrl = null;

        // Upload portrait if provided
        if (portraitFileInput.files && portraitFileInput.files[0]) {
            try {
                const formData = new FormData();
                formData.append('file', portraitFileInput.files[0]);

                const uploadResponse = await fetch(`${API_BASE_URL}/api/Characters/upload-portrait`, {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${token}`
                    },
                    body: formData
                });

                if (!uploadResponse.ok) {
                    const error = await uploadResponse.json();
                    throw new Error(error.message || 'Failed to upload portrait');
                }

                const uploadResult = await uploadResponse.json();
                portraitUrl = uploadResult.portraitUrl;

            } catch (error) {
                errorDiv.textContent = `Portrait upload failed: ${error.message}`;
                errorDiv.classList.remove('d-none');
                return;
            }
        }

        // Prepare request body
        const requestBody = {
            pathbuilderJson: pathbuilderJson,
            portraitUrl: portraitUrl
        };

        // Make API call to import character
        try {
            const response = await fetch(`${API_BASE_URL}/api/Characters/import`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestBody)
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Failed to import character');
            }

            const character = await response.json();

            // Show success message
            successDiv.textContent = `Character "${character.name}" imported successfully! (Pending admin approval)`;
            successDiv.classList.remove('d-none');

            // Clear form
            jsonTextarea.value = '';
            portraitFileInput.value = '';

            // Close modal after 2 seconds and reload page
            setTimeout(() => {
                bootstrap.Modal.getInstance(this.querySelector('#characterImportModal')).hide();
                window.location.reload();
            }, 2000);

        } catch (error) {
            errorDiv.textContent = error.message;
            errorDiv.classList.remove('d-none');
        }
    }
}

customElements.define('character-import-modal', CharacterImportModal);