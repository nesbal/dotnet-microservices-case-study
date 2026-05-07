# API Documentation

Base URL:

```text
http://localhost:8080
```

All requests go through the API Gateway.

---

## Authentication

Authenticated endpoints require a bearer token:

```http
Authorization: Bearer <ACCESS_TOKEN>
```

You can get an access token from the login endpoint.

With `jq`, you can store the token in a shell variable:

```bash
TOKEN=$(curl -s -X POST http://localhost:8080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}' | jq -r '.accessToken')
```

Check the token:

```bash
echo $TOKEN
```

---

## Auth Endpoints

### Register

```http
POST /api/v1/auth/register
```

Authentication: not required.

Registers a new user. New users are assigned the `User` role automatically.

Request body:

```json
{
  "username": "user1",
  "password": "User123!"
}
```

Example:

```bash
curl -i -X POST http://localhost:8080/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "user1",
    "password": "User123!"
  }'
```

Possible responses:

```text
200 OK
400 Bad Request
409 Conflict
```

Notes:
- The password must follow the default ASP.NET Core Identity password rules.

---

### Login

```http
POST /api/v1/auth/login
```

Authentication: not required.

Request body:

```json
{
  "username": "admin",
  "password": "Admin123!"
}
```

Example:

```bash
curl -i -X POST http://localhost:8080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}'
```

Successful response:

```json
{
  "accessToken": "...",
  "refreshToken": "..."
}
```

Possible responses:

```text
200 OK
400 Bad Request
401 Unauthorized
```

---

### Refresh Token

```http
POST /api/v1/auth/refresh
```

Authentication: not required.

The refresh endpoint expects the refresh token as a raw JSON string.

Request body:

```json
"refresh-token-value"
```

Example:

```bash
curl -i -X POST http://localhost:8080/api/v1/auth/refresh \
  -H "Content-Type: application/json" \
  -d '"<REFRESH_TOKEN>"'
```

Store a refresh token from login:

```bash
REFRESH_TOKEN=$(curl -s -X POST http://localhost:8080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}' | jq -r '.refreshToken')
```

Refresh the token:

```bash
curl -i -X POST http://localhost:8080/api/v1/auth/refresh \
  -H "Content-Type: application/json" \
  -d ""$REFRESH_TOKEN""
```

Successful response:

```json
{
  "accessToken": "...",
  "refreshToken": "..."
}
```

Possible responses:

```text
200 OK
400 Bad Request
401 Unauthorized
```

---

### Logout

```http
POST /api/v1/auth/logout
```

Authentication: not required.

The logout endpoint expects the refresh token as a raw JSON string.

Request body:

```json
"refresh-token-value"
```

Example:

```bash
curl -i -X POST http://localhost:8080/api/v1/auth/logout \
  -H "Content-Type: application/json" \
  -d ""$REFRESH_TOKEN""
```

Possible responses:

```text
200 OK
400 Bad Request
```

---

### Current User

```http
GET /api/v1/auth/me
```

Authentication: required.

Returns the username from the current access token.

Example:

```bash
curl -i http://localhost:8080/api/v1/auth/me \
  -H "Authorization: Bearer $TOKEN"
```

Possible responses:

```text
200 OK
401 Unauthorized
```

---

### Promote User To Admin

```http
POST /api/v1/auth/users/{username}/promote
```

Authentication: required.

Authorization: Admin role required.

Promotes an existing user to the `Admin` role.

Example:

```bash
curl -i -X POST http://localhost:8080/api/v1/auth/users/user1/promote \
  -H "Authorization: Bearer $TOKEN"
```

Possible responses:

```text
200 OK
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

---

## Product Endpoints

### List Products

```http
GET /api/v1/products/list
```

Authentication: not required.

Example:

```bash
curl -i http://localhost:8080/api/v1/products/list
```

Possible responses:

```text
200 OK
```

---

### Get Product By ID

```http
GET /api/v1/products/{id}
```

Authentication: not required.

Example:

```bash
curl -i http://localhost:8080/api/v1/products/1
```

Possible responses:

```text
200 OK
404 Not Found
```

---

### Create Product

```http
POST /api/v1/products/create
```

Authentication: required.

Request body:

```json
{
  "name": "Test Product",
  "price": 100
}
```

Example:

```bash
curl -i -X POST http://localhost:8080/api/v1/products/create \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Test Product",
    "price": 100
  }'
```

Possible responses:

```text
201 Created
401 Unauthorized
```

---

### Update Product

```http
PUT /api/v1/products/update/{id}
```

Authentication: required.

Authorization:

- Admin users can update products.
- Product owners can update their own products through the `AdminOrOwner` policy.

The route `id` is used as the product id.

Request body:

```json
{
  "name": "Updated Product",
  "price": 150
}
```

Example:

```bash
curl -i -X PUT http://localhost:8080/api/v1/products/update/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Updated Product",
    "price": 150
  }'
```

Possible responses:

```text
204 No Content
401 Unauthorized
403 Forbidden
404 Not Found
```

---

## Log Endpoints

### List Logs

```http
GET /api/v1/logs/list
```

Authentication: not required.

Returns the latest 100 logs, newest first.

Example:

```bash
curl -i http://localhost:8080/api/v1/logs/list
```

Possible responses:

```text
200 OK
```

---

### Create Log

```http
POST /api/v1/logs/create
```

Authentication: not required.

This endpoint is mainly used internally by the other services.

Request body:

```json
{
  "serviceName": "ProductService",
  "eventType": "ProductCreated",
  "level": "INFO",
  "message": "Product created",
  "userName": "admin",
  "resourceId": "1"
}
```

Example:

```bash
curl -i -X POST http://localhost:8080/api/v1/logs/create \
  -H "Content-Type: application/json" \
  -d '{
    "serviceName": "ProductService",
    "eventType": "ProductCreated",
    "level": "INFO",
    "message": "Product created",
    "userName": "admin",
    "resourceId": "1"
  }'
```

Supported log levels:

```text
INFO
WARNING
ERROR
CRITICAL
```

Other values are normalized to `INFO`.

Possible responses:

```text
200 OK
```

---

## Suggested Manual Test Flow

### 1. Login as admin

```bash
TOKEN=$(curl -s -X POST http://localhost:8080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}' | jq -r '.accessToken')
```

### 2. Check current user

```bash
curl -i http://localhost:8080/api/v1/auth/me \
  -H "Authorization: Bearer $TOKEN"
```

### 3. Create a product

```bash
curl -i -X POST http://localhost:8080/api/v1/products/create \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Manual Test Product",
    "price": 100
  }'
```

### 4. List products

```bash
curl -i http://localhost:8080/api/v1/products/list
```

### 5. Update product

```bash
curl -i -X PUT http://localhost:8080/api/v1/products/update/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Updated Manual Test Product",
    "price": 150
  }'
```

### 6. List logs

```bash
curl -i http://localhost:8080/api/v1/logs/list
```

---

## Common Status Codes

| Status code | Meaning |
| --- | --- |
| `200 OK` | Request completed successfully. |
| `201 Created` | Resource was created successfully. |
| `204 No Content` | Request completed successfully with no response body. |
| `400 Bad Request` | Invalid request body or invalid operation. |
| `401 Unauthorized` | Missing or invalid authentication token. |
| `403 Forbidden` | Authenticated user does not have permission. |
| `404 Not Found` | Resource or route was not found. |
| `405 Method Not Allowed` | Route exists, but the HTTP method is not supported. |
| `409 Conflict` | Resource already exists. |
| `500 Internal Server Error` | Unexpected server-side error. |
