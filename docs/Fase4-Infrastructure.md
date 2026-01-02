# Fase 4: Capa de Infraestructura

## Descripción
Implementación de la capa de infraestructura que contiene las implementaciones concretas de los repositorios, servicios externos y configuraciones de la aplicación.

## Estructura de Carpetas

```
MovieManager.Infrastructure/
├── DependencyInjection.cs
├── External/
│   └── OmdbService.cs
├── Persistence/
│   ├── Context/
│   │   └── MongoDbContext.cs
│   └── Repositories/
│       ├── MovieRepository.cs
│       └── UserRepository.cs
├── Services/
│   ├── CacheService.cs
│   ├── JwtService.cs
│   └── PasswordHasher.cs
└── Settings/
    ├── JwtSettings.cs
    ├── MongoDbSettings.cs
    └── OmdbSettings.cs
```

## Componentes Implementados

### 1. Settings (Configuraciones)

#### MongoDbSettings
Configuración para la conexión a MongoDB Atlas:
- ConnectionString: Cadena de conexión
- DatabaseName: Nombre de la base de datos
- MoviesCollectionName: Nombre de la colección de películas
- UsersCollectionName: Nombre de la colección de usuarios

#### JwtSettings
Configuración para autenticación JWT:
- SecretKey: Clave secreta para firmar tokens
- Issuer: Emisor del token
- Audience: Audiencia del token
- ExpirationMinutes: Tiempo de expiración del access token
- RefreshTokenExpirationDays: Tiempo de expiración del refresh token

#### OmdbSettings
Configuración para la API de OMDb:
- BaseUrl: URL base de la API
- ApiKey: Clave de API

### 2. Persistence (Persistencia)

#### MongoDbContext
Contexto de base de datos que:
- Gestiona la conexión a MongoDB
- Expone las colecciones Movies y Users
- Crea índices automáticamente al inicializar:
  - Movies: Text index en Title, índices en ExternalId, IsDeleted, Director, Year
  - Users: Índice único en Email, índice en RefreshToken

#### MovieRepository
Implementación del repositorio de películas con 17 métodos:
- **CRUD básico**: GetByIdAsync, CreateAsync, UpdateAsync, SoftDeleteAsync
- **Búsquedas**: GetByExternalIdAsync, GetAllAsync, SearchByTitleAsync, GetByGenreAsync, GetByDirectorAsync
- **Especiales**: GetRandomAsync, GetRecommendationsByGenreAsync
- **Estadísticas**: GetTotalCountAsync, GetCountByGenreAsync, GetCountByYearAsync, GetTopDirectorsAsync
- **Validación**: ExistsAsync

#### UserRepository
Implementación del repositorio de usuarios con 6 métodos:
- GetByIdAsync, GetByEmailAsync, GetByRefreshTokenAsync
- CreateAsync, UpdateAsync
- ExistsAsync

### 3. External (Servicios Externos)

#### OmdbService
Servicio para integración con la API de OMDb:
- **SearchByTitleAsync**: Busca películas por título
- **GetByImdbIdAsync**: Obtiene detalles completos de una película por IMDb ID
- Mapea respuestas de OMDb a entidades Movie del dominio
- Maneja valores "N/A" y convierte tipos de datos apropiadamente

### 4. Services (Servicios)

#### JwtService
Servicio de autenticación JWT:
- **GenerateAccessToken**: Genera token de acceso con claims de usuario
- **GenerateRefreshToken**: Genera token de refresco aleatorio (64 bytes)
- **GetRefreshTokenExpiry**: Calcula fecha de expiración del refresh token

#### PasswordHasher
Servicio de hash de contraseñas usando BCrypt:
- **HashPassword**: Genera hash con salt de 12 rondas
- **VerifyPassword**: Verifica contraseña contra hash

#### CacheService
Servicio de caché en memoria usando IMemoryCache:
- **Get<T>**: Obtiene valor del caché
- **Set<T>**: Almacena valor con expiración configurable (default: 5 minutos)
- **Remove**: Elimina entrada del caché
- **TryGetValue<T>**: Intenta obtener valor sin excepción

### 5. DependencyInjection

Extensión para registrar todos los servicios de infraestructura:
- Configura opciones desde appsettings.json
- Registra MongoDbContext como Singleton
- Registra repositorios como Scoped
- Registra OmdbService con HttpClient
- Registra servicios de autenticación como Scoped
- Configura MemoryCache y CacheService

## Patrones Utilizados

- **Repository Pattern**: Abstrae el acceso a datos
- **Options Pattern**: Configuración tipada desde appsettings
- **Dependency Injection**: Inyección de dependencias en todos los servicios
- **Soft Delete**: Eliminación lógica en películas

## Estado de Compilación

✅ Build succeeded - 0 Warnings, 0 Errors
