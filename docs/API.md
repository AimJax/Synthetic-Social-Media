# API Documentation

## Synthetic Social World - REST and WebSocket API

---

## Base Configuration

- **Base URL**: `http://localhost:5000/api`
- **Content-Type**: `application/json`
- **Authentication**: Bearer token in Authorization header

---

## Authentication

### POST /api/auth/login
Login or register as a player.

**Request:**
```json
{
  "deviceId": "unique-device-id",
  "displayName": "PlayerName"
}
```

**Response (200):**
```json
{
  "playerId": "uuid",
  "token": "jwt-token",
  "world": {
    "id": "world-uuid",
    "name": "Synthetic Social World",
    "currentTime": "2025-09-03T14:30:00Z",
    "isPaused": false,
    "speed": 1.0
  }
}
```

### POST /api/auth/refresh
Refresh authentication token.

**Headers:** `Authorization: Bearer {token}`

**Response (200):**
```json
{
  "token": "new-jwt-token",
  "expiresAt": "2025-09-03T15:30:00Z"
}
```

---

## World

### GET /api/world
Get current world state.

**Headers:** `Authorization: Bearer {token}`

**Response (200):**
```json
{
  "id": "world-uuid",
  "name": "Synthetic Social World",
  "currentTime": "2025-09-03T14:30:00Z",
  "isPaused": false,
  "speed": 1.0,
  "npcCount": 20,
  "communityCount": 5,
  "activeEventCount": 2
}
```

### POST /api/world/speed
Change simulation speed (dev only).

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "speed": 10.0
}
```

### POST /api/world/pause
Pause simulation.

### POST /api/world/resume
Resume simulation.

---

## Feed

### GET /api/feed
Get personalized feed.

**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| cursor | string | null | Pagination cursor |
| limit | int | 20 | Items per page (max 50) |

**Response (200):**
```json
{
  "items": [
    {
      "id": "post-uuid",
      "author": {
        "id": "npc-uuid",
        "handle": "sarah_dev",
        "displayName": "Sarah",
        "avatarUrl": "https://...",
        "popularity": 450.5
      },
      "content": "Just shipped a new feature!",
      "community": {
        "id": "community-uuid",
        "name": "Gaming",
        "handle": "gaming"
      },
      "createdAt": "2025-09-03T14:25:00Z",
      "likeCount": 42,
      "dislikeCount": 3,
      "commentCount": 15,
      "shareCount": 8,
      "viewCount": 234,
      "hasLiked": false,
      "hasDisliked": false,
      "hasShared": false
    }
  ],
  "nextCursor": "base64-encoded-cursor",
  "hasMore": true
}
```

### POST /api/feed/refresh
Force feed refresh (invalidates cache).

---

## Posts

### GET /api/posts/{id}
Get single post with comments.

**Response (200):**
```json
{
  "id": "post-uuid",
  "author": { ... },
  "content": "...",
  "comments": [
    {
      "id": "comment-uuid",
      "author": { ... },
      "content": "Great post!",
      "createdAt": "...",
      "likeCount": 5,
      "replies": [ ... ]
    }
  ]
}
```

### POST /api/posts
Create a new post.

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "content": "My thoughts on today's events...",
  "communityId": "community-uuid (optional)"
}
```

**Response (201):**
```json
{
  "id": "new-post-uuid",
  "createdAt": "2025-09-03T14:30:00Z"
}
```

### DELETE /api/posts/{id}
Delete own post.

---

## Comments

### POST /api/posts/{postId}/comments
Add comment to post.

**Request:**
```json
{
  "content": "I totally agree with this!",
  "parentCommentId": "uuid (optional, for replies)"
}
```

### POST /api/comments/{id}/like
Like a comment.

### POST /api/comments/{id}/dislike
Dislike a comment.

---

## Engagement

### POST /api/posts/{id}/like
Like a post.

**Response (200):**
```json
{
  "success": true,
  "newLikeCount": 43,
  "newDislikeCount": 3
}
```

### POST /api/posts/{id}/dislike
Dislike a post.

### POST /api/posts/{id}/share
Share a post (increments share count, notifies followers).

---

## NPCs / Users

### GET /api/users/{id}
Get NPC/player profile.

**Response (200):**
```json
{
  "id": "npc-uuid",
  "handle": "sarah_dev",
  "displayName": "Sarah",
  "bio": "Software developer and gamer",
  "avatarUrl": "https://...",
  "isPlayer": false,
  "popularity": 450.5,
  "followerCount": 1234,
  "followingCount": 567,
  "postCount": 89,
  "createdAt": "2025-01-15T10:00:00Z",
  "lastActiveAt": "2025-09-03T14:25:00Z",
  "personality": {
    "openness": 0.72,
    "extroversion": 0.65,
    "agreeableness": 0.58,
    "aggression": 0.42,
    "humor": 0.78
  },
  "interests": [
    { "topic": "gaming", "weight": 0.9 },
    { "topic": "technology", "weight": 0.85 }
  ],
  "communities": [
    { "id": "uuid", "name": "Gaming", "role": "member" }
  ],
  "relationship": {
    "affinity": 0.3,
    "trust": 0.5,
    "hostility": 0.0,
    "familiarity": 0.8
  }
}
```

### GET /api/users/{id}/posts
Get posts by user.

**Query Parameters:**
- `cursor`: string
- `limit`: int (default 20)

### GET /api/users/{id}/followers
Get user's followers.

### GET /api/users/{id}/following
Get users that this user follows.

---

## Follow

### POST /api/users/{id}/follow
Follow a user.

### DELETE /api/users/{id}/follow
Unfollow a user.

---

## Communities

### GET /api/communities
List communities.

**Query Parameters:**
- `cursor`: string
- `limit`: int
- `sort`: "popular" | "active" | "new" (default: "popular")

**Response (200):**
```json
{
  "items": [
    {
      "id": "community-uuid",
      "name": "Gaming",
      "handle": "gaming",
      "topic": "gaming",
      "description": "For all gamers",
      "memberCount": 5420,
      "popularity": 850.5,
      "isMember": true
    }
  ],
  "nextCursor": "...",
  "hasMore": true
}
```

### GET /api/communities/{id}
Get community details.

### POST /api/communities
Create a community.

**Request:**
```json
{
  "name": "New Community",
  "handle": "new-community",
  "topic": "general",
  "description": "Community description"
}
```

### POST /api/communities/{id}/join
Join a community.

### POST /api/communities/{id}/leave
Leave a community.

### GET /api/communities/{id}/posts
Get posts in community.

### GET /api/communities/{id}/members
Get community members.

---

## Events

### GET /api/events
List upcoming events.

**Query Parameters:**
- `cursor`: string
- `limit`: int
- `communityId`: string (optional filter)

### GET /api/events/{id}
Get event details.

### POST /api/events
Create an event.

**Request:**
```json
{
  "title": "Gaming Night",
  "description": "Let's play together!",
  "communityId": "uuid (optional)",
  "eventType": "meetup",
  "location": "Online",
  "startTime": "2025-09-10T20:00:00Z",
  "endTime": "2025-09-10T23:00:00Z",
  "maxAttendees": 50
}
```

### POST /api/events/{id}/attend
Attend an event.

### DELETE /api/events/{id}/attend
Cancel attendance.

---

## Messages (DMs)

### GET /api/messages
Get conversations list.

### GET /api/messages/{userId}
Get DM conversation with user.

**Query Parameters:**
- `cursor`: string
- `limit`: int

**Response (200):**
```json
{
  "user": {
    "id": "npc-uuid",
    "handle": "sarah_dev",
    "displayName": "Sarah"
  },
  "messages": [
    {
      "id": "msg-uuid",
      "senderId": "npc-uuid",
      "content": "Hey!",
      "createdAt": "2025-09-03T14:00:00Z",
      "isRead": true
    }
  ],
  "nextCursor": "...",
  "hasMore": true
}
```

### POST /api/messages/{userId}
Send DM to user.

**Request:**
```json
{
  "content": "Hello! How are you?"
}
```

### POST /api/messages/{messageId}/read
Mark message as read.

---

## Notifications

### GET /api/notifications
Get notifications.

**Query Parameters:**
- `unreadOnly`: bool (default: false)
- `limit`: int

**Response (200):**
```json
{
  "items": [
    {
      "id": "notif-uuid",
      "type": "like",
      "title": "Sarah liked your post",
      "body": "...",
      "relatedEntityId": "post-uuid",
      "relatedEntityType": "post",
      "isRead": false,
      "createdAt": "2025-09-03T14:25:00Z"
    }
  ],
  "unreadCount": 15
}
```

### POST /api/notifications/{id}/read
Mark notification as read.

### POST /api/notifications/read-all
Mark all notifications as read.

---

## Search

### GET /api/search
Search for users, communities, posts.

**Query Parameters:**
- `q`: string (search query)
- `type`: "all" | "users" | "communities" | "posts" (default: "all")
- `cursor`: string
- `limit`: int

**Response (200):**
```json
{
  "users": [
    { "id": "...", "handle": "...", "displayName": "...", "avatarUrl": "..." }
  ],
  "communities": [
    { "id": "...", "name": "...", "handle": "...", "memberCount": 123 }
  ],
  "posts": [
    { "id": "...", "author": {...}, "content": "...", "createdAt": "..." }
  ]
}
```

---

## Catch-Up

### GET /api/catchup
Get summary of events while player was offline.

**Response (200):**
```json
{
  "since": "2025-09-03T08:00:00Z",
  "until": "2025-09-03T14:30:00Z",
  "summary": {
    "followerChanges": [
      { "npcId": "sarah-uuid", "npcName": "Sarah", "change": "+83" }
    ],
    "communityChanges": [
      { "communityId": "gaming-uuid", "name": "Gaming", "status": "trending" }
    ],
    "drama": [
      { "description": "Mike and Jessica had a public argument", "severity": "medium" }
    ],
    "engagement": {
      "likes": 156,
      "comments": 23,
      "shares": 5
    },
    "dms": [
      { "npcId": "sarah-uuid", "npcName": "Sarah", "count": 2 }
    ],
    "rumors": [
      { "community": "Gaming", "description": "A rumor about you appeared" }
    ]
  }
}
```

---

## WebSocket Events

### Connection
**Endpoint:** `ws://localhost:5000/ws`

**Authentication:** Token in first message or query param

```javascript
// First message: authenticate
ws.send(JSON.stringify({ type: "auth", token: "jwt-token" }));

// Response
ws.onmessage = (event) => {
  const msg = JSON.parse(event.data);
  // Handle different event types
};
```

### Event Types (Server → Client)

#### FeedUpdate
New post from followed user.
```json
{
  "type": "FeedUpdate",
  "data": {
    "post": { ... }
  }
}
```

#### NotificationCreated
New notification.
```json
{
  "type": "NotificationCreated",
  "data": {
    "notification": { ... }
  }
}
```

#### MessageReceived
New DM.
```json
{
  "type": "MessageReceived",
  "data": {
    "conversationId": "npc-uuid",
    "message": { ... }
  }
}
```

#### CommentCreated
New comment on your post.
```json
{
  "type": "CommentCreated",
  "data": {
    "postId": "post-uuid",
    "comment": { ... }
  }
}
```

#### PostEngagementChanged
Like/dislike/share counts changed.
```json
{
  "type": "PostEngagementChanged",
  "data": {
    "postId": "post-uuid",
    "likeCount": 44,
    "dislikeCount": 3,
    "commentCount": 16,
    "shareCount": 9
  }
}
```

#### FollowerChanged
Follower count changed.
```json
{
  "type": "FollowerChanged",
  "data": {
    "npcId": "sarah-uuid",
    "change": "+1",
    "newCount": 1235
  }
}
```

#### SocialEventTriggered
NPC drama or important event.
```json
{
  "type": "SocialEventTriggered",
  "data": {
    "eventType": "argument",
    "participants": ["mike-uuid", "jessica-uuid"],
    "description": "Mike and Jessica are arguing in Gaming",
    "importance": 0.6
  }
}
```

### Event Types (Client → Server)

#### SubscribeToFeed
```json
{
  "type": "SubscribeToFeed"
}
```

#### UnsubscribeFromFeed
```json
{
  "type": "UnsubscribeFromFeed"
}
```

#### MarkAsRead
```json
{
  "type": "MarkAsRead",
  "notificationId": "notif-uuid"
}
```

#### Typing
```json
{
  "type": "Typing",
  "conversationId": "npc-uuid",
  "isTyping": true
}
```

---

## Error Responses

### 400 Bad Request
```json
{
  "error": "validation_error",
  "message": "Content cannot be empty",
  "field": "content"
}
```

### 401 Unauthorized
```json
{
  "error": "unauthorized",
  "message": "Invalid or expired token"
}
```

### 403 Forbidden
```json
{
  "error": "forbidden",
  "message": "You cannot perform this action"
}
```

### 404 Not Found
```json
{
  "error": "not_found",
  "message": "Resource not found"
}
```

### 429 Too Many Requests
```json
{
  "error": "rate_limited",
  "message": "Too many requests",
  "retryAfter": 60
}
```

### 500 Internal Server Error
```json
{
  "error": "internal_error",
  "message": "An unexpected error occurred",
  "requestId": "uuid"
}
```

---

## Rate Limits

| Endpoint | Limit |
|----------|-------|
| POST /api/posts | 10 per minute |
| POST /api/comments | 30 per minute |
| POST /api/messages | 20 per minute |
| POST /api/users/{id}/follow | 15 per minute |
| GET /api/feed | 60 per minute |
| Search | 30 per minute |

---

## Related Documents

- [ARCHITECTURE.md](./ARCHITECTURE.md) - System overview
- [ANDROID.md](./ANDROID.md) - Client architecture
- [FEED_SYSTEM.md](./FEED_SYSTEM.md) - Feed ranking
