# Plan de Trabajo - MoviesApiRest-NetCore

## Resumen de Configuración

| Aspecto | Decisión |
|---------|----------|
| Framework | .NET Core 8 |
| API Externa | OMDb (key: b279cce3) |
| Base de Datos | MongoDB Atlas |
| Arquitectura | Clean Architecture + Minimal APIs |
| Autenticación | JWT (Register/Login/Refresh Token) |
| Roles | Admin, Editor, User |
| Cache | IMemoryCache |
| Rate Limiting | 100 requests/minuto |
| Testing | xUnit |
| Logging | Serilog (consola + archivos .log) |
| Soft Delete | Campo IsDeleted: bool |

### Permisos por Rol
- **Admin:** CRUD completo + estadísticas + importación desde OMDb
- **Editor:** Edición de películas existentes (PUT)
- **User:** Solo lectura (GET)

---

## FASE 1: Configuración Inicial y Estructura del Proyecto
**Objetivo:** Crear la estructura base del proyecto con Clean Architecture

### Tareas:
- [x] 1.1 Crear solución y proyectos
  - MovieManager.sln
  - MovieManager.Domain
  - MovieManager.Application
  - MovieManager.Infrastructure
  - MovieManager.API
  - MovieManager.Tests

- [x] 1.2 Configurar referencias entre proyectos
  - API → Application, Infrastructure
  - Application → Domain
  - Infrastructure → Application, Domain
  - Tests → todos los proyectos

- [x] 1.3 Instalar paquetes NuGet necesarios
  - MongoDB.Driver
  - MediatR
  - FluentValidation
  - Serilog
  - JWT Bearer Authentication
  - Swagger/OpenAPI
  - xUnit, Moq, FluentAssertions

- [x] 1.4 Configurar archivos base
  - appsettings.json con connection strings
  - appsettings.Development.json
  - Configuración de Serilog (consola + archivos)

---

## FASE 2: Capa Domain
**Objetivo:** Definir entidades y contratos del dominio

### Tareas:
- [x] 2.1 Crear entidad Movie
  - Id, ExternalId, Title, Description, Year
  - Genres, Director, Actors, Rating, Duration
  - CreatedAt, UpdatedAt, IsDeleted, Metadata

- [x] 2.2 Crear entidad User
  - Id, Email, PasswordHash, FirstName, LastName
  - Role, CreatedAt, RefreshToken, RefreshTokenExpiry

- [x] 2.3 Crear enumeración UserRole
  - User, Editor, Admin

- [x] 2.4 Crear interfaces de repositorios
  - IMovieRepository
  - IUserRepository

- [x] 2.5 Crear interfaces de servicios externos
  - IOmdbService

---

## FASE 3: Capa Application
**Objetivo:** Implementar casos de uso con MediatR y CQRS

### Tareas:
- [x] 3.1 Configurar MediatR y dependencias

- [x] 3.2 Crear DTOs
  - MovieDto, MovieCreateDto, MovieUpdateDto
  - UserDto, RegisterDto, LoginDto
  - TokenDto, RefreshTokenDto
  - PaginatedResultDto, ResultDto, StatisticsDtos

- [x] 3.3 Crear Commands (escritura)
  - CreateMovieCommand
  - UpdateMovieCommand
  - DeleteMovieCommand
  - ImportMovieCommand, SyncMovieCommand
  - RegisterCommand
  - LoginCommand
  - RefreshTokenCommand

- [x] 3.4 Crear Queries (lectura)
  - GetMovieByIdQuery
  - GetMoviesQuery (con paginación)
  - SearchMoviesQuery
  - GetMoviesByGenreQuery
  - GetMoviesByDirectorQuery
  - GetRandomMovieQuery
  - GetRecommendationsQuery
  - GetTotalMoviesQuery
  - GetGenreStatsQuery
  - GetYearStatsQuery
  - GetTopDirectorsQuery
  - SearchOmdbQuery

- [x] 3.5 Crear Handlers para Commands y Queries

- [x] 3.6 Crear Validators con FluentValidation
  - MovieCreateValidator
  - MovieUpdateValidator
  - RegisterValidator
  - LoginValidator

- [x] 3.7 Crear Behaviors de MediatR
  - ValidationBehavior
  - LoggingBehavior

---

## FASE 4: Capa Infrastructure
**Objetivo:** Implementar acceso a datos y servicios externos

### Tareas:
- [x] 4.1 Configurar MongoDB
  - MongoDbContext
  - Configuración de conexión
  - Índices para búsquedas rápidas

- [x] 4.2 Implementar repositorios
  - MovieRepository (con filtro IsDeleted)
  - UserRepository

- [x] 4.3 Implementar servicio OMDb
  - OmdbService (llamadas HTTP a la API)
  - Mapeo de respuestas OMDb a entidades

- [x] 4.4 Implementar servicio de autenticación
  - JwtService (generación y validación de tokens)
  - PasswordHasher

- [x] 4.5 Configurar cache
  - CacheService con IMemoryCache

- [x] 4.6 Registrar servicios en DI
  - InfrastructureServiceExtensions

---

## FASE 5: Capa API (Presentation)
**Objetivo:** Crear endpoints con Minimal APIs

### Tareas:
- [x] 5.1 Configurar Program.cs
  - Servicios de DI
  - Autenticación JWT
  - Swagger con autenticación
  - Serilog
  - Rate Limiting
  - CORS

- [x] 5.2 Crear endpoints de Autenticación
  - POST /api/auth/register
  - POST /api/auth/login
  - POST /api/auth/refresh-token

- [x] 5.3 Crear endpoints de Películas (CRUD)
  - GET /api/movies (paginado) - [User, Editor, Admin]
  - GET /api/movies/{id} - [User, Editor, Admin]
  - POST /api/movies - [Admin]
  - PUT /api/movies/{id} - [Editor, Admin]
  - DELETE /api/movies/{id} - [Admin] (soft delete)

- [x] 5.4 Crear endpoints de Búsqueda
  - GET /api/movies/search?title={title} - [User, Editor, Admin]
  - GET /api/movies/genre/{genre} - [User, Editor, Admin]
  - GET /api/movies/director/{director} - [User, Editor, Admin]

- [x] 5.5 Crear endpoints de Estadísticas
  - GET /api/statistics/total-movies - [Admin]
  - GET /api/statistics/genres - [Admin]
  - GET /api/statistics/years-distribution - [Admin]
  - GET /api/statistics/top-directors - [Admin]

- [x] 5.6 Crear endpoints de Integración OMDb
  - GET /api/integration/search-external - [Admin]
  - POST /api/integration/import/{externalId} - [Admin]
  - PUT /api/integration/sync/{id} - [Admin]

- [x] 5.7 Crear endpoints Adicionales
  - GET /api/movies/random - [User, Editor, Admin]
  - GET /api/movies/recommendations/{genre} - [User, Editor, Admin]

- [x] 5.8 Configurar middleware
  - Exception handling global
  - Request/Response logging

---

## FASE 6: Seed de Datos Iniciales
**Objetivo:** Cargar películas de ejemplo al iniciar

### Tareas:
- [x] 6.1 Crear servicio de seeding
  - DataSeederService

- [x] 6.2 Implementar importación inicial
  - Lista de películas populares a importar desde OMDb
  - Ejecutar al iniciar si la BD está vacía

- [x] 6.3 Crear usuario Admin por defecto
  - admin@moviemanager.com / Admin123!

---

## FASE 7: Testing
**Objetivo:** Garantizar calidad con pruebas automatizadas

### Tareas:
- [x] 7.1 Configurar proyecto de tests
  - xUnit, Moq, FluentAssertions

- [x] 7.2 Unit Tests - Application Layer
  - Tests para Handlers de Commands
  - Tests para Handlers de Queries
  - Tests para Validators

- [x] 7.3 Unit Tests - Infrastructure Layer
  - Tests para servicios (JWT, Password, Cache)

- [x] 7.4 Integration Tests
  - Tests de endpoints críticos
  - Tests de autenticación

- [x] 7.5 Verificar que todas las pruebas pasen

---

## FASE 8: Documentación y Finalización
**Objetivo:** Documentar y pulir el proyecto

### Tareas:
- [x] 8.1 Configurar Swagger completo
  - Descripciones en español
  - Ejemplos de request/response
  - Agrupación por categorías

- [x] 8.2 Agregar Health Checks
  - MongoDB connection check
  - OMDb API availability check

- [x] 8.3 Actualizar README.md
  - Instrucciones de instalación
  - Configuración requerida
  - Ejemplos de uso

- [x] 8.4 Revisión final
  - Verificar todos los endpoints
  - Verificar permisos por rol
  - Verificar logs funcionando
  - Verificar rate limiting

---

## Progreso General

| Fase | Estado | Descripción |
|------|--------|-------------|
| Fase 1 | ✅ Completada | Configuración Inicial |
| Fase 2 | ✅ Completada | Capa Domain |
| Fase 3 | ✅ Completada | Capa Application |
| Fase 4 | ✅ Completada | Capa Infrastructure |
| Fase 5 | ✅ Completada | Capa API |
| Fase 6 | ✅ Completada | Seed de Datos |
| Fase 7 | ✅ Completada | Testing |
| Fase 8 | ✅ Completada | Documentación |

---

## Notas Importantes

1. **Connection String MongoDB:** Configurado en appsettings.json
2. **OMDb API Key:** b279cce3
3. **Código:** En inglés
4. **Documentación/Comentarios:** En español
5. **Soft Delete:** Solo películas con IsDeleted=false se retornan en consultas

---

## Historial de Cambios

| Fecha | Fase | Acción |
|-------|------|--------|
| 2026-01-02 | Fase 1 | Completada - Estructura del proyecto creada |
| 2026-01-02 | Fase 2 | Completada - Capa Domain implementada |
| 2026-01-02 | Fase 3 | Completada - Capa Application con CQRS y MediatR |
| 2026-01-02 | Fase 4 | Completada - Capa Infrastructure con MongoDB, JWT, OMDb, Cache |
| 2026-01-02 | Fase 5 | Completada - Capa API con Minimal APIs, autenticación y Swagger |
| 2026-01-02 | Fase 6 | Completada - Seeding con usuario Admin y 15 películas de OMDb |
| 2026-01-02 | Fase 7 | Completada - 64 tests unitarios e integración |
| 2026-01-02 | Fase 8 | Completada - Swagger, Health Checks, README actualizado |
