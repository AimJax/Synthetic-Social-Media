package com.syntheticsocialworld.app

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.animation.*
import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Warning
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import com.syntheticsocialworld.app.ui.theme.SyntheticSocialWorldTheme
import com.syntheticsocialworld.app.ui.screens.*
import dagger.hilt.android.AndroidEntryPoint

@AndroidEntryPoint
class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            SyntheticSocialWorldTheme {
                Surface(
                    modifier = Modifier.fillMaxSize(),
                    color = MaterialTheme.colorScheme.background
                ) {
                    SyntheticSocialWorldApp()
                }
            }
        }
    }
}

@Composable
fun SyntheticSocialWorldApp(
    viewModel: AppNavigationViewModel = hiltViewModel()
) {
    val appState by viewModel.appState.collectAsState()
    
    AnimatedContent(
        targetState = appState,
        transitionSpec = {
            fadeIn() togetherWith fadeOut()
        },
        label = "app_navigation"
    ) { state ->
        when (state) {
            is AppState.Loading -> {
                LoadingScreen(message = "Loading...")
            }
            is AppState.Onboarding -> {
                OnboardingScreen(
                    onComplete = { viewModel.completeOnboarding() }
                )
            }
            is AppState.Main -> {
                MainScreen(
                    currentPlayer = state.player,
                    onLogout = { viewModel.logout() }
                )
            }
            is AppState.Error -> {
                ErrorScreen(
                    message = state.message,
                    onRetry = { viewModel.retry() }
                )
            }
        }
    }
}

@Composable
fun ErrorScreen(
    message: String,
    onRetry: () -> Unit
) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(32.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        Icon(
            imageVector = Icons.Default.Warning,
            contentDescription = null,
            modifier = Modifier.size(64.dp),
            tint = MaterialTheme.colorScheme.error
        )
        
        Spacer(modifier = Modifier.height(16.dp))
        
        Text(
            text = "Something went wrong",
            style = MaterialTheme.typography.headlineSmall,
            fontWeight = FontWeight.Bold
        )
        
        Spacer(modifier = Modifier.height(8.dp))
        
        Text(
            text = message,
            style = MaterialTheme.typography.bodyMedium,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
            textAlign = TextAlign.Center
        )
        
        Spacer(modifier = Modifier.height(24.dp))
        
        Button(onClick = onRetry) {
            Text("Try Again")
        }
    }
}
