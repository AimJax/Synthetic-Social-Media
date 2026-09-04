package com.syntheticsocialworld.app.ui.screens

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.syntheticsocialworld.app.data.model.PlayerDto
import com.syntheticsocialworld.app.data.repository.PlayerRepository
import com.syntheticsocialworld.app.data.session.SessionManager
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

sealed class AppState {
    data object Loading : AppState()
    data object Onboarding : AppState()
    data class Main(val player: PlayerDto) : AppState()
    data class Error(val message: String) : AppState()
}

@HiltViewModel
class AppNavigationViewModel @Inject constructor(
    private val playerRepository: PlayerRepository,
    private val sessionManager: SessionManager
) : ViewModel() {
    
    private val _appState = MutableStateFlow<AppState>(AppState.Loading)
    val appState: StateFlow<AppState> = _appState.asStateFlow()
    
    init {
        checkSession()
    }
    
    private fun checkSession() {
        viewModelScope.launch {
            _appState.value = AppState.Loading
            
            try {
                // Small delay for splash effect
                kotlinx.coroutines.delay(500)
                
                if (sessionManager.hasSession) {
                    // Try to load existing player
                    val result = playerRepository.getCurrentPlayer()
                    result.fold(
                        onSuccess = { player ->
                            _appState.value = AppState.Main(player)
                        },
                        onFailure = {
                            // Session exists but player not found - clear session
                            sessionManager.clearSession()
                            _appState.value = AppState.Onboarding
                        }
                    )
                } else {
                    _appState.value = AppState.Onboarding
                }
            } catch (e: Exception) {
                _appState.value = AppState.Error(e.message ?: "Failed to initialize app")
            }
        }
    }
    
    fun completeOnboarding() {
        viewModelScope.launch {
            _appState.value = AppState.Loading
            
            val result = playerRepository.getCurrentPlayer()
            result.fold(
                onSuccess = { player ->
                    _appState.value = AppState.Main(player)
                },
                onFailure = { error ->
                    _appState.value = AppState.Error(error.message ?: "Failed to load profile")
                }
            )
        }
    }
    
    fun logout() {
        playerRepository.clearSession()
        _appState.value = AppState.Onboarding
    }
    
    fun retry() {
        checkSession()
    }
    
    fun refreshPlayer() {
        viewModelScope.launch {
            val result = playerRepository.getCurrentPlayer()
            result.fold(
                onSuccess = { player ->
                    _appState.value = AppState.Main(player)
                },
                onFailure = { /* Keep current state */ }
            )
        }
    }
}
