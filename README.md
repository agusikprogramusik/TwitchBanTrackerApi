# TwitchBanTracker

(https://github.com/agusikprogramusik/twitch-ban-tracker-frontend)

TwitchBanTrackerApi is a web API designed to track user bans and timeouts on Twitch. It provides endpoints to manage and retrieve information about banned users and their messages.


Prerequisites

  •	.NET 6.0 SDK or later
  •	SQL Server
  •	Visual Studio


Getting Started

  1. Clone the Repository
  2. Configuration
    a) Database Connection: Update the connection string in appsettings.json to point to your SQL Server instance.
    b) Twitch API Credentials: Add your Twitch API client_id and client_secret to the configuration.


Project Structure

  •	Controllers: Contains the API controllers.
  •	TwitchController.cs: Manages Twitch-related endpoints.
  •	Services: Contains the service classes.
  •	TwitchListenerService.cs: Listens to Twitch events and processes user timeouts and messages.
  •	Infrastructure: Contains the database context and migrations.
  •	TwitchBanTrackerDbContext.cs: Entity Framework Core database context.
  •	Migrations/: Database migrations.
  •	DependencyInjection: Contains extension methods for service registration.
  •	ServiceCollectionExtension.cs: Registers services and dependencies.
  •	Common: Contains common utilities and mappings.
  •	Mappings/MappingProfile.cs: AutoMapper profile for entity-to-DTO mappings.

Key Classes and Methods

1. TwitchController.cs
  •	Constructor: Initializes the controller with dependencies.
  •	Dependencies:
  •	TwitchBanTrackerDbContext
  •	TwitchUserMessages
  •	IMapper

2. TwitchListenerService.cs
  •	GetAccessToken: Retrieves an access token from Twitch.
  •	Client_OnUserTimedOut: Handles user timeout events.
  •	Client_OnMessageReceived: Handles message received events.

4. ServiceCollectionExtension.cs
  •	AddInfrastructure: Registers services and dependencies, including:
  •	CORS policy
  •	Database context
  •	Controllers with Newtonsoft.Json
  •	Logging
  •	Hosted services
  •	HttpClient
  •	AutoMapper
