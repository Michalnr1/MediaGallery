import type { MediaItem, CreateMediaRequest, CreateMediaResponse } from '../types/media';

// Domyślny adres API (można go też zmienić bezpośrednio na stronie w pasku u góry)
let apiBaseUrl = import.meta.env.VITE_API_URL || 'http://localhost:8080';

export const setApiBaseUrl = (url: string) => {
    apiBaseUrl = url.replace(/\/+$/, ''); // usuwa ukośnik z końca
};

export const getApiBaseUrl = () => apiBaseUrl;

export const mediaApi = {
    // 1. GET /api/Media - Pobranie listy wszystkich elementów
    async getAll(): Promise<MediaItem[]> {
        const res = await fetch(`${apiBaseUrl}/api/Media`);
        if (!res.ok) throw new Error(`Błąd pobierania listy: ${res.statusText}`);
        return res.json();
    },

    // 2. GET /api/Media/{id} - Pobranie szczegółów jednego elementu
    async getDetails(id: string): Promise<MediaItem> {
        const res = await fetch(`${apiBaseUrl}/api/Media/${id}`);
        if (!res.ok) throw new Error(`Nie znaleziono elementu (${res.status})`);
        return res.json();
    },

    // 3. POST /api/Media - Utworzenie rekordu metadanych (zwraca 201 Created)
    async createRecord(data: CreateMediaRequest): Promise<CreateMediaResponse> {
        const res = await fetch(`${apiBaseUrl}/api/Media`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data),
        });
        if (!res.ok) throw new Error(`Błąd tworzenia rekordu: ${res.statusText}`);
        return res.json();
    },

    // 4. POST /api/Media/{id}/heavy-task?delayMs=... - Symulacja obciążenia dla CloudWatch
    async runHeavyTask(id: string, delayMs: number): Promise<{ message: string }> {
        const res = await fetch(`${apiBaseUrl}/api/Media/${id}/heavy-task?delayMs=${delayMs}`, {
            method: 'POST',
        });
        if (!res.ok) throw new Error(`Błąd symulacji obciążenia: ${res.statusText}`);
        return res.json();
    },

    // 5. POST /api/Media/{id}/upload - Przesłanie pliku binarnego
    async uploadFile(id: string, file: File): Promise<{ message: string }> {
        const formData = new FormData();
        formData.append('file', file);

        const res = await fetch(`${apiBaseUrl}/api/Media/${id}/upload`, {
            method: 'POST',
            body: formData,
        });
        if (!res.ok) throw new Error(`Błąd przesyłania pliku: ${res.statusText}`);
        return res.json();
    },

    // 6. GET /api/Media/{id}/download - Adres do pobrania / wyświetlenia pliku
    getDownloadUrl(id: string): string {
        return `${apiBaseUrl}/api/Media/${id}/download`;
    }
};