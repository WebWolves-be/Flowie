# Flowie Project

> **AUTO-UPDATE**: When you modify code patterns or discover new conventions, update this file.

## Project Structure

```
Flowie/
├── backend/           → .NET 8 Minimal API (see backend/CLAUDE.md)
├── frontend/          → Angular app (see frontend/flowie-app/CLAUDE.md)
└── .playwright/       → Playwright CLI config & test scripts
```

## Running Services

| Service  | URL                          |
|----------|------------------------------|
| Backend  | `http://localhost:5229`      |
| Swagger  | `http://localhost:5229/swagger` |
| Frontend | `https://localhost:4200` (self-signed SSL) |

## Test Credentials

| Field    | Value                        |
|----------|------------------------------|
| Email    | `claude.code@testing.be`     |
| Password | `iK845)%U$UYdn25`           |

---

## Self-Validation: Test After Every Change

After implementing or modifying code, **always validate your work**. Do not consider a task complete until you have verified it works.

- **Backend change?** → Test via curl / Swagger. See `backend/CLAUDE.md` for details.
- **Frontend change?** → Test via Playwright CLI or Chrome DevTools MCP. See `frontend/flowie-app/CLAUDE.md` for details.
- **Full-stack change?** → Test both.
