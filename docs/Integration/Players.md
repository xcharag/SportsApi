# Players Module — Frontend Integration Guide

## Overview

A **Player** is a global catalogue entry. Players are enrolled into tournaments via **Rosters** — each `Roster` ties a `Player` to a specific `TeamParticipation` (a team in a specific tournament). This means the same player can wear different shirt numbers or shirt names across tournament seasons.

All endpoints require a valid JWT in `Authorization: Bearer <token>` or the `accessToken` cookie.

---

## 1. List Players

```http
GET /api/v1/players?page=1&perPage=20&name=Martinez&active=true
Authorization: Bearer <token>
```

**Query params**

| Param | Type | Required | Description |
|---|---|---|---|
| `page` | int | No (default 1) | Page number |
| `perPage` | int | No (default 20) | Items per page |
| `name` | string | No | Partial `fullName` filter (case-insensitive) |
| `active` | bool | No | Filter by active status |

**Response `200`**
```json
{
  "data": {
    "page": 1,
    "perPage": 20,
    "count": 200,
    "totalPages": 10,
    "data": [
      {
        "id": "uuid",
        "fullName": "Lionel Martinez",
        "ci": "12345678",
        "phoneNumber": "+1-555-0100",
        "isForeigner": false,
        "active": true,
        "createdAt": "2025-01-10T12:00:00Z",
        "createdBy": "admin"
      }
    ]
  },
  "totalCount": 200,
  "activeCount": 190,
  "inactiveCount": 10
}
```

---

## 2. Get Player by ID

```http
GET /api/v1/players/{id}
Authorization: Bearer <token>
```

**Response `200`** — single player object.  
**Response `404`** `{ "error": "Player not found" }`

---

## 3. Create Player

```http
POST /api/v1/players
Authorization: Bearer <token>
Content-Type: application/json
```

**Body**
```json
{
  "fullName": "Lionel Martinez",
  "ci": "12345678",
  "phoneNumber": "+1-555-0100",
  "isForeigner": false
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `fullName` | string | Yes | Player's full name |
| `ci` | string? | No | National identity / document number |
| `phoneNumber` | string? | No | Contact phone number |
| `isForeigner` | bool | Yes | Whether the player is a foreign national |

**Response `201`**
```json
{
  "id": "new-uuid",
  "fullName": "Lionel Martinez"
}
```

---

## 4. Update Player

```http
PUT /api/v1/players
Authorization: Bearer <token>
Content-Type: application/json
```

**Body**
```json
{
  "id": "existing-uuid",
  "fullName": "Lionel Martinez",
  "ci": "12345678",
  "phoneNumber": "+1-555-0199",
  "isForeigner": false
}
```

**Response `200`**

---

## 5. Delete Player

```http
DELETE /api/v1/players/{id}?hardDelete=false
Authorization: Bearer <token>
```

**Response `204 No Content`**

---

## 6. Enroll Players into a Team-in-Tournament

This is the **main action** for the roster management screen. After a team is registered in a tournament, open its roster and add players in bulk.

The `teamParticipationId` is a **route parameter** — it is **not** included in the body.

```http
POST /api/v1/team-participations/{teamParticipationId}/rosters
Authorization: Bearer <token>
Content-Type: application/json
```

**Body**
```json
{
  "players": [
    {
      "playerId": "player-uuid-1",
      "shirtNumber": 10,
      "shirtName": "MARTINEZ"
    },
    {
      "playerId": "player-uuid-2",
      "shirtNumber": 9,
      "shirtName": "SUAREZ"
    }
  ]
}
```

**Body fields**

| Field | Type | Required | Description |
|---|---|---|---|
| `players[].playerId` | uuid | Yes | Global Player ID (from `GET /api/v1/players`) |
| `players[].shirtNumber` | int? | No | Shirt number for this tournament |
| `players[].shirtName` | string? | No | Name printed on the shirt (may differ from `fullName`) |

**Response `201`**
```json
{
  "enrolledCount": 2,
  "players": [
    {
      "rosterId": "new-roster-uuid-1",
      "playerId": "player-uuid-1",
      "shirtNumber": 10,
      "shirtName": "MARTINEZ"
    },
    {
      "rosterId": "new-roster-uuid-2",
      "playerId": "player-uuid-2",
      "shirtNumber": 9,
      "shirtName": "SUAREZ"
    }
  ]
}
```

---

## 7. List Roster Entries

```http
GET /api/v1/rosters?teamParticipationId={id}&page=1&perPage=50
Authorization: Bearer <token>
```

Returns all players enrolled for a given `TeamParticipation` — i.e. the squad for that team in that tournament.

---

## 8. Update or Remove a Roster Entry

```http
PUT /api/v1/rosters
Authorization: Bearer <token>
Content-Type: application/json

Body: { "id": "roster-uuid", "shirtNumber": 11, "shirtName": "MARTINEZ" }
```

```http
DELETE /api/v1/rosters/{id}
Authorization: Bearer <token>
```

---

## 9. Player Stats

Returns aggregated stats for a player across all tournaments: goals, cards, and other event counts.

```http
GET /api/v1/players/{id}/stats
Authorization: Bearer <token>
```

**Response `200`**
```json
{
  "playerId": "uuid",
  "fullName": "Lionel Martinez",
  "career": {
    "goals": 42,
    "yellowCards": 5,
    "redCards": 0,
    "penalties": 3,
    "offsides": 7,
    "corners": 0,
    "freeKicks": 12
  },
  "tournaments": [
    {
      "tournamentId": "uuid",
      "tournamentName": "Copa del Rey 2026",
      "teamParticipationId": "uuid",
      "teamName": "FC Barcelona",
      "shirtName": "MESSI",
      "shirtNumber": 10,
      "stats": {
        "goals": 7,
        "yellowCards": 1,
        "redCards": 0,
        "penalties": 1,
        "offsides": 2,
        "corners": 0,
        "freeKicks": 3
      }
    }
  ]
}
```

---

## 10. Player Profile

Returns the full player profile: every team the player has played for, the events they produced in each team, and career-level totals.

```http
GET /api/v1/players/{id}/profile
Authorization: Bearer <token>
```

**Response `200`**
```json
{
  "playerId": "uuid",
  "fullName": "Lionel Martinez",
  "ci": "12345678",
  "phoneNumber": "+1-555-0100",
  "isForeigner": false,
  "teams": [
    {
      "teamParticipationId": "uuid",
      "teamName": "FC Barcelona",
      "logoUrl": "https://cdn.example.com/barca.png",
      "tournamentId": "uuid",
      "tournamentName": "Copa del Rey 2026",
      "shirtName": "MESSI",
      "shirtNumber": 10,
      "events": [
        {
          "eventId": "uuid",
          "eventType": 0,
          "minute": 34,
          "matchId": "uuid"
        }
      ]
    }
  ],
  "career": {
    "goals": 42,
    "yellowCards": 5,
    "redCards": 0,
    "penalties": 3,
    "offsides": 7,
    "corners": 0,
    "freeKicks": 12
  }
}
```

---

## Typical Frontend Flow

```
1. Player catalogue screen
   → GET /api/v1/players  (list/search players)

2. Create / edit player
   → POST /api/v1/players
   → PUT  /api/v1/players

3. Tournament team detail — Roster tab
   → GET /api/v1/players  (multi-select list)
   → POST /api/v1/team-participations/{teamParticipationId}/rosters  (confirm with shirt info)

4. Roster viewer
   → GET /api/v1/rosters?teamParticipationId={id}  (show full squad)

5. Correct a shirt number / name
   → PUT /api/v1/rosters

6. View player profile page
   → GET /api/v1/players/{id}/profile  (teams, events, career)
   → GET /api/v1/players/{id}/stats    (per-tournament stat breakdown)
```
