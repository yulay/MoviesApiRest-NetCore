# Colección Postman - Movie Manager API

## Importar en Postman

1. Abrir Postman
2. Click en **Import** (esquina superior izquierda)
3. Arrastrar el archivo `MovieManager_API.postman_collection.json` o seleccionarlo
4. La colección aparecerá en el panel izquierdo

## Uso

### Configuración inicial

1. Ejecutar primero el request **"Login - Admin"** en la carpeta Autenticación
2. El token se guarda automáticamente en la variable `accessToken`
3. Todos los demás requests ya están configurados para usar este token

### Variables de la colección

| Variable | Descripción | Valor por defecto |
|----------|-------------|-------------------|
| `baseUrl` | URL base de la API | `https://localhost:5001` |
| `accessToken` | Token JWT (se actualiza automáticamente) | - |
| `refreshToken` | Refresh token (se actualiza automáticamente) | - |

### Cambiar la URL base

1. Click derecho en la colección → **Edit**
2. Ir a la pestaña **Variables**
3. Cambiar el valor de `baseUrl`

## Estructura de la colección

```
Movie Manager API/
├── Health Checks/
│   ├── Health - Estado Completo
│   ├── Health - Liveness
│   └── Health - Readiness
├── Autenticación/
│   ├── Registrar Usuario
│   ├── Login - Admin
│   └── Refresh Token
├── Películas - CRUD/
│   ├── Listar Películas
│   ├── Obtener Película por ID
│   ├── Crear Película
│   ├── Actualizar Película
│   └── Eliminar Película (Soft Delete)
├── Películas - Búsqueda y Filtros/
│   ├── Buscar por Título
│   ├── Filtrar por Género
│   ├── Filtrar por Director
│   ├── Película Aleatoria
│   └── Recomendaciones por Género
├── Estadísticas/
│   ├── Total de Películas
│   ├── Películas por Género
│   ├── Distribución por Año
│   └── Top Directores
└── Integración OMDb/
    ├── Buscar en OMDb
    ├── Importar desde OMDb
    └── Sincronizar con OMDb
```

## Roles y permisos

| Endpoint | User | Editor | Admin |
|----------|:----:|:------:|:-----:|
| Health Checks | ✓ | ✓ | ✓ |
| Autenticación | ✓ | ✓ | ✓ |
| GET Películas | ✓ | ✓ | ✓ |
| POST Películas | ✗ | ✗ | ✓ |
| PUT Películas | ✗ | ✓ | ✓ |
| DELETE Películas | ✗ | ✗ | ✓ |
| Estadísticas | ✗ | ✗ | ✓ |
| Integración OMDb | ✗ | ✗ | ✓ |

## Usuario de prueba

| Campo | Valor |
|-------|-------|
| Email | admin@moviemanager.com |
| Password | Admin123! |
| Rol | Admin |
