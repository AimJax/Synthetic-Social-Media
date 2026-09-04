package com.syntheticsocialworld.app.ui.screens

import androidx.compose.animation.*
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.Message
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.syntheticsocialworld.app.data.api.SyntheticSocialWorldApi
import com.syntheticsocialworld.app.data.model.*
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class MainViewModel @Inject constructor(
    private val api: SyntheticSocialWorldApi
) : ViewModel() {
    
    var npcs by mutableStateOf<List<NPCDto>>(emptyList())
        private set
    var posts by mutableStateOf<List<PostDto>>(emptyList())
        private set
    var feed by mutableStateOf<List<PostDto>>(emptyList())
        private set
    var stats by mutableStateOf<SimulationStatsDto?>(null)
        private set
    var world by mutableStateOf<WorldDto?>(null)
        private set
    var isLoading by mutableStateOf(false)
        private set
    var error by mutableStateOf<String?>(null)
        private set
    var selectedNpc by mutableStateOf<NPCDto?>(null)
        private set
    var hasSeenOnboarding by mutableStateOf(false)
        private set
    var unreadNotifications by mutableStateOf(0)
        private set
    
    init {
        loadInitialData()
    }
    
    fun completeOnboarding() {
        hasSeenOnboarding = true
    }
    
    fun loadInitialData() {
        viewModelScope.launch {
            isLoading = true
            error = null
            try {
                // Load stats and world
                stats = api.getSimulationStats()
                world = api.getWorld()
                
                // Load NPCs
                npcs = api.getNPCs(limit = 20)
                
                // Load posts
                posts = api.getRecentPosts(limit = 20)
                
                // Load feed for first NPC if available
                if (npcs.isNotEmpty()) {
                    if (selectedNpc == null) {
                        selectedNpc = npcs.first()
                    }
                    selectedNpc?.let { npc ->
                        feed = api.getFeed(npc.id, limit = 20)
                    }
                }
                
            } catch (e: Exception) {
                error = getFriendlyErrorMessage(e)
            } finally {
                isLoading = false
            }
        }
    }
    
    private fun getFriendlyErrorMessage(e: Exception): String {
        val message = e.message ?: ""
        return when {
            message.contains("Unable to resolve host") -> 
                "Can't connect to server. Make sure the app is running."
            message.contains("timeout") -> 
                "Connection timed out. Please try again."
            message.contains("network") -> 
                "Network error. Check your connection."
            else -> "Something went wrong. Pull down to refresh."
        }
    }
    
    fun refreshFeed() {
        viewModelScope.launch {
            isLoading = true
            try {
                selectedNpc?.let { npc ->
                    feed = api.getFeed(npc.id, limit = 20)
                }
            } catch (e: Exception) {
                error = getFriendlyErrorMessage(e)
            } finally {
                isLoading = false
            }
        }
    }
    
    fun loadTrending() {
        viewModelScope.launch {
            try {
                posts = api.getTrendingPosts(limit = 20)
            } catch (e: Exception) {
                error = getFriendlyErrorMessage(e)
            }
        }
    }
    
    fun selectNpc(npc: NPCDto) {
        selectedNpc = npc
        refreshFeed()
    }
    
    fun likePost(postId: String) {
        viewModelScope.launch {
            try {
                selectedNpc?.let { npc ->
                    api.likePost(postId, LikeRequest(npc.id))
                    refreshFeed()
                }
            } catch (e: Exception) {
                error = getFriendlyErrorMessage(e)
            }
        }
    }
    
    fun createPost(content: String) {
        viewModelScope.launch {
            try {
                selectedNpc?.let { npc ->
                    api.createPost(CreatePostRequest(npc.id, content))
                    refreshFeed()
                }
            } catch (e: Exception) {
                error = getFriendlyErrorMessage(e)
            }
        }
    }
    
    fun followNpc(npcId: String) {
        viewModelScope.launch {
            try {
                selectedNpc?.let { currentNpc ->
                    api.follow(FollowRequest(currentNpc.id, npcId))
                    loadInitialData()
                }
            } catch (e: Exception) {
                error = getFriendlyErrorMessage(e)
            }
        }
    }
    
    fun togglePause() {
        viewModelScope.launch {
            try {
                world?.let { w ->
                    api.togglePause(mapOf("isPaused" to !w.isPaused))
                    world = api.getWorld()
                    stats = api.getSimulationStats()
                }
            } catch (e: Exception) {
                error = getFriendlyErrorMessage(e)
            }
        }
    }
    
    fun advanceTime(minutes: Double) {
        viewModelScope.launch {
            try {
                api.advanceTime(mapOf("minutes" to minutes))
                world = api.getWorld()
                stats = api.getSimulationStats()
            } catch (e: Exception) {
                error = getFriendlyErrorMessage(e)
            }
        }
    }
    
    fun dismissError() {
        error = null
    }
}

// ============== MAIN SCREEN ==============

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MainScreen(
    currentPlayer: com.syntheticsocialworld.app.data.model.PlayerDto? = null,
    onLogout: (() -> Unit)? = null,
    viewModel: MainViewModel = hiltViewModel()
) {
    var selectedTab by remember { mutableIntStateOf(0) }
    
    Scaffold(
        topBar = {
            TopAppBar(
                title = { 
                    Text(
                        text = when (selectedTab) {
                            0 -> "Home"
                            1 -> "Explore"
                            2 -> "Create"
                            3 -> "Messages"
                            4 -> "Profile"
                            else -> "App"
                        },
                        fontWeight = FontWeight.Bold
                    )
                },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.surface
                ),
                actions = {
                    // Notifications badge
                    BadgedBox(
                        badge = {
                            if (viewModel.unreadNotifications > 0) {
                                Badge { Text("${viewModel.unreadNotifications}") }
                            }
                        }
                    ) {
                        IconButton(onClick = { /* Open notifications */ }) {
                            Icon(Icons.Default.Notifications, contentDescription = "Notifications")
                        }
                    }
                }
            )
        },
        bottomBar = {
            NavigationBar {
                NavigationBarItem(
                    icon = { Icon(Icons.Default.Home, contentDescription = "Home") },
                    label = { Text("Home") },
                    selected = selectedTab == 0,
                    onClick = { selectedTab = 0 }
                )
                NavigationBarItem(
                    icon = { Icon(Icons.Default.Explore, contentDescription = "Explore") },
                    label = { Text("Explore") },
                    selected = selectedTab == 1,
                    onClick = { selectedTab = 1 }
                )
                NavigationBarItem(
                    icon = { Icon(Icons.Default.Add, contentDescription = "Create") },
                    label = { Text("Create") },
                    selected = selectedTab == 2,
                    onClick = { selectedTab = 2 }
                )
                NavigationBarItem(
                    icon = { Icon(Icons.AutoMirrored.Filled.Message, contentDescription = "Messages") },
                    label = { Text("Messages") },
                    selected = selectedTab == 3,
                    onClick = { selectedTab = 3 }
                )
                NavigationBarItem(
                    icon = { Icon(Icons.Default.Person, contentDescription = "Profile") },
                    label = { Text("Profile") },
                    selected = selectedTab == 4,
                    onClick = { selectedTab = 4 }
                )
            }
        }
    ) { paddingValues ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
        ) {
            when (selectedTab) {
                0 -> HomeFeed(viewModel)
                1 -> ExploreScreen(viewModel)
                2 -> CreatePostScreen(viewModel)
                3 -> MessagesScreen(viewModel)
                4 -> ProfileScreen(viewModel)
            }
            
            // Global error snackbar
            viewModel.error?.let { error ->
                Snackbar(
                    modifier = Modifier
                        .align(Alignment.BottomCenter)
                        .padding(16.dp),
                    action = {
                        TextButton(onClick = { viewModel.dismissError() }) {
                            Text("Dismiss")
                        }
                    }
                ) {
                    Text(error)
                }
            }
        }
    }
}

// ============== ONBOARDING SCREEN ==============

@Composable
fun OnboardingScreen(onComplete: () -> Unit) {
    val pages = listOf(
        OnboardingPage(
            title = "Welcome to your new social world",
            description = "You've entered a living, breathing social network. The people here have their own lives, relationships, and stories.",
            image = Icons.Default.Groups
        ),
        OnboardingPage(
            title = "Meet interesting people",
            description = "Each person has their own personality, interests, and mood. Follow them, message them, and see what they're up to.",
            image = Icons.Default.PersonSearch
        ),
        OnboardingPage(
            title = "Share your thoughts",
            description = "Post updates, like content, and join conversations. The more you interact, the more the world responds.",
            image = Icons.Default.Edit
        ),
        OnboardingPage(
            title = "Watch the world evolve",
            description = "Over time, you'll see relationships form, trends emerge, and the community grow. Come back to see what's new!",
            image = Icons.Default.AutoAwesome
        )
    )
    
    var currentPage by remember { mutableIntStateOf(0) }
    
    Column(
        modifier = Modifier
            .fillMaxSize()
            .background(MaterialTheme.colorScheme.background)
            .padding(24.dp),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        // Page content
        Box(
            modifier = Modifier
                .weight(1f)
                .fillMaxWidth(),
            contentAlignment = Alignment.Center
        ) {
            OnboardingPageContent(pages[currentPage])
        }
        
        // Page indicators
        Row(
            horizontalArrangement = Arrangement.Center,
            modifier = Modifier.padding(vertical = 24.dp)
        ) {
            repeat(pages.size) { index ->
                Box(
                    modifier = Modifier
                        .padding(horizontal = 4.dp)
                        .size(if (index == currentPage) 12.dp else 8.dp)
                        .clip(CircleShape)
                        .background(
                            if (index == currentPage) 
                                MaterialTheme.colorScheme.primary 
                            else 
                                MaterialTheme.colorScheme.outlineVariant
                        )
                )
            }
        }
        
        // Navigation buttons
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.spacedBy(12.dp)
        ) {
            // Back button (only show if not on first page)
            if (currentPage > 0) {
                OutlinedButton(
                    onClick = { currentPage-- },
                    modifier = Modifier.weight(1f).height(56.dp),
                    shape = RoundedCornerShape(28.dp)
                ) {
                    Icon(Icons.Default.ArrowBack, contentDescription = "Back")
                }
            }
            
            // Main action button
            Button(
                onClick = {
                    if (currentPage == pages.size - 1) {
                        onComplete()
                    } else {
                        currentPage++
                    }
                },
                modifier = Modifier
                    .weight(if (currentPage > 0) 2f else 1f)
                    .height(56.dp),
                shape = RoundedCornerShape(28.dp)
            ) {
                Text(
                    text = if (currentPage == pages.size - 1) "Get Started" else "Continue",
                    fontSize = 16.sp
                )
                if (currentPage < pages.size - 1) {
                    Spacer(modifier = Modifier.width(8.dp))
                    Icon(Icons.Default.ArrowForward, contentDescription = null)
                }
            }
        }
        
        // Skip button
        if (currentPage < pages.size - 1) {
            TextButton(onClick = onComplete) {
                Text("Skip onboarding")
            }
        }
    }
}

data class OnboardingPage(
    val title: String,
    val description: String,
    val image: androidx.compose.ui.graphics.vector.ImageVector
)

@Composable
fun OnboardingPageContent(page: OnboardingPage) {
    Column(
        modifier = Modifier.fillMaxSize(),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        Icon(
            imageVector = page.image,
            contentDescription = null,
            modifier = Modifier.size(120.dp),
            tint = MaterialTheme.colorScheme.primary
        )
        
        Spacer(modifier = Modifier.height(48.dp))
        
        Text(
            text = page.title,
            style = MaterialTheme.typography.headlineMedium,
            fontWeight = FontWeight.Bold,
            textAlign = TextAlign.Center
        )
        
        Spacer(modifier = Modifier.height(16.dp))
        
        Text(
            text = page.description,
            style = MaterialTheme.typography.bodyLarge,
            textAlign = TextAlign.Center,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
            modifier = Modifier.padding(horizontal = 24.dp)
        )
    }
}

// ============== HOME FEED ==============

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun HomeFeed(viewModel: MainViewModel) {
    val isLoading = viewModel.isLoading
    val feed = viewModel.feed
    val selectedNpc = viewModel.selectedNpc
    
    Box(modifier = Modifier.fillMaxSize()) {
        if (isLoading && feed.isEmpty()) {
            // Initial loading state
            LoadingContent()
        } else if (feed.isEmpty()) {
            // Empty state
            EmptyFeedState(
                onCreatePost = { /* Switch to create tab */ },
                onExplore = { /* Switch to explore tab */ }
            )
        } else {
            LazyColumn(
                modifier = Modifier.fillMaxSize(),
                contentPadding = PaddingValues(16.dp),
                verticalArrangement = Arrangement.spacedBy(12.dp)
            ) {
                // User context card
                item {
                    UserContextCard(npc = selectedNpc)
                }
                
                // Section header
                item {
                    Text(
                        text = "Latest from people you follow",
                        style = MaterialTheme.typography.titleMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
                
                // Feed posts
                items(feed, key = { it.id }) { post ->
                    AnimatedVisibility(
                        visible = true,
                        enter = fadeIn() + slideInVertically()
                    ) {
                        PostCard(
                            post = post,
                            onLike = { viewModel.likePost(post.id) }
                        )
                    }
                }
                
                // Pull to refresh indicator
                item {
                    if (isLoading) {
                        Box(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(16.dp),
                            contentAlignment = Alignment.Center
                        ) {
                            CircularProgressIndicator(modifier = Modifier.size(24.dp))
                        }
                    }
                }
            }
        }
    }
}

@Composable
fun UserContextCard(npc: NPCDto?) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        colors = CardDefaults.cardColors(
            containerColor = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.5f)
        )
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(16.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            // Avatar placeholder
            Box(
                modifier = Modifier
                    .size(48.dp)
                    .clip(CircleShape)
                    .background(MaterialTheme.colorScheme.primary),
                contentAlignment = Alignment.Center
            ) {
                Text(
                    text = npc?.displayName?.firstOrNull()?.uppercase() ?: "?",
                    color = MaterialTheme.colorScheme.onPrimary,
                    fontWeight = FontWeight.Bold
                )
            }
            
            Spacer(modifier = Modifier.width(12.dp))
            
            Column(modifier = Modifier.weight(1f)) {
                Text(
                    text = "Posting as",
                    style = MaterialTheme.typography.labelSmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
                Text(
                    text = npc?.displayName ?: "Loading...",
                    style = MaterialTheme.typography.titleMedium,
                    fontWeight = FontWeight.SemiBold
                )
            }
            
            // Mood indicator
            npc?.mood?.let { mood ->
                MoodBadge(mood = mood.primaryMood)
            }
        }
    }
}

@Composable
fun MoodBadge(mood: String) {
    val (emoji, color) = when (mood.lowercase()) {
        "happy", "excited" -> "😊" to Color(0xFF4CAF50)
        "sad" -> "😢" to Color(0xFF2196F3)
        "angry" -> "😠" to Color(0xFFF44336)
        "anxious", "worried" -> "😰" to Color(0xFFFF9800)
        else -> "😐" to Color(0xFF9E9E9E)
    }
    
    Surface(
        shape = RoundedCornerShape(16.dp),
        color = color.copy(alpha = 0.15f)
    ) {
        Text(
            text = emoji,
            modifier = Modifier.padding(horizontal = 12.dp, vertical = 4.dp),
            fontSize = 20.sp
        )
    }
}

@Composable
fun LoadingContent() {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp),
        verticalArrangement = Arrangement.spacedBy(12.dp)
    ) {
        // Skeleton cards
        repeat(5) {
            SkeletonCard()
        }
    }
}

@Composable
fun SkeletonCard() {
    Card(
        modifier = Modifier.fillMaxWidth()
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Row {
                Box(
                    modifier = Modifier
                        .size(40.dp)
                        .clip(CircleShape)
                        .background(MaterialTheme.colorScheme.surfaceVariant)
                )
                Spacer(modifier = Modifier.width(12.dp))
                Column(modifier = Modifier.weight(1f)) {
                    Box(
                        modifier = Modifier
                            .fillMaxWidth(0.4f)
                            .height(16.dp)
                            .clip(RoundedCornerShape(4.dp))
                            .background(MaterialTheme.colorScheme.surfaceVariant)
                    )
                    Spacer(modifier = Modifier.height(4.dp))
                    Box(
                        modifier = Modifier
                            .fillMaxWidth(0.25f)
                            .height(12.dp)
                            .clip(RoundedCornerShape(4.dp))
                            .background(MaterialTheme.colorScheme.surfaceVariant)
                    )
                }
            }
            Spacer(modifier = Modifier.height(12.dp))
            Box(
                modifier = Modifier
                    .fillMaxWidth()
                    .height(60.dp)
                    .clip(RoundedCornerShape(8.dp))
                    .background(MaterialTheme.colorScheme.surfaceVariant)
            )
        }
    }
}

@Composable
fun EmptyFeedState(onCreatePost: () -> Unit, onExplore: () -> Unit) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(32.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        Icon(
            imageVector = Icons.Default.Article,
            contentDescription = null,
            modifier = Modifier.size(80.dp),
            tint = MaterialTheme.colorScheme.outline
        )
        
        Spacer(modifier = Modifier.height(24.dp))
        
        Text(
            text = "Your feed is empty",
            style = MaterialTheme.typography.headlineSmall,
            fontWeight = FontWeight.Bold
        )
        
        Spacer(modifier = Modifier.height(8.dp))
        
        Text(
            text = "Follow some people or create your first post to get started!",
            style = MaterialTheme.typography.bodyMedium,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
            textAlign = TextAlign.Center
        )
        
        Spacer(modifier = Modifier.height(32.dp))
        
        Button(
            onClick = onExplore,
            modifier = Modifier.fillMaxWidth()
        ) {
            Icon(Icons.Default.Explore, contentDescription = null)
            Spacer(modifier = Modifier.width(8.dp))
            Text("Discover People")
        }
    }
}

// ============== POST CARD ==============

@Composable
fun PostCard(post: PostDto, onLike: () -> Unit) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(16.dp)
    ) {
        Column(
            modifier = Modifier.padding(16.dp)
        ) {
            // Author row
            Row(
                modifier = Modifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically
            ) {
                // Avatar
                Box(
                    modifier = Modifier
                        .size(40.dp)
                        .clip(CircleShape)
                        .background(
                            Brush.linearGradient(
                                colors = listOf(
                                    MaterialTheme.colorScheme.primary,
                                    MaterialTheme.colorScheme.secondary
                                )
                            )
                        ),
                    contentAlignment = Alignment.Center
                ) {
                    Text(
                        text = post.authorName.firstOrNull()?.uppercase() ?: "?",
                        color = MaterialTheme.colorScheme.onPrimary,
                        fontWeight = FontWeight.Bold
                    )
                }
                
                Spacer(modifier = Modifier.width(12.dp))
                
                Column(modifier = Modifier.weight(1f)) {
                    Text(
                        text = post.authorName,
                        style = MaterialTheme.typography.titleSmall,
                        fontWeight = FontWeight.SemiBold
                    )
                    Text(
                        text = "@${post.authorHandle}",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
                
                // More options
                IconButton(onClick = { /* Show options */ }) {
                    Icon(
                        Icons.Default.MoreHoriz,
                        contentDescription = "More",
                        tint = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
            }
            
            Spacer(modifier = Modifier.height(12.dp))
            
            // Post content
            Text(
                text = post.content,
                style = MaterialTheme.typography.bodyLarge
            )
            
            Spacer(modifier = Modifier.height(16.dp))
            
            // Action buttons
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                // Like button
                Row(verticalAlignment = Alignment.CenterVertically) {
                    IconButton(onClick = onLike) {
                        Icon(
                            Icons.Default.FavoriteBorder,
                            contentDescription = "Like",
                            tint = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                    Text(
                        text = "${post.likeCount}",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
                
                // Comment button
                Row(verticalAlignment = Alignment.CenterVertically) {
                    IconButton(onClick = { /* Open comments */ }) {
                        Icon(
                            Icons.Default.ChatBubbleOutline,
                            contentDescription = "Comment",
                            tint = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                    Text(
                        text = "${post.commentCount}",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
                
                // Share button
                IconButton(onClick = { /* Share */ }) {
                    Icon(
                        Icons.Default.Share,
                        contentDescription = "Share",
                        tint = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
            }
        }
    }
}

// ============== EXPLORE SCREEN ==============

@Composable
fun ExploreScreen(viewModel: MainViewModel) {
    val npcs = viewModel.npcs
    val isLoading = viewModel.isLoading
    var searchQuery by remember { mutableStateOf("") }
    
    LazyColumn(
        modifier = Modifier.fillMaxSize(),
        contentPadding = PaddingValues(16.dp),
        verticalArrangement = Arrangement.spacedBy(12.dp)
    ) {
        // Search bar
        item {
            OutlinedTextField(
                value = searchQuery,
                onValueChange = { searchQuery = it },
                modifier = Modifier.fillMaxWidth(),
                placeholder = { Text("Search people...") },
                leadingIcon = {
                    Icon(Icons.Default.Search, contentDescription = null)
                },
                trailingIcon = {
                    if (searchQuery.isNotEmpty()) {
                        IconButton(onClick = { searchQuery = "" }) {
                            Icon(Icons.Default.Clear, contentDescription = "Clear")
                        }
                    }
                },
                singleLine = true,
                shape = RoundedCornerShape(24.dp)
            )
        }
        
        // Section header
        item {
            Text(
                text = "People in this world",
                style = MaterialTheme.typography.titleMedium,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
        }
        
        if (isLoading && npcs.isEmpty()) {
            items(5) {
                SkeletonNPCCard()
            }
        } else {
            val filteredNPCs = if (searchQuery.isBlank()) {
                npcs
            } else {
                npcs.filter { 
                    it.displayName.contains(searchQuery, ignoreCase = true) ||
                    it.handle.contains(searchQuery, ignoreCase = true) ||
                    it.bio?.contains(searchQuery, ignoreCase = true) == true
                }
            }
            
            if (filteredNPCs.isEmpty()) {
                item {
                    EmptySearchState(query = searchQuery)
                }
            } else {
                items(filteredNPCs, key = { it.id }) { npc ->
                    NPCCard(
                        npc = npc,
                        onFollow = { viewModel.followNpc(npc.id) },
                        onSelect = { viewModel.selectNpc(npc) }
                    )
                }
            }
        }
    }
}

@Composable
fun SkeletonNPCCard() {
    Card(modifier = Modifier.fillMaxWidth()) {
        Column(modifier = Modifier.padding(16.dp)) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Box(
                    modifier = Modifier
                        .size(48.dp)
                        .clip(CircleShape)
                        .background(MaterialTheme.colorScheme.surfaceVariant)
                )
                Spacer(modifier = Modifier.width(12.dp))
                Column(modifier = Modifier.weight(1f)) {
                    Box(
                        modifier = Modifier
                            .fillMaxWidth(0.4f)
                            .height(16.dp)
                            .clip(RoundedCornerShape(4.dp))
                            .background(MaterialTheme.colorScheme.surfaceVariant)
                    )
                    Spacer(modifier = Modifier.height(4.dp))
                    Box(
                        modifier = Modifier
                            .fillMaxWidth(0.25f)
                            .height(12.dp)
                            .clip(RoundedCornerShape(4.dp))
                            .background(MaterialTheme.colorScheme.surfaceVariant)
                    )
                }
                Box(
                    modifier = Modifier
                        .width(80.dp)
                        .height(36.dp)
                        .clip(RoundedCornerShape(18.dp))
                        .background(MaterialTheme.colorScheme.surfaceVariant)
                )
            }
        }
    }
}

@Composable
fun EmptySearchState(query: String) {
    Column(
        modifier = Modifier
            .fillMaxWidth()
            .padding(32.dp),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Icon(
            imageVector = Icons.Default.SearchOff,
            contentDescription = null,
            modifier = Modifier.size(48.dp),
            tint = MaterialTheme.colorScheme.outline
        )
        Spacer(modifier = Modifier.height(16.dp))
        Text(
            text = "No results for \"$query\"",
            style = MaterialTheme.typography.bodyLarge
        )
    }
}

@Composable
fun NPCCard(npc: NPCDto, onFollow: () -> Unit, onSelect: () -> Unit) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        onClick = onSelect,
        shape = RoundedCornerShape(16.dp)
    ) {
        Column(
            modifier = Modifier.padding(16.dp)
        ) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically
            ) {
                // Avatar
                Box(
                    modifier = Modifier
                        .size(56.dp)
                        .clip(CircleShape)
                        .background(
                            Brush.linearGradient(
                                colors = listOf(
                                    MaterialTheme.colorScheme.primary,
                                    MaterialTheme.colorScheme.tertiary
                                )
                            )
                        ),
                    contentAlignment = Alignment.Center
                ) {
                    Text(
                        text = npc.displayName.firstOrNull()?.uppercase() ?: "?",
                        style = MaterialTheme.typography.titleLarge,
                        color = MaterialTheme.colorScheme.onPrimary,
                        fontWeight = FontWeight.Bold
                    )
                }
                
                Spacer(modifier = Modifier.width(12.dp))
                
                Column(modifier = Modifier.weight(1f)) {
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        Text(
                            text = npc.displayName,
                            style = MaterialTheme.typography.titleMedium,
                            fontWeight = FontWeight.SemiBold
                        )
                        Spacer(modifier = Modifier.width(8.dp))
                        MoodBadge(mood = npc.mood?.primaryMood ?: "neutral")
                    }
                    Text(
                        text = "@${npc.handle}",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
                
                FilledTonalButton(onClick = onFollow) {
                    Text("Follow")
                }
            }
            
            npc.bio?.let { bio ->
                Spacer(modifier = Modifier.height(12.dp))
                Text(
                    text = bio,
                    style = MaterialTheme.typography.bodyMedium,
                    maxLines = 2,
                    overflow = TextOverflow.Ellipsis
                )
            }
            
            npc.interests?.take(4)?.let { interests ->
                Spacer(modifier = Modifier.height(12.dp))
                Row(
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    interests.forEach { interest ->
                        SuggestionChip(
                            onClick = { },
                            label = {
                                Text(
                                    text = interest.topic,
                                    style = MaterialTheme.typography.labelSmall
                                )
                            }
                        )
                    }
                }
            }
        }
    }
}

// ============== CREATE POST SCREEN ==============

@Composable
fun CreatePostScreen(viewModel: MainViewModel) {
    var postContent by remember { mutableStateOf("") }
    val isLoading = viewModel.isLoading
    val selectedNpc = viewModel.selectedNpc
    var showSuccessMessage by remember { mutableStateOf(false) }
    
    LaunchedEffect(showSuccessMessage) {
        if (showSuccessMessage) {
            kotlinx.coroutines.delay(2000)
            showSuccessMessage = false
            postContent = ""
        }
    }
    
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp)
    ) {
        // User context
        selectedNpc?.let { npc ->
            Row(
                verticalAlignment = Alignment.CenterVertically,
                modifier = Modifier.padding(bottom = 16.dp)
            ) {
                Box(
                    modifier = Modifier
                        .size(40.dp)
                        .clip(CircleShape)
                        .background(MaterialTheme.colorScheme.primary),
                    contentAlignment = Alignment.Center
                ) {
                    Text(
                        text = npc.displayName.firstOrNull()?.uppercase() ?: "?",
                        color = MaterialTheme.colorScheme.onPrimary,
                        fontWeight = FontWeight.Bold
                    )
                }
                Spacer(modifier = Modifier.width(12.dp))
                Column {
                    Text(
                        text = "Posting as",
                        style = MaterialTheme.typography.labelSmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                    Text(
                        text = npc.displayName,
                        style = MaterialTheme.typography.titleSmall,
                        fontWeight = FontWeight.SemiBold
                    )
                }
            }
        }
        
        // Post input
        OutlinedTextField(
            value = postContent,
            onValueChange = { 
                if (it.length <= 500) { // Character limit
                    postContent = it
                }
            },
            modifier = Modifier
                .fillMaxWidth()
                .weight(1f),
            placeholder = { 
                Text("What's on your mind?") 
            },
            shape = RoundedCornerShape(16.dp)
        )
        
        // Character count
        Text(
            text = "${postContent.length}/500",
            style = MaterialTheme.typography.bodySmall,
            color = if (postContent.length > 450) 
                MaterialTheme.colorScheme.error 
            else 
                MaterialTheme.colorScheme.onSurfaceVariant,
            modifier = Modifier.align(Alignment.End)
        )
        
        Spacer(modifier = Modifier.height(16.dp))
        
        // Success message
        AnimatedVisibility(visible = showSuccessMessage) {
            Card(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(bottom = 16.dp),
                colors = CardDefaults.cardColors(
                    containerColor = MaterialTheme.colorScheme.primaryContainer
                )
            ) {
                Row(
                    modifier = Modifier.padding(16.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Icon(
                        Icons.Default.CheckCircle,
                        contentDescription = null,
                        tint = MaterialTheme.colorScheme.primary
                    )
                    Spacer(modifier = Modifier.width(12.dp))
                    Text("Post shared successfully!")
                }
            }
        }
        
        // Post button
        Button(
            onClick = {
                if (postContent.isNotBlank()) {
                    viewModel.createPost(postContent)
                    showSuccessMessage = true
                }
            },
            modifier = Modifier
                .fillMaxWidth()
                .height(56.dp),
            enabled = postContent.isNotBlank() && !isLoading,
            shape = RoundedCornerShape(28.dp)
        ) {
            if (isLoading) {
                CircularProgressIndicator(
                    modifier = Modifier.size(24.dp),
                    color = MaterialTheme.colorScheme.onPrimary
                )
            } else {
                Icon(Icons.Default.Send, contentDescription = null)
                Spacer(modifier = Modifier.width(8.dp))
                Text("Share Post", fontSize = 16.sp)
            }
        }
    }
}

// ============== MESSAGES SCREEN ==============

@Composable
fun MessagesScreen(viewModel: MainViewModel) {
    val npcs = viewModel.npcs
    val selectedNpc = viewModel.selectedNpc
    var selectedConversation by remember { mutableStateOf<NPCDto?>(null) }
    
    if (selectedConversation != null) {
        ChatScreen(
            recipient = selectedConversation!!,
            onBack = { selectedConversation = null }
        )
    } else {
        LazyColumn(
            modifier = Modifier.fillMaxSize(),
            contentPadding = PaddingValues(16.dp),
            verticalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            item {
                Text(
                    text = "Messages",
                    style = MaterialTheme.typography.headlineSmall,
                    fontWeight = FontWeight.Bold
                )
                Spacer(modifier = Modifier.height(8.dp))
                Text(
                    text = "Start a conversation with someone",
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
                Spacer(modifier = Modifier.height(16.dp))
            }
            
            val otherNPCs = npcs.filter { it.id != selectedNpc?.id }
            
            if (otherNPCs.isEmpty()) {
                item {
                    EmptyMessagesState()
                }
            } else {
                items(otherNPCs, key = { it.id }) { npc ->
                    ConversationPreviewCard(
                        npc = npc,
                        onClick = { selectedConversation = npc }
                    )
                }
            }
        }
    }
}

@Composable
fun ConversationPreviewCard(npc: NPCDto, onClick: () -> Unit) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        onClick = onClick,
        shape = RoundedCornerShape(12.dp)
    ) {
        Row(
            modifier = Modifier.padding(16.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Box(
                modifier = Modifier
                    .size(48.dp)
                    .clip(CircleShape)
                    .background(MaterialTheme.colorScheme.secondaryContainer),
                contentAlignment = Alignment.Center
            ) {
                Text(
                    text = npc.displayName.firstOrNull()?.uppercase() ?: "?",
                    style = MaterialTheme.typography.titleMedium,
                    color = MaterialTheme.colorScheme.onSecondaryContainer
                )
            }
            
            Spacer(modifier = Modifier.width(12.dp))
            
            Column(modifier = Modifier.weight(1f)) {
                Text(
                    text = npc.displayName,
                    style = MaterialTheme.typography.titleSmall,
                    fontWeight = FontWeight.SemiBold
                )
                Text(
                    text = "@${npc.handle}",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
            
            Icon(
                Icons.Default.ChevronRight,
                contentDescription = null,
                tint = MaterialTheme.colorScheme.onSurfaceVariant
            )
        }
    }
}

@Composable
fun EmptyMessagesState() {
    Column(
        modifier = Modifier
            .fillMaxWidth()
            .padding(32.dp),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Icon(
            imageVector = Icons.AutoMirrored.Filled.Message,
            contentDescription = null,
            modifier = Modifier.size(64.dp),
            tint = MaterialTheme.colorScheme.outline
        )
        Spacer(modifier = Modifier.height(16.dp))
        Text(
            text = "No conversations yet",
            style = MaterialTheme.typography.titleMedium
        )
        Text(
            text = "Start chatting with someone from Explore!",
            style = MaterialTheme.typography.bodyMedium,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
            textAlign = TextAlign.Center
        )
    }
}

// ============== CHAT SCREEN ==============

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ChatScreen(recipient: NPCDto, onBack: () -> Unit) {
    var messageText by remember { mutableStateOf("") }
    
    Column(modifier = Modifier.fillMaxSize()) {
        // Chat header
        TopAppBar(
            title = {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Box(
                        modifier = Modifier
                            .size(36.dp)
                            .clip(CircleShape)
                            .background(MaterialTheme.colorScheme.primaryContainer),
                        contentAlignment = Alignment.Center
                    ) {
                        Text(
                            text = recipient.displayName.firstOrNull()?.uppercase() ?: "?",
                            color = MaterialTheme.colorScheme.onPrimaryContainer,
                            fontWeight = FontWeight.Bold
                        )
                    }
                    Spacer(modifier = Modifier.width(12.dp))
                    Column {
                        Text(
                            text = recipient.displayName,
                            style = MaterialTheme.typography.titleMedium
                        )
                        Text(
                            text = "@${recipient.handle}",
                            style = MaterialTheme.typography.bodySmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                }
            },
            navigationIcon = {
                IconButton(onClick = onBack) {
                    Icon(Icons.Default.ArrowBack, contentDescription = "Back")
                }
            }
        )
        
        // Chat messages area
        Box(
            modifier = Modifier
                .weight(1f)
                .fillMaxWidth()
                .padding(16.dp),
            contentAlignment = Alignment.Center
        ) {
            Column(horizontalAlignment = Alignment.CenterHorizontally) {
                Icon(
                    imageVector = Icons.Default.ChatBubbleOutline,
                    contentDescription = null,
                    modifier = Modifier.size(48.dp),
                    tint = MaterialTheme.colorScheme.outline
                )
                Spacer(modifier = Modifier.height(8.dp))
                Text(
                    text = "Start the conversation!",
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
        }
        
        // Message input
        Surface(
            modifier = Modifier.fillMaxWidth(),
            shadowElevation = 8.dp
        ) {
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(16.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                OutlinedTextField(
                    value = messageText,
                    onValueChange = { messageText = it },
                    modifier = Modifier.weight(1f),
                    placeholder = { Text("Type a message...") },
                    shape = RoundedCornerShape(24.dp),
                    maxLines = 3
                )
                
                Spacer(modifier = Modifier.width(8.dp))
                
                FilledIconButton(
                    onClick = { 
                        if (messageText.isNotBlank()) {
                            // Would send message
                            messageText = ""
                        }
                    },
                    enabled = messageText.isNotBlank()
                ) {
                    Icon(Icons.Default.Send, contentDescription = "Send")
                }
            }
        }
    }
}

// ============== PROFILE SCREEN ==============

@Composable
fun ProfileScreen(viewModel: MainViewModel) {
    val selectedNpc = viewModel.selectedNpc
    val stats = viewModel.stats
    val world = viewModel.world
    val isLoading = viewModel.isLoading
    
    LazyColumn(
        modifier = Modifier.fillMaxSize(),
        contentPadding = PaddingValues(16.dp),
        verticalArrangement = Arrangement.spacedBy(16.dp)
    ) {
        // Profile header
        item {
            selectedNpc?.let { npc ->
                ProfileHeader(npc = npc)
            } ?: Box(
                modifier = Modifier
                    .fillMaxWidth()
                    .height(200.dp),
                contentAlignment = Alignment.Center
            ) {
                if (isLoading) {
                    CircularProgressIndicator()
                } else {
                    Text("No profile selected")
                }
            }
        }
        
        // Mood card
        selectedNpc?.mood?.let { mood ->
            item {
                MoodCard(mood = mood)
            }
        }
        
        // Interests
        selectedNpc?.interests?.let { interests ->
            if (interests.isNotEmpty()) {
                item {
                    InterestsCard(interests = interests)
                }
            }
        }
        
        // World activity
        item {
            ActivityCard(
                stats = stats,
                world = world,
                posts = viewModel.posts.filter { it.authorId == selectedNpc?.id }
            )
        }
        
        // Switch profile section
        item {
            SwitchProfileCard(
                currentNpc = selectedNpc,
                allNpcs = viewModel.npcs,
                onSelect = { viewModel.selectNpc(it) }
            )
        }
    }
}

@Composable
fun ProfileHeader(npc: NPCDto) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        colors = CardDefaults.cardColors(
            containerColor = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.3f)
        ),
        shape = RoundedCornerShape(24.dp)
    ) {
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .padding(24.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            // Large avatar
            Box(
                modifier = Modifier
                    .size(100.dp)
                    .clip(CircleShape)
                    .background(
                        Brush.linearGradient(
                            colors = listOf(
                                MaterialTheme.colorScheme.primary,
                                MaterialTheme.colorScheme.tertiary
                            )
                        )
                    ),
                contentAlignment = Alignment.Center
            ) {
                Text(
                    text = npc.displayName.firstOrNull()?.uppercase() ?: "?",
                    style = MaterialTheme.typography.displayMedium,
                    color = MaterialTheme.colorScheme.onPrimary,
                    fontWeight = FontWeight.Bold
                )
            }
            
            Spacer(modifier = Modifier.height(16.dp))
            
            Text(
                text = npc.displayName,
                style = MaterialTheme.typography.headlineMedium,
                fontWeight = FontWeight.Bold
            )
            
            Text(
                text = "@${npc.handle}",
                style = MaterialTheme.typography.bodyLarge,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
            
            npc.bio?.let { bio ->
                Spacer(modifier = Modifier.height(12.dp))
                Text(
                    text = bio,
                    style = MaterialTheme.typography.bodyMedium,
                    textAlign = TextAlign.Center
                )
            }
        }
    }
}

@Composable
fun MoodCard(mood: MoodDto) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(16.dp)
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Text(
                text = "Current Mood",
                style = MaterialTheme.typography.titleMedium,
                fontWeight = FontWeight.SemiBold
            )
            
            Spacer(modifier = Modifier.height(12.dp))
            
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceEvenly
            ) {
                MoodIndicator(label = "Happy", value = mood.happiness, emoji = "😊")
                MoodIndicator(label = "Excited", value = mood.excitement, emoji = "🎉")
                MoodIndicator(label = "Calm", value = 1.0 - mood.anxiety, emoji = "😌")
            }
        }
    }
}

@Composable
fun MoodIndicator(label: String, value: Double, emoji: String) {
    Column(horizontalAlignment = Alignment.CenterHorizontally) {
        Text(text = emoji, fontSize = 24.sp)
        Text(
            text = "${(value * 100).toInt()}%",
            style = MaterialTheme.typography.titleSmall,
            fontWeight = FontWeight.Bold
        )
        Text(
            text = label,
            style = MaterialTheme.typography.labelSmall,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
    }
}

@Composable
fun InterestsCard(interests: List<InterestDto>) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(16.dp)
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Text(
                text = "Interests",
                style = MaterialTheme.typography.titleMedium,
                fontWeight = FontWeight.SemiBold
            )
            
            Spacer(modifier = Modifier.height(12.dp))
            
            FlowRow(
                horizontalArrangement = Arrangement.spacedBy(8.dp),
                verticalArrangement = Arrangement.spacedBy(8.dp)
            ) {
                interests.forEach { interest ->
                    SuggestionChip(
                        onClick = { },
                        label = { Text(interest.topic) }
                    )
                }
            }
        }
    }
}

@OptIn(ExperimentalLayoutApi::class)
@Composable
fun FlowRow(
    horizontalArrangement: Arrangement.Horizontal,
    verticalArrangement: Arrangement.Vertical,
    content: @Composable () -> Unit
) {
    // Simple implementation - in production would use proper FlowRow
    androidx.compose.foundation.layout.FlowRow(
        horizontalArrangement = horizontalArrangement,
        verticalArrangement = verticalArrangement
    ) {
        content()
    }
}

@Composable
fun ActivityCard(
    stats: SimulationStatsDto?,
    world: WorldDto?,
    posts: List<PostDto>
) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(16.dp)
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Text(
                text = "Your Activity",
                style = MaterialTheme.typography.titleMedium,
                fontWeight = FontWeight.SemiBold
            )
            
            Spacer(modifier = Modifier.height(12.dp))
            
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceEvenly
            ) {
                StatItem(value = "${posts.size}", label = "Posts")
                StatItem(value = "${posts.sumOf { it.likeCount }}", label = "Likes")
                StatItem(value = "${posts.sumOf { it.commentCount }}", label = "Comments")
            }
            
            Spacer(modifier = Modifier.height(16.dp))
            
            // World status
            world?.let { w ->
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Icon(
                        imageVector = if (w.isPaused) Icons.Default.Pause else Icons.Default.PlayArrow,
                        contentDescription = null,
                        tint = if (w.isPaused) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.primary,
                        modifier = Modifier.size(20.dp)
                    )
                    Spacer(modifier = Modifier.width(8.dp))
                    Text(
                        text = if (w.isPaused) "World is paused" else "World is active",
                        style = MaterialTheme.typography.bodyMedium
                    )
                }
            }
        }
    }
}

@Composable
fun StatItem(value: String, label: String) {
    Column(horizontalAlignment = Alignment.CenterHorizontally) {
        Text(
            text = value,
            style = MaterialTheme.typography.headlineSmall,
            fontWeight = FontWeight.Bold
        )
        Text(
            text = label,
            style = MaterialTheme.typography.labelMedium,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
    }
}

@Composable
fun SwitchProfileCard(
    currentNpc: NPCDto?,
    allNpcs: List<NPCDto>,
    onSelect: (NPCDto) -> Unit
) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(16.dp)
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Text(
                text = "Switch Profile",
                style = MaterialTheme.typography.titleMedium,
                fontWeight = FontWeight.SemiBold
            )
            
            Spacer(modifier = Modifier.height(4.dp))
            
            Text(
                text = "Currently viewing as ${currentNpc?.displayName ?: "Unknown"}",
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
            
            Spacer(modifier = Modifier.height(12.dp))
            
            // Show a few options to switch
            allNpcs.take(5).forEach { npc ->
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(vertical = 4.dp),
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.SpaceBetween
                ) {
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        Box(
                            modifier = Modifier
                                .size(32.dp)
                                .clip(CircleShape)
                                .background(MaterialTheme.colorScheme.surfaceVariant),
                            contentAlignment = Alignment.Center
                        ) {
                            Text(
                                text = npc.displayName.firstOrNull()?.uppercase() ?: "?",
                                style = MaterialTheme.typography.labelMedium
                            )
                        }
                        Spacer(modifier = Modifier.width(8.dp))
                        Text(
                            text = npc.displayName,
                            style = MaterialTheme.typography.bodyMedium
                        )
                    }
                    
                    if (npc.id == currentNpc?.id) {
                        Icon(
                            Icons.Default.Check,
                            contentDescription = "Current",
                            tint = MaterialTheme.colorScheme.primary
                        )
                    } else {
                        TextButton(onClick = { onSelect(npc) }) {
                            Text("Switch")
                        }
                    }
                }
            }
            
            if (allNpcs.size > 5) {
                TextButton(onClick = { /* Show all in explore */ }) {
                    Text("See all ${allNpcs.size} profiles")
                }
            }
        }
    }
}
