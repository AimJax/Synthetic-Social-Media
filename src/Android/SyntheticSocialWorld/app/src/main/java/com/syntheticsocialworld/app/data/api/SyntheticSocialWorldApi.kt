package com.syntheticsocialworld.app.data.api

import retrofit2.http.*
import com.syntheticsocialworld.app.data.model.*

/**
 * Retrofit API interface for Synthetic Social World backend
 */
interface SyntheticSocialWorldApi {
    
    // Health check
    @GET("health")
    suspend fun healthCheck(): HealthResponse
    
    // API Info
    @GET("api/info")
    suspend fun getApiInfo(): ApiInfoResponse
    
    // NPCs
    @GET("api/npcs")
    suspend fun getNPCs(
        @Query("limit") limit: Int = 50,
        @Query("offset") offset: Int = 0
    ): List<NPCDto>
    
    @GET("api/npcs/{id}")
    suspend fun getNPCById(@Path("id") id: String): NPCDto
    
    @GET("api/npcs/by-handle/{handle}")
    suspend fun getNPCByHandle(@Path("handle") handle: String): NPCDto
    
    @GET("api/npcs/{id}/posts")
    suspend fun getNPCPosts(
        @Path("id") npcId: String,
        @Query("limit") limit: Int = 50,
        @Query("offset") offset: Int = 0
    ): List<PostDto>
    
    @GET("api/npcs/{id}/followers")
    suspend fun getNPCFollowers(@Path("id") npcId: String): List<NPCSummaryDto>
    
    @GET("api/npcs/{id}/following")
    suspend fun getNPCFollowing(@Path("id") npcId: String): List<NPCSummaryDto>
    
    // Posts
    @GET("api/posts")
    suspend fun getRecentPosts(
        @Query("limit") limit: Int = 20,
        @Query("offset") offset: Int = 0
    ): List<PostDto>
    
    @GET("api/posts/{id}")
    suspend fun getPostById(@Path("id") id: String): PostDto
    
    @POST("api/posts")
    suspend fun createPost(@Body request: CreatePostRequest): PostDto
    
    @GET("api/posts/{id}/comments")
    suspend fun getPostComments(
        @Path("id") postId: String,
        @Query("limit") limit: Int = 50
    ): List<CommentDto>
    
    @POST("api/posts/{id}/comments")
    suspend fun addComment(
        @Path("id") postId: String,
        @Body request: CreateCommentRequest
    ): CommentDto
    
    @POST("api/posts/{id}/like")
    suspend fun likePost(
        @Path("id") postId: String,
        @Body request: LikeRequest
    ): LikeResponse
    
    // Feed
    @GET("api/feed/{npcId}")
    suspend fun getFeed(
        @Path("npcId") npcId: String,
        @Query("limit") limit: Int = 20,
        @Query("cursor") cursor: String? = null
    ): List<PostDto>
    
    @GET("api/feed/trending")
    suspend fun getTrendingPosts(@Query("limit") limit: Int = 20): List<PostDto>
    
    @GET("api/feed/discovery/{npcId}")
    suspend fun getDiscoveryPosts(
        @Path("npcId") npcId: String,
        @Query("limit") limit: Int = 20
    ): List<PostDto>
    
    // Social
    @POST("api/social/follow")
    suspend fun follow(@Body request: FollowRequest): FollowResponse
    
    @DELETE("api/social/unfollow")
    suspend fun unfollow(@Body request: FollowRequest)
    
    @GET("api/social/following/{followerId}/{followedId}")
    suspend fun isFollowing(
        @Path("followerId") followerId: String,
        @Path("followedId") followedId: String
    ): Boolean
    
    @GET("api/social/relationship/{sourceId}/{targetId}")
    suspend fun getRelationship(
        @Path("sourceId") sourceId: String,
        @Path("targetId") targetId: String
    ): RelationshipDto
    
    @PUT("api/social/relationship")
    suspend fun updateRelationship(@Body request: UpdateRelationshipRequest): RelationshipDto
    
    @GET("api/social/messages/{userId1}/{userId2}")
    suspend fun getConversation(
        @Path("userId1") userId1: String,
        @Path("userId2") userId2: String,
        @Query("limit") limit: Int = 50,
        @Query("offset") offset: Int = 0
    ): List<MessageDto>
    
    @POST("api/social/messages")
    suspend fun sendMessage(@Body request: SendMessageRequest): MessageDto
    
    @GET("api/social/notifications/{userId}")
    suspend fun getNotifications(
        @Path("userId") userId: String,
        @Query("unreadOnly") unreadOnly: Boolean = false,
        @Query("limit") limit: Int = 50
    ): List<NotificationDto>
    
    // Simulation
    @GET("api/simulation/world")
    suspend fun getWorld(): WorldDto
    
    @GET("api/simulation/stats")
    suspend fun getSimulationStats(): SimulationStatsDto
    
    @GET("api/communities")
    suspend fun getCommunities(
        @Query("limit") limit: Int = 20
    ): List<CommunityDto>
    
    @PUT("api/simulation/world/pause")
    suspend fun togglePause(@Body request: Map<String, Boolean>): Map<String, Boolean>
    
    @POST("api/simulation/advance")
    suspend fun advanceTime(@Body request: Map<String, Double>): Map<String, String>
    
    // Search
    @GET("api/search")
    suspend fun search(
        @Query("query") query: String,
        @Query("filter") filter: String? = null,
        @Query("limit") limit: Int = 20
    ): SearchResults
}
