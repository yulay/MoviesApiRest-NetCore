# Fase 6: Seed de Datos Iniciales

## Descripción
Implementación del servicio de seeding para cargar datos iniciales al iniciar la aplicación, incluyendo un usuario administrador y películas populares desde OMDb.

## Estructura

```
MovieManager.Infrastructure/
└── Services/
    └── DataSeederService.cs
```

## Componentes Implementados

### IDataSeederService / DataSeederService

Servicio que se ejecuta automáticamente al iniciar la aplicación y:

1. **Crea usuario Admin por defecto** (si no existe)
2. **Importa películas populares desde OMDb** (si la BD está vacía)

## Usuario Admin por Defecto

| Campo | Valor |
|-------|-------|
| Email | admin@moviemanager.com |
| Password | Admin123! |
| FirstName | Admin |
| LastName | System |
| Role | Admin |

## Películas Iniciales

Se importan 15 películas clásicas/populares desde OMDb:

| IMDb ID | Película |
|---------|----------|
| tt0111161 | The Shawshank Redemption |
| tt0068646 | The Godfather |
| tt0468569 | The Dark Knight |
| tt0071562 | The Godfather Part II |
| tt0050083 | 12 Angry Men |
| tt0108052 | Schindler's List |
| tt0167260 | The Lord of the Rings: The Return of the King |
| tt0110912 | Pulp Fiction |
| tt0060196 | The Good, the Bad and the Ugly |
| tt0137523 | Fight Club |
| tt0120737 | The Lord of the Rings: The Fellowship of the Ring |
| tt0109830 | Forrest Gump |
| tt1375666 | Inception |
| tt0080684 | Star Wars: Episode V - The Empire Strikes Back |
| tt0167261 | The Lord of the Rings: The Two Towers |

## Comportamiento del Seeder

### Usuario Admin
- **Si existe:** Se omite la creación, se registra en log
- **Si no existe:** Se crea con los datos por defecto

### Películas
- **Si hay películas en BD:** Se omite el seed completo
- **Si BD vacía:** Se importan las 15 películas desde OMDb
- **Delay de 100ms:** Entre cada importación para no saturar la API
- **Manejo de errores:** Continúa con la siguiente película si una falla

## Integración en Program.cs

```csharp
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDataSeederService>();
    await seeder.SeedAsync();
}
```

El seeder se ejecuta:
- **Después** de configurar todos los middlewares y endpoints
- **Antes** de `app.Run()`
- **Una sola vez** al iniciar la aplicación

## Logs Generados

```
[INF] Admin user created successfully: admin@moviemanager.com
[INF] Starting movie seed from OMDb API...
[DBG] Imported movie: The Shawshank Redemption (1994)
[DBG] Imported movie: The Godfather (1972)
...
[INF] Movie seed completed. Imported: 15, Failed: 0
```

Si ya existe data:
```
[INF] Admin user already exists, skipping creation
[INF] Database already contains 15 movies, skipping seed
```

## Registro de Dependencias

En `Infrastructure/DependencyInjection.cs`:
```csharp
services.AddScoped<IDataSeederService, DataSeederService>();
```

## Estado de Compilación

✅ Build succeeded - 0 Warnings, 0 Errors

## Notas

- El seeder es **idempotente**: puede ejecutarse múltiples veces sin duplicar datos
- Las películas se importan con todos sus metadatos desde OMDb
- El usuario Admin tiene rol Admin para acceso completo a la API
