package com.syntheticsocialworld.app.data.repository

import android.content.Context
import androidx.room.Room
import com.syntheticsocialworld.app.data.api.SyntheticSocialWorldApi
import com.syntheticsocialworld.app.data.local.*
import com.syntheticsocialworld.app.data.model.*
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import kotlinx.coroutines.flow.flowOn
import kotlinx.coroutines.withContext
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Repository that provides data with offline caching support.
 * Uses a cache-first strategy: return cached data immediately, 
 * then fetch fresh data from the API in the background.
 */
@Singleton
class CachingRepository @Inject constructor(
    private val api: SyntheticSocialWorldApi,
    @ApplicationContext private val context: Context
) {
    private val database: AppDatabase = Room.databaseBuilder(
        context,
        AppDatabase::class.java,
        "synthetic_social_world_cache"
    ).fallbackToDestructiveMigration().build()
    
    private val npcDao = database.npcDao()
    private val postDao = database.postDao()
    private val communityDao = database.communityDao()
    private val feedDao = database.feedDao()
    private val syncStatusDao = database.syncStatusDao()
    
    companion object {
        private const val CACHE_DURATION_MS = 5 * 60 * 1000L // 5 minutes
        private const val STALE_THRESHOLD_MS = 30 * 60 * 1000L // 30 minutes
    }
    
    // NPCs
    fun getNPCs(forceRefresh: Boolean = false): Flow<Result<List<NPCDto>>> = flow {
        // First emit cached data if available
        val cachedNPCs = npcDao.getAllNPCs()
        cachedNPCs.collect { cached ->
            if (cached.isNotEmpty()) {
                val dtos = cached.map { it.toDto() }
                emit(Result.success(dtos))
            }
        }
        
        // Then try to fetch fresh data
        if (forceRefresh || shouldRefresh("npcs")) {
            try {
                val freshNPCs = api.getNPCs(limit = 50)
                npcDao.insertNPCs(freshNPCs.map { it.toCached() })
                updateSyncStatus("npcs")
                emit(Result.success(freshNPCs))
            } catch (e: Exception) {
                // Already emitted cached data, just log error
            }
        }
    }.flowOn(Dispatchers.IO)
    
    suspend fun getNPCById(id: String, forceRefresh: Boolean = false): Result<NPCDto> {
        // Try cache first
        val cached = npcDao.getNPCById(id)
        if (cached != null && !forceRefresh) {
            return Result.success(cached.toDto())
        }
        
        // Fetch from API
        return try {
            val npc = api.getNPCById(id)
            npcDao.insertNPC(npc.toCached())
            Result.success(npc)
        } catch (e: Exception) {
            cached?.let { Result.success(it.toDto()) } ?: Result.failure(e)
        }
    }
    
    suspend fun searchNPCs(query: String): Result<List<NPCDto>> {
        // Search local cache first
        val cached = npcDao.searchNPCs(query)
        if (cached.isNotEmpty()) {
            return Result.success(cached.map { it.toDto() })
        }
        
        // Try API
        return try {
            val results = api.search(query = query, filter = "npcs")
            Result.success(results.npcs.map { it.toDto() })
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    // Posts
    fun getRecentPosts(forceRefresh: Boolean = false): Flow<Result<List<PostDto>>> = flow {
        val cached = postDao.getAllPosts()
        cached.collect { posts ->
            if (posts.isNotEmpty()) {
                emit(Result.success(posts.map { it.toDto() }))
            }
        }
        
        if (forceRefresh || shouldRefresh("posts")) {
            try {
                val freshPosts = api.getRecentPosts(limit = 50)
                postDao.insertPosts(freshPosts.map { it.toCached() })
                updateSyncStatus("posts")
                emit(Result.success(freshPosts))
            } catch (e: Exception) {
                // Already emitted cached data
            }
        }
    }.flowOn(Dispatchers.IO)
    
    suspend fun getPostById(id: String, forceRefresh: Boolean = false): Result<PostDto> {
        val cached = postDao.getPostById(id)
        if (cached != null && !forceRefresh) {
            return Result.success(cached.toDto())
        }
        
        return try {
            val post = api.getPostById(id)
            postDao.insertPost(post.toCached())
            Result.success(post)
        } catch (e: Exception) {
            cached?.let { Result.success(it.toDto()) } ?: Result.failure(e)
        }
    }
    
    // Communities
    fun getCommunities(forceRefresh: Boolean = false): Flow<Result<List<CommunityDto>>> = flow {
        val cached = communityDao.getAllCommunities()
        cached.collect { communities ->
            if (communities.isNotEmpty()) {
                emit(Result.success(communities.map { it.toDto() }))
            }
        }
        
        if (forceRefresh || shouldRefresh("communities")) {
            try {
                val freshCommunities = api.getCommunities()
                communityDao.insertCommunities(freshCommunities.map { it.toCached() })
                updateSyncStatus("communities")
                emit(Result.success(freshCommunities))
            } catch (e: Exception) {
                // Already emitted cached data
            }
        }
    }.flowOn(Dispatchers.IO)
    
    // Feed
    fun getFeed(viewerId: String, forceRefresh: Boolean = false): Flow<Result<List<PostDto>>> = flow {
        val cachedFeed = feedDao.getFeed(viewerId)
        cachedFeed.collect { posts ->
            if (posts.isNotEmpty()) {
                emit(Result.success(posts.map { it.toDto() }))
            }
        }
        
        if (forceRefresh || shouldRefresh("feed")) {
            try {
                val freshFeed = api.getFeed(viewerId, limit = 50)
                // Cache the posts
                postDao.insertPosts(freshFeed.map { it.toCached() })
                // Cache the feed order
                val feedItems = freshFeed.mapIndexed { index, post ->
                    CachedFeedItem(viewerId = viewerId, postId = post.id, rank = index)
                }
                feedDao.clearFeed(viewerId)
                feedDao.insertFeedItems(feedItems)
                updateSyncStatus("feed")
                emit(Result.success(freshFeed))
            } catch (e: Exception) {
                // Already emitted cached data
            }
        }
    }.flowOn(Dispatchers.IO)
    
    // Like a post (and update cache)
    suspend fun likePost(postId: String, npcId: String): Result<LikeResponse> {
        return try {
            val result = api.likePost(postId, LikeRequest(npcId))
            // Update cache
            val cached = postDao.getPostById(postId)
            cached?.let {
                postDao.insertPost(it.copy(likeCount = result.likeCount))
            }
            Result.success(result)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    // Create post (and update cache)
    suspend fun createPost(request: CreatePostRequest): Result<PostDto> {
        return try {
            val post = api.createPost(request)
            postDao.insertPost(post.toCached())
            Result.success(post)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    // Clear all cache
    suspend fun clearCache() {
        npcDao.clearAll()
        postDao.clearAll()
        communityDao.clearAll()
    }
    
    // Check if data should be refreshed
    private suspend fun shouldRefresh(entityType: String): Boolean {
        val status = syncStatusDao.getSyncStatus(entityType)
        if (status == null) return true
        val age = System.currentTimeMillis() - status.lastSyncTime
        return age > CACHE_DURATION_MS
    }
    
    private suspend fun updateSyncStatus(entityType: String) {
        syncStatusDao.updateSyncStatus(
            SyncStatus(entityType = entityType, lastSyncTime = System.currentTimeMillis())
        )
    }
    
    // Extension functions for mapping
    private fun CachedNPC.toDto() = NPCDto(
        id = id,
        handle = handle,
        displayName = displayName,
        bio = bio,
        isPlayer = false,
        activityLevel = 0.5,
        reputation = 0.5,
        popularity = popularity,
        followerCount = followerCount,
        followingCount = followingCount,
        lastActiveAt = lastActiveAt,
        createdAt = lastActiveAt,
        personality = null,
        mood = null,
        interests = null
    )
    
    private fun NPCDto.toCached() = CachedNPC(
        id = id,
        handle = handle,
        displayName = displayName,
        bio = bio,
        avatarUrl = null,
        followerCount = followerCount,
        followingCount = followingCount,
        popularity = popularity,
        lastActiveAt = lastActiveAt
    )
    
    private fun NPCSearchResult.toDto() = NPCDto(
        id = id,
        handle = handle,
        displayName = displayName,
        bio = bio,
        isPlayer = false,
        activityLevel = 0.5,
        reputation = 0.5,
        popularity = popularity,
        followerCount = followerCount,
        followingCount = 0,
        lastActiveAt = "",
        createdAt = "",
        personality = null,
        mood = null,
        interests = null
    )
    
    private fun CachedPost.toDto() = PostDto(
        id = id,
        authorId = authorId,
        authorName = authorName,
        authorHandle = authorHandle,
        content = content,
        communityId = communityId,
        likeCount = likeCount,
        dislikeCount = 0,
        commentCount = commentCount,
        shareCount = shareCount,
        viewCount = viewCount,
        importanceScore = 0.5,
        popularity = 0.5,
        createdAt = createdAt,
        updatedAt = createdAt
    )
    
    private fun PostDto.toCached() = CachedPost(
        id = id,
        authorId = authorId,
        authorName = authorName,
        authorHandle = authorHandle,
        content = content,
        communityId = communityId,
        likeCount = likeCount,
        commentCount = commentCount,
        shareCount = shareCount,
        viewCount = viewCount,
        createdAt = createdAt
    )
    
    private fun CachedCommunity.toDto() = CommunityDto(
        id = id,
        handle = handle,
        name = name,
        description = description,
        topic = topic,
        memberCount = memberCount,
        popularity = 0.5,
        cultureScore = 0.5,
        createdAt = ""
    )
    
    private fun CommunityDto.toCached() = CachedCommunity(
        id = id,
        handle = handle,
        name = name,
        description = description,
        topic = topic,
        memberCount = memberCount
    )
}
