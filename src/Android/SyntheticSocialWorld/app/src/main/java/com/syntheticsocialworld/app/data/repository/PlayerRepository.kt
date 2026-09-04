package com.syntheticsocialworld.app.data.repository

import com.syntheticsocialworld.app.data.api.SyntheticSocialWorldApi
import com.syntheticsocialworld.app.data.model.*
import com.syntheticsocialworld.app.data.session.SessionManager
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Repository for player-related operations.
 */
@Singleton
class PlayerRepository @Inject constructor(
    private val api: SyntheticSocialWorldApi,
    private val sessionManager: SessionManager
) {
    /**
     * Get the current player.
     * @throws IllegalStateException if no session exists.
     */
    suspend fun getCurrentPlayer(): Result<PlayerDto> = withContext(Dispatchers.IO) {
        try {
            val playerId = sessionManager.playerId 
                ?: return@withContext Result.failure(IllegalStateException("No player session"))
            
            val player = api.getCurrentPlayer(playerId)
            Result.success(player)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    /**
     * Check if a player exists.
     */
    suspend fun checkPlayerExists(): Result<Boolean> = withContext(Dispatchers.IO) {
        try {
            val response = api.checkPlayerExists(sessionManager.playerId)
            Result.success(response.exists)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    /**
     * Create a new player.
     */
    suspend fun createPlayer(request: CreatePlayerRequest): Result<PlayerDto> = withContext(Dispatchers.IO) {
        try {
            val player = api.createPlayer(request)
            sessionManager.createSession(player.id)
            sessionManager.onboardingCompleted = true
            Result.success(player)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    /**
     * Update the current player's profile.
     */
    suspend fun updatePlayer(request: UpdatePlayerRequest): Result<PlayerDto> = withContext(Dispatchers.IO) {
        try {
            val playerId = sessionManager.playerId 
                ?: return@withContext Result.failure(IllegalStateException("No player session"))
            
            val player = api.updatePlayer(playerId, request)
            Result.success(player)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    /**
     * Get the current player's posts.
     */
    suspend fun getMyPosts(limit: Int = 20, offset: Int = 0): Result<List<PlayerPostDto>> = withContext(Dispatchers.IO) {
        try {
            val playerId = sessionManager.playerId 
                ?: return@withContext Result.failure(IllegalStateException("No player session"))
            
            val posts = api.getMyPosts(playerId, limit, offset)
            Result.success(posts)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    /**
     * Create a post as the current player.
     */
    suspend fun createPost(content: String, communityId: String? = null): Result<PlayerPostDto> = withContext(Dispatchers.IO) {
        try {
            val playerId = sessionManager.playerId 
                ?: return@withContext Result.failure(IllegalStateException("No player session"))
            
            val request = CreatePlayerPostRequest(content, communityId)
            val post = api.createPlayerPost(playerId, request)
            Result.success(post)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    /**
     * Check if user has a session.
     */
    fun hasSession(): Boolean = sessionManager.hasSession
    
    /**
     * Clear session (logout).
     */
    fun clearSession() {
        sessionManager.clearSession()
    }
}
