package com.syntheticsocialworld.app.ui.screens

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
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
class CommunityDetailViewModel @Inject constructor(
    private val api: SyntheticSocialWorldApi
) : ViewModel() {
    
    var community by mutableStateOf<CommunityDto?>(null)
        private set
    var posts by mutableStateOf<List<PostDto>>(emptyList())
        private set
    var members by mutableStateOf<List<NPCSummaryDto>>(emptyList())
        private set
    var isLoading by mutableStateOf(false)
        private set
    var error by mutableStateOf<String?>(null)
        private set
    var isJoined by mutableStateOf(false)
        private set
    
    fun loadCommunity(communityId: String) {
        viewModelScope.launch {
            isLoading = true
            error = null
            try {
                // Load community details
                val allCommunities = api.getCommunities()
                community = allCommunities.find { it.id == communityId }
                
                // Load community posts (would need a dedicated endpoint)
                // For now, we'll filter from recent posts
                val recentPosts = api.getRecentPosts(limit = 50)
                posts = recentPosts.filter { it.communityId == communityId }
                
            } catch (e: Exception) {
                error = e.message ?: "Failed to load community"
            } finally {
                isLoading = false
            }
        }
    }
    
    fun joinCommunity(communityId: String, userId: String) {
        viewModelScope.launch {
            try {
                // Would call join endpoint if available
                isJoined = true
            } catch (e: Exception) {
                error = e.message
            }
        }
    }
    
    fun leaveCommunity(communityId: String, userId: String) {
        viewModelScope.launch {
            try {
                // Would call leave endpoint if available
                isJoined = false
            } catch (e: Exception) {
                error = e.message
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun CommunityDetailScreen(
    communityId: String,
    viewModel: CommunityDetailViewModel = hiltViewModel(),
    onBack: () -> Unit = {},
    onPostClick: (PostDto) -> Unit = {},
    onMemberClick: (NPCSummaryDto) -> Unit = {}
) {
    var showJoinDialog by remember { mutableStateOf(false) }
    
    LaunchedEffect(communityId) {
        viewModel.loadCommunity(communityId)
    }
    
    Column(
        modifier = Modifier.fillMaxSize()
    ) {
        // Header
        TopAppBar(
            title = { Text(viewModel.community?.name ?: "Community") },
            navigationIcon = {
                IconButton(onClick = onBack) {
                    Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Back")
                }
            },
            actions = {
                IconButton(onClick = { /* Share */ }) {
                    Icon(Icons.Default.Share, contentDescription = "Share")
                }
            }
        )
        
        if (viewModel.isLoading) {
            Box(
                modifier = Modifier.fillMaxSize(),
                contentAlignment = Alignment.Center
            ) {
                CircularProgressIndicator()
            }
        } else if (viewModel.error != null) {
            Box(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(16.dp),
                contentAlignment = Alignment.Center
            ) {
                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                    Icon(
                        Icons.Default.Error,
                        contentDescription = null,
                        modifier = Modifier.size(64.dp),
                        tint = MaterialTheme.colorScheme.error
                    )
                    Spacer(modifier = Modifier.height(16.dp))
                    Text(
                        text = viewModel.error ?: "Error loading community",
                        color = MaterialTheme.colorScheme.error
                    )
                    Spacer(modifier = Modifier.height(16.dp))
                    Button(onClick = { viewModel.loadCommunity(communityId) }) {
                        Text("Retry")
                    }
                }
            }
        } else {
            viewModel.community?.let { community ->
                LazyColumn(
                    modifier = Modifier.fillMaxSize(),
                    contentPadding = PaddingValues(16.dp),
                    verticalArrangement = Arrangement.spacedBy(16.dp)
                ) {
                    // Community header
                    item {
                        CommunityHeader(
                            community = community,
                            isJoined = viewModel.isJoined,
                            onJoinClick = { showJoinDialog = true },
                            onLeaveClick = { viewModel.leaveCommunity(communityId, "player") }
                        )
                    }
                    
                    // Description
                    community.description?.let { desc ->
                        item {
                            Card(modifier = Modifier.fillMaxWidth()) {
                                Column(modifier = Modifier.padding(16.dp)) {
                                    Text(
                                        text = "About",
                                        style = MaterialTheme.typography.titleMedium
                                    )
                                    Spacer(modifier = Modifier.height(8.dp))
                                    Text(
                                        text = desc,
                                        style = MaterialTheme.typography.bodyMedium,
                                        color = MaterialTheme.colorScheme.onSurfaceVariant
                                    )
                                }
                            }
                        }
                    }
                    
                    // Stats
                    item {
                        Card(modifier = Modifier.fillMaxWidth()) {
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(16.dp),
                                horizontalArrangement = Arrangement.SpaceEvenly
                            ) {
                                CommunityStatItem(
                                    value = "${community.memberCount}",
                                    label = "Members"
                                )
                                CommunityStatItem(
                                    value = "${viewModel.posts.size}",
                                    label = "Posts"
                                )
                                CommunityStatItem(
                                    value = "${(community.popularity * 100).toInt()}%",
                                    label = "Popularity"
                                )
                            }
                        }
                    }
                    
                    // Posts section
                    item {
                        Text(
                            text = "Recent Posts",
                            style = MaterialTheme.typography.titleMedium
                        )
                    }
                    
                    if (viewModel.posts.isEmpty()) {
                        item {
                            Card(modifier = Modifier.fillMaxWidth()) {
                                Box(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .padding(32.dp),
                                    contentAlignment = Alignment.Center
                                ) {
                                    Column(horizontalAlignment = Alignment.CenterHorizontally) {
                                        Icon(
                                            Icons.Default.Article,
                                            contentDescription = null,
                                            modifier = Modifier.size(48.dp),
                                            tint = MaterialTheme.colorScheme.onSurfaceVariant
                                        )
                                        Spacer(modifier = Modifier.height(8.dp))
                                        Text(
                                            text = "No posts yet",
                                            style = MaterialTheme.typography.bodyMedium,
                                            color = MaterialTheme.colorScheme.onSurfaceVariant
                                        )
                                    }
                                }
                            }
                        }
                    } else {
                        items(viewModel.posts) { post ->
                            CommunityPostCard(
                                post = post,
                                onClick = { onPostClick(post) }
                            )
                        }
                    }
                }
            }
        }
    }
    
    // Join dialog
    if (showJoinDialog) {
        AlertDialog(
            onDismissRequest = { showJoinDialog = false },
            title = { Text("Join Community") },
            text = { Text("Do you want to join this community?") },
            confirmButton = {
                Button(onClick = {
                    viewModel.joinCommunity(communityId, "player")
                    showJoinDialog = false
                }) {
                    Text("Join")
                }
            },
            dismissButton = {
                TextButton(onClick = { showJoinDialog = false }) {
                    Text("Cancel")
                }
            }
        )
    }
}

@Composable
fun CommunityHeader(
    community: CommunityDto,
    isJoined: Boolean,
    onJoinClick: () -> Unit,
    onLeaveClick: () -> Unit
) {
    Card(modifier = Modifier.fillMaxWidth()) {
        Column(
            modifier = Modifier.padding(16.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            // Community icon
            Surface(
                modifier = Modifier.size(80.dp),
                shape = MaterialTheme.shapes.large,
                color = MaterialTheme.colorScheme.primaryContainer
            ) {
                Box(contentAlignment = Alignment.Center) {
                    Icon(
                        Icons.Default.Group,
                        contentDescription = null,
                        modifier = Modifier.size(48.dp),
                        tint = MaterialTheme.colorScheme.onPrimaryContainer
                    )
                }
            }
            
            Spacer(modifier = Modifier.height(16.dp))
            
            Text(
                text = community.name,
                style = MaterialTheme.typography.headlineSmall
            )
            
            Text(
                text = "@${community.handle}",
                style = MaterialTheme.typography.bodyMedium,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
            
            community.topic?.let { topic ->
                Spacer(modifier = Modifier.height(8.dp))
                AssistChip(
                    onClick = {},
                    label = { Text(topic) },
                    leadingIcon = {
                        Icon(
                            Icons.Default.Tag,
                            contentDescription = null,
                            modifier = Modifier.size(18.dp)
                        )
                    }
                )
            }
            
            Spacer(modifier = Modifier.height(16.dp))
            
            if (isJoined) {
                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    OutlinedButton(onClick = onLeaveClick) {
                        Icon(Icons.Default.Logout, contentDescription = null, modifier = Modifier.size(18.dp))
                        Spacer(modifier = Modifier.width(4.dp))
                        Text("Leave")
                    }
                    FilledTonalButton(onClick = { /* Create post */ }) {
                        Icon(Icons.Default.Edit, contentDescription = null, modifier = Modifier.size(18.dp))
                        Spacer(modifier = Modifier.width(4.dp))
                        Text("Post")
                    }
                }
            } else {
                Button(onClick = onJoinClick) {
                    Icon(Icons.Default.PersonAdd, contentDescription = null, modifier = Modifier.size(18.dp))
                    Spacer(modifier = Modifier.width(4.dp))
                    Text("Join Community")
                }
            }
        }
    }
}

@Composable
fun CommunityStatItem(
    value: String,
    label: String
) {
    Column(horizontalAlignment = Alignment.CenterHorizontally) {
        Text(
            text = value,
            style = MaterialTheme.typography.headlineSmall,
            color = MaterialTheme.colorScheme.primary
        )
        Text(
            text = label,
            style = MaterialTheme.typography.bodySmall,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
    }
}

@Composable
fun CommunityPostCard(
    post: PostDto,
    onClick: () -> Unit
) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        onClick = onClick
    ) {
        Column(
            modifier = Modifier.padding(16.dp)
        ) {
            Row(
                verticalAlignment = Alignment.CenterVertically
            ) {
                Surface(
                    modifier = Modifier.size(40.dp),
                    shape = MaterialTheme.shapes.medium,
                    color = MaterialTheme.colorScheme.secondaryContainer
                ) {
                    Box(contentAlignment = Alignment.Center) {
                        Icon(
                            Icons.Default.Person,
                            contentDescription = null,
                            tint = MaterialTheme.colorScheme.onSecondaryContainer
                        )
                    }
                }
                Spacer(modifier = Modifier.width(12.dp))
                Column {
                    Text(
                        text = post.authorName,
                        style = MaterialTheme.typography.titleSmall
                    )
                    Text(
                        text = "@${post.authorHandle}",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
            }
            
            Spacer(modifier = Modifier.height(12.dp))
            
            Text(
                text = post.content,
                style = MaterialTheme.typography.bodyMedium
            )
            
            Spacer(modifier = Modifier.height(12.dp))
            
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.spacedBy(16.dp)
            ) {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Icon(
                        Icons.Default.Favorite,
                        contentDescription = "Likes",
                        modifier = Modifier.size(18.dp),
                        tint = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                    Spacer(modifier = Modifier.width(4.dp))
                    Text(
                        text = "${post.likeCount}",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Icon(
                        Icons.Default.ChatBubbleOutline,
                        contentDescription = "Comments",
                        modifier = Modifier.size(18.dp),
                        tint = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                    Spacer(modifier = Modifier.width(4.dp))
                    Text(
                        text = "${post.commentCount}",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Icon(
                        Icons.Default.Visibility,
                        contentDescription = "Views",
                        modifier = Modifier.size(18.dp),
                        tint = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                    Spacer(modifier = Modifier.width(4.dp))
                    Text(
                        text = "${post.viewCount}",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
            }
        }
    }
}
