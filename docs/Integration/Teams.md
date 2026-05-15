# Teams Module — Frontend Integration Guide

## Overview

A **Team** is a global catalogue entry (the real-world organisation). It can participate in multiple tournaments across multiple seasons. The user creates and manages teams independently of any tournament.

When a team enters a specific tournament, use the **Register Teams** endpoint under the Tournaments module (`POST /api/v1/tournaments/{tournamentId}/team-participations`) — this creates a `TeamParticipation` for that season.

All endpoints require a valid JWT in `Authorization: Bearer <token>` or the `accessToken` cookie.

---

## 1. List Teams

```http
GET /api/v1/teams?page=1&perPage=20&name=Barcelona&active=true
Authorization: Bearer <token>
```

**Query params**

| Param | Type | Required | Description |
|---|---|---|---|
| `page` | int | No (default 1) | Page number |
| `perPage` | int | No (default 20) | Items per page |
| `name` | string | No | Partial `defaultName` filter (case-insensitive) |
| `active` | bool | No | Filter by active status |

**Response `200`**
```json
{
  "data": {
    "page": 1,
    "perPage": 20,
    "count": 32,
    "totalPages": 2,
    "data": [
      {
        "id": "uuid",
        "defaultName": "FC Barcelona",
        "defaultLogoUrl": "https://cdn.example.com/barca.png",
        "active": true,
        "createdAt": "2025-01-10T12:00:00Z",
        "createdBy": "admin"
      }
    ]
  },
  "totalCount": 32,
  "activeCount": 28,
  "inactiveCount": 4
}
```

---

## 2. Get Team by ID

```http
GET /api/v1/teams/{id}
Authorization: Bearer <token>
```

**Response `200`** — single team object.  
**Response `404`** `{ "error": "Team not found" }`

---

## 3. Create Team

```http
POST /api/v1/teams
Authorization: Bearer <token>
Content-Type: application/json
```

**Body**
```json
{
  "defaultName": "FC Barcelona",
  "defaultLogoUrl": "https://cdn.example.com/barca.png"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `defaultName` | string | Yes | The team's canonical name in the global catalogue |
| `defaultLogoUrl` | string? | No | Default logo URL (can be overridden per tournament) |

**Response `201`**
```json
{
  "id": "new-uuid",
  "defaultName": "FC Barcelona",
  "defaultLogoUrl": "https://cdn.example.com/barca.png"
}
```

---

## 4. Update Team

```http
PUT /api/v1/teams
Authorization: Bearer <token>
Content-Type: application/json
```

**Body**
```json
{
  "id": "existing-uuid",
  "defaultName": "FC Barcelona (Updated)",
  "defaultLogoUrl": "https://cdn.example.com/barca-new.png"
}
```

**Response `200`**

---

## 5. Delete Team

```http
DELETE /api/v1/teams/{id}?hardDelete=false
Authorization: Bearer <token>
```

Default is soft-delete (`active=false`). Pass `hardDelete=true` for permanent removal.

**Response `204 No Content`**

---

## 6. View Tournament History for a Team

Use the TeamParticipations endpoint filtered by `teamId` to see all tournaments a team has entered:

```http
GET /api/v1/team-participations?teamId={id}&page=1&perPage=20
Authorization: Bearer <token>
```

Each result includes the `tournamentId`, the display `name` and `logoUrl` used for that season.

---

## Typical Frontend Flow

```
1. Team catalogue screen
   → GET /api/v1/teams  (list/search teams)

2. Create / edit team
   → POST /api/v1/teams
   → PUT  /api/v1/teams

3. Tournament setup — "Register Teams" step
   → GET /api/v1/teams  (show multi-select list)
   → POST /api/v1/tournaments/{tournamentId}/team-participations  (confirm selection)

4. View a team's tournament history
   → GET /api/v1/team-participations?teamId={id}

5. View a team's current round status in a tournament
   → GET /api/v1/rounds-classified?teamParticipationId={teamParticipationId}
```
