# PROYECTO TIENDA MINECRAFT

## Descripción del Proyecto

Sistema completo de tienda web inspirado en plataformas como Tebex para servidores de Minecraft. El proyecto está diseñado bajo una arquitectura desacoplada compuesta por un cliente web interactivo en **ASP.NET Core MVC** y un servicio backend en **ASP.NET Core Web API** testeable mediante **Swagger**, respaldado por una base de datos relacional en **PostgreSQL**.

## Estructura de la Solución

```text
TebexMinecraft/
├── TebexMinecraft.Modelos/         # Entidades compartidas (User, Rank, Prefix, Order, UserRank, etc.)
├── TebexMinecraft.Consumer/        # Cliente genérico CRUD HTTP para consumo de la API
├── TebexMinecraft.API/             # Web API conectada a PostgreSQL
│   ├── Controllers/               # UsersController, OrdersController, RanksController
│   ├── Data/                      # DbContext
│   └── Program.cs                 # Configuración del servicio API
└── TebexMinecraft.MVC/             # Aplicación Web MVC
    ├── Controllers/               # RanksController, PrefixesController, CartController, AccountController
    ├── Middlewares/               # AdminAuthMiddleware (Protección HTTP Basic)
    ├── Views/                     # Plantillas y vistas de tienda
    └── Program.cs                 # Configuración de sesiones MVC

```

## Agradecimientos

Agradecimiento especial a los docentes del curso de desarrollo en **.NET**, así como a los servicios abiertos de la **API de Mojang** por facilitar la integración de identidades oficiales de Minecraft en aplicaciones web modernas.

## NOTA:
* **Uso de IA:** Se utilizó Inteligencia Artificial como apoyo para implementar la lógica del `ServerSyncController`, la integración y consumo de la **API de Mojang**, y la maquetación de las vistas HTML del proyecto MVC.
