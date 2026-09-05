# SecureApiIntegration

A practical ASP.NET Core Web API project demonstrating secure authentication using JWT, Refresh Tokens, and custom middleware.

## Features

- User Registration
- Password Hashing
- User Login
- JWT Access Token
- Refresh Token
- Refresh Token Rotation
- Role Claims
- Protected APIs using `[Authorize]`
- Global Exception Handling Middleware
- Proper HTTP Status Codes
- Entity Framework Core
- SQLite Database
- Dependency Injection

## Authentication Flow

```text
Register
   ↓
Hash Password
   ↓
Login
   ↓
Validate Email & Password
   ↓
Access Token + Refresh Token
   ↓
Access Token is used for protected APIs
   ↓
Access Token expires
   ↓
Client sends Refresh Token
   ↓
Server validates Refresh Token
   ↓
Old Refresh Token is revoked
   ↓
New Access Token + Refresh Token are generated
```

## JWT Authentication

After a successful login, the server generates an Access Token containing claims such as:

- User Id
- Email
- Role

The JWT is signed using a secret key.

ASP.NET Core JWT Bearer Authentication validates:

- Signature
- Issuer
- Audience
- Expiration

After successful validation, the authenticated user information is available through `HttpContext.User`.

## Refresh Token Rotation

The project implements Refresh Token Rotation.

When a user logs in:

```text
Login
   ↓
Access Token #1
Refresh Token #1
```

When the Refresh Token is used:

```text
Refresh Token #1
   ↓
Validate token
   ↓
Generate Access Token #2
Generate Refresh Token #2
   ↓
Revoke Refresh Token #1
```

The old Refresh Token cannot be used again.

## Refresh Token Security

Refresh Tokens are generated using a cryptographically secure random number generator.

```csharp
RandomNumberGenerator.GetBytes(64);
```

The plain Refresh Token is returned to the client, but it is not stored directly in the database.

Instead:

```text
Plain Refresh Token
        ↓
      SHA-256
        ↓
Token Hash stored in database
```

When the client sends a Refresh Token, the server hashes it and searches for the corresponding hash in the database.

The server also verifies that the token:

- Exists
- Has not expired
- Has not been revoked

## Password Security

Passwords are never stored as plain text.

The project uses:

```csharp
PasswordHasher<User>
```

During registration, the password is hashed before being stored.

During login, the entered password is verified against the stored hash.

## API Endpoints

### Register

```http
POST /api/auth/register
```

Example request:

```json
{
  "email": "user@example.com",
  "password": "YourPassword123!"
}
```

### Login

```http
POST /api/auth/login
```

Example request:

```json
{
  "email": "user@example.com",
  "password": "YourPassword123!"
}
```

Example response:

```json
{
  "accessToken": "...",
  "refreshToken": "...",
  "accessTokenExpiresAtUtc": "...",
  "refreshTokenExpiresAtUtc": "..."
}
```

### Refresh Token

```http
POST /api/auth/refresh
```

Example request:

```json
{
  "refreshToken": "<refresh-token>"
}
```

If the Refresh Token is valid, the server returns a new Access Token and a new Refresh Token.

### Protected Expense Endpoint

```http
GET /api/expenses/{id}
Authorization: Bearer <access-token>
```

A valid Access Token is required to access protected endpoints.

## Global Exception Handling

The project uses custom middleware for centralized exception handling.

Example flow:

```text
Request
   ↓
Exception Middleware
   ↓
Authentication
   ↓
Authorization
   ↓
Controller
   ↓
Exception occurs
   ↓
Exception Middleware catches it
   ↓
HTTP Response
```

Examples of HTTP status codes used:

- `200 OK`
- `201 Created`
- `400 Bad Request`
- `401 Unauthorized`
- `404 Not Found`
- `409 Conflict`
- `500 Internal Server Error`

## Dependency Injection

Services are registered using ASP.NET Core Dependency Injection.

Example:

```csharp
builder.Services.AddScoped<ITokenService, TokenService>();
```

`TokenService` is registered as Scoped because it depends on `AppDbContext`.

## Technologies

- ASP.NET Core Web API
- .NET 10
- Entity Framework Core
- SQLite
- JWT Bearer Authentication
- Microsoft PasswordHasher
- SHA-256
- ASP.NET Core Middleware
- Dependency Injection
- Git
- GitHub

## Security

Sensitive values such as the JWT signing key are not stored in the repository.

For local development, the JWT signing key is stored using .NET User Secrets.

Example:

```bash
dotnet user-secrets set "Jwt:Key" "<your-secret-key>"
```

Local SQLite database files are excluded from Git using `.gitignore`.

```gitignore
*.db
*.db-shm
*.db-wal
```

## Project Purpose

This project was created to practice and demonstrate important ASP.NET Core backend concepts, including:

- JWT Authentication
- Access Tokens
- Refresh Tokens
- Refresh Token Rotation
- Password Hashing
- Claims
- Authorization
- Middleware
- Global Exception Handling
- Dependency Injection
- Entity Framework Core
- HTTP Status Codes
- Secure configuration management

