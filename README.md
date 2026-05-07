# .NET Microservices Case Study

## Quick Start

Create the environment file first:

```bash
cp .env.example .env
```

Update the values in `.env` if needed, especially `JWT_KEY` and `ADMIN_PASSWORD`.

* JWT_KEY must be longer than 32 characters because HS256 requires a key greater than 256 bits.

* The admin password must follow the default ASP.NET Core Identity password rules:
  - at least 6 characters
  - at least 1 uppercase letter
  - at least 1 lowercase letter
  - at least 1 digit
  - at least 1 non-alphanumeric character, for example `!`, `?`, `@`, `#`

Example:

```env
ADMIN_PASSWORD=Admin123!
JWT_KEY=THIS_IS_A_SUPER_LONG_SECRET_KEY_1234567890
```

---

## Option 1: Run with local Docker Compose build

Use this option when you want Docker to build the service images from the source code.

```bash
docker compose up --build
```

The API Gateway will be available at:

```text
http://localhost:8080
```

To stop the containers:

```bash
docker compose down
```

---

## Option 2: Run with Docker Hub images

Use this option when you want to run the already published images from Docker Hub.

```bash
docker compose -f docker-compose.hub.yml up
```

The API Gateway will be available at:

```text
http://localhost:8080
```

To stop the containers:

```bash
docker compose -f docker-compose.hub.yml down
```

---

## Gateway port

By default, the gateway uses port `8080`.

To change it, update this value in `.env`:

```env
GATEWAY_PORT=5296
```

Then the application will be available at:

```text
http://localhost:5296
```

---

## Project Overview

This repository is a small .NET microservices case study.

It demonstrates:

- API Gateway routing
- JWT authentication
- Refresh token flow
- Role-based and policy-based authorization
- Service-to-service HTTP communication
- Centralized log publishing
- Redis-backed caching
- Docker Compose based local development
- Docker Hub image publishing with GitHub Actions

The project is intentionally small, but structured to show how multiple services can run together behind a single gateway.

---

## Architecture

The application has one public entry point: the API Gateway.

```text
Client
  |
  v
API Gateway
  |
  |-- AuthService
  |-- ProductService
  |-- LogService
  |-- Redis
```

Only the Gateway is exposed to the host machine.

The internal services are available only inside the Docker network.

```text
Gateway:        exposed on localhost:8080
AuthService:    internal only
ProductService: internal only
LogService:     internal only
Redis:          internal only
```

This keeps the external API surface smaller and closer to a production-style deployment model.

---

## Documentation

- [API Documentation](docs/api.md)
- [Postman Collection](docs/postman_collection.json)

## Services

### Gateway

The Gateway is the single public entry point of the system.

It forwards external requests to the internal services.

Example external routes:

```text
/api/v1/auth/*
/api/v1/products/*
/api/v1/logs/*
```

### AuthService

AuthService handles:

- user registration
- login
- JWT generation
- refresh token generation
- default admin user creation
- Identity roles

The default admin user is created from environment variables:

```env
ADMIN_USERNAME=admin
ADMIN_PASSWORD=Admin123!
```

### ProductService

ProductService handles product operations.

It includes:

- product listing
- product details
- product creation
- product update
- authorization checks
- owner-based update policy
- Redis caching

Product creation requires authentication.

Product update requires either:

- Admin role
- owner access through the `AdminOrOwner` policy

### LogService

LogService stores application logs published by other services.

It exposes simple endpoints for creating and listing logs.

Other services call LogService internally through Docker networking.

### Redis

Redis is used by ProductService for caching.

Redis is not exposed to the host machine.

---

## API Examples

### Login

```bash
curl -i -X POST http://localhost:8080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}'
```

### Store access token in a shell variable

This requires `jq`.

```bash
TOKEN=$(curl -s -X POST http://localhost:8080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}' | jq -r '.accessToken')
```

Check the token:

```bash
echo $TOKEN
```

### List products

```bash
curl -i http://localhost:8080/api/v1/products/list \
  -H "Authorization: Bearer $TOKEN"
```

### Create product

```bash
curl -i -X POST http://localhost:8080/api/v1/products/create \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Test Product",
    "price": 100
  }'
```

### Get product by id

```bash
curl -i http://localhost:8080/api/v1/products/1 \
  -H "Authorization: Bearer $TOKEN"
```

### Update product

```bash
curl -i -X PUT http://localhost:8080/api/v1/products/update/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "id": 1,
    "name": "Updated Product",
    "price": 150
  }'
```

### List logs

```bash
curl -i http://localhost:8080/api/v1/logs/list
```

---

## Docker Setup

The project includes two Docker Compose files.

### `docker-compose.yml`

Builds images locally from the source code.

Use it for development:

```bash
docker compose up --build
```

### `docker-compose.hub.yml`

Uses already published Docker Hub images.

Use it to test the published image setup:

```bash
docker compose -f docker-compose.hub.yml up
```

Published images:

```text
nesbal/dotnet-authservice
nesbal/dotnet-productservice
nesbal/dotnet-logservice
nesbal/dotnet-gateway
```

---

## Local Data

The services use local SQLite files mounted under the `data` directory.

This means data can remain available even after containers are stopped.

To reset local data:

```bash
docker compose down
rm -rf data/auth data/product data/log
docker compose up --build
```

For Docker Hub compose:

```bash
docker compose -f docker-compose.hub.yml down
rm -rf data/auth data/product data/log
docker compose -f docker-compose.hub.yml up
```

---

## CI/CD

The repository includes a GitHub Actions workflow for publishing Docker images.

Workflow file:

```text
.github/workflows/docker-publish.yml
```

The workflow builds and pushes separate Docker images for:

- AuthService
- ProductService
- LogService
- Gateway

Images are pushed to Docker Hub with:

- `latest`
- commit SHA tag

Required GitHub repository secrets:

```text
DOCKERHUB_USERNAME
DOCKERHUB_TOKEN
```

---

## Configuration

Configuration is provided through `.env`.

Important values:

```env
GATEWAY_PORT=8080
JWT_KEY=THIS_IS_A_SUPER_LONG_SECRET_KEY_1234567890
JWT_ISSUER=nsb-service
JWT_AUDIENCE=api
ADMIN_USERNAME=admin
ADMIN_PASSWORD=Admin123!
LOG_SERVICE_URL=http://logservice/
```

The `.env` file should not be committed.

Use `.env.example` as the template.

---

## Notes on the Design

### 12-Factor alignment

The project follows several 12-factor friendly practices:

- configuration is passed through environment variables
- services run as separate containers
- only the Gateway is exposed publicly
- internal services communicate over the Docker network
- Docker Hub images provide a repeatable deployment artifact

### SOLID-oriented structure

The services separate responsibilities across:

- controllers
- commands
- handlers
- services
- data context
- extension methods

This keeps `Program.cs` smaller and avoids putting application logic directly into startup configuration.

---

## Useful Commands

### See running containers

```bash
docker compose ps
```

For Docker Hub compose:

```bash
docker compose -f docker-compose.hub.yml ps
```

### See logs

```bash
docker compose logs gateway
docker compose logs authservice
docker compose logs productservice
docker compose logs logservice
```

For Docker Hub compose:

```bash
docker compose -f docker-compose.hub.yml logs gateway
docker compose -f docker-compose.hub.yml logs authservice
docker compose -f docker-compose.hub.yml logs productservice
docker compose -f docker-compose.hub.yml logs logservice
```

### Pull latest Docker Hub images

```bash
docker compose -f docker-compose.hub.yml pull
```

### Recreate containers from Docker Hub images

```bash
docker compose -f docker-compose.hub.yml up --force-recreate
```
