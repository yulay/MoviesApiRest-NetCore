# Fase 2: Capa Domain

**Estado:** ✅ Completada
**Fecha:** 2026-01-02

---

## Resumen

Se implementó la capa de dominio con las entidades principales, enumeraciones e interfaces de repositorios siguiendo los principios de Clean Architecture.

---

## 1. Estructura de Carpetas Creada

```
src/MovieManager.Domain/
├── Entities/
│   ├── Movie.cs
│   └── User.cs
├── Enums/
│   └── UserRole.cs
└── Interfaces/
    ├── IMovieRepository.cs
    ├── IUserRepository.cs
    └── IOmdbService.cs
```

---

## 2. Entidades

### Movie
| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| Id | string | Identificador único (ObjectId de MongoDB) |
| ExternalId | string | ID de la API externa (IMDb) |
| Title | string | Título de la película |
| Description | string | Sinopsis |
| Year | int | Año de estreno |
| Genres | List\<string\> | Lista de géneros |
| Director | string | Director |
| Actors | List\<string\> | Lista de actores |
| Rating | decimal | Calificación |
| Duration | int | Duración en minutos |
| Poster | string | URL del póster |
| CreatedAt | DateTime | Fecha de creación |
| UpdatedAt | DateTime? | Fecha de última actualización |
| IsDeleted | bool | Indicador de soft delete |
| Metadata | Dictionary\<string, object\> | Datos adicionales |

### User
| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| Id | string | Identificador único |
| Email | string | Correo electrónico (único) |
| PasswordHash | string | Hash de la contraseña |
| FirstName | string | Nombre |
| LastName | string | Apellido |
| Role | UserRole | Rol del usuario |
| CreatedAt | DateTime | Fecha de creación |
| UpdatedAt | DateTime? | Fecha de última actualización |
| RefreshToken | string? | Token de refresco JWT |
| RefreshTokenExpiry | DateTime? | Expiración del refresh token |
| IsActive | bool | Indica si el usuario está activo |

---

## 3. Enumeraciones

### UserRole
| Valor | Código | Permisos |
|-------|--------|----------|
| User | 0 | Solo lectura (GET) |
| Editor | 1 | Lectura + Edición (GET, PUT) |
| Admin | 2 | CRUD completo + Estadísticas + Importación |

---

## 4. Interfaces de Repositorios

### IMovieRepository
| Método | Descripción |
|--------|-------------|
| GetByIdAsync | Obtener película por ID |
| GetByExternalIdAsync | Obtener por ID externo (IMDb) |
| GetAllAsync | Listar con paginación |
| SearchByTitleAsync | Buscar por título |
| GetByGenreAsync | Filtrar por género |
| GetByDirectorAsync | Filtrar por director |
| GetRandomAsync | Obtener película aleatoria |
| GetRecommendationsByGenreAsync | Recomendaciones por género |
| GetTotalCountAsync | Total de películas |
| GetCountByGenreAsync | Estadísticas por género |
| GetCountByYearAsync | Estadísticas por año |
| GetTopDirectorsAsync | Top directores |
| CreateAsync | Crear película |
| UpdateAsync | Actualizar película |
| SoftDeleteAsync | Eliminar (soft delete) |
| ExistsAsync | Verificar existencia |

### IUserRepository
| Método | Descripción |
|--------|-------------|
| GetByIdAsync | Obtener usuario por ID |
| GetByEmailAsync | Obtener por email |
| GetByRefreshTokenAsync | Obtener por refresh token |
| CreateAsync | Crear usuario |
| UpdateAsync | Actualizar usuario |
| ExistsAsync | Verificar si existe el email |

---

## 5. Interfaz de Servicio Externo

### IOmdbService
| Método | Descripción |
|--------|-------------|
| SearchByTitleAsync | Buscar películas en OMDb por título |
| GetByImdbIdAsync | Obtener detalles por ID de IMDb |

### OmdbSearchResult
Clase auxiliar para resultados de búsqueda:
- ImdbId
- Title
- Year
- Type
- Poster

---

## 6. Verificación

- ✅ MovieManager.Domain compila sin errores
- ✅ Toda la solución compila sin errores
- ✅ Estructura de carpetas organizada
- ✅ Soft delete implementado con campo IsDeleted

---

## Próxima Fase

**Fase 3: Capa Application**
- Configurar MediatR
- Crear DTOs
- Crear Commands y Queries (CQRS)
- Crear Handlers
- Crear Validators con FluentValidation
