package com.syntheticsocialworld.app.ui.screens

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
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
class SearchViewModel @Inject constructor(
    private val api: SyntheticSocialWorldApi
) : ViewModel() {
    
    var searchQuery by mutableStateOf("")
        private set
    var searchResults by mutableStateOf<SearchResults?>(null)
        private set
    var isSearching by mutableStateOf(false)
        private set
    var error by mutableStateOf<String?>(null)
        private set
    var selectedFilter by mutableStateOf(SearchFilter.ALL)
    
    fun updateQuery(query: String) {
        searchQuery = query
        if (query.length >= 2) {
            performSearch()
        } else {
            searchResults = null
        }
    }
    
    fun setFilter(filter: SearchFilter) {
        selectedFilter = filter
        if (searchQuery.length >= 2) {
            performSearch()
        }
    }
    
    private fun performSearch() {
        viewModelScope.launch {
            isSearching = true
            error = null
            try {
                searchResults = api.search(
                    query = searchQuery,
                    filter = selectedFilter.apiFilter
                )
            } catch (e: Exception) {
                error = e.message ?: "Search failed"
            } finally {
                isSearching = false
            }
        }
    }
    
    fun followNpc(npcId: String, currentUserId: String) {
        viewModelScope.launch {
            try {
                api.follow(FollowRequest(currentUserId, npcId))
                // Refresh search results
                if (searchQuery.length >= 2) {
                    performSearch()
                }
            } catch (e: Exception) {
                error = e.message
            }
        }
    }
}

enum class SearchFilter(val label: String, val apiFilter: String?) {
    ALL("All", null),
    NPCs("People", "npcs"),
    POSTS("Posts", "posts"),
    COMMUNITIES("Communities", "communities"),
    EVENTS("Events", "events")
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SearchScreen(
    viewModel: SearchViewModel = hiltViewModel(),
    onNpcClick: (NPCSearchResult) -> Unit = {},
    onPostClick: (PostSearchResult) -> Unit = {},
    onCommunityClick: (CommunitySearchResult) -> Unit = {}
) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp)
    ) {
        // Search bar
        OutlinedTextField(
            value = viewModel.searchQuery,
            onValueChange = { viewModel.updateQuery(it) },
            modifier = Modifier.fillMaxWidth(),
            placeholder = { Text("Search NPCs, posts, communities...") },
            leadingIcon = {
                Icon(Icons.Default.Search, contentDescription = "Search")
            },
            trailingIcon = {
                if (viewModel.searchQuery.isNotEmpty()) {
                    IconButton(onClick = { viewModel.updateQuery("") }) {
                        Icon(Icons.Default.Clear, contentDescription = "Clear")
                    }
                }
            },
            singleLine = true
        )
        
        Spacer(modifier = Modifier.height(12.dp))
        
        // Filter chips
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            SearchFilter.entries.forEach { filter ->
                FilterChip(
                    selected = viewModel.selectedFilter == filter,
                    onClick = { viewModel.setFilter(filter) },
                    label = { Text(filter.label) }
                )
            }
        }
        
        Spacer(modifier = Modifier.height(16.dp))
        
        // Error
        viewModel.error?.let { err ->
            Card(
                modifier = Modifier.fillMaxWidth(),
                colors = CardDefaults.cardColors(
                    containerColor = MaterialTheme.colorScheme.errorContainer
                )
            ) {
                Text(
                    text = err,
                    modifier = Modifier.padding(16.dp),
                    color = MaterialTheme.colorScheme.onErrorContainer
                )
            }
            Spacer(modifier = Modifier.height(8.dp))
        }
        
        // Loading
        if (viewModel.isSearching) {
            Box(
                modifier = Modifier.fillMaxWidth(),
                contentAlignment = Alignment.Center
            ) {
                CircularProgressIndicator()
            }
        }
        
        // Results
        val results = viewModel.searchResults
        if (results != null && !viewModel.isSearching) {
            LazyColumn(
                verticalArrangement = Arrangement.spacedBy(12.dp)
            ) {
                // NPCs
                if (viewModel.selectedFilter == SearchFilter.ALL || viewModel.selectedFilter == SearchFilter.NPCs) {
                    if (results.npcs.isNotEmpty()) {
                        item {
                            Text(
                                text = "People",
                                style = MaterialTheme.typography.titleMedium
                            )
                        }
                        items(results.npcs) { npc ->
                            SearchNPCCard(
                                npc = npc,
                                onClick = { onNpcClick(npc) },
                                onFollow = { viewModel.followNpc(npc.id, "player") }
                            )
                        }
                    }
                }
                
                // Posts
                if (viewModel.selectedFilter == SearchFilter.ALL || viewModel.selectedFilter == SearchFilter.POSTS) {
                    if (results.posts.isNotEmpty()) {
                        item {
                            Text(
                                text = "Posts",
                                style = MaterialTheme.typography.titleMedium
                            )
                        }
                        items(results.posts) { post ->
                            SearchPostCard(
                                post = post,
                                onClick = { onPostClick(post) }
                            )
                        }
                    }
                }
                
                // Communities
                if (viewModel.selectedFilter == SearchFilter.ALL || viewModel.selectedFilter == SearchFilter.COMMUNITIES) {
                    if (results.communities.isNotEmpty()) {
                        item {
                            Text(
                                text = "Communities",
                                style = MaterialTheme.typography.titleMedium
                            )
                        }
                        items(results.communities) { community ->
                            SearchCommunityCard(
                                community = community,
                                onClick = { onCommunityClick(community) }
                            )
                        }
                    }
                }
                
                // Empty state
                if (results.npcs.isEmpty() && results.posts.isEmpty() && results.communities.isEmpty()) {
                    item {
                        Box(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(32.dp),
                            contentAlignment = Alignment.Center
                        ) {
                            Column(horizontalAlignment = Alignment.CenterHorizontally) {
                                Icon(
                                    Icons.Default.SearchOff,
                                    contentDescription = null,
                                    modifier = Modifier.size(64.dp),
                                    tint = MaterialTheme.colorScheme.onSurfaceVariant
                                )
                                Spacer(modifier = Modifier.height(16.dp))
                                Text(
                                    text = "No results found",
                                    style = MaterialTheme.typography.titleMedium,
                                    color = MaterialTheme.colorScheme.onSurfaceVariant
                                )
                                Text(
                                    text = "Try a different search term",
                                    style = MaterialTheme.typography.bodyMedium,
                                    color = MaterialTheme.colorScheme.onSurfaceVariant
                                )
                            }
                        }
                    }
                }
            }
        } else if (viewModel.searchQuery.isEmpty()) {
            // Initial state
            Box(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(32.dp),
                contentAlignment = Alignment.Center
            ) {
                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                    Icon(
                        Icons.Default.Search,
                        contentDescription = null,
                        modifier = Modifier.size(64.dp),
                        tint = MaterialTheme.colorScheme.primary
                    )
                    Spacer(modifier = Modifier.height(16.dp))
                    Text(
                        text = "Search the World",
                        style = MaterialTheme.typography.titleMedium
                    )
                    Text(
                        text = "Find NPCs, posts, and communities",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
            }
        }
    }
}

@Composable
fun SearchNPCCard(
    npc: NPCSearchResult,
    onClick: () -> Unit,
    onFollow: () -> Unit
) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        onClick = onClick
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(12.dp),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {
            Row(
                modifier = Modifier.weight(1f),
                verticalAlignment = Alignment.CenterVertically
            ) {
                Icon(
                    Icons.Default.Person,
                    contentDescription = null,
                    modifier = Modifier.size(48.dp),
                    tint = MaterialTheme.colorScheme.primary
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
                    // Note: NPCSearchResult doesn't have mood field
                }
            }
            OutlinedButton(onClick = onFollow) {
                Icon(Icons.Default.PersonAdd, contentDescription = null, modifier = Modifier.size(18.dp))
                Spacer(modifier = Modifier.width(4.dp))
                Text("Follow")
            }
        }
    }
}

@Composable
fun SearchPostCard(
    post: PostSearchResult,
    onClick: () -> Unit
) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        onClick = onClick
    ) {
        Column(
            modifier = Modifier.padding(12.dp)
        ) {
            Row(
                verticalAlignment = Alignment.CenterVertically
            ) {
                Icon(
                    Icons.Default.Article,
                    contentDescription = null,
                    modifier = Modifier.size(32.dp),
                    tint = MaterialTheme.colorScheme.secondary
                )
                Spacer(modifier = Modifier.width(8.dp))
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
            Spacer(modifier = Modifier.height(8.dp))
            Text(
                text = post.content,
                style = MaterialTheme.typography.bodyMedium,
                maxLines = 3
            )
            Spacer(modifier = Modifier.height(8.dp))
            Row(
                horizontalArrangement = Arrangement.spacedBy(16.dp)
            ) {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Icon(Icons.Default.Favorite, contentDescription = null, modifier = Modifier.size(16.dp))
                    Spacer(modifier = Modifier.width(4.dp))
                    Text("${post.likeCount}", style = MaterialTheme.typography.bodySmall)
                }
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Icon(Icons.Default.ChatBubbleOutline, contentDescription = null, modifier = Modifier.size(16.dp))
                    Spacer(modifier = Modifier.width(4.dp))
                    Text("${post.commentCount}", style = MaterialTheme.typography.bodySmall)
                }
            }
        }
    }
}

@Composable
fun SearchCommunityCard(
    community: CommunitySearchResult,
    onClick: () -> Unit
) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        onClick = onClick
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(12.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Icon(
                Icons.Default.Group,
                contentDescription = null,
                modifier = Modifier.size(48.dp),
                tint = MaterialTheme.colorScheme.tertiary
            )
            Spacer(modifier = Modifier.width(12.dp))
            Column(modifier = Modifier.weight(1f)) {
                Text(
                    text = community.name,
                    style = MaterialTheme.typography.titleMedium
                )
                community.topic?.let { topic ->
                    Text(
                        text = topic,
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.primary
                    )
                }
                community.description?.let { desc ->
                    Text(
                        text = desc,
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                        maxLines = 2
                    )
                }
                Text(
                    text = "${community.memberCount} members",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
            Icon(
                Icons.Default.ChevronRight,
                contentDescription = "View",
                tint = MaterialTheme.colorScheme.onSurfaceVariant
            )
        }
    }
}
