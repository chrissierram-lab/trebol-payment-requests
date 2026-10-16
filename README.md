# Solicitudes de pago

Aplicación full stack para registrar y consultar solicitudes de pago. Angular consume únicamente la API .NET; .NET calcula cada comisión llamando por HTTP al servicio Java y guarda la solicitud en memoria solo después de recibir un resultado válido.

## Versiones probadas

- Ubuntu 22.04 x64.
- .NET SDK 8.0.131 y ASP.NET Core 8.0.31.
- OpenJDK Temurin 21.0.4.
- Spring Boot 4.0.8; Maven Wrapper 3.3.4 con distribución Maven 3.10.0.
- Angular y Angular CLI 22.2.3; Node.js 24.21.0; npm 11.19.0; TypeScript 6.0.3.

## Prerrequisitos

Instalar .NET SDK 8, JDK 21 y Node.js 24.15 o superior dentro de la línea 24. El proyecto Java incluye Maven Wrapper, por lo que no hace falta instalar Maven globalmente. npm instala las dependencias del frontend desde `package-lock.json`.

## Componentes y puertos

| Componente | Directorio | Puerto | Responsabilidad |
| --- | --- | ---: | --- |
| API .NET | `src/PaymentRequests.Api` | 5055 | Validación de solicitudes, integración HTTP con Java y almacenamiento en memoria. |
| Servicio Java | `src/fee-calculator` | 8081 | Cálculo de comisión por canal con `BigDecimal`. |
| Frontend Angular | `frontend` | 4200 | Formulario reactivo, consulta y presentación de solicitudes. |

El endpoint Java es `POST /api/v1/fees/calculate`. La API .NET expone `POST /api/v1/payment-requests` y `GET /api/v1/payment-requests`. Angular utiliza el proxy de desarrollo definido en `frontend/proxy.conf.json`, que reenvía `/api` a `http://localhost:5055`.

## Instalar, compilar y ejecutar

Ejecutar cada componente en una terminal independiente, en este orden:

1. Servicio Java:

   ```bash
   cd src/fee-calculator
   ./mvnw clean verify
   ./mvnw spring-boot:run
   ```

2. API .NET, desde la raíz del proyecto:

   ```bash
   dotnet restore PaymentRequests.sln
   dotnet build PaymentRequests.sln
   dotnet run --project src/PaymentRequests.Api/PaymentRequests.Api.csproj --urls http://localhost:5055
   ```

3. Angular:

   ```bash
   cd frontend
   npm ci
   npm start -- --host localhost --port 4200
   ```

Abrir <http://localhost:4200/>. Para compilar Angular para producción, desde `frontend/` ejecutar `npm run build`.

## Pruebas

Ejecutar desde la raíz, salvo cuando se indique otro directorio:

```bash
dotnet test PaymentRequests.sln
```

```bash
cd src/fee-calculator
./mvnw test
```

```bash
cd frontend
npm test -- --watch=false
```

Las pruebas cubren: falla del cálculo Java sin persistencia y validaciones de .NET; tasas de los tres canales, redondeo y respuestas HTTP de Java; validación del formulario Angular y conservación de sus datos ante un error `503`.

## Ejemplos HTTP

Con Java y .NET iniciados, crear una solicitud (usar una referencia nueva en cada ejecución):

```bash
curl -i -X POST http://localhost:5055/api/v1/payment-requests \
  -H 'Content-Type: application/json' \
  -d '{"externalReference":"PAY-2026-0001","amount":1.00,"channel":"MOBILE"}'
```

La respuesta exitosa es `201 Created`, con comisión `0.01`, total `1.01` y estado `PENDING`.

Consultar las solicitudes registradas:

```bash
curl -i http://localhost:5055/api/v1/payment-requests
```

Calcular directamente una comisión (opcional):

```bash
curl -i -X POST http://localhost:8081/api/v1/fees/calculate \
  -H 'Content-Type: application/json' \
  -d '{"amount":1000.00,"channel":"BRANCH"}'
```

## Responsabilidades y fallas de integración

Java concentra las tasas (`WEB` 1 %, `MOBILE` 0,5 % y `BRANCH` 1,5 %), calcula con `BigDecimal` y redondea a dos decimales con `RoundingMode.HALF_UP`. .NET normaliza la referencia, valida antes de llamar a Java, valida la respuesta de cálculo y persiste el resultado junto con el estado `PENDING`. La comparación de referencias distingue mayúsculas de minúsculas. Angular contiene los contratos TypeScript y realiza todas las llamadas HTTP desde un servicio.

La URL de Java y el timeout de tres segundos son configurables en `src/PaymentRequests.Api/appsettings.json`. Una caída, timeout, respuesta inválida o error HTTP de Java produce `503` con `code` y `message`, sin guardar la solicitud. Los errores de validación responden `400`; una referencia repetida, `409`. Las APIs no exponen trazas internas.

## Supuestos y limitaciones

- El almacenamiento de .NET es en memoria y se pierde al reiniciar el proceso; no hay base de datos, autenticación, paginación, reintentos ni circuit breaker.
- Solo se implementa el flujo de creación y consulta solicitado. Los servicios se ejecutan localmente en terminales separadas; no se requiere Docker.
- El registro duplicado se protege atómicamente dentro de una instancia mediante `ConcurrentDictionary`. En varias instancias, la memoria no basta para garantizar unicidad.

## Criterios técnicos

**Unicidad concurrente y persistencia.** En producción usaría una restricción única sobre la referencia normalizada en la base de datos y trataría la inserción de forma atómica, convirtiendo la violación de unicidad en `409`. Para reemplazar el almacén en memoria, definiría una interfaz de repositorio, implementaría el acceso con una base de datos y guardaría en una transacción respetando las reglas de consistencia entre el cálculo y el registro.

**Angular puede consultar pero no registrar.** Revisaría primero en la pestaña Network la URL, el método, el payload y el status de `POST`; después comprobaría los mensajes de validación y el JSON de error. Si usa el proxy local, verificaría su destino y el puerto de .NET; finalmente revisaría logs de .NET y disponibilidad de Java para distinguir un `400`, `409` o `503`.

**Migración de .NET 5 a .NET 8.** Inventariaría dependencias, APIs obsoletas y cambios de hosting, configuración, serialización y comportamiento antes de actualizar el target framework. Comunicaría al equipo el impacto y los riesgos conocidos, propondría una rama de migración y validaría restore/build, pruebas automatizadas y pruebas de regresión en un entorno de staging antes del despliegue, con un plan de reversa.
