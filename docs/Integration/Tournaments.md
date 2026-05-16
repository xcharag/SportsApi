# Tournaments Module — Frontend Integration Guide

## Overview

A **Tournament** is the top-level aggregate. It manages teams (via `TeamParticipation`), matches, and tracks team progression through rounds (`RoundsClassified`).

**GET endpoints are public** — no authentication required.  
**POST / PUT / DELETE endpoints** require a valid JWT in `Authorization: Bearer <token>` or the `accessToken` cookie.

---

## 1. List Tournaments

```http
GET /api/v1/tournaments?page=1&perPage=20&name=Champions&active=true
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
  "bannerUrl": null,
  "teamsPerGroupThatClassify": 2
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `teamsPerGroupThatClassify` | int | No (default 2) | Number of teams per group that advance to the knockout stage |

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
| `teams[].nextRoundKey` | string? | No | The bracket slot the winner of this team's R16 match will occupy. Required for knockout seeding. |
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
> The `nextRoundKey` value is stored on the `RoundsClassified` entry and is used by the **auto-advance** logic when a knockout match finishes.

---

## 9. Group Stage Standings

Returns group standings sorted by points, then goal difference, then goals scored.

```http
GET /api/v1/tournaments/{tournamentId}/standings
GET /api/v1/tournaments/{tournamentId}/standings?groupKey=A
```

**Query params**

| Param | Type | Required | Description |
|---|---|---|---|
| `groupKey` | string | No | Filter to a single group (`A`, `B`, etc.). Omit for all groups. |

**Response `200`**
```json
{
  "groups": [
    {
      "groupKey": "A",
      "standings": [
        {
          "teamParticipationId": "uuid",
          "displayName": "FC Barcelona",
          "logoUrl": null,
          "played": 3,
          "won": 2,
          "drawn": 1,
          "lost": 0,
          "goalsFor": 7,
          "goalsAgainst": 2,
          "goalDifference": 5,
          "points": 7
        }
      ]
    }
  ]
}
```

---

## 10. Playoff Bracket

Returns the knockout bracket for rounds R16 through Final. Each round contains slots; each slot has a home entry, away entry, the match (if created), and the winner (if the match is finished).

```http
GET /api/v1/tournaments/{tournamentId}/bracket
```

**Response `200`**
```json
{
  "rounds": [
    {
      "round": 2,
      "roundName": "Round of 16",
      "slots": [
        {
          "winnerAdvancesTo": "QF-1",
          "homeEntry": { "teamParticipationId": "uuid", "displayName": "FC Barcelona", "logoUrl": null, "roundKey": "R16-1", "isActive": true },
          "awayEntry": { "teamParticipationId": "uuid", "displayName": "Real Madrid", "logoUrl": null, "roundKey": "R16-2", "isActive": true },
          "match": { "id": "uuid", "homeScore": 2, "awayScore": 1, "status": 2 },
          "winner": { "teamParticipationId": "uuid", "displayName": "FC Barcelona", "roundKey": "R16-1", "isActive": true }
        }
      ]
    }
  ]
}
```

---

## 11. Top Scorers

Returns the top scorers for the tournament ranked by goals (dense rank — tied players share the same rank).

```http
GET /api/v1/tournaments/{tournamentId}/top-scorers?limit=10
```

**Query params**

| Param | Type | Required | Description |
|---|---|---|---|
| `limit` | int | No (default 10) | Maximum rows to return |

**Response `200`**
```json
{
  "topScorers": [
    {
      "rank": 1,
      "playerId": "uuid",
      "playerName": "Lionel Messi",
      "rosterId": "uuid",
      "teamParticipationId": "uuid",
      "teamName": "FC Barcelona",
      "shirtName": "MESSI",
      "shirtNumber": 10,
      "goals": 7
    }
  ]
}
```

---

## Typical Frontend Flow

```
1. Tournament list screen
   → GET /api/v1/tournaments

2. Create tournament
   → POST /api/v1/tournaments  (include teamsPerGroupThatClassify)

3. Tournament setup — "Register Teams" step
   → GET /api/v1/teams  (show multi-select list)
   → POST /api/v1/tournaments/{tournamentId}/team-participations  (include nextRoundKey per team for knockout seeding)

4. View registered teams / round bracket
   → GET /api/v1/team-participations?tournamentId={id}
   → GET /api/v1/rounds-classified?tournamentId={id}&round=1

5. Group stage standings
   → GET /api/v1/tournaments/{id}/standings

6. Playoff bracket view
   → GET /api/v1/tournaments/{id}/bracket

7. Top scorers widget
   → GET /api/v1/tournaments/{id}/top-scorers?limit=5

8. Advance to next round (knockout) — happens automatically
   When a knockout match status → Finished:
     - Backend promotes winner's RC to next round (using NextRoundKey)
     - Backend soft-deletes loser's RC (Active=false)
   No frontend action required.
```
