# 🧪 Guía de Testing Unitario con NUnit

Esta guía explica cómo configurar y ejecutar tests unitarios en el proyecto usando **NUnit**.

---

## 📦 Paquetes Instalados

El proyecto de tests incluye los siguientes paquetes NuGet:

- **NUnit** (v4.2.2) - Framework de testing
- **NUnit3TestAdapter** (v4.6.0) - Adaptador para Visual Studio y dotnet test
- **Moq** (v4.20.72) - Framework para crear mocks y stubs
- **FluentAssertions** (v8.8.0) - Biblioteca para assertions más legibles
- **Microsoft.NET.Test.Sdk** (v17.12.0) - SDK de testing de .NET
- **coverlet.collector** (v6.0.2) - Para cobertura de código

---

## 📁 Estructura del Proyecto de Tests

```
Tests/
└── UnitTests/
    ├── MillionBack.Tests.csproj                  # Proyecto de tests
    ├── Controllers/
    │   └── PropertiesControllerTests.cs          # Tests de endpoints (10 tests)
    ├── Services/
    │   ├── PropertyServiceTests.cs               # Tests básicos del servicio (4 tests)
    │   └── PropertyServiceAdditionalTests.cs     # Tests de caché, mapeo y errores (9 tests)
    ├── Validators/
    │   └── PropertyFilterDtoValidatorTests.cs    # Tests de validación (8 tests)
    └── Middleware/
        └── ExceptionHandlerMiddlewareTests.cs    # Tests de manejo de excepciones (7 tests)
```

**Total: 38 tests unitarios** ✅

---

## 🚀 Ejecutar Tests

### Opción 1: Desde la línea de comandos (Recomendado)

```bash
# Ejecutar todos los tests
dotnet test Tests/UnitTests/MillionBack.Tests.csproj

# Ejecutar tests con más detalles
dotnet test Tests/UnitTests/MillionBack.Tests.csproj --verbosity normal

# Ejecutar tests y generar reporte de cobertura
dotnet test Tests/UnitTests/MillionBack.Tests.csproj --collect:"XPlat Code Coverage"

# Ejecutar un test específico por nombre
dotnet test Tests/UnitTests/MillionBack.Tests.csproj --filter "FullyQualifiedName~PropertyServiceTests"
```

### Opción 2: Desde Visual Studio

1. Abre el **Test Explorer** (Test → Test Explorer)
2. Los tests deberían aparecer automáticamente
3. Haz clic en "Run All" o ejecuta tests individuales

### Opción 3: Desde Visual Studio Code

1. Instala la extensión **.NET Core Test Explorer**
2. Abre el panel de tests (icono de probeta en la barra lateral)
3. Ejecuta los tests desde ahí

---

## 📊 Cobertura de Tests

El proyecto cuenta con **38 tests unitarios** distribuidos en las siguientes categorías:

| Categoría | Archivo | Tests | Descripción |
|-----------|---------|-------|-------------|
| **Controllers** | `PropertiesControllerTests.cs` | 10 | Tests de endpoints HTTP (GET, búsqueda, errores) |
| **Services** | `PropertyServiceTests.cs` | 4 | Tests básicos de lógica de negocio |
| **Services** | `PropertyServiceAdditionalTests.cs` | 9 | Tests de caché, mapeo de datos y errores |
| **Validators** | `PropertyFilterDtoValidatorTests.cs` | 8 | Tests de validación de filtros |
| **Middleware** | `ExceptionHandlerMiddlewareTests.cs` | 7 | Tests de manejo de excepciones |
| **TOTAL** | - | **38** | **100% de tests pasando** ✅ |

### Desglose por Funcionalidad

#### Controllers (10 tests)
- ✅ `GetAllProperties` - 3 tests (caso exitoso, valores límite, errores)
- ✅ `GetPropertyById` - 3 tests (existe, no existe, errores)
- ✅ `SearchProperties` - 3 tests (con filtros, sin filtros, errores)

#### Services (13 tests)
- ✅ `PropertyServiceTests` - 4 tests (obtención, búsqueda, detalle)
- ✅ `PropertyServiceAdditionalTests` - 9 tests (caché, mapeo Owner/Images/Traces, errores)

#### Validators (8 tests)
- ✅ Validación de precios negativos
- ✅ Validación de rangos de precios
- ✅ Casos válidos e inválidos

#### Middleware (7 tests)
- ✅ Manejo de diferentes tipos de excepciones
- ✅ Códigos de estado HTTP correctos
- ✅ Logging de errores

---

## 📝 Ejemplos de Tests

### Test de Controller

```csharp
[Test]
public async Task GetAllProperties_WhenCalled_ReturnsOkWithPagedResult()
{
    // Arrange
    var page = 1;
    var pageSize = 20;
    var pagedResult = new PagedResult<PropertyDto>
    {
        Items = new List<PropertyDto>
        {
            new PropertyDto { IdProperty = "PROP001", Name = "Casa 1", Price = 100000 },
            new PropertyDto { IdProperty = "PROP002", Name = "Casa 2", Price = 200000 }
        },
        Total = 2,
        Page = page,
        PageSize = pageSize
    };

    _propertyServiceMock
        .Setup(x => x.GetAllPropertiesPagedAsync(page, pageSize))
        .ReturnsAsync(pagedResult);

    // Act
    var result = await _controller.GetAllProperties(page, pageSize);

    // Assert
    result.Result.Should().BeOfType<OkObjectResult>();
    var okResult = result.Result as OkObjectResult;
    okResult!.StatusCode.Should().Be(200);
    
    var response = okResult.Value as ApiResponse<PagedResult<PropertyDto>>;
    response.Should().NotBeNull();
    response!.Success.Should().BeTrue();
    response.Data!.Items.Should().HaveCount(2);
}
```

### Test de Servicio con Mocks

```csharp
[Test]
public async Task GetPropertyByIdAsync_WhenPropertyExists_ReturnsPropertyDetailDto()
{
    // Arrange
    var propertyId = "507f1f77bcf86cd799439011";
    var property = new Property 
    { 
        Id = propertyId, 
        IdProperty = "PROP001",
        Name = "Casa de Prueba",
        Address = "Calle Test 123",
        Price = 250000,
        IdOwner = "OWNER001"
    };

    var propertyDetailDto = new PropertyDetailDto
    {
        IdProperty = property.IdProperty,
        Name = property.Name,
        Address = property.Address,
        Price = property.Price
    };

    _propertyRepositoryMock
        .Setup(x => x.GetByIdAsync(propertyId))
        .ReturnsAsync(property);

    _mapperMock
        .Setup(x => x.Map<PropertyDetailDto>(property))
        .Returns(propertyDetailDto);

    _ownerRepositoryMock
        .Setup(x => x.GetOwnersByIdsAsync(It.IsAny<List<string>>()))
        .ReturnsAsync(new Dictionary<string, Owner>());

    _propertyImageRepositoryMock
        .Setup(x => x.GetEnabledImagesByPropertyIdsAsync(It.IsAny<List<string>>()))
        .ReturnsAsync(new Dictionary<string, List<PropertyImage>>());

    _propertyTraceRepositoryMock
        .Setup(x => x.GetTracesByPropertyIdsAsync(It.IsAny<List<string>>()))
        .ReturnsAsync(new Dictionary<string, List<PropertyTrace>>());

    // Act
    var result = await _propertyService.GetPropertyByIdAsync(propertyId);

    // Assert
    result.Should().NotBeNull();
    result!.Name.Should().Be("Casa de Prueba");
    result.Address.Should().Be("Calle Test 123");
    result.Price.Should().Be(250000);
    
    _propertyRepositoryMock.Verify(x => x.GetByIdAsync(propertyId), Times.Once);
}
```

### Test de Caché

```csharp
[Test]
public async Task GetAllPropertiesPagedAsync_WhenResultIsCached_ReturnsCachedResult()
{
    // Arrange
    var page = 1;
    var pageSize = 20;
    var cachedResult = new PagedResult<PropertyDto>
    {
        Items = new List<PropertyDto> 
        { 
            new PropertyDto { IdProperty = "PROP001", Name = "Cached" } 
        },
        Total = 1,
        Page = page,
        PageSize = pageSize
    };

    var cacheKey = $"properties_page_{page}_size_{pageSize}";
    _memoryCache.Set(cacheKey, cachedResult, TimeSpan.FromMinutes(5));

    // Act
    var result = await _propertyService.GetAllPropertiesPagedAsync(page, pageSize);

    // Assert
    result.Should().NotBeNull();
    result.Items.Should().HaveCount(1);
    result.Items.First().Name.Should().Be("Cached");
    
    // Verificar que NO se llamó al repositorio
    _propertyRepositoryMock.Verify(
        x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>()), 
        Times.Never);
}
```

### Test de Mapeo de Datos

```csharp
[Test]
public async Task GetPropertyByIdAsync_WhenPropertyHasOwner_IncludesOwnerInDto()
{
    // Arrange
    var propertyId = "507f1f77bcf86cd799439011";
    var property = new Property
    {
        Id = propertyId,
        IdProperty = "PROP001",
        Name = "Casa de Prueba",
        IdOwner = "OWNER001"
    };

    var owner = new Owner
    {
        Id = "OWNER001",
        IdOwner = "OWNER001",
        Name = "Juan Pérez",
        Address = "Calle Owner 123",
        Photo = "photo.jpg",
        Birthday = new DateTime(1980, 1, 1)
    };

    _propertyRepositoryMock
        .Setup(x => x.GetByIdAsync(propertyId))
        .ReturnsAsync(property);

    _mapperMock
        .Setup(x => x.Map<PropertyDetailDto>(property))
        .Returns(new PropertyDetailDto { Name = property.Name });

    _mapperMock
        .Setup(x => x.Map<OwnerDto>(owner))
        .Returns(new OwnerDto
        {
            Name = owner.Name,
            Address = owner.Address,
            Photo = owner.Photo,
            Birthday = owner.Birthday
        });

    _ownerRepositoryMock
        .Setup(x => x.GetOwnersByIdsAsync(It.IsAny<List<string>>()))
        .ReturnsAsync(new Dictionary<string, Owner> { { "OWNER001", owner } });

    // Act
    var result = await _propertyService.GetPropertyByIdAsync(propertyId);

    // Assert
    result.Should().NotBeNull();
    result!.Owner.Should().NotBeNull();
    result.Owner!.Name.Should().Be("Juan Pérez");
    result.Owner.Address.Should().Be("Calle Owner 123");
}
```

### Test de Validador

```csharp
[Test]
public void Validate_WhenMinPriceIsNegative_ShouldFail()
{
    // Arrange
    var filter = new PropertyFilterDto { MinPrice = -100 };

    // Act
    var result = _validator.Validate(filter);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.PropertyName == "MinPrice");
}
```

### Test de Middleware

```csharp
[Test]
public async Task InvokeAsync_WhenGenericException_ReturnsInternalServerError()
{
    // Arrange
    var exception = new Exception("Generic error");
    _nextMock
        .Setup(x => x(It.IsAny<HttpContext>()))
        .ThrowsAsync(exception);

    // Act
    await _middleware.InvokeAsync(_httpContext);

    // Assert
    _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    _httpContext.Response.ContentType.Should().Be("application/json");

    var responseBody = await GetResponseBody();
    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(
        responseBody, 
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
    );

    errorResponse.Should().NotBeNull();
    errorResponse!.StatusCode.Should().Be(500);
    errorResponse.Message.Should().Be("Error interno del servidor");
}
```

### Test de Manejo de Errores

```csharp
[Test]
public async Task GetAllProperties_WhenServiceThrowsException_ReturnsInternalServerError()
{
    // Arrange
    _propertyServiceMock
        .Setup(x => x.GetAllPropertiesPagedAsync(It.IsAny<int>(), It.IsAny<int>()))
        .ThrowsAsync(new Exception("Database error"));

    // Act
    var result = await _controller.GetAllProperties(1, 20);

    // Assert
    result.Result.Should().BeOfType<ObjectResult>();
    var objectResult = result.Result as ObjectResult;
    objectResult!.StatusCode.Should().Be(500);

    var response = objectResult.Value as ApiResponse<object>;
    response.Should().NotBeNull();
    response!.Success.Should().BeFalse();
    response.Message.Should().Contain("Error al obtener las propiedades");
}
```

---

## 🎯 Convenciones de Naming

### Nombres de Tests

Seguimos el patrón: `[Método]_[Condición]_[ResultadoEsperado]`

Ejemplos:
- `GetPropertyByIdAsync_WhenPropertyExists_ReturnsPropertyDetailDto`
- `Validate_WhenMinPriceIsNegative_ShouldFail`
- `SearchPropertiesPagedAsync_WithFilters_ReturnsFilteredResults`
- `GetAllProperties_WhenServiceThrowsException_ReturnsInternalServerError`
- `GetAllPropertiesPagedAsync_WhenResultIsCached_ReturnsCachedResult`
- `InvokeAsync_WhenGenericException_ReturnsInternalServerError`

### Estructura AAA (Arrange-Act-Assert)

Todos los tests siguen el patrón AAA:

1. **Arrange**: Configurar datos de prueba y mocks
2. **Act**: Ejecutar el método bajo prueba
3. **Assert**: Verificar el resultado

---

## 🔧 Configuración de Mocks con Moq

### Ejemplo Básico

```csharp
// Crear mock
var mockRepository = new Mock<IPropertyRepository>();

// Configurar comportamiento
mockRepository
    .Setup(x => x.GetByIdAsync("123"))
    .ReturnsAsync(new Property { Id = "123" });

// Verificar que se llamó
mockRepository.Verify(x => x.GetByIdAsync("123"), Times.Once);
```

### Configurar con It.Is<T>

```csharp
// Usar It.Is para condiciones más complejas
_propertyServiceMock
    .Setup(x => x.SearchPropertiesPagedAsync(
        It.Is<PropertyFilterDto>(f => f.Name == "Casa" && f.MinPrice == 100000),
        1,
        20))
    .ReturnsAsync(pagedResult);
```

### Configurar Múltiples Llamadas

```csharp
mockRepository
    .SetupSequence(x => x.GetAllAsync())
    .ReturnsAsync(new List<Property> { property1 })
    .ReturnsAsync(new List<Property> { property2 });
```

### Mockear Múltiples Dependencias

```csharp
[SetUp]
public void SetUp()
{
    _propertyRepositoryMock = new Mock<IPropertyRepository>();
    _ownerRepositoryMock = new Mock<IOwnerRepository>();
    _propertyImageRepositoryMock = new Mock<IPropertyImageRepository>();
    _propertyTraceRepositoryMock = new Mock<IPropertyTraceRepository>();
    _mapperMock = new Mock<IMapper>();
    _memoryCache = new MemoryCache(new MemoryCacheOptions());

    _propertyService = new PropertyService(
        _propertyRepositoryMock.Object,
        _ownerRepositoryMock.Object,
        _propertyImageRepositoryMock.Object,
        _propertyTraceRepositoryMock.Object,
        _mapperMock.Object,
        _memoryCache
    );
}
```

### Mockear Excepciones

```csharp
_propertyRepositoryMock
    .Setup(x => x.GetAllAsync())
    .ThrowsAsync(new Exception("Database connection failed"));

// En el test
await _propertyService
    .Invoking(x => x.GetAllPropertiesAsync())
    .Should()
    .ThrowAsync<Exception>()
    .WithMessage("Database connection failed");
```

---

## ✅ Assertions con FluentAssertions

### Ventajas sobre Assert tradicional

```csharp
// ❌ Tradicional (menos legible)
Assert.AreEqual(expected, actual);
Assert.IsNotNull(result);
Assert.IsTrue(result.Count > 0);

// ✅ FluentAssertions (más legible)
actual.Should().Be(expected);
result.Should().NotBeNull();
result.Count.Should().BeGreaterThan(0);
```

### Ejemplos Comunes

```csharp
// Igualdad
result.Should().Be(expected);
result.Should().NotBeNull();
result.Should().NotBeNull().And.BeOfType<PropertyDetailDto>();

// Colecciones
list.Should().HaveCount(2);
list.Should().Contain(item);
list.Should().BeEmpty();
list.Should().HaveCountGreaterThan(0);

// Tipos y objetos
result.Result.Should().BeOfType<OkObjectResult>();
var okResult = result.Result as OkObjectResult;
okResult!.StatusCode.Should().Be(200);

// Propiedades anidadas
response.Data!.Items.Should().HaveCount(2);
response.Data.Total.Should().Be(2);
response.Success.Should().BeTrue();

// Excepciones
action.Should().Throw<ArgumentException>();
action.Should().Throw<ArgumentException>()
    .WithMessage("Mensaje esperado");

// Verificación de llamadas a mocks
mock.Verify(x => x.Method(), Times.Once);
mock.Verify(x => x.Method(It.IsAny<string>()), Times.Never);
```

---

## 📊 Cobertura de Código

### Generar Reporte de Cobertura

```bash
# Instalar coverlet si no está instalado
dotnet tool install -g coverlet.console

# Ejecutar tests con cobertura
dotnet test Tests/UnitTests/MillionBack.Tests.csproj \
    --collect:"XPlat Code Coverage" \
    --results-directory:"./TestResults"

# Ver reporte (requiere reportgenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator \
    -reports:"./TestResults/**/coverage.cobertura.xml" \
    -targetdir:"./TestResults/Coverage" \
    -reporttypes:Html
```

---

## 🎓 Mejores Prácticas

### 1. Un Test = Una Aserción Principal

```csharp
// ✅ Bueno
[Test]
public void Test_ShouldReturnCorrectName()
{
    var result = service.GetName();
    result.Should().Be("Expected Name");
}

// ❌ Evitar (múltiples aserciones en un test)
[Test]
public void Test_MultipleAssertions()
{
    var result = service.GetData();
    result.Name.Should().Be("Name");
    result.Age.Should().Be(25);
    result.Address.Should().Be("Address");
}
```

### 2. Tests Independientes

Cada test debe poder ejecutarse de forma independiente sin depender de otros tests.

### 3. Usar SetUp y TearDown

```csharp
[SetUp]
public void SetUp()
{
    // Configuración común para todos los tests
    _mockRepository = new Mock<IPropertyRepository>();
}

[TearDown]
public void TearDown()
{
    // Limpieza después de cada test
    _memoryCache.Dispose();
}
```

### 4. Nombres Descriptivos

Los nombres de los tests deben ser descriptivos y explicar qué están probando.

---

## 🐛 Debugging Tests

### En Visual Studio

1. Coloca un breakpoint en el test
2. Click derecho → "Debug Test"

### En Visual Studio Code

1. Crea un `launch.json`:
```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Test",
            "type": "coreclr",
            "request": "launch",
            "program": "${workspaceFolder}/Tests/UnitTests/bin/Debug/net9.0/MillionBack.Tests.dll",
            "args": [],
            "cwd": "${workspaceFolder}",
            "stopAtEntry": false
        }
    ]
}
```

---

## 📚 Recursos Adicionales

- [Documentación oficial de NUnit](https://docs.nunit.org/)
- [Documentación de Moq](https://github.com/moq/moq4)
- [Documentación de FluentAssertions](https://fluentassertions.com/)
- [Guía de Testing en .NET](https://docs.microsoft.com/en-us/dotnet/core/testing/)

---

## ❓ Preguntas Frecuentes

### ¿Cómo agrego un nuevo test?

1. Crea un nuevo archivo en `Tests/UnitTests/[Carpeta]/[Nombre]Tests.cs`
2. Hereda de `[TestFixture]`
3. Agrega métodos con el atributo `[Test]`

### ¿Cómo mockeo una dependencia?

Usa `Moq` para crear mocks:

```csharp
var mock = new Mock<IDependency>();
mock.Setup(x => x.Method()).ReturnsAsync(result);
```

### ¿Cómo verifico que un método se llamó?

```csharp
mock.Verify(x => x.Method(), Times.Once);
mock.Verify(x => x.Method(It.IsAny<string>()), Times.Once);
```

---

## 📋 Lista Completa de Tests Implementados

### PropertiesControllerTests (10 tests)

1. ✅ `GetAllProperties_WhenCalled_ReturnsOkWithPagedResult`
2. ✅ `GetAllProperties_WhenPageIsZero_UsesDefaultPage`
3. ✅ `GetAllProperties_WhenPageSizeExceedsMax_LimitsToMaxPageSize`
4. ✅ `GetAllProperties_WhenServiceThrowsException_ReturnsInternalServerError`
5. ✅ `GetPropertyById_WhenPropertyExists_ReturnsOkWithPropertyDetail`
6. ✅ `GetPropertyById_WhenPropertyDoesNotExist_ReturnsNotFound`
7. ✅ `GetPropertyById_WhenServiceThrowsException_ReturnsInternalServerError`
8. ✅ `SearchProperties_WhenCalledWithFilters_ReturnsOkWithFilteredResults`
9. ✅ `SearchProperties_WhenNoFiltersProvided_ReturnsOkWithAllResults`
10. ✅ `SearchProperties_WhenServiceThrowsException_ReturnsInternalServerError`

### PropertyServiceTests (4 tests)

1. ✅ `GetPropertyByIdAsync_WhenPropertyExists_ReturnsPropertyDetailDto`
2. ✅ `GetPropertyByIdAsync_WhenPropertyDoesNotExist_ReturnsNull`
3. ✅ `GetAllPropertiesPagedAsync_WhenCalled_ReturnsPagedResult`
4. ✅ `SearchPropertiesPagedAsync_WithFilters_ReturnsFilteredResults`

### PropertyServiceAdditionalTests (9 tests)

1. ✅ `GetAllPropertiesPagedAsync_WhenResultIsCached_ReturnsCachedResult`
2. ✅ `GetAllPropertiesPagedAsync_WhenNotCached_CallsRepositoryAndCachesResult`
3. ✅ `SearchPropertiesPagedAsync_WhenResultIsCached_ReturnsCachedResult`
4. ✅ `SearchPropertiesPagedAsync_WhenResultIsEmpty_DoesNotCache`
5. ✅ `GetPropertyByIdAsync_WhenPropertyHasOwner_IncludesOwnerInDto`
6. ✅ `GetPropertyByIdAsync_WhenPropertyHasImages_IncludesImagesInDto`
7. ✅ `GetPropertyByIdAsync_WhenPropertyHasTraces_IncludesTracesInDto`
8. ✅ `GetAllPropertiesPagedAsync_WhenRepositoryThrowsException_PropagatesException`
9. ✅ `MapPropertiesToDtoAsync_WhenPropertiesHaveOwnersAndImages_MapsCorrectly`

### PropertyFilterDtoValidatorTests (8 tests)

1. ✅ `Validate_WhenMinPriceIsNegative_ShouldFail`
2. ✅ `Validate_WhenMaxPriceIsNegative_ShouldFail`
3. ✅ `Validate_WhenMaxPriceIsLessThanMinPrice_ShouldFail`
4. ✅ `Validate_WhenMinPriceIsZero_ShouldPass`
5. ✅ `Validate_WhenMaxPriceIsGreaterThanMinPrice_ShouldPass`
6. ✅ `Validate_WhenMinPriceEqualsMaxPrice_ShouldPass`
7. ✅ `Validate_WhenNoPriceFilters_ShouldPass`
8. ✅ `Validate_WhenAllFiltersAreValid_ShouldPass`

### ExceptionHandlerMiddlewareTests (7 tests)

1. ✅ `InvokeAsync_WhenNoException_DoesNotHandleException`
2. ✅ `InvokeAsync_WhenGenericException_ReturnsInternalServerError`
3. ✅ `InvokeAsync_WhenArgumentNullException_ReturnsBadRequest`
4. ✅ `InvokeAsync_WhenArgumentException_ReturnsBadRequestWithMessage`
5. ✅ `InvokeAsync_WhenKeyNotFoundException_ReturnsNotFound`
6. ✅ `InvokeAsync_WhenExceptionOccurs_LogsError`

---

## 🎯 Estrategias de Testing Aplicadas

### 1. Testing de Controllers

**Objetivo:** Verificar que los endpoints HTTP responden correctamente.

**Características:**
- ✅ Tests de casos exitosos (200 OK)
- ✅ Tests de casos de error (404, 500)
- ✅ Tests de validación de parámetros
- ✅ Tests de códigos de estado HTTP
- ✅ Tests de estructura de respuestas

### 2. Testing de Services

**Objetivo:** Verificar la lógica de negocio y mapeo de datos.

**Características:**
- ✅ Tests de obtención de datos
- ✅ Tests de búsqueda y filtrado
- ✅ Tests de caché (hits y misses)
- ✅ Tests de mapeo de datos relacionados (Owner, Images, Traces)
- ✅ Tests de manejo de errores

### 3. Testing de Validators

**Objetivo:** Verificar que las validaciones funcionan correctamente.

**Características:**
- ✅ Tests de casos inválidos
- ✅ Tests de casos válidos
- ✅ Tests de rangos de valores
- ✅ Tests de mensajes de error

### 4. Testing de Middleware

**Objetivo:** Verificar el manejo global de excepciones.

**Características:**
- ✅ Tests de diferentes tipos de excepciones
- ✅ Tests de códigos de estado HTTP
- ✅ Tests de logging
- ✅ Tests de formato de respuesta

---

## 🔍 Testing de Caché

### Ejemplo: Test de Cache Hit

```csharp
[Test]
public async Task GetAllPropertiesPagedAsync_WhenResultIsCached_ReturnsCachedResult()
{
    // Arrange
    var page = 1;
    var pageSize = 20;
    var cachedResult = new PagedResult<PropertyDto> { /* ... */ };
    
    var cacheKey = $"properties_page_{page}_size_{pageSize}";
    _memoryCache.Set(cacheKey, cachedResult, TimeSpan.FromMinutes(5));

    // Act
    var result = await _propertyService.GetAllPropertiesPagedAsync(page, pageSize);

    // Assert
    result.Should().NotBeNull();
    result.Items.Should().HaveCount(1);
    
    // Verificar que NO se llamó al repositorio
    _propertyRepositoryMock.Verify(
        x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>()), 
        Times.Never);
}
```

### Ejemplo: Test de Cache Miss

```csharp
[Test]
public async Task GetAllPropertiesPagedAsync_WhenNotCached_CallsRepositoryAndCachesResult()
{
    // Arrange
    var page = 1;
    var pageSize = 20;
    var properties = new List<Property> { /* ... */ };

    _propertyRepositoryMock
        .Setup(x => x.GetPagedAsync(page, pageSize))
        .ReturnsAsync((properties, 1L));

    // Act
    var result = await _propertyService.GetAllPropertiesPagedAsync(page, pageSize);

    // Assert
    _propertyRepositoryMock.Verify(x => x.GetPagedAsync(page, pageSize), Times.Once);

    // Verificar que se cacheó el resultado
    var cacheKey = $"properties_page_{page}_size_{pageSize}";
    _memoryCache.TryGetValue(cacheKey, out _).Should().BeTrue();
}
```

---

## 🧩 Testing de Mapeo de Datos

### Ejemplo: Test de Mapeo Completo

```csharp
[Test]
public async Task GetPropertyByIdAsync_WhenPropertyHasOwnerAndImages_IncludesAllInDto()
{
    // Arrange
    var property = new Property { Id = "1", IdOwner = "OWNER001" };
    var owner = new Owner { Id = "OWNER001", Name = "Juan Pérez" };
    var images = new List<PropertyImage>
    {
        new PropertyImage { IdProperty = "1", File = "image1.jpg", Enabled = true },
        new PropertyImage { IdProperty = "1", File = "image2.jpg", Enabled = true }
    };

    // Configurar mocks...

    // Act
    var result = await _propertyService.GetPropertyByIdAsync("1");

    // Assert
    result!.Owner.Should().NotBeNull();
    result.Owner!.Name.Should().Be("Juan Pérez");
    result.Images.Should().HaveCount(2);
    result.Images.Should().Contain("image1.jpg");
    result.Images.Should().Contain("image2.jpg");
}
```

---

## ✅ Checklist para Nuevos Tests

- [ ] El test tiene un nombre descriptivo siguiendo el patrón `[Método]_[Condición]_[ResultadoEsperado]`
- [ ] Sigue el patrón AAA (Arrange-Act-Assert)
- [ ] Usa mocks para dependencias externas
- [ ] Tiene assertions claras con FluentAssertions
- [ ] Es independiente de otros tests
- [ ] Prueba un comportamiento específico
- [ ] El código está limpio y bien comentado
- [ ] Usa `SetUp` y `TearDown` si es necesario
- [ ] Verifica que los mocks se llamaron correctamente
- [ ] Cubre casos edge (null, vacío, errores)

---

## 🎯 Resultados de Ejecución

```bash
Test Run Successful.
Total tests: 38
     Passed: 38
 Total time: 0.6454 Seconds
```

**Estado:** ✅ **Todos los tests pasan correctamente**

---

## 📚 Referencias en el Código

### Archivos de Tests

- `Tests/UnitTests/Controllers/PropertiesControllerTests.cs` - Tests de endpoints
- `Tests/UnitTests/Services/PropertyServiceTests.cs` - Tests básicos de servicios
- `Tests/UnitTests/Services/PropertyServiceAdditionalTests.cs` - Tests avanzados de servicios
- `Tests/UnitTests/Validators/PropertyFilterDtoValidatorTests.cs` - Tests de validación
- `Tests/UnitTests/Middleware/ExceptionHandlerMiddlewareTests.cs` - Tests de middleware

### Archivos de Implementación

- `src/API/Controllers/PropertiesController.cs` - Controlador bajo prueba
- `src/Core/Application/Services/PropertyService.cs` - Servicio bajo prueba
- `src/Core/Application/Validators/PropertyFilterDtoValidator.cs` - Validador bajo prueba
- `src/API/Middleware/ExceptionHandlerMiddleware.cs` - Middleware bajo prueba

---

¡Feliz testing! 🎉

**Desarrollado por:** Juan Fernando Álvarez Gallego  
**Prueba Técnica - MillionBack API**

