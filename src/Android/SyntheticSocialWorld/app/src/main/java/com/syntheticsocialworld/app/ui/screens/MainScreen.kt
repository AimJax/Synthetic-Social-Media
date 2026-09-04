package com.syntheticsocialworld.app.ui.screens

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.Message
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
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
    
    init {
        loadInitialData()
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
                    selectedNpc = npcs.first()
                    feed = api.getFeed(npcs.first().id, limit = 20)
                }
                
            } catch (e: Exception) {
                error = e.message ?: "Failed to load data"
            } finally {
                isLoading = false
            }
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
                error = e.message
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
                error = e.message
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
                error = e.message
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
                error = e.message
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
                error = e.message
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
                error = e.message
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
                error = e.message
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MainScreen(
    viewModel: MainViewModel = hiltViewModel()
) {
    var selectedTab by remember { mutableIntStateOf(0) }
    
    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Synthetic Social World") },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.primaryContainer,
                    titleContentColor = MaterialTheme.colorScheme.onPrimaryContainer
                ),
                actions = {
                    IconButton(onClick = { viewModel.loadInitialData() }) {
                        Icon(Icons.Default.Refresh, contentDescription = "Refresh")
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
        Column(
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
        }
    }
}

@Composable
fun HomeFeed(viewModel: MainViewModel) {
    val isLoading = viewModel.isLoading
    val feed = viewModel.feed
    val selectedNpc = viewModel.selectedNpc
    val error = viewModel.error
    
    LazyColumn(
        modifier = Modifier.fillMaxSize(),
        contentPadding = PaddingValues(16.dp),
        verticalArrangement = Arrangement.spacedBy(12.dp)
    ) {
        // Current user info
        item {
            Card(
                modifier = Modifier.fillMaxWidth(),
                colors = CardDefaults.cardColors(
                    containerColor = MaterialTheme.colorScheme.primaryContainer
                )
            ) {
                Column(modifier = Modifier.padding(16.dp)) {
                    Text(
                        text = "Logged in as: ${selectedNpc?.displayName ?: "Loading..."}",
                        style = MaterialTheme.typography.titleMedium
                    )
                    Text(
                        text = "@${selectedNpc?.handle ?: ""}",
                        style = MaterialTheme.typography.bodySmall
                    )
                    selectedNpc?.mood?.let { mood ->
                        Text(
                            text = "Mood: ${mood.primaryMood} (${String.format("%.0f", mood.happiness * 100)}% happiness)",
                            style = MaterialTheme.typography.bodySmall
                        )
                    }
                }
            }
        }
        
        // Error message
        error?.let { err ->
            item {
                Card(
                    modifier = Modifier.fillMaxWidth(),
                    colors = CardDefaults.cardColors(
                        containerColor = MaterialTheme.colorScheme.errorContainer
                    )
                ) {
                    Text(
                        text = "Error: $err",
                        modifier = Modifier.padding(16.dp),
                        color = MaterialTheme.colorScheme.onErrorContainer
                    )
                }
            }
        }
        
        // Loading
        if (isLoading) {
            item {
                Box(
                    modifier = Modifier.fillMaxWidth(),
                    contentAlignment = Alignment.Center
                ) {
                    CircularProgressIndicator()
                }
            }
        }
        
        // Feed posts
        item {
            Text(
                text = "Your Feed",
                style = MaterialTheme.typography.headlineSmall
            )
        }
        
        if (feed.isEmpty() && !isLoading) {
            item {
                Text(
                    text = "No posts yet. Follow some NPCs or create a post!",
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
        }
        
        items(feed) { post ->
            PostCard(
                post = post,
                onLike = { viewModel.likePost(post.id) }
            )
        }
    }
}

@Composable
fun PostCard(post: PostDto, onLike: () -> Unit) {
    Card(
        modifier = Modifier.fillMaxWidth()
    ) {
        Column(
            modifier = Modifier.padding(16.dp)
        ) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween
            ) {
                Column {
                    Text(
                        text = post.authorName,
                        style = MaterialTheme.typography.titleMedium
                    )
                    Text(
                        text = "@${post.authorHandle}",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
                IconButton(onClick = { /* More options */ }) {
                    Icon(Icons.Default.MoreVert, contentDescription = "More")
                }
            }
            Spacer(modifier = Modifier.height(8.dp))
            Text(
                text = post.content,
                style = MaterialTheme.typography.bodyLarge
            )
            Spacer(modifier = Modifier.height(12.dp))
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceEvenly
            ) {
                TextButton(onClick = onLike) {
                    Icon(Icons.Default.Favorite, contentDescription = "Like", modifier = Modifier.size(20.dp))
                    Spacer(modifier = Modifier.width(4.dp))
                    Text("${post.likeCount}")
                }
                TextButton(onClick = { /* Comment */ }) {
                    Icon(Icons.Default.ChatBubbleOutline, contentDescription = "Comment", modifier = Modifier.size(20.dp))
                    Spacer(modifier = Modifier.width(4.dp))
                    Text("${post.commentCount}")
                }
                TextButton(onClick = { /* Share */ }) {
                    Icon(Icons.Default.Share, contentDescription = "Share", modifier = Modifier.size(20.dp))
                }
            }
        }
    }
}

@Composable
fun ExploreScreen(viewModel: MainViewModel) {
    val npcs = viewModel.npcs
    val stats = viewModel.stats
    
    LazyColumn(
        modifier = Modifier.fillMaxSize(),
        contentPadding = PaddingValues(16.dp),
        verticalArrangement = Arrangement.spacedBy(12.dp)
    ) {
        item {
            Text(
                text = "Explore NPCs",
                style = MaterialTheme.typography.headlineMedium
            )
            stats?.let { s ->
                Text(
                    text = "${s.counts.npcs} NPCs, ${s.counts.posts} posts, ${s.counts.communities} communities",
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
        }
        
        items(npcs) { npc ->
            NPCCard(
                npc = npc,
                onFollow = { viewModel.followNpc(npc.id) },
                onSelect = { viewModel.selectNpc(npc) }
            )
        }
    }
}

@Composable
fun NPCCard(npc: NPCDto, onFollow: () -> Unit, onSelect: () -> Unit) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        onClick = onSelect
    ) {
        Column(
            modifier = Modifier.padding(16.dp)
        ) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Column(modifier = Modifier.weight(1f)) {
                    Text(
                        text = npc.displayName,
                        style = MaterialTheme.typography.titleMedium
                    )
                    Text(
                        text = "@${npc.handle}",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
                Button(onClick = onFollow) {
                    Text("Follow")
                }
            }
            
            npc.bio?.let { bio ->
                Spacer(modifier = Modifier.height(8.dp))
                Text(
                    text = bio,
                    style = MaterialTheme.typography.bodyMedium
                )
            }
            
            Spacer(modifier = Modifier.height(8.dp))
            
            npc.mood?.let { mood ->
                Text(
                    text = "Mood: ${mood.primaryMood}",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.primary
                )
            }
            
            npc.personality?.let { personality ->
                Spacer(modifier = Modifier.height(4.dp))
                Text(
                    text = "Extroversion: ${String.format("%.0f", personality.extroversion * 100)}%",
                    style = MaterialTheme.typography.bodySmall
                )
            }
            
            npc.interests?.take(3)?.let { interests ->
                Spacer(modifier = Modifier.height(4.dp))
                Text(
                    text = "Interests: ${interests.joinToString(", ") { it.topic }}",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
        }
    }
}

@Composable
fun CreatePostScreen(viewModel: MainViewModel) {
    var postContent by remember { mutableStateOf("") }
    
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp)
    ) {
        Text(
            text = "Create Post",
            style = MaterialTheme.typography.headlineMedium
        )
        
        viewModel.selectedNpc?.let { npc ->
            Text(
                text = "Posting as ${npc.displayName}",
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
        }
        
        Spacer(modifier = Modifier.height(16.dp))
        
        OutlinedTextField(
            value = postContent,
            onValueChange = { postContent = it },
            modifier = Modifier
                .fillMaxWidth()
                .height(200.dp),
            placeholder = { Text("What's happening in the synthetic world?") },
            maxLines = 10
        )
        
        Spacer(modifier = Modifier.height(16.dp))
        
        Button(
            onClick = {
                if (postContent.isNotBlank()) {
                    viewModel.createPost(postContent)
                    postContent = ""
                }
            },
            modifier = Modifier.fillMaxWidth(),
            enabled = postContent.isNotBlank()
        ) {
            Icon(Icons.Default.Send, contentDescription = null)
            Spacer(modifier = Modifier.width(8.dp))
            Text("Post")
        }
    }
}

@Composable
fun MessagesScreen(viewModel: MainViewModel) {
    val npcs = viewModel.npcs
    val selectedNpc = viewModel.selectedNpc
    
    LazyColumn(
        modifier = Modifier.fillMaxSize(),
        contentPadding = PaddingValues(16.dp),
        verticalArrangement = Arrangement.spacedBy(8.dp)
    ) {
        item {
            Text(
                text = "Messages",
                style = MaterialTheme.typography.headlineMedium
            )
            Text(
                text = "Start a conversation with an NPC",
                style = MaterialTheme.typography.bodyMedium,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
        }
        
        items(npcs.filter { it.id != selectedNpc?.id }) { npc ->
            Card(
                modifier = Modifier.fillMaxWidth(),
                onClick = { /* Open chat */ }
            ) {
                Row(
                    modifier = Modifier.padding(16.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Icon(
                        Icons.Default.Person,
                        contentDescription = null,
                        modifier = Modifier.size(40.dp)
                    )
                    Spacer(modifier = Modifier.width(12.dp))
                    Column {
                        Text(
                            text = npc.displayName,
                            style = MaterialTheme.typography.titleMedium
                        )
                        Text(
                            text = "@${npc.handle}",
                            style = MaterialTheme.typography.bodySmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                }
            }
        }
    }
}

@Composable
fun ProfileScreen(viewModel: MainViewModel) {
    val selectedNpc = viewModel.selectedNpc
    val stats = viewModel.stats
    val world = viewModel.world
    
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp)
    ) {
        Text(
            text = "Profile",
            style = MaterialTheme.typography.headlineMedium
        )
        
        selectedNpc?.let { npc ->
            Card(
                modifier = Modifier.fillMaxWidth(),
                colors = CardDefaults.cardColors(
                    containerColor = MaterialTheme.colorScheme.primaryContainer
                )
            ) {
                Column(modifier = Modifier.padding(16.dp)) {
                    Text(
                        text = npc.displayName,
                        style = MaterialTheme.typography.headlineSmall
                    )
                    Text(
                        text = "@${npc.handle}",
                        style = MaterialTheme.typography.bodyMedium
                    )
                    npc.bio?.let {
                        Spacer(modifier = Modifier.height(8.dp))
                        Text(text = it)
                    }
                }
            }
            
            Spacer(modifier = Modifier.height(16.dp))
            
            npc.mood?.let { mood ->
                Card(modifier = Modifier.fillMaxWidth()) {
                    Column(modifier = Modifier.padding(16.dp)) {
                        Text(
                            text = "Mood Status",
                            style = MaterialTheme.typography.titleMedium
                        )
                        Text("Primary: ${mood.primaryMood}")
                        Text("Happiness: ${String.format("%.0f", mood.happiness * 100)}%")
                        Text("Excitement: ${String.format("%.0f", mood.excitement * 100)}%")
                    }
                }
            }
            
            Spacer(modifier = Modifier.height(16.dp))
            
            Card(modifier = Modifier.fillMaxWidth()) {
                Column(modifier = Modifier.padding(16.dp)) {
                    Text(
                        text = "Personality",
                        style = MaterialTheme.typography.titleMedium
                    )
                    npc.personality?.let { p ->
                        Text("Extroversion: ${String.format("%.0f", p.extroversion * 100)}%")
                        Text("Agreeableness: ${String.format("%.0f", p.agreeableness * 100)}%")
                        Text("Neuroticism: ${String.format("%.0f", p.neuroticism * 100)}%")
                        Text("Humor: ${String.format("%.0f", p.humor * 100)}%")
                    }
                }
            }
        }
        
        Spacer(modifier = Modifier.height(16.dp))
        
        // Simulation Controls
        Card(modifier = Modifier.fillMaxWidth()) {
            Column(modifier = Modifier.padding(16.dp)) {
                Text(
                    text = "Simulation Control",
                    style = MaterialTheme.typography.titleMedium
                )
                
                world?.let { w ->
                    Text("Status: ${if (w.isPaused) "PAUSED" else "Running"}")
                    Text("World Time: ${w.currentTime}")
                }
                
                Spacer(modifier = Modifier.height(8.dp))
                
                Row(
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    Button(
                        onClick = { viewModel.togglePause() },
                        colors = if (world?.isPaused == true) 
                            ButtonDefaults.buttonColors() 
                        else 
                            ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.secondary)
                    ) {
                        Icon(Icons.Default.Pause, contentDescription = null)
                        Spacer(modifier = Modifier.width(4.dp))
                        Text(if (world?.isPaused == true) "Resume" else "Pause")
                    }
                    
                    Button(onClick = { viewModel.advanceTime(60.0) }) {
                        Icon(Icons.Default.SkipNext, contentDescription = null)
                        Spacer(modifier = Modifier.width(4.dp))
                        Text("+1 Hour")
                    }
                }
            }
        }
        
        Spacer(modifier = Modifier.height(16.dp))
        
        // Stats
        stats?.let { s ->
            Card(modifier = Modifier.fillMaxWidth()) {
                Column(modifier = Modifier.padding(16.dp)) {
                    Text(
                        text = "World Statistics",
                        style = MaterialTheme.typography.titleMedium
                    )
                    Text("NPCs: ${s.counts.npcs}")
                    Text("Posts: ${s.counts.posts}")
                    Text("Communities: ${s.counts.communities}")
                    Text("Relationships: ${s.counts.relationships}")
                    Text("Total Likes: ${s.engagement.totalLikes}")
                    Text("Total Comments: ${s.engagement.totalComments}")
                }
            }
        }
    }
}
