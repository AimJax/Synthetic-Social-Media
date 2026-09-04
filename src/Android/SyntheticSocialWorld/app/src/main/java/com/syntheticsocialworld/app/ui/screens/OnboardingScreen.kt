package com.syntheticsocialworld.app.ui.screens

import androidx.compose.animation.*
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.layout.FlowRow
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalFocusManager
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardCapitalization
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import com.syntheticsocialworld.app.ui.theme.*

@OptIn(ExperimentalMaterial3Api::class, ExperimentalLayoutApi::class)
@Composable
fun OnboardingScreen(
    onComplete: () -> Unit,
    viewModel: OnboardingViewModel = hiltViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()
    val focusManager = LocalFocusManager.current
    
    LaunchedEffect(uiState.isComplete) {
        if (uiState.isComplete) {
            onComplete()
        }
    }
    
    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(MaterialTheme.colorScheme.background)
    ) {
        Column(
            modifier = Modifier.fillMaxSize()
        ) {
            // Progress indicator
            LinearProgressIndicator(
                progress = { (uiState.currentStep + 1) / 5f },
                modifier = Modifier
                    .fillMaxWidth()
                    .height(4.dp),
                color = MaterialTheme.colorScheme.primary,
                trackColor = MaterialTheme.colorScheme.surfaceVariant,
            )
            
            // Content
            AnimatedContent(
                targetState = uiState.currentStep,
                transitionSpec = {
                    if (targetState > initialState) {
                        slideInHorizontally { it } + fadeIn() togetherWith
                            slideOutHorizontally { -it } + fadeOut()
                    } else {
                        slideInHorizontally { -it } + fadeIn() togetherWith
                            slideOutHorizontally { it } + fadeOut()
                    }
                },
                modifier = Modifier.weight(1f),
                label = "onboarding_step"
            ) { step ->
                when (step) {
                    0 -> WelcomeStep(onNext = { viewModel.nextStep() })
                    1 -> DisplayNameStep(
                        displayName = uiState.displayName,
                        onDisplayNameChange = { viewModel.updateDisplayName(it) },
                        onNext = { viewModel.nextStep() },
                        onBack = { viewModel.previousStep() }
                    )
                    2 -> HandleStep(
                        handle = uiState.handle,
                        isHandleAvailable = uiState.isHandleAvailable,
                        isCheckingHandle = uiState.isCheckingHandle,
                        handleError = uiState.handleError,
                        onHandleChange = { viewModel.updateHandle(it) },
                        onCheckHandle = { viewModel.checkHandleAvailability() },
                        onNext = { viewModel.nextStep() },
                        onBack = { viewModel.previousStep() }
                    )
                    3 -> BioStep(
                        bio = uiState.bio,
                        onBioChange = { viewModel.updateBio(it) },
                        onNext = { viewModel.nextStep() },
                        onBack = { viewModel.previousStep() }
                    )
                    4 -> InterestsStep(
                        selectedInterests = uiState.selectedInterests,
                        onInterestToggle = { viewModel.toggleInterest(it) },
                        onComplete = { viewModel.createPlayer() },
                        isLoading = uiState.isLoading,
                        onBack = { viewModel.previousStep() }
                    )
                }
            }
        }
        
        // Error snackbar
        if (uiState.error != null) {
            Snackbar(
                modifier = Modifier
                    .align(Alignment.BottomCenter)
                    .padding(16.dp),
                action = {
                    TextButton(onClick = { viewModel.clearError() }) {
                        Text("Dismiss")
                    }
                }
            ) {
                Text(uiState.error!!)
            }
        }
    }
}

@Composable
private fun WelcomeStep(onNext: () -> Unit) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(32.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        Icon(
            imageVector = Icons.Default.Groups,
            contentDescription = null,
            modifier = Modifier.size(120.dp),
            tint = MaterialTheme.colorScheme.primary
        )
        
        Spacer(modifier = Modifier.height(32.dp))
        
        Text(
            text = "Welcome to\nSynthetic Social World",
            style = MaterialTheme.typography.headlineLarge,
            fontWeight = FontWeight.Bold,
            textAlign = TextAlign.Center,
            color = MaterialTheme.colorScheme.onBackground
        )
        
        Spacer(modifier = Modifier.height(16.dp))
        
        Text(
            text = "Step into a living world where AI-powered characters form friendships, share stories, and build communities.\n\nCreate your character and join them!",
            style = MaterialTheme.typography.bodyLarge,
            textAlign = TextAlign.Center,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
        
        Spacer(modifier = Modifier.height(48.dp))
        
        Button(
            onClick = onNext,
            modifier = Modifier
                .fillMaxWidth()
                .height(56.dp),
            shape = RoundedCornerShape(16.dp)
        ) {
            Text("Get Started", fontSize = 18.sp)
        }
    }
}

@Composable
private fun DisplayNameStep(
    displayName: String,
    onDisplayNameChange: (String) -> Unit,
    onNext: () -> Unit,
    onBack: () -> Unit
) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(24.dp)
            .verticalScroll(rememberScrollState()),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.Start
        ) {
            IconButton(onClick = onBack) {
                Icon(Icons.Default.ArrowBack, contentDescription = "Back")
            }
        }
        
        Spacer(modifier = Modifier.height(32.dp))
        
        Icon(
            imageVector = Icons.Default.Person,
            contentDescription = null,
            modifier = Modifier.size(80.dp),
            tint = MaterialTheme.colorScheme.primary
        )
        
        Spacer(modifier = Modifier.height(24.dp))
        
        Text(
            text = "What should we call you?",
            style = MaterialTheme.typography.headlineSmall,
            fontWeight = FontWeight.Bold,
            textAlign = TextAlign.Center
        )
        
        Spacer(modifier = Modifier.height(8.dp))
        
        Text(
            text = "This is how others will see you in the world",
            style = MaterialTheme.typography.bodyMedium,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
            textAlign = TextAlign.Center
        )
        
        Spacer(modifier = Modifier.height(32.dp))
        
        OutlinedTextField(
            value = displayName,
            onValueChange = onDisplayNameChange,
            label = { Text("Display Name") },
            placeholder = { Text("Enter your name") },
            singleLine = true,
            modifier = Modifier.fillMaxWidth(),
            shape = RoundedCornerShape(12.dp),
            keyboardOptions = KeyboardOptions(
                capitalization = KeyboardCapitalization.Words,
                imeAction = ImeAction.Done
            )
        )
        
        Spacer(modifier = Modifier.weight(1f))
        
        Button(
            onClick = onNext,
            enabled = displayName.isNotBlank() && displayName.length >= 2,
            modifier = Modifier
                .fillMaxWidth()
                .height(56.dp),
            shape = RoundedCornerShape(16.dp)
        ) {
            Text("Continue")
        }
        
        Spacer(modifier = Modifier.height(24.dp))
    }
}

@Composable
private fun HandleStep(
    handle: String,
    isHandleAvailable: Boolean?,
    isCheckingHandle: Boolean,
    handleError: String?,
    onHandleChange: (String) -> Unit,
    onCheckHandle: () -> Unit,
    onNext: () -> Unit,
    onBack: () -> Unit
) {
    val focusManager = LocalFocusManager.current
    
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(24.dp)
            .verticalScroll(rememberScrollState()),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.Start
        ) {
            IconButton(onClick = onBack) {
                Icon(Icons.Default.ArrowBack, contentDescription = "Back")
            }
        }
        
        Spacer(modifier = Modifier.height(32.dp))
        
        Icon(
            imageVector = Icons.Default.AlternateEmail,
            contentDescription = null,
            modifier = Modifier.size(80.dp),
            tint = MaterialTheme.colorScheme.primary
        )
        
        Spacer(modifier = Modifier.height(24.dp))
        
        Text(
            text = "Choose your handle",
            style = MaterialTheme.typography.headlineSmall,
            fontWeight = FontWeight.Bold,
            textAlign = TextAlign.Center
        )
        
        Spacer(modifier = Modifier.height(8.dp))
        
        Text(
            text = "Your unique identity in the social world",
            style = MaterialTheme.typography.bodyMedium,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
            textAlign = TextAlign.Center
        )
        
        Spacer(modifier = Modifier.height(32.dp))
        
        OutlinedTextField(
            value = handle,
            onValueChange = { 
                if (it.all { c -> c.isLetterOrDigit() || c == '_' }) {
                    onHandleChange(it.lowercase())
                }
            },
            label = { Text("Handle") },
            placeholder = { Text("your_handle") },
            leadingIcon = { Text("@", color = MaterialTheme.colorScheme.onSurfaceVariant) },
            singleLine = true,
            isError = handleError != null || (isHandleAvailable == false),
            modifier = Modifier.fillMaxWidth(),
            shape = RoundedCornerShape(12.dp),
            keyboardOptions = KeyboardOptions(
                capitalization = KeyboardCapitalization.None,
                imeAction = ImeAction.Done
            ),
            keyboardActions = KeyboardActions(
                onDone = {
                    focusManager.clearFocus()
                    onCheckHandle()
                }
            ),
            supportingText = {
                when {
                    handleError != null -> Text(handleError, color = MaterialTheme.colorScheme.error)
                    isCheckingHandle -> Text("Checking...")
                    isHandleAvailable == true -> Text("Handle available!", color = Color(0xFF4CAF50))
                    isHandleAvailable == false -> Text("Handle already taken", color = MaterialTheme.colorScheme.error)
                    else -> Text("Letters, numbers, and underscores only")
                }
            }
        )
        
        Spacer(modifier = Modifier.height(16.dp))
        
        if (isHandleAvailable != true && handle.isNotBlank()) {
            OutlinedButton(
                onClick = onCheckHandle,
                enabled = !isCheckingHandle && handle.length >= 3,
                modifier = Modifier.fillMaxWidth()
            ) {
                if (isCheckingHandle) {
                    CircularProgressIndicator(
                        modifier = Modifier.size(20.dp),
                        strokeWidth = 2.dp
                    )
                } else {
                    Text("Check Availability")
                }
            }
        }
        
        Spacer(modifier = Modifier.weight(1f))
        
        Button(
            onClick = onNext,
            enabled = isHandleAvailable == true && handle.length >= 3,
            modifier = Modifier
                .fillMaxWidth()
                .height(56.dp),
            shape = RoundedCornerShape(16.dp)
        ) {
            Text("Continue")
        }
        
        Spacer(modifier = Modifier.height(24.dp))
    }
}

@Composable
private fun BioStep(
    bio: String,
    onBioChange: (String) -> Unit,
    onNext: () -> Unit,
    onBack: () -> Unit
) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(24.dp)
            .verticalScroll(rememberScrollState()),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.Start
        ) {
            IconButton(onClick = onBack) {
                Icon(Icons.Default.ArrowBack, contentDescription = "Back")
            }
        }
        
        Spacer(modifier = Modifier.height(32.dp))
        
        Icon(
            imageVector = Icons.Default.Edit,
            contentDescription = null,
            modifier = Modifier.size(80.dp),
            tint = MaterialTheme.colorScheme.primary
        )
        
        Spacer(modifier = Modifier.height(24.dp))
        
        Text(
            text = "Tell us about yourself",
            style = MaterialTheme.typography.headlineSmall,
            fontWeight = FontWeight.Bold,
            textAlign = TextAlign.Center
        )
        
        Spacer(modifier = Modifier.height(8.dp))
        
        Text(
            text = "Optional: Let others know who you are",
            style = MaterialTheme.typography.bodyMedium,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
            textAlign = TextAlign.Center
        )
        
        Spacer(modifier = Modifier.height(32.dp))
        
        OutlinedTextField(
            value = bio,
            onValueChange = { if (it.length <= 280) onBioChange(it) },
            label = { Text("Bio") },
            placeholder = { Text("What makes you unique?") },
            modifier = Modifier
                .fillMaxWidth()
                .height(150.dp),
            shape = RoundedCornerShape(12.dp),
            keyboardOptions = KeyboardOptions(
                capitalization = KeyboardCapitalization.Sentences
            ),
            supportingText = {
                Text("${bio.length}/280")
            }
        )
        
        Spacer(modifier = Modifier.weight(1f))
        
        Button(
            onClick = onNext,
            modifier = Modifier
                .fillMaxWidth()
                .height(56.dp),
            shape = RoundedCornerShape(16.dp)
        ) {
            Text(if (bio.isBlank()) "Skip" else "Continue")
        }
        
        Spacer(modifier = Modifier.height(24.dp))
    }
}

@Composable
@OptIn(ExperimentalLayoutApi::class)
private fun InterestsStep(
    selectedInterests: Set<String>,
    onInterestToggle: (String) -> Unit,
    onComplete: () -> Unit,
    isLoading: Boolean,
    onBack: () -> Unit
) {
    val suggestedInterests = listOf(
        "Technology", "Music", "Sports", "Gaming", "Movies",
        "Books", "Food", "Travel", "Art", "Science",
        "Photography", "Fitness", "Fashion", "Business", "Politics"
    )
    
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(24.dp)
            .verticalScroll(rememberScrollState()),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.Start
        ) {
            IconButton(onClick = onBack, enabled = !isLoading) {
                Icon(Icons.Default.ArrowBack, contentDescription = "Back")
            }
        }
        
        Spacer(modifier = Modifier.height(16.dp))
        
        Icon(
            imageVector = Icons.Default.Interests,
            contentDescription = null,
            modifier = Modifier.size(80.dp),
            tint = MaterialTheme.colorScheme.primary
        )
        
        Spacer(modifier = Modifier.height(24.dp))
        
        Text(
            text = "What interests you?",
            style = MaterialTheme.typography.headlineSmall,
            fontWeight = FontWeight.Bold,
            textAlign = TextAlign.Center
        )
        
        Spacer(modifier = Modifier.height(8.dp))
        
        Text(
            text = "Select a few interests to personalize your experience",
            style = MaterialTheme.typography.bodyMedium,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
            textAlign = TextAlign.Center
        )
        
        Spacer(modifier = Modifier.height(24.dp))
        
        // Interest chips
        FlowRow(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.spacedBy(8.dp),
            verticalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            suggestedInterests.forEach { interest ->
                FilterChip(
                    selected = selectedInterests.contains(interest),
                    onClick = { onInterestToggle(interest) },
                    label = { Text(interest) },
                    leadingIcon = if (selectedInterests.contains(interest)) {
                        { Icon(Icons.Default.Check, contentDescription = null, Modifier.size(18.dp)) }
                    } else null,
                    enabled = !isLoading
                )
            }
        }
        
        Spacer(modifier = Modifier.height(16.dp))
        
        Text(
            text = "${selectedInterests.size} selected",
            style = MaterialTheme.typography.bodySmall,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
        
        Spacer(modifier = Modifier.weight(1f))
        
        Button(
            onClick = onComplete,
            enabled = !isLoading,
            modifier = Modifier
                .fillMaxWidth()
                .height(56.dp),
            shape = RoundedCornerShape(16.dp)
        ) {
            if (isLoading) {
                CircularProgressIndicator(
                    modifier = Modifier.size(24.dp),
                    color = MaterialTheme.colorScheme.onPrimary,
                    strokeWidth = 2.dp
                )
            } else {
                Text("Join the World!")
            }
        }
        
        Spacer(modifier = Modifier.height(24.dp))
    }
}
