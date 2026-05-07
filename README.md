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
