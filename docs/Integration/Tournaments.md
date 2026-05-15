# Tournaments Module — Frontend Integration Guide

## Overview

A **Tournament** is the top-level aggregate. It manages teams (via `TeamParticipation`), matches, and tracks team progression through rounds (`RoundsClassified`).

All endpoints require a valid JWT in `Authorization: Bearer <token>` or the `accessToken` cookie.

---

## 1. List Tournaments

```http
GET /api/v1/tournaments?page=1&perPage=20&name=Champions&active=true
Authorization: Bearer <token>
```

**Query params**

| Param | Type | Required | Description |
|---|---|---|---|
| `page` | int | No (default 1) | Page number |
| `perPage` | int | No (default 20) | Items per page |
| `name` | string | No | Partial-name filter (case-insensitive) |
| `active` | bool | No | `true` = only active, `false` = only inactive, omit = all |

**Response `200`**
```json
{
  "data": {
    "page": 1,
    "perPage": 20,
    "count": 5,
    "totalPages": 1,
    "data": [
      {
        "id": "uuid",
        "name": "Champions League 2025",
        "description": "...",
        "startDate": "2025-09-01T00:00:00Z",
        "endDate": "2026-05-31T00:00:00Z",
        "logoUrl": null,
        "bannerUrl": null,
        "active": true,
        "createdAt": "2025-01-10T12:00:00Z",
        "createdBy": "admin"
      }
    ]
  },
  "totalCount": 5,
  "activeCount": 3,
  "inactiveCount": 2
}
```

---

## 2. Get Tournament by ID

```http
GET /api/v1/tournaments/{id}
Authorization: Bearer <token>
```

**Response `200`** — single tournament object.

**Response `404`**
```json
{ "error": "Tournament not found" }
```

---

## 3. Create Tournament

```http
POST /api/v1/tournaments
Authorization: Bearer <token>
Content-Type: application/json
```

**Body**
```json
{
  "name": "Copa del Rey 2026",
  "description": "Annual cup competition",
  "startDate": "2026-01-10T00:00:00Z",
  "endDate": "2026-05-20T00:00:00Z",
  "logoUrl": null,
  "bannerUrl": null
}
```

**Response `201`**
```json
{
  "id": "new-uuid",
  "name": "Copa del Rey 2026"
}
```

---

## 4. Update Tournament

```http
PUT /api/v1/tournaments
Authorization: Bearer <token>
Content-Type: application/json
```

**Body** (all fields including `id`)
```json
{
  "id": "existing-uuid",
  "name": "Copa del Rey 2026 (Updated)",
  "description": "Updated description",
  "startDate": "2026-01-10T00:00:00Z",
  "endDate": "2026-05-25T00:00:00Z",
  "logoUrl": null,
  "bannerUrl": null
}
```

**Response `200`**

---

## 5. Delete Tournament

```http
DELETE /api/v1/tournaments/{id}?hardDelete=false
Authorization: Bearer <token>
```

`hardDelete=true` permanently removes the record. Default is soft-delete (sets `active=false`).

**Response `204 No Content`**

---

## 6. Register Teams into a Tournament

This is the **main action** for the tournament setup screen. Clicking "Register Teams" sends all selected teams at once.

The `tournamentId` is a **route parameter** — it is **not** included in the body.

```http
POST /api/v1/tournaments/{tournamentId}/team-participations
Authorization: Bearer <token>
Content-Type: application/json
```

**Body**
```json
{
  "teams": [
    {
      "teamId": "team-uuid-1",
      "name": "FC Barcelona",
      "logoUrl": "https://cdn.example.com/barca.png",
      "roundKey": "A",
      "groupPosition": 1
    },
    {
      "teamId": "team-uuid-2",
      "name": "Real Madrid CF",
      "logoUrl": null,
      "roundKey": "A",
      "groupPosition": 2
    },
    {
      "teamId": "team-uuid-3",
      "name": "Atletico de Madrid",
      "logoUrl": null,
      "roundKey": "B",
      "groupPosition": 1
    }
  ]
}
```

**Body fields**

| Field | Type | Required | Description |
|---|---|---|---|
| `teams[].teamId` | uuid | Yes | Global Team ID (from `GET /api/v1/teams`) |
| `teams[].name` | string | Yes | Display name for this tournament (can differ from the global `defaultName`) |
| `teams[].logoUrl` | string? | No | Override logo for this tournament season |
| `teams[].roundKey` | string | Yes | Group/bracket key: `"A"`, `"B"`, `"C"` in group stage; `"AA"`, `"AB"` from R16 onwards |
| `teams[].groupPosition` | int? | No | Seed position within the group (reserved for future auto-match generation) |

**Response `201`**
```json
{
  "registeredCount": 3,
  "teams": [
    {
      "teamParticipationId": "new-uuid-1",
      "teamId": "team-uuid-1",
      "name": "FC Barcelona",
      "roundKey": "A"
    },
    {
      "teamParticipationId": "new-uuid-2",
      "teamId": "team-uuid-2",
      "name": "Real Madrid CF",
      "roundKey": "A"
    },
    {
      "teamParticipationId": "new-uuid-3",
      "teamId": "team-uuid-3",
      "name": "Atletico de Madrid",
      "roundKey": "B"
    }
  ]
}
```

> **Note:** Each team gets one `TeamParticipation` + one `RoundsClassified` entry with `round = Group (1)`.  
> To advance teams to later rounds (R16, Quarter-Finals...) soft-delete the eliminated team's `RoundsClassified` entry.

---

## 7. Get Team Participations for a Tournament

```http
GET /api/v1/team-participations?tournamentId={id}&page=1&perPage=50
Authorization: Bearer <token>
```

Returns all teams registered in the specified tournament.

---

## 8. Get Active Teams per Round

```http
GET /api/v1/rounds-classified?tournamentId={id}&round=2&page=1&perPage=50
Authorization: Bearer <token>
```

Returns only `active=true` entries by default (teams still competing).

| `round` value | Stage |
|---|---|
| 1 | Group |
| 2 | R16 |
| 3 | Quarter-Finals |
| 4 | Semi-Finals |
| 5 | Final |

---

## Typical Frontend Flow

```
1. Tournament list screen
   → GET /api/v1/tournaments

2. Create tournament
   → POST /api/v1/tournaments

3. Tournament setup — "Register Teams" step
   → GET /api/v1/teams  (show multi-select list)
   → POST /api/v1/tournaments/{tournamentId}/team-participations  (confirm selection)

4. View registered teams / round bracket
   → GET /api/v1/team-participations?tournamentId={id}
   → GET /api/v1/rounds-classified?tournamentId={id}&round=1

5. Advance to next round (eliminate losers)
   → DELETE /api/v1/rounds-classified/{id}  (soft-delete = team eliminated)
```
