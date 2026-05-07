# .NET Microservices Case Study

## Quick Start

Create the environment file first:

```bash
cp .env.example .env
```

Update the values in `.env` if needed, especially `JWT_KEY` and `ADMIN_PASSWORD`.

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
