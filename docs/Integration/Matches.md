# Matches Module — Frontend Integration Guide

## Overview

A **Match** is a game between two `TeamParticipation` entries (home and away) within a specific tournament round. Match events (goals, cards, etc.) are recorded separately via the Events module.

`RoundsClassified` tracks which teams are still active in competition — a team is eliminated when its `RoundsClassified` entry is soft-deleted (`active=false`).

**GET endpoints are public** — no authentication required.  
**POST / PUT / DELETE endpoints** require a valid JWT in `Authorization: Bearer <token>` or the `accessToken` cookie.

---

## Match Status Values

| Value | Name | Description |
|---|---|---|
| 0 | `Pending` | Scheduled, not yet started |
| 1 | `InGame` | Currently being played |
| 2 | `Finished` | Final score is set |
| 3 | `Cancelled` | Match cancelled |

## Match Round Values

| Value | Name |
|---|---|
| 1 | `Group` |
| 2 | `R16` |
| 3 | `QuarterFinals` |
| 4 | `SemiFinals` |
| 5 | `Final` |

---

## 1. List Matches

```http
GET /api/v1/matches?page=1&perPage=20&tournamentId={id}&round=1&status=0
```

**Query params**

| Param | Type | Required | Description |
|---|---|---|---|
| `page` | int | No | Page number |
| `perPage` | int | No | Items per page |
| `tournamentId` | uuid | No | Filter by tournament |
| `round` | int | No | Filter by `MatchRound` enum value |
| `status` | int | No | Filter by `MatchStatus` enum value |
| `teamParticipationId` | uuid | No | Filter matches involving a specific TeamParticipation (season-scoped) |
| `teamId` | uuid | No | Filter matches by global `Team.Id` — returns all matches (home **or** away) across every tournament that team has participated in |
| `active` | bool | No | Soft-delete filter |

**Response `200`**
```json
{
  "data": {
    "page": 1,
    "perPage": 20,
    "count": 48,
    "totalPages": 3,
    "data": [
      {
        "id": "uuid",
        "matchDay": 1,
        "scoreHomeTeam": 2,
        "scoreAwayTeam": 1,
        "matchDate": "2025-10-05T18:00:00Z",
        "field": "Camp Nou",
        "location": "Barcelona, Spain",
        "status": 2,
        "round": 1,
        "newMatchId": null,
        "homeTeamId": "team-participation-uuid-1",
        "awayTeamId": "team-participation-uuid-2",
        "homeTeam": { "id": "...", "name": "FC Barcelona", "logoUrl": null },
        "awayTeam": { "id": "...", "name": "Real Madrid CF", "logoUrl": null }
      }
    ]
  }
}
```

---

## 2. Get Match by ID

```http
GET /api/v1/matches/{id}
```

Includes `homeTeam`, `awayTeam`, and `events` navigation.

**Response `200`** — single match object.  
**Response `404`** `{ "error": "Match not found" }`

---

## 3. Create Match

```http
POST /api/v1/matches
Authorization: Bearer <token>
Content-Type: application/json
```

**Body**
```json
{
  "matchDay": 1,
  "matchDate": "2025-10-05T18:00:00Z",
  "field": "Camp Nou",
  "location": "Barcelona, Spain",
  "round": 1,
  "homeTeamId": "team-participation-uuid-1",
  "awayTeamId": "team-participation-uuid-2"
}
```

`scoreHomeTeam` and `scoreAwayTeam` default to `0`. `status` defaults to `Pending (0)`.

**Response `201`**

---

## 4. Update Match

```http
PUT /api/v1/matches
Authorization: Bearer <token>
Content-Type: application/json
```

**Body** (include `id` + all updatable fields)
```json
{
  "id": "match-uuid",
  "matchDay": 1,
  "matchDate": "2025-10-05T18:00:00Z",
  "field": "Camp Nou",
  "location": "Barcelona, Spain",
  "scoreHomeTeam": 2,
  "scoreAwayTeam": 1,
  "status": 2,
  "round": 1,
  "homeTeamId": "team-participation-uuid-1",
  "awayTeamId": "team-participation-uuid-2",
  "manualWinnerId": null
}
```

| Field | Type | Description |
|---|---|---|
| `status` | int | `0` Pending, `1` InGame, `2` Finished, `3` Cancelled |
| `manualWinnerId` | uuid? | Override winner for penalty shoot-outs. **Required** when a knockout round match ends in a draw (equal scores). Must be the `TeamParticipationId` of the winning team. |

**Response `200`**

> **Auto-advance side-effect:** When `status` is set to `Finished` for a knockout round match (R16, QF, SF, Final), the backend automatically:
> 1. Promotes the winning team's `RoundsClassified` to the next bracket slot (using `NextRoundKey`).
> 2. Soft-deletes the losing team's `RoundsClassified` entry.
> 
> If scores are tied and `manualWinnerId` is not provided, the update will fail with a validation error.

---

## 5. Delete Match

```http
DELETE /api/v1/matches/{id}?hardDelete=false
Authorization: Bearer <token>
```

**Response `204 No Content`**

---

## 6. Live Match Stream (SSE)

Subscribe to real-time score and status updates for a match.

```http
GET /api/v1/matches/{matchId}/live
```

> **Note:** No authentication is required for the live stream. It is fully public.

**Response headers**
```
Content-Type: text/event-stream
Cache-Control: no-cache
X-Accel-Buffering: no
```

**Events**

| Event name | Triggered when |
|---|---|
| `heartbeat` | Immediately on connect |
| `update` | Score changes or match status changes |

**`update` payload shapes**

*Score update (Goal event created):*
```json
{ "type": "score", "matchId": "uuid", "homeScore": 2, "awayScore": 1, "eventId": "uuid", "eventType": 0, "minute": 34, "favorableTo": 0 }
```

*Status update (match updated to InGame/Finished/etc.):*
```json
{ "type": "status", "matchId": "uuid", "status": "Finished", "homeScore": 2, "awayScore": 1 }
```

*Generic event (non-goal events — yellow card, free kick, etc.):*
```json
{ "type": "event", "matchId": "uuid", "eventId": "uuid", "eventType": 1, "minute": 55, "favorableTo": 1 }
```

**JavaScript usage**
```js
const es = new EventSource(`/api/v1/matches/${matchId}/live`);

es.addEventListener('update', (e) => {
  const payload = JSON.parse(e.data);
  console.log(payload.type, payload);
});

// Always clean up:
es.close();
```

---

## 7. RoundsClassified — Who Is Still Active?

`RoundsClassified` determines which teams are **still competing** in the tournament. A team is eliminated when its `RoundsClassified` entry is soft-deleted (`active=false`).

### List active teams in a round

```http
GET /api/v1/rounds-classified?tournamentId={id}&round=2&page=1&perPage=50
```

> By default only `active=true` (still competing) records are returned. Pass `active=false` to see eliminated teams.

**Response `200`**
```json
{
  "data": {
    "page": 1,
    "perPage": 50,
    "count": 16,
    "totalPages": 1,
    "data": [
      {
        "id": "uuid",
        "round": 2,
        "roundKey": "AA",
        "groupPosition": null,
        "teamParticipationId": "team-participation-uuid",
        "teamParticipation": {
          "id": "...",
          "name": "FC Barcelona",
          "tournamentId": "..."
        },
        "active": true
      }
    ]
  },
  "totalCount": 16,
  "activeCount": 16,
  "inactiveCount": 0
}
```

### Get a single entry

```http
GET /api/v1/rounds-classified/{id}
```

### Eliminate a team (soft-delete)

```http
DELETE /api/v1/rounds-classified/{id}
Authorization: Bearer <token>
```

---

## Typical Frontend Flow — Match Day

```
1. Load tournament matches
   → GET /api/v1/matches?tournamentId={id}&round=1

2. Open match detail (public view)
   → GET /api/v1/matches/{id}  (includes team names + events)
   → SSE: GET /api/v1/matches/{id}/live
      (subscribe for real-time score/status updates)

3. Start match (admin)
   → PUT /api/v1/matches  { id, status: 1 (InGame) }

4. Record events (admin)
   → POST /api/v1/events  (each goal auto-increments the score)

5. Finalise match (admin)
   → PUT /api/v1/matches  { id, status: 2 (Finished), manualWinnerId?: uuid if draw }
   (backend auto-advances winner and eliminates loser)

6. View updated bracket
   → GET /api/v1/tournaments/{id}/bracket
```
