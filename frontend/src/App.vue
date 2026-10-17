<script setup lang="ts">
    import { ref, onMounted } from 'vue';
    import type { MediaItem } from './types/media';
    import { mediaApi, getApiBaseUrl, setApiBaseUrl } from './services/api';

    // Konfiguracja połączenia z API
    const apiUrlInput = ref(getApiBaseUrl());
    const statusMessage = ref('');
    const errorMessage = ref('');
    const isLoading = ref(false);

    // Dane galerii
    const mediaList = ref<MediaItem[]>([]);
    const selectedItem = ref<MediaItem | null>(null);

    // Formularz dodawania
    const title = ref('');
    const description = ref('');
    const selectedFile = ref<File | null>(null);

    // Symulator obciążenia CloudWatch (Punkt 10 instrukcji)
    const delayMs = ref(3000);
    const repeatCount = ref(12);
    const heavyTaskProgress = ref('');

    const updateApiUrl = () => {
        setApiBaseUrl(apiUrlInput.value);
        loadMediaList();
    };

    const clearMessages = () => {
        statusMessage.value = '';
        errorMessage.value = '';
    };

    // 1. GET /api/Media
    const loadMediaList = async () => {
        clearMessages();
        isLoading.value = true;
        try {
            mediaList.value = await mediaApi.getAll();
        } catch (err: any) {
            errorMessage.value = `Nie udało się połączyć z API (${apiUrlInput.value}). Sprawdź port i CORS! Szczegóły: ${err.message}`;
        } finally {
            isLoading.value = false;
        }
    };

    // 2. GET /api/Media/{id}
    const showDetails = async (id: string) => {
        clearMessages();
        try {
            selectedItem.value = await mediaApi.getDetails(id);
        } catch (err: any) {
            errorMessage.value = err.message;
        }
    };

    const onFileChange = (e: Event) => {
        const target = e.target as HTMLInputElement;
        selectedFile.value = target.files && target.files.length > 0 ? target.files[0] : null;
    };

    // 3. POST /api/Media + 5. POST /api/Media/{id}/upload
    const handleCreateAndUpload = async () => {
        if (!title.value.trim()) {
            errorMessage.value = 'Podaj tytuł wpisu.';
            return;
        }
        clearMessages();
        isLoading.value = true;

        try {
            // Krok A: Utworzenie rekordu metadanych w DynamoDB
            const created = await mediaApi.createRecord({
                title: title.value,
                description: description.value,
            });

            // Krok B: Jeśli wybrano plik, od razu wyślij go do endpointu uploadu
            if (selectedFile.value) {
                await mediaApi.uploadFile(created.mediaId, selectedFile.value);
                statusMessage.value = `Utworzono wpis i przesłano plik! (ID: ${created.mediaId})`;
            } else {
                statusMessage.value = `Utworzono sam rekord metadanych ze statusem Pending (ID: ${created.mediaId}). Możesz dograć plik później.`;
            }

            // Reset formularza
            title.value = '';
            description.value = '';
            selectedFile.value = null;
            await loadMediaList();
        } catch (err: any) {
            errorMessage.value = err.message;
        } finally {
            isLoading.value = false;
        }
    };

    // Dołączanie pliku do istniejącego rekordu Pending (5. POST /api/Media/{id}/upload)
    const uploadToExisting = async (id: string, e: Event) => {
        const target = e.target as HTMLInputElement;
        if (!target.files || target.files.length === 0) return;

        clearMessages();
        isLoading.value = true;
        try {
            await mediaApi.uploadFile(id, target.files[0]);
            statusMessage.value = 'Plik został pomyślnie dograny!';
            await loadMediaList();
            if (selectedItem.value?.mediaId === id) {
                await showDetails(id);
            }
        } catch (err: any) {
            errorMessage.value = err.message;
        } finally {
            isLoading.value = false;
        }
    };

    // 4. POST /api/Media/{id}/heavy-task (Symulacja obciążenia pod CloudWatch)
    const triggerHeavyTaskSeries = async (id: string) => {
        clearMessages();
        for (let i = 1; i <= repeatCount.value; i++) {
            heavyTaskProgress.value = `Wykonywanie żądania ${i} z ${repeatCount.value} (opóźnienie: ${delayMs.value} ms)...`;
            try {
                await mediaApi.runHeavyTask(id, delayMs.value);
            } catch (err: any) {
                errorMessage.value = `Błąd w serii CloudWatch: ${err.message}`;
                heavyTaskProgress.value = '';
                return;
            }
        }
        heavyTaskProgress.value = '';
        statusMessage.value = `Zakończono serię ${repeatCount.value} żądań z opóźnieniem ${delayMs.value}ms! Sprawdź wykresy w CloudWatch.`;
    };

    onMounted(() => {
        loadMediaList();
    });
</script>

<template>
    <div class="container">
        <header class="header">
            <h1>📸 MediaGallery – Cloud Project (Vue.js + .NET + AWS)</h1>
            <div class="api-bar">
                <label>Adres Backend API:</label>
                <input v-model="apiUrlInput" placeholder="np. https://localhost:32769" />
                <button @click="updateApiUrl">Połącz / Odśwież</button>
            </div>
        </header>

        <div v-if="statusMessage" class="alert success">{{ statusMessage }}</div>
        <div v-if="errorMessage" class="alert error">{{ errorMessage }}</div>
        <div v-if="heavyTaskProgress" class="alert warning">⏳ {{ heavyTaskProgress }}</div>

        <div class="grid">
            <!-- LEWA KOLUMNA: Formularz dodawania (POST /api/Media oraz POST /upload) -->
            <section class="card">
                <h2>➕ Dodaj nowe medium</h2>
                <form @submit.prevent="handleCreateAndUpload" class="form">
                    <label>
                        Tytuł zdjęcia / dokumentu *
                        <input v-model="title" type="text" placeholder="np. Wycieczka w Tatry" required />
                    </label>

                    <label>
                        Opis (opcjonalnie)
                        <textarea v-model="description" rows="3" placeholder="Krótki opis..."></textarea>
                    </label>

                    <label>
                        Plik multimedialny (opcjonalnie w 1. kroku)
                        <input type="file" @change="onFileChange" />
                    </label>

                    <button type="submit" :disabled="isLoading" class="btn-primary">
                        {{ isLoading ? 'Przetwarzanie...' : 'Utwórz rekord i wyślij plik' }}
                    </button>
                </form>
            </section>

            <!-- PRAWA KOLUMNA: Szczegóły wybranego elementu + Test CloudWatch -->
            <section class="card">
                <h2>🔍 Szczegóły i Akcje (Endpoint GET /{id} & CloudWatch)</h2>
                <div v-if="selectedItem" class="details">
                    <p><strong>ID:</strong> <code>{{ selectedItem.mediaId }}</code></p>
                    <p><strong>Tytuł:</strong> {{ selectedItem.title }}</p>
                    <p><strong>Opis:</strong> {{ selectedItem.description || 'Brak' }}</p>
                    <p><strong>Oryginalny plik:</strong> {{ selectedItem.originalFileName || 'Jeszcze nie przesłano' }}</p>
                    <p>
                        <strong>Status:</strong>
                        <span :class="['badge', selectedItem.status.toLowerCase()]">{{ selectedItem.status }}</span>
                    </p>
                    <p><strong>Utworzono:</strong> {{ new Date(selectedItem.createdAt).toLocaleString() }}</p>

                    <!-- Pobieranie pliku: GET /api/Media/{id}/download -->
                    <div v-if="selectedItem.status === 'Ready'" class="download-box">
                        <a :href="mediaApi.getDownloadUrl(selectedItem.mediaId)" target="_blank" class="btn-download">
                            ⬇️ Pobierz plik ({{ selectedItem.originalFileName }})
                        </a>
                    </div>

                    <hr />
                    <!-- Symulator CloudWatch (Punkt 10 instrukcji) -->
                    <h3>🔥 Symulator obciążenia (CloudWatch Alarm Test)</h3>
                    <div class="cw-controls">
                        <label>
                            Opóźnienie (ms):
                            <input v-model.number="delayMs" type="number" step="500" />
                        </label>
                        <label>
                            Liczba wywołań:
                            <input v-model.number="repeatCount" type="number" min="1" max="50" />
                        </label>
                    </div>
                    <button @click="triggerHeavyTaskSeries(selectedItem.mediaId)" class="btn-danger">
                        Uruchom serię {{ repeatCount }}x POST /heavy-task
                    </button>
                </div>
                <p v-else class="muted">Kliknij „Szczegóły” przy wybranym elemencie na liście poniżej.</p>
            </section>
        </div>

        <!-- LISTA ELEMENTÓW: GET /api/Media -->
        <section class="card list-section">
            <div class="list-header">
                <h2>📂 Twoje Multimedia w bazie DynamoDB ({{ mediaList.length }})</h2>
                <button @click="loadMediaList">🔄 Odśwież listę</button>
            </div>

            <table v-if="mediaList.length > 0" class="media-table">
                <thead>
                    <tr>
                        <th>Tytuł</th>
                        <th>Plik</th>
                        <th>Status</th>
                        <th>Akcje</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="item in mediaList" :key="item.mediaId">
                        <td><strong>{{ item.title }}</strong><br /><small>{{ item.description }}</small></td>
                        <td>{{ item.originalFileName || '—' }}</td>
                        <td>
                            <span :class="['badge', item.status.toLowerCase()]">{{ item.status }}</span>
                        </td>
                        <td class="actions">
                            <button @click="showDetails(item.mediaId)">🔍 Szczegóły</button>

                            <!-- Możliwość dogrania pliku jeśli status to Pending -->
                            <label class="upload-inline">
                                📤 Wgraj plik
                                <input type="file" @change="(e) => uploadToExisting(item.mediaId, e)" hidden />
                            </label>

                            <a v-if="item.status === 'Ready'"
                               :href="mediaApi.getDownloadUrl(item.mediaId)"
                               class="btn-link"
                               target="_blank">
                                ⬇️ Pobierz
                            </a>
                        </td>
                    </tr>
                </tbody>
            </table>
            <p v-else class="muted">Brak rekordów w bazie. Dodaj pierwszy element za pomocą formularza powyżej!</p>
        </section>
    </div>
</template>

<style scoped>
    .container {
        max-width: 1100px;
        margin: 0 auto;
        padding: 20px;
        font-family: system-ui, -apple-system, sans-serif;
        color: #1f2937;
    }

    .header {
        display: flex;
        flex-wrap: wrap;
        justify-content: space-between;
        align-items: center;
        background: #111827;
        color: white;
        padding: 16px 24px;
        border-radius: 10px;
        margin-bottom: 20px;
    }

        .header h1 {
            font-size: 1.25rem;
            margin: 0;
        }

    .api-bar {
        display: flex;
        gap: 8px;
        align-items: center;
    }

        .api-bar input {
            padding: 6px 10px;
            border-radius: 6px;
            border: 1px solid #4b5563;
            width: 240px;
        }

    .grid {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 20px;
        margin-bottom: 20px;
    }

    .card {
        background: #ffffff;
        border: 1px solid #e5e7eb;
        border-radius: 10px;
        padding: 20px;
        box-shadow: 0 1px 3px rgba(0,0,0,0.05);
    }

    .form {
        display: flex;
        flex-direction: column;
        gap: 12px;
    }

        .form label {
            display: flex;
            flex-direction: column;
            font-weight: 600;
            font-size: 0.9rem;
            gap: 4px;
        }

    input[type="text"], input[type="number"], textarea {
        padding: 8px;
        border: 1px solid #d1d5db;
        border-radius: 6px;
    }

    button {
        cursor: pointer;
        padding: 8px 14px;
        border-radius: 6px;
        border: 1px solid #d1d5db;
        background: #f9fafb;
        font-weight: 600;
    }

        button:hover {
            background: #f3f4f6;
        }

    .btn-primary {
        background: #2563eb;
        color: white;
        border: none;
        padding: 10px;
    }

        .btn-primary:hover {
            background: #1d4ed8;
        }

    .btn-danger {
        background: #dc2626;
        color: white;
        border: none;
        width: 100%;
        margin-top: 10px;
        padding: 10px;
    }

        .btn-danger:hover {
            background: #b91c1c;
        }

    .btn-download, .btn-link {
        display: inline-block;
        background: #059669;
        color: white;
        text-decoration: none;
        padding: 6px 12px;
        border-radius: 6px;
        font-size: 0.85rem;
    }

    .upload-inline {
        cursor: pointer;
        padding: 6px 12px;
        border-radius: 6px;
        border: 1px solid #d1d5db;
        background: #f9fafb;
        font-size: 0.85rem;
    }

    .badge {
        padding: 3px 8px;
        border-radius: 12px;
        font-size: 0.8rem;
        font-weight: bold;
    }

        .badge.ready {
            background: #d1fae5;
            color: #065f46;
        }

        .badge.pending {
            background: #fef3c7;
            color: #92400e;
        }

    .alert {
        padding: 12px 16px;
        border-radius: 8px;
        margin-bottom: 16px;
        font-weight: 500;
    }

        .alert.success {
            background: #dcfce7;
            color: #166534;
        }

        .alert.error {
            background: #fee2e2;
            color: #991b1b;
        }

        .alert.warning {
            background: #fef9c3;
            color: #854d0e;
        }

    .media-table {
        width: 100%;
        border-collapse: collapse;
        margin-top: 12px;
    }

        .media-table th, .media-table td {
            border-bottom: 1px solid #e5e7eb;
            padding: 10px;
            text-align: left;
        }

    .actions {
        display: flex;
        gap: 8px;
        align-items: center;
    }

    .cw-controls {
        display: flex;
        gap: 12px;
    }

    .list-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .muted {
        color: #6b7280;
    }
</style>