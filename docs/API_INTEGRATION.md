# Archive API — Integration Guide

Base URL (Production):

```text
https://archive-windows-app.onrender.com
```

Interactive Swagger UI:

```text
https://archive-windows-app.onrender.com/swagger/index.html
```

OpenAPI JSON:

```text
https://archive-windows-app.onrender.com/swagger/v1/swagger.json
```

> **Note:** The free Render plan may sleep after idle time. The first request after sleep can take 30–60 seconds.

---

## 1. Authentication overview

| Topic | Detail |
|--------|--------|
| Auth type | JWT Bearer |
| Header | `Authorization: Bearer {token}` |
| Token lifetime | 8 hours |
| Roles | `User`, `Admin`, `SuperAdmin` |

### Default seeded accounts

| Username | Password | Role | What they can do |
|----------|----------|------|------------------|
| `user` | `UserPass123!` | User | View books & categories only |
| `librarian` | `LibPass123!` | Admin | Manage books + list/delete users |
| `admin` | `AdminPass123!` | SuperAdmin | Full access + create users with any role |

### Permission matrix

| Action | User | Admin (librarian) | SuperAdmin (admin) |
|--------|------|-------------------|--------------------|
| Login / view books | Yes | Yes | Yes |
| Add / edit / delete books | No | Yes | Yes |
| List / delete users | No | Yes | Yes |
| Create user with role | No | No | Yes (`POST /api/users`) |
| Public register | Creates **User** only (after seed) | — | — |

Login response includes:

```json
{
  "token": "...",
  "role": "Admin",
  "username": "librarian",
  "permissions": {
    "canViewBooks": true,
    "canManageBooks": true,
    "canManageUsers": true,
    "canCreateUsers": false
  }
}
```

Also available: `GET /api/auth/me` (JWT required).

---

## 2. Quick start (integration flow)

1. Call `POST /api/auth/login` with username/password.
2. Read `token` from the response.
3. Send `Authorization: Bearer {token}` on every protected request.
4. Call books / categories / users endpoints as needed.

### Login example (cURL)

```bash
curl -X POST "https://archive-windows-app.onrender.com/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"username\":\"admin\",\"password\":\"AdminPass123!\"}"
```

### Login response

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "role": "SuperAdmin",
  "username": "admin"
}
```

### Authenticated request example

```bash
curl "https://archive-windows-app.onrender.com/api/books" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

## 3. Auth endpoints

### `GET /api/auth/setup`

Checks whether any user already exists.

- Auth: Public  
- Response: `true` | `false`

```bash
curl "https://archive-windows-app.onrender.com/api/auth/setup"
```

### `POST /api/auth/login`

- Auth: Public  
- Body:

```json
{
  "username": "admin",
  "password": "AdminPass123!"
}
```

- Success: `200` + `LoginResponse`  
- Failure: `401` `{ "error": "..." }`

### `POST /api/auth/register`

- Auth: Public  
- Body:

```json
{
  "username": "newuser",
  "password": "StrongPass123!",
  "role": "User"
}
```

- `role` optional values: `User`, `Admin`, `SuperAdmin`  
- Success: `200` user object  
- Failure: `400` `{ "error": "..." }`

---

## 4. Books endpoints (JWT required)

### `GET /api/books`

List / filter books.

Query params (optional):

| Param | Type | Description |
|-------|------|-------------|
| `search` | string | Search in title / author / notes |
| `category` | string | Exact category filter |

```bash
curl "https://archive-windows-app.onrender.com/api/books?search=تاريخ&category=أدب" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Book response object

```json
{
  "id": 1,
  "title": "كتاب تجريبي",
  "author": "مؤلف",
  "category": "تاريخ",
  "publisher": "دار النشر",
  "year": 2024,
  "language": "عربي",
  "notes": "ملاحظة",
  "coverUrl": null,
  "filePath": "C:\\books\\sample.pdf",
  "fileUrl": null,
  "createdAt": "2026-07-11 13:53"
}
```

### `POST /api/books`

Create a book.

```json
{
  "title": "كتاب جديد",
  "author": "مؤلف",
  "category": "تاريخ",
  "publisher": "دار",
  "year": 2024,
  "language": "عربي",
  "notes": "",
  "coverPath": null,
  "coverUrl": null,
  "filePath": null,
  "fileUrl": null
}
```

Notes:

- `title` is required (business-wise).
- `coverPath` is mapped to `coverUrl` on the server.
- `filePath` is stored and used by the Windows client to open local files.

### `PUT /api/books/{id}`

Update a book. `id` in URL must equal `id` in body.

```json
{
  "id": 1,
  "title": "كتاب معدل",
  "author": "مؤلف",
  "category": "تاريخ",
  "publisher": "دار",
  "year": 2025,
  "language": "عربي",
  "notes": "تم التعديل",
  "coverPath": null,
  "filePath": null
}
```

If `coverPath` / `filePath` are omitted or null, existing values are kept.

### `DELETE /api/books/{id}`

- Success: `204 No Content`
- Not found: `404`

```bash
curl -X DELETE "https://archive-windows-app.onrender.com/api/books/1" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## 5. Categories

### `GET /api/categories`

JWT required. Returns distinct categories from books.

```json
[
  { "id": 0, "name": "تاريخ" },
  { "id": 0, "name": "أدب" }
]
```

---

## 6. Users (Admin / SuperAdmin only)

### `GET /api/users`

### `GET /api/users/{id}`

### `DELETE /api/users/{id}`

Response example:

```json
{
  "id": 1,
  "username": "admin",
  "role": "SuperAdmin",
  "createdAt": "2026-07-11 13:11"
}
```

Unauthorized role → `403 Forbidden`.

---

## 7. Windows Desktop integration

The WinForms + WebView2 client talks to the API via `HttpClient`.

Config file (`appsettings.json` next to the `.exe`):

```json
{
  "ApiBaseUrl": "https://archive-windows-app.onrender.com/"
}
```

Client flow:

```text
UI (HTML/JS)
  → postMessage
  → Form1.cs host
  → HttpClient + Bearer token
  → https://archive-windows-app.onrender.com/api/...
```

Recommended client timeout: **≥ 100 seconds** (Render cold start).

Published app folder:

```text
publish/Archive.Desktop/Archive.Desktop.exe
```

---

## 8. Swagger usage

1. Open [Swagger UI](https://archive-windows-app.onrender.com/swagger/index.html).
2. Call `POST /api/auth/login`.
3. Copy `token`.
4. Click **Authorize**.
5. Paste token only (Swagger adds `Bearer ` automatically when scheme is HTTP bearer).
6. Call protected endpoints.

---

## 9. Error responses

| HTTP | Meaning |
|------|---------|
| `400` | Bad request / validation / business rule |
| `401` | Missing/invalid token or bad login |
| `403` | Authenticated but role not allowed |
| `404` | Resource not found |
| `204` | Success with no body (delete) |

Typical error body:

```json
{ "error": "رسالة الخطأ" }
```

---

## 10. C# client snippet

```csharp
var http = new HttpClient
{
    BaseAddress = new Uri("https://archive-windows-app.onrender.com/"),
    Timeout = TimeSpan.FromSeconds(100)
};

var login = await http.PostAsJsonAsync("api/auth/login", new
{
    username = "admin",
    password = "AdminPass123!"
});

var auth = await login.Content.ReadFromJsonAsync<LoginResponse>();
http.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", auth!.Token);

var books = await http.GetFromJsonAsync<List<BookResponse>>("api/books");
```

---

## 11. JavaScript (fetch) snippet

```javascript
const base = "https://archive-windows-app.onrender.com";

const loginRes = await fetch(`${base}/api/auth/login`, {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({ username: "admin", password: "AdminPass123!" })
});

const { token } = await loginRes.json();

const booksRes = await fetch(`${base}/api/books`, {
  headers: { Authorization: `Bearer ${token}` }
});

const books = await booksRes.json();
```

---

## 12. Checklist for integrators

- [ ] Use HTTPS base URL ending with `/` for `HttpClient.BaseAddress`
- [ ] Store JWT securely for the session
- [ ] Send Bearer header on books/categories/users calls
- [ ] Handle Render cold-start delays
- [ ] Treat `DELETE` success as `204`
- [ ] For Windows local files, send absolute `filePath` the desktop machine can open

---

## Support links

- Swagger: https://archive-windows-app.onrender.com/swagger/index.html
- Source: https://github.com/Lwz299/archive-windows-app
