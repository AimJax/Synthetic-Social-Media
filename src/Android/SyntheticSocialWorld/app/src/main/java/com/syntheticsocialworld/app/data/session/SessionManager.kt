package com.syntheticsocialworld.app.data.session

import android.content.Context
import android.content.SharedPreferences
import dagger.hilt.android.qualifiers.ApplicationContext
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Manages the current player session.
 * Stores the player's ID and provides session state.
 */
@Singleton
class SessionManager @Inject constructor(
    @ApplicationContext context: Context
) {
    private val prefs: SharedPreferences = context.getSharedPreferences(
        PREFS_NAME, Context.MODE_PRIVATE
    )
    
    /**
     * Get the current player ID, or null if not logged in.
     */
    var playerId: String?
        get() = prefs.getString(KEY_PLAYER_ID, null)
        private set(value) {
            prefs.edit().putString(KEY_PLAYER_ID, value).apply()
        }
    
    /**
     * Check if a player session exists.
     */
    val hasSession: Boolean
        get() = !playerId.isNullOrEmpty()
    
    /**
     * Check if onboarding has been completed.
     */
    var onboardingCompleted: Boolean
        get() = prefs.getBoolean(KEY_ONBOARDING_COMPLETED, false)
        set(value) {
            prefs.edit().putBoolean(KEY_ONBOARDING_COMPLETED, value).apply()
        }
    
    /**
     * Create a new session for a player.
     */
    fun createSession(newPlayerId: String) {
        playerId = newPlayerId
    }
    
    /**
     * Clear the current session (logout).
     */
    fun clearSession() {
        prefs.edit()
            .remove(KEY_PLAYER_ID)
            .remove(KEY_ONBOARDING_COMPLETED)
            .apply()
    }
    
    companion object {
        private const val PREFS_NAME = "synthetic_social_world_session"
        private const val KEY_PLAYER_ID = "player_id"
        private const val KEY_ONBOARDING_COMPLETED = "onboarding_completed"
    }
}
