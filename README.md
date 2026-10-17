# 📸 MediaGallery – Cloud Native AWS Project

Projekt aplikacji webowej typu galeria multimediów, zaprojektowany z myślą o wdrożeniu w chmurze AWS (w modelu serverless/kontenerowym). Aplikacja składa się z niezależnych modułów frontendu i backendu, a jej obecna wersja jest w pełni skonteneryzowana i gotowa do uruchomienia lokalnie.

Projekt powstał jako realizacja założeń architektonicznych dla środowiska AWS Academy Learner Lab.

## 🚀 Technologie

* **Backend:** C#, .NET 10 (ASP.NET Core Web API)
* **Frontend:** Vue.js 3, TypeScript, Vite, Nginx
* **Baza Danych:** Amazon DynamoDB (Local)
* **Infrastruktura:** Docker, Docker Compose

## 📂 Struktura projektu

* `/MediaGallery.Api` – Kod źródłowy backendu (.NET) oraz jego plik `Dockerfile`.
* `/frontend` – Kod źródłowy interfejsu użytkownika (Vue.js) wraz z konfiguracją serwera Nginx i osobnym plikiem `Dockerfile`.
* `docker-compose.yml` – Orkiestracja lokalnego środowiska deweloperskiego (uruchamia bazę danych DynamoDB oraz serwer frontendu).

## 🛠️ Wymagania wstępne

Aby uruchomić projekt lokalnie, upewnij się, że posiadasz zainstalowane:
1. [Docker Desktop](https://www.docker.com/products/docker-desktop/) (uruchomiony w tle).
2. [Visual Studio 2026](https://visualstudio.microsoft.com/) lub .NET 10 SDK.
3. Node.js (opcjonalnie, jeśli chcesz modyfikować frontend bez przebudowywania kontenera).

## 💻 Uruchomienie lokalne (Krok po kroku)

### Krok 1: Uruchomienie Bazy Danych i Frontendu (Docker)
Otwórz terminal w głównym katalogu projektu (tam, gdzie znajduje się plik `docker-compose.yml`) i wykonaj polecenie:

```bash
docker-compose up -d --build

```

* Komenda pobierze oficjalny obraz `amazon/dynamodb-local`, udostępniając bazę na porcie **8000**.
* Skompiluje i uruchomi aplikację Vue.js serwowaną przez Nginx na porcie **3000**.

### Krok 2: Uruchomienie Backendu (API)

1. Otwórz plik `MediaGallery.sln` w programie Visual Studio.
2. Uruchom projekt `MediaGallery.Api` (najlepiej używając profilu z obsługą HTTPS).
3. Backend automatycznie połączy się z lokalnym kontenerem DynamoDB i podczas pierwszego startu **utworzy wymaganą tabelę `MediaItems**`.
4. W przeglądarce otworzy się interfejs **Swagger** (np. `https://localhost:32769/swagger`), w którym możesz testować API. Skopiuj adres bazowy API (bez `/swagger`).

### Krok 3: Połączenie modułów

1. Wejdź w przeglądarce pod adres: [http://localhost:3000](http://localhost:3000) (Twój Frontend).
2. W prawym górnym rogu aplikacji, w polu "Adres Backend API", wklej skopiowany wcześniej adres z Visual Studio (np. `https://localhost:32769`).
3. Kliknij **"Połącz / Odśwież"**. Od teraz frontend komunikuje się z Twoim lokalnym API.

## 📡 Dostępne Endpointy API

Architektura zakłada dwuetapowe przesyłanie plików (najpierw inicjalizacja metadanych, następnie transfer pliku binarnego), co docelowo ułatwi integrację z **AWS S3 Pre-Signed URLs**:

| Metoda | Endpoint | Opis |
| --- | --- | --- |
| **GET** | `/api/Media` | Pobiera listę wszystkich multimediów. |
| **GET** | `/api/Media/{id}` | Pobiera szczegóły i metadane konkretnego pliku. |
| **POST** | `/api/Media` | Tworzy nowy rekord w bazie ze statusem `Pending` (oczekuje na plik). |
| **POST** | `/api/Media/{id}/upload` | Przyjmuje fizyczny plik, łączy z metadanymi i zmienia status na `Ready`. |
| **GET** | `/api/Media/{id}/download` | Udostępnia plik binarny do pobrania. |
| **POST** | `/api/Media/{id}/heavy-task` | Symuluje obciążenie i opóźnienia serwera (na potrzeby testów metryk AWS CloudWatch). |

## 🗺️ Roadmap (Kolejne kroki AWS)

Projekt będzie sukcesywnie rozwijany o natywne usługi chmurowe:

* [ ] Wykorzystanie **Terraform** do automatyzacji wdrażania infrastruktury (IaC).
* [ ] Zabezpieczenie endpointów uwierzytelnianiem **Amazon Cognito**.
* [ ] Przeniesienie zapisu plików lokalnych bezpośrednio do **Amazon S3**.
* [ ] Zastąpienie lokalnego emulatora prawdziwą chmurową bazą **Amazon DynamoDB**.
* [ ] Wdrożenie obu kontenerów na **AWS Fargate** (Serverless Compute for Containers).
* [ ] Integracja z **AWS Lambda** (np. do generowania miniaturek wgrywanych zdjęć).
* [ ] Wpięcie telemetrii do **Amazon CloudWatch** (metryki, dashboardy, alarmy).
