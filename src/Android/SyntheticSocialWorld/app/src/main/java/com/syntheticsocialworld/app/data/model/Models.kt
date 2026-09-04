package com.syntheticsocialworld.app.data.model

import com.google.gson.annotations.SerializedName

// API Response models
data class HealthResponse(
    val status: String,
    val timestamp: String
)

data class ApiInfoResponse(
    val name: String,
    val version: String,
    val description: String
)

// NPC models
data class NPCDto(
    val id: String,
    val handle: String,
    @SerializedName("displayName") val displayName: String,
    val bio: String?,
    val isPlayer: Boolean,
    val activityLevel: Double,
    val reputation: Double,
    val popularity: Double,
    val followerCount: Int,
    val followingCount: Int,
    val lastActiveAt: String,
    val createdAt: String,
    val personality: PersonalityDto?,
    val mood: MoodDto?,
    val interests: List<InterestDto>?
)

data class NPCSummaryDto(
    val id: String,
    val handle: String,
    @SerializedName("displayName") val displayName: String
)

data class PersonalityDto(
    val openness: Double,
    val extroversion: Double,
    val agreeableness: Double,
    val conscientiousness: Double,
    val neuroticism: Double,
    val confidence: Double,
    val empathy: Double,
    val sarcasm: Double,
    val humor: Double,
    val aggression: Double
)

data class MoodDto(
    val happiness: Double,
    val sadness: Double,
    val anger: Double,
    val excitement: Double,
    val anxiety: Double,
    val primaryMood: String
)

data class InterestDto(
    val topic: String,
    val weight: Double
)

// Post models
data class PostDto(
    val id: String,
    val authorId: String,
    val authorName: String,
    val authorHandle: String,
    val content: String,
    val communityId: String?,
    val likeCount: Int,
    val dislikeCount: Int,
    val commentCount: Int,
    val shareCount: Int,
    val viewCount: Int,
    val importanceScore: Double,
    val popularity: Double,
    val createdAt: String,
    val updatedAt: String
)

data class CommentDto(
    val id: String,
    val postId: String,
    val authorId: String,
    val authorName: String,
    val content: String,
    val likeCount: Int,
    val createdAt: String
)

// Community models
data class CommunityDto(
    val id: String,
    val handle: String,
    val name: String,
    val description: String?,
    val topic: String?,
    val memberCount: Int,
    val popularity: Double,
    val cultureScore: Double,
    val createdAt: String
)

// Relationship models
data class RelationshipDto(
    val id: String,
    val sourceNpcId: String,
    val targetNpcId: String,
    val affinity: Double,
    val trust: Double,
    val respect: Double,
    val attraction: Double,
    val hostility: Double,
    val familiarity: Double,
    val lastInteractionAt: String?
)

// Message models
data class MessageDto(
    val id: String,
    val senderId: String,
    val sender: NPCSummaryDto?,
    val recipientId: String,
    val recipient: NPCSummaryDto?,
    val content: String,
    val isRead: Boolean,
    val createdAt: String
)

// Notification models
data class NotificationDto(
    val id: String,
    val recipientId: String,
    val type: String,
    val title: String,
    val body: String?,
    val isRead: Boolean,
    val createdAt: String
)

// World/Simulation models
data class WorldDto(
    val id: String,
    val name: String,
    val currentTime: String,
    val isPaused: Boolean,
    val lastProcessedAt: String
)

data class SimulationStatsDto(
    val world: WorldInfoDto,
    val counts: CountsDto,
    val engagement: EngagementDto
)

data class WorldInfoDto(
    val name: String,
    val currentTime: String,
    val isPaused: Boolean
)

data class CountsDto(
    val npcs: Int,
    val posts: Int,
    val comments: Int,
    val messages: Int,
    val communities: Int,
    val relationships: Int,
    val pendingActions: Int,
    val memories: Int
)

data class EngagementDto(
    val totalLikes: Int,
    val totalComments: Int,
    val totalViews: Int
)

// Request models
data class CreatePostRequest(
    val authorId: String,
    val content: String,
    val communityId: String? = null,
    val importanceScore: Double? = null
)

data class CreateCommentRequest(
    val authorId: String,
    val content: String,
    val parentCommentId: String? = null
)

data class LikeRequest(
    val npcId: String
)

data class LikeResponse(
    val likeCount: Int
)

data class FollowRequest(
    val followerId: String,
    val followedId: String
)

data class UpdateRelationshipRequest(
    val sourceId: String,
    val targetId: String,
    val affinity: Double? = null,
    val trust: Double? = null,
    val respect: Double? = null,
    val attraction: Double? = null,
    val hostility: Double? = null,
    val familiarity: Double? = null
)

data class SendMessageRequest(
    val senderId: String,
    val recipientId: String,
    val content: String
)

data class FollowResponse(
    val message: String
)

// Search models
data class SearchResults(
    val npcs: List<NPCSearchResult> = emptyList(),
    val posts: List<PostSearchResult> = emptyList(),
    val communities: List<CommunitySearchResult> = emptyList()
)

data class NPCSearchResult(
    val id: String,
    val handle: String,
    val displayName: String,
    val bio: String?,
    val avatarUrl: String?,
    val followerCount: Int,
    val popularity: Double
)

data class PostSearchResult(
    val id: String,
    val content: String,
    val authorId: String,
    val authorName: String,
    val authorHandle: String,
    val likeCount: Int,
    val commentCount: Int,
    val createdAt: String
)

data class CommunitySearchResult(
    val id: String,
    val name: String,
    val description: String?,
    val memberCount: Int,
    val topic: String?
)

// Chat/DM models
data class ChatThreadDto(
    val id: String,
    val participantId: String,
    val participantName: String,
    val participantHandle: String,
    val lastMessage: String?,
    val lastMessageAt: String?,
    val unreadCount: Int
)

data class ConversationDto(
    val threadId: String,
    val messages: List<MessageDto>
)
