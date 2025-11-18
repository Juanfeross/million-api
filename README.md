# 🏠 MillionBack API

> **Prueba Técnica** - API REST desarrollada en .NET 9.0 para la gestión de propiedades inmobiliarias. Proporciona endpoints para consultar, buscar y obtener detalles de propiedades, incluyendo información de propietarios e imágenes.
>
> **Desarrollado por:** Juan Fernando Álvarez Gallego

## 📋 Tabla de Contenidos

- [Características](#-características)
- [Tecnologías](#-tecnologías)
- [Requisitos](#-requisitos)
- [Instalación](#-instalación)
- [Configuración](#-configuración)
- [Uso](#-uso)
- [Documentación API](#-documentación-api)
- [Arquitectura](#-arquitectura)
- [Endpoints](#-endpoints)
- [Optimizaciones de Rendimiento](#-optimizaciones-de-rendimiento)
- [Scripts Útiles](#-scripts-útiles)
- [Testing](#-testing)

## ✨ Características

- ✅ **Clean Architecture**: Arquitectura limpia y escalable
- ✅ **Paginación**: Soporte completo para paginación de resultados
- ✅ **Búsqueda Avanzada**: Filtros por nombre, dirección y rango de precios
- ✅ **Historial de Propiedad**: El detalle incluye Owner completo, imágenes y `PropertyTraces`
- ✅ **Caché en Memoria**: Optimización de rendimiento con IMemoryCache
- ✅ **Documentación Swagger**: Documentación interactiva de la API
- ✅ **Validación**: Validación de entrada con FluentValidation
- ✅ **Manejo de Errores**: Manejo centralizado de excepciones
- ✅ **CORS**: Configurado para permitir peticiones desde cualquier origen

## 🛠 Tecnologías

- **.NET 9.0**: Framework principal
- **MongoDB**: Base de datos NoSQL
- **MongoDB.Driver**: Driver oficial de MongoDB para .NET
- **AutoMapper**: Mapeo de objetos
- **FluentValidation**: Validación de modelos
- **Swashbuckle (Swagger)**: Documentación de API
- **IMemoryCache**: Caché en memoria para optimización

## 📦 Requisitos

- .NET 9.0 SDK o superior
- MongoDB (local o MongoDB Atlas)
- Node.js y npm (para scripts de seeding)

## 🚀 Instalación

1. **Clonar el repositorio**
   ```bash
   git clone <repository-url>
   cd millionback
   ```

2. **Restaurar dependencias**
   ```bash
   dotnet restore
   ```

3. **Configurar variables de entorno**
   Crear un archivo `.env` en la raíz del proyecto:
   ```env
   MONGO_CONN=mongodb://localhost:27017
   MONGO_DB=millionback
   ```

   Para MongoDB Atlas:
   ```env
   MONGO_CONN=mongodb+srv://usuario:password@cluster.mongodb.net/?retryWrites=true&w=majority
   MONGO_DB=millionback
   ```

4. **Ejecutar la aplicación**
   ```bash
   dotnet run
   ```

5. **Acceder a Swagger**
   Abrir el navegador en: `http://localhost:5158` (o el puerto configurado)

## ⚙️ Configuración

### Variables de Entorno

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `MONGO_CONN` | Cadena de conexión a MongoDB | `mongodb://localhost:27017` |
| `MONGO_DB` | Nombre de la base de datos | `millionback` |

### Puertos

Por defecto, la aplicación se ejecuta en el puerto **5158**. Para cambiar el puerto:

1. Editar `src/Properties/launchSettings.json`
2. O usar la variable de entorno `ASPNETCORE_URLS`

## 📖 Uso

### Ejecutar la aplicación

```bash
dotnet run
```

### Compilar la aplicación

```bash
dotnet build
```

### Ejecutar tests (si existen)

```bash
dotnet test
```

## 📚 Documentación API

La documentación interactiva de la API está disponible a través de Swagger UI cuando la aplicación está en modo desarrollo:

- **URL**: `http://localhost:5158`
- **Swagger JSON**: `http://localhost:5158/swagger/v1/swagger.json`

### Características de Swagger

- ✅ Documentación completa de todos los endpoints
- ✅ Ejemplos de requests y responses
- ✅ Pruebas interactivas desde el navegador
- ✅ Descripción detallada de parámetros y modelos
- ✅ Códigos de respuesta HTTP

## 🏗 Arquitectura

El proyecto sigue los principios de **Clean Architecture** con las siguientes capas:

```
millionback/
├── src/                  # Código fuente del proyecto
│   ├── Core/
│   │   ├── Domain/           # Entidades y interfaces del dominio
│   │   │   ├── Entities/     # Entidades de negocio
│   │   │   └── Interfaces/   # Contratos de repositorios
│   │   └── Application/      # Lógica de aplicación
│   │       ├── DTOs/         # Objetos de transferencia de datos
│   │       ├── Interfaces/   # Contratos de servicios
│   │       ├── Mappings/     # Configuración de AutoMapper
│   │       ├── Services/     # Servicios de aplicación
│   │       └── Validators/   # Validadores de FluentValidation
│   ├── Infrastructure/       # Implementaciones de infraestructura
│   │   └── Data/
│   │       └── MongoDB/      # Implementación de MongoDB
│   │           ├── Context/  # Contexto de base de datos
│   │           └── Repositories/ # Implementación de repositorios
│   ├── API/                  # Capa de presentación
│   │   ├── Controllers/      # Controladores REST
│   │   ├── Filters/         # Filtros de acción
│   │   ├── Middleware/       # Middleware personalizado
│   │   └── Models/          # Modelos de respuesta
│   ├── Properties/           # Configuración de lanzamiento
│   ├── Program.cs           # Punto de entrada de la aplicación
│   └── appsettings*.json    # Archivos de configuración
├── scripts/                 # Scripts de utilidad (seeding, etc.)
├── Tests/                    # Tests unitarios e integración
└── millionback.csproj       # Archivo de proyecto .NET
```

## 🔌 Endpoints

### 1. Obtener Propiedades Paginadas

```http
GET /api/properties?page=1&pageSize=20
```

**Parámetros:**
- `page` (int, opcional): Número de página (default: 1)
- `pageSize` (int, opcional): Tamaño de página (default: 20, max: 100)

**Respuesta:**
```json
{
  "success": true,
  "message": "Propiedades obtenidas exitosamente",
  "data": {
    "items": [...],
    "total": 300,
    "page": 1,
    "pageSize": 20,
    "totalPages": 15
  }
}
```

### 2. Obtener Detalles de Propiedad

```http
GET /api/properties/{id}
```

**Parámetros:**
- `id` (string, requerido): ObjectId de la propiedad

**Respuesta:**
```json
{
  "success": true,
  "message": "Propiedad obtenida exitosamente",
  "data": {
    "idProperty": "...",
    "name": "Casa moderna",
    "address": "...",
    "price": 250000,
    "images": [
      "https://picsum.photos/seed/property1/800/600",
      "https://picsum.photos/seed/property2/800/600"
    ],
    "owner": {
      "idOwner": "OWNER0001",
      "name": "Pedro Gómez",
      "address": "Calle Secundaria 735, Toluca",
      "photo": "https://picsum.photos/seed/owner1/200/200"
    },
    "traces": [
      { "idPropertyTrace": "TRACE000001", "name": "Compra inicial", "dateSale": "2023-01-01T00:00:00Z", "value": 350000, "tax": 28000 },
      { "idPropertyTrace": "TRACE000002", "name": "Renovación de hipoteca", "dateSale": "2022-06-15T00:00:00Z", "value": 320000, "tax": 24000 }
    ]
  }
}
```

### 3. Buscar Propiedades

```http
GET /api/properties/search?name=casa&minPrice=100000&maxPrice=500000&page=1&pageSize=20
```

**Parámetros:**
- `name` (string, opcional): Búsqueda por nombre
- `address` (string, opcional): Búsqueda por dirección
- `minPrice` (decimal, opcional): Precio mínimo
- `maxPrice` (decimal, opcional): Precio máximo
- `page` (int, opcional): Número de página
- `pageSize` (int, opcional): Tamaño de página

## ⚡ Optimizaciones de Rendimiento

El proyecto incluye múltiples optimizaciones implementadas que mejoran significativamente el rendimiento:

### Resumen de Mejoras

- ✅ **Batch Loading:** Reducción de ~160 queries a ~3-4 queries (97.5% menos)
- ✅ **Proyección MongoDB:** Solo campos necesarios (~30-50% menos datos)
- ✅ **Ejecución en Paralelo:** Queries simultáneas con `Task.WhenAll()`
- ✅ **Caché en Memoria:** TTL de 5-10 minutos para datos frecuentes
- ✅ **Índices Optimizados:** Índices estratégicos en todas las colecciones
- ✅ **Optimización de Count:** `EstimatedDocumentCountAsync` para conteos sin filtro

### Métricas de Rendimiento

| Escenario | Antes | Después | Mejora |
|-----------|-------|---------|--------|
| Listar 20 propiedades | 6000ms | 800ms | **87% más rápido** |
| Con caché | 6000ms | 200-400ms | **93-97% más rápido** |
| Queries por petición | ~160 | ~3-4 | **97.5% menos** |

### Documentación Completa

Para información detallada sobre todas las optimizaciones implementadas, consulta el archivo [`OPTIMIZATIONS.md`](./OPTIMIZATIONS.md) que incluye:

- Descripción técnica de cada optimización
- Código de implementación
- Métricas detalladas antes/después
- Arquitectura de caché y batch loading
- Mejores prácticas aplicadas
- Próximas optimizaciones posibles

## 🛠 Scripts Útiles

Los scripts viven en `scripts/seed.ps1` y se ejecutan vía `npm run ...`. Todos aceptan los parámetros internos del PowerShell:

| Script | Descripción | Parámetros clave |
|--------|-------------|------------------|
| `npm run seed` | Inserta datos por defecto (100 owners / 300 propiedades / 600 imágenes / 3 traces) | - |
| `npm run seed:reset` | Limpia las colecciones antes de insertar | `-Reset` |
| `npm run seed:custom -- --Owners 200 --Props 500 --Images 1000 --Traces 4` | Datos personalizados (usa `npm_config_*`) | `Owners`, `Props`, `Images`, `Traces` |
| `npm run seed:stress` | Escenario de estrés (800 owners / 5000 propiedades / 10000 imágenes / 6 traces) | Incluye `-Reset` por defecto |
| `npm run seed:unique` | Inserta una única propiedad “benchmark” con 80 imágenes para pruebas puntuales | `-SingleMode` + `SingleProperty*` |

> Nota: agrega `-- --EnvPath "..\\.env"` si quieres usar un archivo `.env` alternativo.

### Parámetros soportados

El script (`seed.ps1`) acepta:

- `-Owners`, `-Props`, `-Images`, `-Traces`: controla cantidades.
- `-Reset`: limpia `Owners`, `Properties`, `PropertyImages`, `PropertyTraces`.
- `-EnvPath`: ruta al `.env` a usar (por defecto `./.env`).
- `-SingleMode`, `-SinglePropertyName`, `-SinglePropertyPrice`, `-SinglePropertyImages`: generan una sola propiedad con muchas imágenes (útil para demos).

Ejemplos adicionales:

```bash
# Poblar otra base (usa variables del .env seleccionado)
npm run seed:custom -- --EnvPath "../.env.staging" --Owners 300 --Props 1200 --Images 2500 --Traces 5

# Insertar una única propiedad VIP con 120 imágenes
npm run seed:unique -- --SinglePropertyName "Casa VIP" --SinglePropertyPrice 123456789 --SinglePropertyImages 120
```

### Estructura de Datos creada

- **Owners**: `IdOwner`, `Name`, `Address`, `Photo`, `Birthday`.
- **Properties**: `IdProperty`, `Name`, `Address`, `Price`, `CodeInternal`, `Year`, `IdOwner`.
- **PropertyImages**: `IdPropertyImage`, `IdProperty`, `File`, `Enabled`.
- **PropertyTraces**: `IdPropertyTrace`, `IdProperty`, `DateSale`, `Name`, `Value`, `Tax`.

## 📝 Estructura de Respuesta

Todas las respuestas siguen el formato estándar:

```json
{
  "success": true|false,
  "message": "Mensaje descriptivo",
  "data": {...},
  "errors": []
}
```

## 🔒 Manejo de Errores

La API maneja errores de forma centralizada:

- **400 Bad Request**: Validación de entrada fallida
- **404 Not Found**: Recurso no encontrado
- **500 Internal Server Error**: Error del servidor

Todos los errores se registran y devuelven en formato estándar.

## 🗄 Colecciones MongoDB y cómo replicarlas

### Descripción rápida

| Colección | Propósito | Campos clave | Índices |
|-----------|-----------|--------------|---------|
| `Owners` | Propietarios | `_id`, `IdOwner`, `Name`, `Address`, `Photo`, `Birthday` | Índice en `Name` |
| `Properties` | Propiedades | `_id`, `IdProperty`, `Name`, `Address`, `Price`, `CodeInternal`, `Year`, `IdOwner` | Texto (Name+Address), `Price`, `IdOwner`, `Name`, `Address` |
| `PropertyImages` | Imágenes | `_id`, `IdPropertyImage`, `IdProperty`, `File`, `Enabled` | `IdProperty`, `IdProperty+Enabled` |
| `PropertyTraces` | Historial | `_id`, `IdPropertyTrace`, `IdProperty`, `DateSale`, `Name`, `Value`, `Tax` | `IdProperty`, `DateSale` |

### Opción A: Usar la misma DB remota
1. Solicita/crea el connection string (ej. MongoDB Atlas).
2. Coloca los valores en tu `.env`:
   ```env
   MONGO_CONN=mongodb+srv://usuario:pwd@cluster.mongodb.net/?retryWrites=true&w=majority&appName=million-challenge
   MONGO_DB=milliondb
   ```
3. Ejecuta `dotnet run` y la API usará esa DB directamente (sin seed adicional).

### Opción B: Replicar datos en otra DB
1. Crea una nueva base en tu clúster/local (`MONGO_DB` distinto).
2. Actualiza el `.env` o usa `--EnvPath` apuntando al archivo con la nueva conexión.
3. Ejecuta alguno de los seeds (por ejemplo `npm run seed:stress`) para poblarla.

### Opción C: Clonar la DB exacta
- **mongodump/mongorestore**: si necesitas una copia literal, puedes correr:
  ```bash
  mongodump --uri="<cadena-origen>" --db milliondb --out ./dump
  mongorestore --uri="<cadena-destino>" ./dump/milliondb
  ```
- Alternativamente, apunta tus scripts de seed a la nueva cadena y ejecuta `npm run seed:custom` con los mismos parámetros del entorno original.

> Recuerda que los scripts siempre respetan las variables `MONGO_CONN` y `MONGO_DB`. Basta con cambiar esas variables para poblar cualquier ambiente (local, staging, producción).

## 🧪 Testing

### Testing Unitario

El proyecto incluye una suite completa de tests unitarios desarrollada con **NUnit**, **Moq** y **FluentAssertions**.

#### Estructura de Tests

```
Tests/
└── UnitTests/
    ├── Controllers/
    │   └── PropertiesControllerTests.cs      # Tests de endpoints
    ├── Services/
    │   ├── PropertyServiceTests.cs            # Tests básicos del servicio
    │   └── PropertyServiceAdditionalTests.cs # Tests de caché, mapeo y errores
    ├── Validators/
    │   └── PropertyFilterDtoValidatorTests.cs # Tests de validación
    └── Middleware/
        └── ExceptionHandlerMiddlewareTests.cs # Tests de manejo de excepciones
```

#### Ejecutar Tests

```bash
# Ejecutar todos los tests
dotnet test Tests/UnitTests/MillionBack.Tests.csproj

# Con más detalles
dotnet test Tests/UnitTests/MillionBack.Tests.csproj --verbosity normal

# Ejecutar un test específico
dotnet test Tests/UnitTests/MillionBack.Tests.csproj --filter "FullyQualifiedName~PropertyServiceTests"

# Generar reporte de cobertura
dotnet test Tests/UnitTests/MillionBack.Tests.csproj --collect:"XPlat Code Coverage"
```

#### Cobertura de Tests

El proyecto cuenta con **38 tests unitarios** que cubren:

- ✅ **Controllers** (9 tests): Todos los endpoints del `PropertiesController`
  - Tests de casos exitosos
  - Tests de manejo de errores
  - Tests de validación de parámetros

- ✅ **Services** (13 tests): Lógica de negocio en `PropertyService`
  - Tests de obtención de propiedades
  - Tests de búsqueda y filtrado
  - Tests de caché en memoria
  - Tests de mapeo de datos (Owner, Images, Traces)
  - Tests de manejo de excepciones

- ✅ **Validators** (7 tests): Validación de `PropertyFilterDto`
  - Tests de precios negativos
  - Tests de rangos de precios
  - Tests de casos válidos e inválidos

- ✅ **Middleware** (8 tests): Manejo de excepciones
  - Tests de diferentes tipos de excepciones
  - Tests de códigos de estado HTTP
  - Tests de logging

#### Tecnologías de Testing

- **NUnit** (v4.2.2): Framework de testing
- **Moq** (v4.20.72): Framework para mocking y stubs
- **FluentAssertions** (v8.8.0): Biblioteca para assertions más legibles
- **Microsoft.NET.Test.Sdk** (v17.12.0): SDK de testing de .NET
- **coverlet.collector** (v6.0.2): Para cobertura de código

#### Ejemplo de Test

```csharp
[Test]
public async Task GetPropertyById_WhenPropertyExists_ReturnsOkWithPropertyDetail()
{
    // Arrange
    var propertyId = "507f1f77bcf86cd799439011";
    var propertyDetail = new PropertyDetailDto { Name = "Casa de Prueba" };

    _propertyServiceMock
        .Setup(x => x.GetPropertyByIdAsync(propertyId))
        .ReturnsAsync(propertyDetail);

    // Act
    var result = await _controller.GetPropertyById(propertyId);

    // Assert
    result.Result.Should().BeOfType<OkObjectResult>();
    var okResult = result.Result as OkObjectResult;
    okResult!.StatusCode.Should().Be(200);
}
```

#### Documentación Completa

Para más información sobre testing, consulta el archivo [`TESTING_GUIDE.md`](./TESTING_GUIDE.md) que incluye:
- Guía detallada de uso de NUnit
- Ejemplos de mocking con Moq
- Mejores prácticas
- Debugging de tests
- Generación de reportes de cobertura

### Testing de API

Para probar la API manualmente:

1. **Swagger UI**: Usar la interfaz interactiva en `http://localhost:5158`
2. **Postman**: Importar la colección (ver `POSTMAN_GUIDE.md`)
3. **cURL**: Ejemplos disponibles en la documentación de Swagger

## 📄 Licencia

Este proyecto está bajo la Licencia MIT.

## 👤 Autor

**Juan Fernando Álvarez Gallego**

Este proyecto fue desarrollado como parte de una prueba técnica, implementando una API REST completa con arquitectura limpia, optimizaciones de rendimiento y una suite exhaustiva de tests unitarios.

## 📞 Contacto

- **Email:** alvarezjfernandog@gmail.com
- **Teléfono:** +57 302 285 60 79
- **GitHub:** [Juanfeross](https://github.com/Juanfeross)
- **LinkedIn:** [Juan Fernando Álvarez Gallego](https://www.linkedin.com/in/juan-fernando-%C3%A1lvarez-gallego-b97b31212/)

---

**Prueba Técnica** - Desarrollado por **Juan Fernando Álvarez Gallego**
**Tecnologías:** .NET 9.0, MongoDB, Clean Architecture, NUnit

