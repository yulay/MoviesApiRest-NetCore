# Fase 7: Testing

## Descripción
Implementación de pruebas automatizadas para garantizar la calidad del código, incluyendo unit tests e integration tests.

## Estructura de Tests

```
MovieManager.Tests/
├── Application/
│   ├── Handlers/
│   │   ├── CreateMovieCommandHandlerTests.cs
│   │   ├── DeleteMovieCommandHandlerTests.cs
│   │   ├── GetMovieByIdQueryHandlerTests.cs
│   │   ├── LoginCommandHandlerTests.cs
│   │   └── RegisterCommandHandlerTests.cs
│   └── Validators/
│       ├── MovieCreateValidatorTests.cs
│       └── RegisterValidatorTests.cs
├── Infrastructure/
│   ├── CacheServiceTests.cs
│   ├── JwtServiceTests.cs
│   └── PasswordHasherTests.cs
└── Integration/
    ├── CustomWebApplicationFactory.cs
    ├── AuthEndpointsTests.cs
    └── MovieEndpointsTests.cs
```

## Tecnologías Utilizadas

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| xUnit | 2.4.2 | Framework de testing |
| Moq | 4.20.72 | Mocking de dependencias |
| FluentAssertions | 8.8.0 | Assertions legibles |
| Microsoft.AspNetCore.Mvc.Testing | 8.0.11 | Integration tests |

## Unit Tests Implementados

### Application Layer - Handlers

#### CreateMovieCommandHandlerTests
- ✅ `Handle_ValidMovie_ReturnsSuccessResult`
- ✅ `Handle_SetsCreatedAtToUtcNow`

#### DeleteMovieCommandHandlerTests
- ✅ `Handle_ExistingMovie_ReturnsSuccessResult`
- ✅ `Handle_NonExistingMovie_ReturnsFailureResult`

#### GetMovieByIdQueryHandlerTests
- ✅ `Handle_ExistingMovie_ReturnsSuccessWithMovieDto`
- ✅ `Handle_NonExistingMovie_ReturnsFailure`
- ✅ `Handle_DeletedMovie_ReturnsFailure`

#### LoginCommandHandlerTests
- ✅ `Handle_ValidCredentials_ReturnsTokenDto`
- ✅ `Handle_InvalidEmail_ReturnsFailure`
- ✅ `Handle_InvalidPassword_ReturnsFailure`
- ✅ `Handle_InactiveUser_ReturnsFailure`

#### RegisterCommandHandlerTests
- ✅ `Handle_NewUser_ReturnsSuccessWithToken`
- ✅ `Handle_ExistingEmail_ReturnsFailure`
- ✅ `Handle_NewUser_SetsDefaultRoleAsUser`

### Application Layer - Validators

#### MovieCreateValidatorTests
- ✅ `Validate_ValidMovie_ReturnsNoErrors`
- ✅ `Validate_EmptyTitle_ReturnsError`
- ✅ `Validate_TitleTooLong_ReturnsError`
- ✅ `Validate_InvalidYear_ReturnsError` (Theory)
- ✅ `Validate_ValidYear_ReturnsNoErrors`
- ✅ `Validate_InvalidRating_ReturnsError` (Theory)
- ✅ `Validate_NegativeDuration_ReturnsError`

#### RegisterValidatorTests
- ✅ `Validate_ValidRegistration_ReturnsNoErrors`
- ✅ `Validate_InvalidEmail_ReturnsError` (Theory)
- ✅ `Validate_InvalidPassword_ReturnsError` (Theory)
- ✅ `Validate_EmptyFirstName_ReturnsError`
- ✅ `Validate_EmptyLastName_ReturnsError`
- ✅ `Validate_FirstNameTooLong_ReturnsError`

### Infrastructure Layer

#### PasswordHasherTests
- ✅ `HashPassword_ReturnsHashedString`
- ✅ `HashPassword_SamePasswordDifferentHashes`
- ✅ `VerifyPassword_CorrectPassword_ReturnsTrue`
- ✅ `VerifyPassword_IncorrectPassword_ReturnsFalse`
- ✅ `VerifyPassword_EmptyPassword_ReturnsFalse`

#### JwtServiceTests
- ✅ `GenerateAccessToken_ReturnsValidJwt`
- ✅ `GenerateAccessToken_ContainsUserClaims`
- ✅ `GenerateAccessToken_HasCorrectExpiration`
- ✅ `GenerateRefreshToken_ReturnsBase64String`
- ✅ `GenerateRefreshToken_ReturnsDifferentTokensEachTime`
- ✅ `GetRefreshTokenExpiry_ReturnsCorrectDate`

#### CacheServiceTests
- ✅ `Set_And_Get_ReturnsStoredValue`
- ✅ `Get_NonExistentKey_ReturnsDefault`
- ✅ `Set_ComplexObject_ReturnsCorrectly`
- ✅ `Remove_ExistingKey_RemovesValue`
- ✅ `TryGetValue_ExistingKey_ReturnsTrueAndValue`
- ✅ `TryGetValue_NonExistentKey_ReturnsFalse`
- ✅ `Set_WithExpiration_ExpiresCorrectly`

## Integration Tests

#### AuthEndpointsTests
- ✅ `Register_ValidData_ReturnsOk`
- ✅ `Register_ExistingEmail_ReturnsBadRequest`
- ✅ `Login_ValidCredentials_ReturnsToken`
- ✅ `Login_InvalidCredentials_ReturnsBadRequest`

#### MovieEndpointsTests
- ✅ `GetMovies_WithoutAuth_ReturnsUnauthorized`
- ✅ `GetMovies_WithAuth_ReturnsOk`
- ✅ `GetMovieById_ExistingMovie_ReturnsOk`
- ✅ `CreateMovie_AsAdmin_ReturnsCreated`
- ✅ `CreateMovie_AsUser_ReturnsForbidden`
- ✅ `DeleteMovie_AsAdmin_ReturnsOk`

## Patrones de Testing Utilizados

### Arrange-Act-Assert (AAA)
Cada test sigue el patrón:
1. **Arrange**: Configurar mocks y datos de prueba
2. **Act**: Ejecutar el método bajo prueba
3. **Assert**: Verificar resultados con FluentAssertions

### Test Isolation
- Cada test es independiente
- Uso de mocks para aislar dependencias
- No se depende de estado compartido

### Theory Tests
Para validadores con múltiples casos de prueba:
```csharp
[Theory]
[InlineData(1800)]
[InlineData(2100)]
public void Validate_InvalidYear_ReturnsError(int year)
```

## Ejecución de Tests

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar con detalle
dotnet test --verbosity normal

# Ejecutar con cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## Resultados

| Categoría | Tests | Estado |
|-----------|-------|--------|
| Unit Tests - Handlers | 14 | ✅ |
| Unit Tests - Validators | 14 | ✅ |
| Unit Tests - Infrastructure | 13 | ✅ |
| Integration Tests | 10 | ✅ |
| **Total** | **51+** | **Pasando** |

## Notas

- Los tests de infrastructure verifican servicios críticos (JWT, Password, Cache)
- Los integration tests usan `CustomWebApplicationFactory` con mocks
- FluentAssertions proporciona mensajes de error descriptivos
- Moq permite simular comportamiento de repositorios sin BD real
