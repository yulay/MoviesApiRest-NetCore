# Fase 5: Capa API (Presentation)

## Descripción
Implementación de la capa de presentación con Minimal APIs, autenticación JWT, autorización por roles, rate limiting y documentación Swagger.

## Estructura de Carpetas

```
MovieManager.API/
├── Program.cs
├── appsettings.json
├── Endpoints/
│   ├── AuthEndpoints.cs
│   ├── MovieEndpoints.cs
│   ├── StatisticsEndpoints.cs
│   └── IntegrationEndpoints.cs
└── Middleware/
    └── ExceptionHandlingMiddleware.cs
```

## Configuración de Program.cs

### Servicios Configurados
- **Serilog**: Logging estructurado (consola + archivos)
- **JWT Authentication**: Bearer token con validación completa
- **Authorization Policies**: 3 políticas por rol
- **Rate Limiting**: 100 requests/minuto por IP
- **CORS**: Permite todos los orígenes (configurable)
- **Swagger**: Con soporte para autenticación JWT

### Políticas de Autorización
| Política | Roles Permitidos |
|----------|------------------|
| AdminOnly | Admin |
| EditorOrAdmin | Editor, Admin |
| AllAuthenticated | User, Editor, Admin |

## Endpoints Implementados

### Autenticación (`/api/auth`)
| Método | Ruta | Descripción | Autorización |
|--------|------|-------------|--------------|
| POST | /register | Registrar usuario | Público |
| POST | /login | Iniciar sesión | Público |
| POST | /refresh-token | Renovar token | Público |

### Películas (`/api/movies`)
| Método | Ruta | Descripción | Autorización |
|--------|------|-------------|--------------|
| GET | / | Lista paginada | AllAuthenticated |
| GET | /{id} | Obtener por ID | AllAuthenticated |
| POST | / | Crear película | AdminOnly |
| PUT | /{id} | Actualizar | EditorOrAdmin |
| DELETE | /{id} | Eliminar (soft) | AdminOnly |
| GET | /search?title= | Buscar por título | AllAuthenticated |
| GET | /genre/{genre} | Filtrar por género | AllAuthenticated |
| GET | /director/{director} | Filtrar por director | AllAuthenticated |
| GET | /random | Película aleatoria | AllAuthenticated |
| GET | /recommendations/{genre} | Recomendaciones | AllAuthenticated |

### Estadísticas (`/api/statistics`)
| Método | Ruta | Descripción | Autorización |
|--------|------|-------------|--------------|
| GET | /total-movies | Total de películas | AdminOnly |
| GET | /genres | Stats por género | AdminOnly |
| GET | /years-distribution | Stats por año | AdminOnly |
| GET | /top-directors | Top directores | AdminOnly |

### Integración OMDb (`/api/integration`)
| Método | Ruta | Descripción | Autorización |
|--------|------|-------------|--------------|
| GET | /search-external?title= | Buscar en OMDb | AdminOnly |
| POST | /import/{externalId} | Importar película | AdminOnly |
| PUT | /sync/{id} | Sincronizar datos | AdminOnly |

## Middleware de Manejo de Errores

El `ExceptionHandlingMiddleware` captura todas las excepciones y retorna respuestas JSON estructuradas:

| Excepción | HTTP Status | Mensaje |
|-----------|-------------|---------|
| ValidationException | 400 | Error de validación |
| UnauthorizedAccessException | 401 | No autorizado |
| KeyNotFoundException | 404 | Recurso no encontrado |
| ArgumentException | 400 | Mensaje del error |
| Otras | 500 | Error interno |

## Configuración de appsettings.json

```json
{
  "MongoDb": {
    "ConnectionString": "...",
    "DatabaseName": "MoviesDb",
    "MoviesCollectionName": "Movies",
    "UsersCollectionName": "Users"
  },
  "Jwt": {
    "SecretKey": "...",
    "Issuer": "MovieManagerAPI",
    "Audience": "MovieManagerClients",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Omdb": {
    "BaseUrl": "http://www.omdbapi.com/",
    "ApiKey": "b279cce3"
  }
}
```

## Características Implementadas

### Rate Limiting
- Límite: 100 requests por minuto
- Particionado por IP o usuario autenticado
- Respuesta 429 al exceder límite

### Swagger/OpenAPI
- Documentación automática de endpoints
- Soporte para autenticación Bearer JWT
- Descripciones en español
- Disponible en `/swagger`

### Logging con Serilog
- Logging estructurado
- Request/Response logging automático
- Salida a consola y archivos rotativos
- Archivo: `logs/moviemanager-{date}.log`

## Uso del API

### 1. Registrar Usuario
```bash
POST /api/auth/register
{
  "email": "usuario@ejemplo.com",
  "password": "Password123!",
  "firstName": "Juan",
  "lastName": "Pérez"
}
```

### 2. Iniciar Sesión
```bash
POST /api/auth/login
{
  "email": "usuario@ejemplo.com",
  "password": "Password123!"
}
```

### 3. Usar Token en Requests
```bash
GET /api/movies
Authorization: Bearer {accessToken}
```

### 4. Renovar Token
```bash
POST /api/auth/refresh-token
{
  "refreshToken": "{refreshToken}"
}
```

## Estado de Compilación

✅ Build succeeded - 0 Warnings, 0 Errors
