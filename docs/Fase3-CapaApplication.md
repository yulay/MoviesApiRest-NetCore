# Fase 3: Capa Application

**Estado:** ✅ Completada
**Fecha:** 2026-01-02

---

## Resumen

Se implementó la capa de aplicación con MediatR para CQRS, FluentValidation para validaciones, y los DTOs necesarios para la comunicación entre capas.

---

## 1. Estructura de Carpetas Creada

```
src/MovieManager.Application/
├── Behaviors/
│   ├── LoggingBehavior.cs
│   └── ValidationBehavior.cs
├── DTOs/
│   ├── Auth/
│   │   ├── LoginDto.cs
│   │   ├── RefreshTokenDto.cs
│   │   ├── RegisterDto.cs
│   │   └── TokenDto.cs
│   ├── Common/
│   │   ├── PaginatedResultDto.cs
│   │   └── ResultDto.cs
│   ├── Integration/
│   │   └── OmdbSearchResultDto.cs
│   ├── Movies/
│   │   ├── MovieCreateDto.cs
│   │   ├── MovieDto.cs
│   │   └── MovieUpdateDto.cs
│   ├── Statistics/
│   │   ├── DirectorStatDto.cs
│   │   ├── GenreStatDto.cs
│   │   └── YearStatDto.cs
│   └── Users/
│       └── UserDto.cs
├── Features/
│   ├── Auth/
│   │   └── Commands/
│   │       ├── LoginCommand.cs
│   │       ├── RefreshTokenCommand.cs
│   │       └── RegisterCommand.cs
│   ├── Integration/
│   │   ├── Commands/
│   │   │   ├── ImportMovieCommand.cs
│   │   │   └── SyncMovieCommand.cs
│   │   └── Queries/
│   │       └── SearchOmdbQuery.cs
│   ├── Movies/
│   │   ├── Commands/
│   │   │   ├── CreateMovieCommand.cs
│   │   │   ├── DeleteMovieCommand.cs
│   │   │   └── UpdateMovieCommand.cs
│   │   └── Queries/
│   │       ├── GetMovieByIdQuery.cs
│   │       ├── GetMoviesByDirectorQuery.cs
│   │       ├── GetMoviesByGenreQuery.cs
│   │       ├── GetMoviesQuery.cs
│   │       ├── GetRandomMovieQuery.cs
│   │       ├── GetRecommendationsQuery.cs
│   │       └── SearchMoviesQuery.cs
│   └── Statistics/
│       └── Queries/
│           ├── GetGenreStatsQuery.cs
│           ├── GetTopDirectorsQuery.cs
│           ├── GetTotalMoviesQuery.cs
│           └── GetYearStatsQuery.cs
├── Interfaces/
│   ├── IJwtService.cs
│   └── IPasswordHasher.cs
├── Validators/
│   ├── LoginValidator.cs
│   ├── MovieCreateValidator.cs
│   ├── MovieUpdateValidator.cs
│   └── RegisterValidator.cs
└── DependencyInjection.cs
```

---

## 2. DTOs Creados

### Common
| DTO | Descripción |
|-----|-------------|
| ResultDto\<T\> | Respuesta genérica con Success, Data, Message, Errors |
| PaginatedResultDto\<T\> | Resultado paginado con Items, Page, PageSize, TotalCount |

### Auth
| DTO | Descripción |
|-----|-------------|
| RegisterDto | Email, Password, ConfirmPassword, FirstName, LastName |
| LoginDto | Email, Password |
| TokenDto | AccessToken, RefreshToken, ExpiresAt, Email, Role |
| RefreshTokenDto | RefreshToken |

### Movies
| DTO | Descripción |
|-----|-------------|
| MovieDto | Todos los campos de película para lectura |
| MovieCreateDto | Campos para crear película |
| MovieUpdateDto | Campos para actualizar película |

### Statistics
| DTO | Descripción |
|-----|-------------|
| GenreStatDto | Genre, Count |
| YearStatDto | Year, Count |
| DirectorStatDto | Director, Count |

---

## 3. Commands (Escritura)

### Movies
| Command | Descripción |
|---------|-------------|
| CreateMovieCommand | Crear nueva película |
| UpdateMovieCommand | Actualizar película existente |
| DeleteMovieCommand | Soft delete de película |

### Auth
| Command | Descripción |
|---------|-------------|
| RegisterCommand | Registrar nuevo usuario |
| LoginCommand | Iniciar sesión |
| RefreshTokenCommand | Renovar token JWT |

### Integration
| Command | Descripción |
|---------|-------------|
| ImportMovieCommand | Importar película desde OMDb |
| SyncMovieCommand | Sincronizar película con OMDb |

---

## 4. Queries (Lectura)

### Movies
| Query | Descripción |
|-------|-------------|
| GetMovieByIdQuery | Obtener película por ID |
| GetMoviesQuery | Listar películas con paginación |
| SearchMoviesQuery | Buscar por título |
| GetMoviesByGenreQuery | Filtrar por género |
| GetMoviesByDirectorQuery | Filtrar por director |
| GetRandomMovieQuery | Obtener película aleatoria |
| GetRecommendationsQuery | Recomendaciones por género |

### Statistics
| Query | Descripción |
|-------|-------------|
| GetTotalMoviesQuery | Total de películas |
| GetGenreStatsQuery | Conteo por género |
| GetYearStatsQuery | Distribución por año |
| GetTopDirectorsQuery | Top directores |

### Integration
| Query | Descripción |
|-------|-------------|
| SearchOmdbQuery | Buscar en OMDb |

---

## 5. Validators

| Validator | Validaciones |
|-----------|--------------|
| MovieCreateValidator | Título requerido, año válido, rating 0-10 |
| MovieUpdateValidator | Mismas que Create |
| RegisterValidator | Email válido, password segura, nombres requeridos |
| LoginValidator | Email y password requeridos |

---

## 6. Behaviors (Pipeline)

| Behavior | Función |
|----------|---------|
| ValidationBehavior | Ejecuta validaciones antes del handler |
| LoggingBehavior | Registra inicio, fin y tiempo de ejecución |

---

## 7. Interfaces de Servicios

| Interfaz | Métodos |
|----------|---------|
| IJwtService | GenerateAccessToken, GenerateRefreshToken, GetRefreshTokenExpiry |
| IPasswordHasher | HashPassword, VerifyPassword |

---

## 8. Dependency Injection

```csharp
services.AddApplication();
```

Registra:
- MediatR con handlers del assembly
- FluentValidation validators
- ValidationBehavior
- LoggingBehavior

---

## 9. Verificación

- ✅ Solución compila sin errores
- ✅ Patrón CQRS implementado con MediatR
- ✅ Validaciones con FluentValidation
- ✅ Pipeline behaviors configurados
- ✅ DTOs organizados por funcionalidad

---

## Próxima Fase

**Fase 4: Capa Infrastructure**
- Implementar repositorios con MongoDB
- Implementar OmdbService
- Implementar JwtService
- Implementar PasswordHasher
- Configurar cache con IMemoryCache
