# Movie Manager API

API REST para gestión de películas con integración OMDb, construida con .NET Core 8 y MongoDB.

## Características

- **CRUD completo** de películas con soft delete
- **Integración con OMDb** para importar y sincronizar datos
- **Autenticación JWT** con refresh tokens
- **Sistema de roles**: Admin, Editor, User
- **Búsqueda y filtrado** por título, género y director
- **Estadísticas** de la colección
- **Rate limiting**: 100 requests/minuto
- **Health Checks** para MongoDB y OMDb
- **Documentación Swagger** interactiva

## Arquitectura

El proyecto sigue **Clean Architecture** con las siguientes capas:

```
src/
├── MovieManager.Domain/          # Entidades y contratos
├── MovieManager.Application/     # Casos de uso (CQRS con MediatR)
├── MovieManager.Infrastructure/  # MongoDB, JWT, OMDb, Cache
├── MovieManager.API/             # Minimal APIs, Middleware
└── MovieManager.Tests/           # Unit + Integration tests
```

## Requisitos Previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MongoDB](https://www.mongodb.com/try/download/community) (local o Atlas)
- [OMDb API Key](https://www.omdbapi.com/apikey.aspx) (gratuita)

## Instalación

### 1. Clonar el repositorio

```bash
git clone https://github.com/tu-usuario/MoviesApiRest-NetCore.git
cd MoviesApiRest-NetCore
```

### 2. Configurar appsettings.json

Editar `src/MovieManager.API/appsettings.json`:

```json
{
  "MongoDb": {
    "ConnectionString": "mongodb+srv://usuario:password@cluster.mongodb.net",
    "DatabaseName": "MoviesDb"
  },
  "Jwt": {
    "SecretKey": "TuClaveSecretaMuyLargaYSeguraDeAlMenos32Caracteres!",
    "Issuer": "MovieManagerAPI",
    "Audience": "MovieManagerClients",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Omdb": {
    "BaseUrl": "http://www.omdbapi.com/",
    "ApiKey": "tu-api-key"
  }
}
```

### 3. Restaurar dependencias y ejecutar

```bash
dotnet restore
dotnet run --project src/MovieManager.API
```

La API estará disponible en: `https://localhost:5001/swagger`

## Usuario de Prueba

Al iniciar la aplicación, se crea automáticamente un usuario administrador:

| Campo | Valor |
|-------|-------|
| Email | admin@moviemanager.com |
| Password | Admin123! |
| Rol | Admin |

## Endpoints

### Autenticación

| Método | Endpoint | Descripción | Acceso |
|--------|----------|-------------|--------|
| POST | `/api/auth/register` | Registrar usuario | Público |
| POST | `/api/auth/login` | Iniciar sesión | Público |
| POST | `/api/auth/refresh-token` | Renovar token | Público |

### Películas

| Método | Endpoint | Descripción | Acceso |
|--------|----------|-------------|--------|
| GET | `/api/movies` | Listar películas (paginado) | User+ |
| GET | `/api/movies/{id}` | Obtener película | User+ |
| POST | `/api/movies` | Crear película | Admin |
| PUT | `/api/movies/{id}` | Actualizar película | Editor+ |
| DELETE | `/api/movies/{id}` | Eliminar película (soft) | Admin |
| GET | `/api/movies/search` | Buscar por título | User+ |
| GET | `/api/movies/genre/{genre}` | Filtrar por género | User+ |
| GET | `/api/movies/director/{director}` | Filtrar por director | User+ |
| GET | `/api/movies/random` | Película aleatoria | User+ |
| GET | `/api/movies/recommendations/{genre}` | Recomendaciones | User+ |

### Estadísticas (Solo Admin)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/statistics/total-movies` | Total de películas |
| GET | `/api/statistics/genres` | Películas por género |
| GET | `/api/statistics/years-distribution` | Películas por año |
| GET | `/api/statistics/top-directors` | Top directores |

### Integración OMDb (Solo Admin)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/integration/search-external` | Buscar en OMDb |
| POST | `/api/integration/import/{imdbId}` | Importar película |
| PUT | `/api/integration/sync/{id}` | Sincronizar con OMDb |

### Health Checks

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/health` | Estado completo |
| GET | `/health/ready` | Verificar BD |
| GET | `/health/live` | Verificar API |

## Ejemplos de Uso

### Login

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@moviemanager.com",
    "password": "Admin123!"
  }'
```

Respuesta:
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "abc123...",
    "email": "admin@moviemanager.com",
    "role": "Admin",
    "expiresAt": "2024-01-02T12:00:00Z"
  }
}
```

### Obtener películas

```bash
curl -X GET "https://localhost:5001/api/movies?page=1&pageSize=10" \
  -H "Authorization: Bearer {token}"
```

### Importar desde OMDb

```bash
# 1. Buscar película
curl -X GET "https://localhost:5001/api/integration/search-external?title=Matrix" \
  -H "Authorization: Bearer {token}"

# 2. Importar por IMDb ID
curl -X POST https://localhost:5001/api/integration/import/tt0133093 \
  -H "Authorization: Bearer {token}"
```

## Tests

```bash
# Ejecutar todos los tests
dotnet test

# Con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Con detalle
dotnet test --verbosity normal
```

Resultados: **64 tests** (51 unit + 13 integration)

## Tecnologías

| Categoría | Tecnología |
|-----------|------------|
| Framework | .NET 8 |
| Base de Datos | MongoDB |
| Autenticación | JWT Bearer |
| Validación | FluentValidation |
| CQRS | MediatR |
| Logging | Serilog |
| Testing | xUnit, Moq, FluentAssertions |
| Documentación | Swagger/OpenAPI |

## Configuración Adicional

### Serilog

Los logs se escriben en:
- Consola (desarrollo)
- Archivos en `logs/` (producción)

### Rate Limiting

- 100 requests por minuto por usuario/IP
- Respuesta 429 cuando se excede

### CORS

Configurado para permitir cualquier origen (desarrollo). Restringir en producción.

## Estructura del Proyecto

```
MovieManager.sln
├── src/
│   ├── MovieManager.Domain/
│   │   ├── Entities/         # Movie, User
│   │   ├── Enums/            # UserRole
│   │   └── Interfaces/       # IMovieRepository, IUserRepository
│   │
│   ├── MovieManager.Application/
│   │   ├── DTOs/             # Data Transfer Objects
│   │   ├── Features/         # Commands y Queries (CQRS)
│   │   ├── Validators/       # FluentValidation
│   │   └── Behaviors/        # MediatR pipelines
│   │
│   ├── MovieManager.Infrastructure/
│   │   ├── Persistence/      # MongoDbContext, Repositories
│   │   ├── Services/         # JWT, OMDb, Cache, Seeder
│   │   └── Settings/         # Configuration classes
│   │
│   ├── MovieManager.API/
│   │   ├── Endpoints/        # Minimal APIs
│   │   ├── Middleware/       # Exception handling
│   │   └── HealthChecks/     # MongoDB, OMDb checks
│   │
│   └── MovieManager.Tests/
│       ├── Application/      # Handler y Validator tests
│       ├── Infrastructure/   # Service tests
│       └── Integration/      # API endpoint tests
│
└── docs/
    ├── WorkPlan.md           # Plan de trabajo
    └── Fase*.md              # Documentación por fase
```

## Licencia

MIT License - ver [LICENSE](LICENSE) para detalles.
