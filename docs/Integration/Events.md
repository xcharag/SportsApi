# Events Module — Frontend Integration Guide

## Overview

A **Match Event** records something that happened during a match — a goal, a yellow card, a red card, a penalty, etc. Each event is linked to a `Match` and to a `Roster` entry (the specific player who performed the action in that tournament squad).

**GET endpoints are public** — no authentication required.  
**POST / PUT / DELETE endpoints** require a valid JWT in `Authorization: Bearer <token>` or the `accessToken` cookie.

---

## Event Type Values

| Value | Name | Description |
|---|---|---|
| 0 | `Goal` | A goal was scored |
| 1 | `YellowCard` | Yellow card shown |
| 2 | `RedCard` | Red card shown |
| 3 | `Penalty` | Penalty decision |
| 4 | `Offside` | Offside call |
| 5 | `Corner` | Corner kick |
| 6 | `FreeKick` | Free kick awarded |

## FavorableTo Values

Indicates which side benefits from the event (e.g. the team that scored, or the team that received the foul):

| Value | Name | Description |
|---|---|---|
| 0 | `Home` | Favors the home team |
| 1 | `Away` | Favors the away team |

---

## 1. List Events

```http
GET /api/v1/events?page=1&perPage=50&matchId={id}&eventType=0
```

**Query params**

| Param | Type | Required | Description |
|---|---|---|---|
| `page` | int | No | Page number |
| `perPage` | int | No | Items per page |
| `matchId` | uuid | No | Filter by match |
| `rosterId` | uuid | No | Filter by player roster entry |
| `eventType` | int | No | Filter by `EventType` enum value |
| `active` | bool | No | Soft-delete filter |

**Response `200`**
```json
{
  "data": {
    "page": 1,
    "perPage": 50,
    "count": 5,
    "totalPages": 1,
    "data": [
      {
        "id": "uuid",
        "matchId": "match-uuid",
        "rosterId": "roster-uuid",
        "eventType": 0,
        "favorableTo": 0,
        "minute": 23,
        "active": true,
        "roster": {
          "id": "...",
          "playerId": "...",
          "shirtNumber": 10,
          "shirtName": "MARTINEZ"
        }
      }
    ]
  }
}
```

---

## 2. Get Event by ID

```http
GET /api/v1/events/{id}
```

**Response `200`** — single event object with `roster` navigation.  
**Response `404`** `{ "error": "Event not found" }`

---

## 3. Record an Event

```http
POST /api/v1/events
Authorization: Bearer <token>
Content-Type: application/json
```

**Body**
```json
{
  "matchId": "match-uuid",
  "rosterId": "roster-uuid",
  "eventType": 0,
  "favorableTo": 0,
  "minute": 23
}
```

**Body fields**

| Field | Type | Required | Description |
|---|---|---|---|
| `matchId` | uuid | Yes | The match this event belongs to |
| `rosterId` | uuid | Yes | The Roster entry of the player who performed the action |
| `eventType` | int | Yes | `EventType` enum value (0–6) |
| `favorableTo` | int | Yes | Which team benefits: `0` = Home, `1` = Away |
| `minute` | int | Yes | Match minute when the event occurred |

**Response `201`**

> **Auto-score side-effect:** When `eventType = 0` (Goal), the backend automatically increments the parent match's score:
> - `favorableTo = 0` (Home) → `scoreHomeTeam += 1`
> - `favorableTo = 1` (Away) → `scoreAwayTeam += 1`
> 
> The score update is atomic (same database transaction as the event insert). Any clients subscribed to the match's SSE stream (`GET /api/v1/matches/{matchId}/live`) will receive a real-time `update` event of type `"score"` immediately after the event is recorded.

---

## 4. Update Event

```http
PUT /api/v1/events
Authorization: Bearer <token>
Content-Type: application/json
```

**Body**
```json
{
  "id": "event-uuid",
  "matchId": "match-uuid",
  "rosterId": "roster-uuid",
  "eventType": 0,
  "favorableTo": 0,
  "minute": 25
}
```

**Response `200`**

---

## 5. Delete Event

```http
DELETE /api/v1/events/{id}?hardDelete=false
Authorization: Bearer <token>
```

**Response `204 No Content`**

---

## Typical Frontend Flow — Live Match Tracking

```
1. Open a match in "InGame" state
   → GET /api/v1/matches/{id}  (get homeTeam / awayTeam with squad info)

2. Load existing events
   → GET /api/v1/events?matchId={id}

3. Load rosters to pick the player for an event
   → GET /api/v1/rosters?teamParticipationId={homeTeamId}
   → GET /api/v1/rosters?teamParticipationId={awayTeamId}

4. Record a goal
   → POST /api/v1/events  { matchId, rosterId, eventType: 0, minute: 23, favorableTo: 0 }

5. After final whistle — set scores and mark finished
   → PUT /api/v1/matches  { ...match, scoreHomeTeam: X, scoreAwayTeam: Y, status: 2 }
```

> **Tip:** To display goal scorers on the match card, query  
> `GET /api/v1/events?matchId={id}&eventType=0` and render `roster.shirtName + " " + minute + "'"`
