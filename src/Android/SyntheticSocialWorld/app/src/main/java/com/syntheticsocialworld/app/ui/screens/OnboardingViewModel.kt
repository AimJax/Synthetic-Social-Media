package com.syntheticsocialworld.app.ui.screens

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.syntheticsocialworld.app.data.model.CreatePlayerRequest
import com.syntheticsocialworld.app.data.repository.PlayerRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

data class OnboardingUiState(
    val currentStep: Int = 0,
    val displayName: String = "",
    val handle: String = "",
    val bio: String = "",
    val selectedInterests: Set<String> = emptySet(),
    val isHandleAvailable: Boolean? = null,
    val isCheckingHandle: Boolean = false,
    val handleError: String? = null,
    val isLoading: Boolean = false,
    val error: String? = null,
    val isComplete: Boolean = false
)

@HiltViewModel
class OnboardingViewModel @Inject constructor(
    private val playerRepository: PlayerRepository
) : ViewModel() {
    
    private val _uiState = MutableStateFlow(OnboardingUiState())
    val uiState: StateFlow<OnboardingUiState> = _uiState.asStateFlow()
    
    fun nextStep() {
        _uiState.update { it.copy(currentStep = minOf(it.currentStep + 1, 4)) }
    }
    
    fun previousStep() {
        _uiState.update { it.copy(currentStep = maxOf(it.currentStep - 1, 0)) }
    }
    
    fun updateDisplayName(name: String) {
        _uiState.update { it.copy(displayName = name.take(100)) }
    }
    
    fun updateHandle(handle: String) {
        _uiState.update { 
            it.copy(
                handle = handle,
                isHandleAvailable = null,
                handleError = null
            )
        }
    }
    
    fun updateBio(bio: String) {
        _uiState.update { it.copy(bio = bio.take(280)) }
    }
    
    fun toggleInterest(interest: String) {
        _uiState.update { state ->
            val newInterests = if (state.selectedInterests.contains(interest)) {
                state.selectedInterests - interest
            } else if (state.selectedInterests.size < 5) {
                state.selectedInterests + interest
            } else {
                state.selectedInterests
            }
            state.copy(selectedInterests = newInterests)
        }
    }
    
    fun checkHandleAvailability() {
        val handle = _uiState.value.handle
        if (handle.length < 3) {
            _uiState.update { it.copy(handleError = "Handle must be at least 3 characters") }
            return
        }
        
        if (!handle.matches(Regex("^[a-z0-9_]+$"))) {
            _uiState.update { it.copy(handleError = "Only lowercase letters, numbers, and underscores") }
            return
        }
        
        viewModelScope.launch {
            _uiState.update { it.copy(isCheckingHandle = true, handleError = null) }
            
            // For now, we'll assume the handle is available
            // In production, this would call the API to check
            kotlinx.coroutines.delay(500) // Simulate API call
            
            // Simple validation - just mark as available
            _uiState.update { 
                it.copy(
                    isCheckingHandle = false,
                    isHandleAvailable = true
                )
            }
        }
    }
    
    fun createPlayer() {
        val state = _uiState.value
        
        if (state.displayName.isBlank()) {
            _uiState.update { it.copy(error = "Please enter a display name") }
            return
        }
        
        if (state.handle.isBlank()) {
            _uiState.update { it.copy(error = "Please choose a handle") }
            return
        }
        
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true, error = null) }
            
            val request = CreatePlayerRequest(
                displayName = state.displayName,
                handle = state.handle,
                bio = state.bio.takeIf { it.isNotBlank() },
                interests = state.selectedInterests.toList().takeIf { it.isNotEmpty() }
            )
            
            val result = playerRepository.createPlayer(request)
            
            result.fold(
                onSuccess = {
                    _uiState.update { it.copy(isLoading = false, isComplete = true) }
                },
                onFailure = { error ->
                    _uiState.update { 
                        it.copy(
                            isLoading = false,
                            error = error.message ?: "Failed to create player"
                        )
                    }
                }
            )
        }
    }
    
    fun clearError() {
        _uiState.update { it.copy(error = null) }
    }
}
