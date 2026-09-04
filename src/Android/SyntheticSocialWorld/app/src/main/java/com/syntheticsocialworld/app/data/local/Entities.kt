package com.syntheticsocialworld.app.data.local

import androidx.room.*
import kotlinx.coroutines.flow.Flow

/**
 * Room database for offline caching of API data
 */

@Entity(tableName = "cached_npcs")
data class CachedNPC(
    @PrimaryKey val id: String,
    val handle: String,
    val displayName: String,
    val bio: String?,
    val avatarUrl: String?,
    val followerCount: Int,
    val followingCount: Int,
    val popularity: Double,
    val lastActiveAt: String,
    val cachedAt: Long = System.currentTimeMillis()
)

@Entity(tableName = "cached_posts")
data class CachedPost(
    @PrimaryKey val id: String,
    val authorId: String,
    val authorName: String,
    val authorHandle: String,
    val content: String,
    val communityId: String?,
    val likeCount: Int,
    val commentCount: Int,
    val shareCount: Int,
    val viewCount: Int,
    val createdAt: String,
    val cachedAt: Long = System.currentTimeMillis()
)

@Entity(tableName = "cached_communities")
data class CachedCommunity(
    @PrimaryKey val id: String,
    val handle: String,
    val name: String,
    val description: String?,
    val topic: String?,
    val memberCount: Int,
    val cachedAt: Long = System.currentTimeMillis()
)

@Entity(tableName = "cached_feed", primaryKeys = ["viewerId", "postId"])
data class CachedFeedItem(
    val viewerId: String,
    val postId: String,
    val rank: Int,
    val cachedAt: Long = System.currentTimeMillis()
)

@Entity(tableName = "sync_status")
data class SyncStatus(
    @PrimaryKey val entityType: String,
    val lastSyncTime: Long,
    val isStale: Boolean = false
)

@Dao
interface NPCDao {
    @Query("SELECT * FROM cached_npcs ORDER BY cachedAt DESC")
    fun getAllNPCs(): Flow<List<CachedNPC>>
    
    @Query("SELECT * FROM cached_npcs WHERE id = :id")
    suspend fun getNPCById(id: String): CachedNPC?
    
    @Query("SELECT * FROM cached_npcs WHERE displayName LIKE '%' || :query || '%' OR handle LIKE '%' || :query || '%'")
    suspend fun searchNPCs(query: String): List<CachedNPC>
    
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertNPCs(npcs: List<CachedNPC>)
    
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertNPC(npc: CachedNPC)
    
    @Query("DELETE FROM cached_npcs WHERE cachedAt < :threshold")
    suspend fun deleteOldNPCs(threshold: Long)
    
    @Query("DELETE FROM cached_npcs")
    suspend fun clearAll()
}

@Dao
interface PostDao {
    @Query("SELECT * FROM cached_posts ORDER BY cachedAt DESC")
    fun getAllPosts(): Flow<List<CachedPost>>
    
    @Query("SELECT * FROM cached_posts WHERE id = :id")
    suspend fun getPostById(id: String): CachedPost?
    
    @Query("SELECT * FROM cached_posts WHERE authorId = :authorId ORDER BY cachedAt DESC")
    fun getPostsByAuthor(authorId: String): Flow<List<CachedPost>>
    
    @Query("SELECT * FROM cached_posts WHERE communityId = :communityId ORDER BY cachedAt DESC")
    fun getPostsByCommunity(communityId: String): Flow<List<CachedPost>>
    
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertPosts(posts: List<CachedPost>)
    
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertPost(post: CachedPost)
    
    @Query("DELETE FROM cached_posts WHERE cachedAt < :threshold")
    suspend fun deleteOldPosts(threshold: Long)
    
    @Query("DELETE FROM cached_posts")
    suspend fun clearAll()
}

@Dao
interface CommunityDao {
    @Query("SELECT * FROM cached_communities ORDER BY name ASC")
    fun getAllCommunities(): Flow<List<CachedCommunity>>
    
    @Query("SELECT * FROM cached_communities WHERE id = :id")
    suspend fun getCommunityById(id: String): CachedCommunity?
    
    @Query("SELECT * FROM cached_communities WHERE name LIKE '%' || :query || '%'")
    suspend fun searchCommunities(query: String): List<CachedCommunity>
    
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertCommunities(communities: List<CachedCommunity>)
    
    @Query("DELETE FROM cached_communities WHERE cachedAt < :threshold")
    suspend fun deleteOldCommunities(threshold: Long)
    
    @Query("DELETE FROM cached_communities")
    suspend fun clearAll()
}

@Dao
interface FeedDao {
    @Query("""
        SELECT p.* FROM cached_posts p
        INNER JOIN cached_feed f ON p.id = f.postId
        WHERE f.viewerId = :viewerId
        ORDER BY f.rank ASC
    """)
    fun getFeed(viewerId: String): Flow<List<CachedPost>>
    
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertFeedItems(items: List<CachedFeedItem>)
    
    @Query("DELETE FROM cached_feed WHERE viewerId = :viewerId")
    suspend fun clearFeed(viewerId: String)
}

@Dao
interface SyncStatusDao {
    @Query("SELECT * FROM sync_status WHERE entityType = :entityType")
    suspend fun getSyncStatus(entityType: String): SyncStatus?
    
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun updateSyncStatus(status: SyncStatus)
    
    @Query("SELECT * FROM sync_status")
    fun getAllSyncStatuses(): Flow<List<SyncStatus>>
}

@Database(
    entities = [
        CachedNPC::class,
        CachedPost::class,
        CachedCommunity::class,
        CachedFeedItem::class,
        SyncStatus::class
    ],
    version = 1,
    exportSchema = false
)
abstract class AppDatabase : RoomDatabase() {
    abstract fun npcDao(): NPCDao
    abstract fun postDao(): PostDao
    abstract fun communityDao(): CommunityDao
    abstract fun feedDao(): FeedDao
    abstract fun syncStatusDao(): SyncStatusDao
}
