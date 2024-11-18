DCXAirSolution

Descripción:
DCXAirSolution es una API desarrollada en C# utilizando ASP.NET Core que permite la búsqueda de vuelos basados en criterios como aeropuertos de origen y destino, tipo de moneda y tipo de viaje (ida o ida y vuelta). La solución utiliza Redis para almacenar temporalmente los resultados y mejorar el rendimiento.

Requisitos Previos:
.NET 7 SDK
Redis
Visual Studio

Configuración del Proyecto:

Clonar el Repositorio:

git clone https://github.com/lsofiagonzalez22/DCXAirSolution.git
cd DCXAirSolution

Instalación:
Asegúrate de tener Redis ejecutándose localmente. Puedes iniciarlo con el siguiente comando (si tienes Docker instalado):

docker run -d -p 6379:6379 redis
Abre la solución DCXAirSolution.sln en Visual Studio.

Configuración de Redis:
El proyecto ya está configurado para conectarse a Redis en localhost:6379. Si tu Redis está en otro servidor, actualiza la línea en el archivo Program.cs:


builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));

Construcción y Ejecución:
Para ejecutar el proyecto, abre la terminal y ejecuta:

dotnet build
dotnet run
Esto iniciará la aplicación en http://localhost:5297.

Endpoints de la API:

Buscar Vuelos
GET /api/Flights/search

Parámetros de consulta:

origin (string): Código del aeropuerto de origen (Ejemplo: DGD).
destination (string): Código del aeropuerto de destino (Ejemplo: DGRE).
currency (string): Moneda (Ejemplo: EUR).
tripType (string): Tipo de viaje (Oneway o Roundtrip).

Swagger:
Para explorar la documentación de la API, navega a:

http://localhost:5297/swagger/index.html

Notas Adicionales:
El proyecto utiliza CORS para permitir solicitudes desde cualquier origen.
Actualmente, no se está utilizando una base de datos. Los datos se almacenan temporalmente en Redis para optimizar la búsqueda.
