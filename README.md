# 📸 MediaGallery – Cloud Native AWS Project

Projekt aplikacji webowej typu galeria multimediów, zaprojektowany z myślą o wdrożeniu w chmurze AWS (w modelu serverless/kontenerowym). Aplikacja składa się z niezależnych modułów frontendu i backendu. Obecna wersja jest w pełni skonteneryzowana i gotowa do uruchomienia lokalnie jednym poleceniem.

Projekt powstał jako realizacja założeń architektonicznych dla środowiska AWS Academy Learner Lab.

## 🚀 Technologie

* **Backend:** C#, .NET 10 (ASP.NET Core Web API)
* **Frontend:** Vue.js 3, TypeScript, Vite, Nginx
* **Baza Danych:** Amazon DynamoDB (Local)
* **Infrastruktura:** Docker, Docker Compose

## 📂 Struktura projektu

* `/MediaGallery.Api` – Kod źródłowy backendu (.NET) oraz jego plik `Dockerfile`.
* `/frontend` – Kod źródłowy interfejsu użytkownika (Vue.js) wraz z konfiguracją serwera Nginx i plikiem `Dockerfile`.
* `docker-compose.yml` – Orkiestracja lokalnego środowiska deweloperskiego (uruchamia jednocześnie bazę danych DynamoDB, API oraz serwer frontendu).

## 🛠️ Wymagania wstępne

Aby uruchomić projekt lokalnie na dowolnym komputerze, potrzebujesz **tylko jednego** narzędzia:
1. [Docker Desktop](https://www.docker.com/products/docker-desktop/) (uruchomiony w tle).

*(Nie potrzebujesz zainstalowanego Visual Studio ani środowiska Node.js do samego uruchomienia projektu).*

## 💻 Uruchomienie lokalne (One-Click Start)

Otwórz terminal w głównym katalogu projektu (tam, gdzie znajduje się plik `docker-compose.yml`) i wykonaj polecenie:

```bash
docker-compose up -d --build
```

Polecenie to automatycznie:

1. Pobierze i uruchomi lokalną bazę DynamoDB na porcie 8000.
2. Skompiluje kod C# i uruchomi Backend API na porcie 8080.
3. Skompiluje aplikację Vue.js i udostępni Frontend na porcie 3000.
4. (Backend podczas pierwszego startu samodzielnie połączy się z bazą danych i utworzy wymaganą tabelę MediaItems).

## 🌐 Dostęp do aplikacji
Po zakończeniu budowania kontenerów:

* Interfejs Użytkownika (Frontend) jest dostępny pod adresem: http://localhost:3000
* Dokumentacja API (Swagger) jest dostępna pod adresem: http://localhost:8080/swagger

Aplikacja jest od razu gotowa do pracy – Frontend domyślnie komunikuje się z adresem http://localhost:8080.

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
