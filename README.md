# orleans-notification-sample
Sample C# project to create a notification system with Orleans

## Overview

This project demonstrates a simple notification system built with [Microsoft Orleans](https://dotnet.github.io/orleans/), a cross-platform framework for building distributed applications with .NET. The sample showcases how to use Orleans grains to send and receive notifications in a scalable and reliable way.

## Features

- Orleans Silo and Client setup
- Notification grain interface and implementation
- Example of sending and receiving notifications
- **Swagger/OpenAPI UI for testing the API endpoints**

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 or later (optional)

### Running the Sample

1. Clone the repository:
    ```bash
    git clone https://github.com/joelsodias/orleans-notifier-sample.git
    cd orleans-notifier-sample
    ```

2. Build the solution:
    ```bash
    dotnet build
    ```

3. Run the Orleans Silo (host):
    ```bash
    dotnet run --project SiloHost
    ```

4. In a new terminal, run the client (API server):
    ```bash
    dotnet run --project Client
    ```

5. Open your browser and navigate to `http://localhost:5000/swagger` (or the port shown in the terminal) to access the Swagger UI and interact with the API endpoints for sending and receiving notifications.

## Project Structure

- `Contracts`  
  Contains the grain interfaces and data contracts shared between the client and the silo.

- `Grains`  
  Implements the grain logic, including business logic for sending, storing, and delivering notifications.

- `SiloHost`  
  Hosts the Orleans Silo, managing the lifecycle of grains and handling client requests.

- `Client`  
  ASP.NET Core Web API project acting as a client to the Orleans cluster, exposing endpoints for sending notifications and querying results. The UI is provided via Swagger/OpenAPI.

- `Common`  
  Shared utilities and configuration extensions used across projects.

- `Orleans.Streaming.Redis`  
  Custom Orleans streaming provider implementation for Redis.

## License

This project is licensed under the MIT License.
