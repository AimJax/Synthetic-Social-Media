package com.syntheticsocialworld.app.ui.screens

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.lazy.rememberLazyListState
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.automirrored.filled.Send
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
class ChatViewModel @Inject constructor(
    private val api: SyntheticSocialWorldApi
) : ViewModel() {
    
    var threads by mutableStateOf<List<ChatThreadDto>>(emptyList())
        private set
    var selectedThread by mutableStateOf<ChatThreadDto?>(null)
        private set
    var messages by mutableStateOf<List<MessageDto>>(emptyList())
        private set
    var isLoading by mutableStateOf(false)
        private set
    var error by mutableStateOf<String?>(null)
        private set
    var currentUserId by mutableStateOf("player")
        private set
    
    init {
        loadThreads()
    }
    
    fun loadThreads() {
        viewModelScope.launch {
            isLoading = true
            error = null
            try {
                // Load all NPCs as potential chat threads
                val npcs = api.getNPCs(limit = 50)
                threads = npcs
                    .filter { !it.isPlayer }
                    .map { npc ->
                        ChatThreadDto(
                            id = "thread_${npc.id}",
                            participantId = npc.id,
                            participantName = npc.displayName,
                            participantHandle = npc.handle,
                            lastMessage = null,
                            lastMessageAt = null,
                            unreadCount = 0
                        )
                    }
            } catch (e: Exception) {
                error = e.message ?: "Failed to load conversations"
            } finally {
                isLoading = false
            }
        }
    }
    
    fun selectThread(thread: ChatThreadDto) {
        selectedThread = thread
        loadMessages(thread.participantId)
    }
    
    fun closeThread() {
        selectedThread = null
        messages = emptyList()
    }
    
    fun loadMessages(participantId: String) {
        viewModelScope.launch {
            isLoading = true
            try {
                messages = api.getConversation(currentUserId, participantId, limit = 50)
            } catch (e: Exception) {
                error = e.message
            } finally {
                isLoading = false
            }
        }
    }
    
    fun sendMessage(content: String) {
        val thread = selectedThread ?: return
        
        viewModelScope.launch {
            try {
                val request = SendMessageRequest(
                    senderId = currentUserId,
                    recipientId = thread.participantId,
                    content = content
                )
                val newMessage = api.sendMessage(request)
                messages = messages + newMessage
            } catch (e: Exception) {
                error = e.message
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ChatScreen(
    viewModel: ChatViewModel = hiltViewModel()
) {
    if (viewModel.selectedThread != null) {
        ConversationScreen(
            thread = viewModel.selectedThread!!,
            messages = viewModel.messages,
            isLoading = viewModel.isLoading,
            onBack = { viewModel.closeThread() },
            onSendMessage = { viewModel.sendMessage(it) }
        )
    } else {
        ThreadListScreen(
            threads = viewModel.threads,
            isLoading = viewModel.isLoading,
            error = viewModel.error,
            onThreadClick = { viewModel.selectThread(it) },
            onRefresh = { viewModel.loadThreads() }
        )
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ThreadListScreen(
    threads: List<ChatThreadDto>,
    isLoading: Boolean,
    error: String?,
    onThreadClick: (ChatThreadDto) -> Unit,
    onRefresh: () -> Unit
) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp)
    ) {
        Text(
            text = "Messages",
            style = MaterialTheme.typography.headlineMedium
        )
        Text(
            text = "Start conversations with NPCs",
            style = MaterialTheme.typography.bodyMedium,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
        
        Spacer(modifier = Modifier.height(16.dp))
        
        error?.let { err ->
            Card(
                modifier = Modifier.fillMaxWidth(),
                colors = CardDefaults.cardColors(
                    containerColor = MaterialTheme.colorScheme.errorContainer
                )
            ) {
                Row(
                    modifier = Modifier.padding(16.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Icon(
                        Icons.Default.Error,
                        contentDescription = null,
                        tint = MaterialTheme.colorScheme.onErrorContainer
                    )
                    Spacer(modifier = Modifier.width(8.dp))
                    Text(
                        text = err,
                        color = MaterialTheme.colorScheme.onErrorContainer,
                        modifier = Modifier.weight(1f)
                    )
                    IconButton(onClick = onRefresh) {
                        Icon(Icons.Default.Refresh, contentDescription = "Retry")
                    }
                }
            }
            Spacer(modifier = Modifier.height(8.dp))
        }
        
        if (isLoading && threads.isEmpty()) {
            Box(
                modifier = Modifier.fillMaxSize(),
                contentAlignment = Alignment.Center
            ) {
                CircularProgressIndicator()
            }
        } else if (threads.isEmpty()) {
            Box(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(32.dp),
                contentAlignment = Alignment.Center
            ) {
                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                    Icon(
                        Icons.Default.ChatBubbleOutline,
                        contentDescription = null,
                        modifier = Modifier.size(64.dp),
                        tint = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                    Spacer(modifier = Modifier.height(16.dp))
                    Text(
                        text = "No conversations yet",
                        style = MaterialTheme.typography.titleMedium
                    )
                    Text(
                        text = "Start chatting with NPCs!",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
            }
        } else {
            LazyColumn(
                verticalArrangement = Arrangement.spacedBy(8.dp)
            ) {
                items(threads) { thread ->
                    ThreadCard(
                        thread = thread,
                        onClick = { onThreadClick(thread) }
                    )
                }
            }
        }
    }
}

@Composable
fun ThreadCard(
    thread: ChatThreadDto,
    onClick: () -> Unit
) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        onClick = onClick
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(16.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            // Avatar
            Surface(
                modifier = Modifier.size(48.dp),
                shape = MaterialTheme.shapes.medium,
                color = MaterialTheme.colorScheme.primaryContainer
            ) {
                Box(contentAlignment = Alignment.Center) {
                    Icon(
                        Icons.Default.Person,
                        contentDescription = null,
                        modifier = Modifier.size(28.dp),
                        tint = MaterialTheme.colorScheme.onPrimaryContainer
                    )
                }
            }
            
            Spacer(modifier = Modifier.width(12.dp))
            
            Column(modifier = Modifier.weight(1f)) {
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween
                ) {
                    Text(
                        text = thread.participantName,
                        style = MaterialTheme.typography.titleMedium
                    )
                    thread.lastMessageAt?.let { time ->
                        Text(
                            text = formatRelativeTime(time),
                            style = MaterialTheme.typography.bodySmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                }
                Text(
                    text = "@${thread.participantHandle}",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
                thread.lastMessage?.let { msg ->
                    Text(
                        text = msg,
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                        maxLines = 1
                    )
                }
            }
            
            if (thread.unreadCount > 0) {
                Spacer(modifier = Modifier.width(8.dp))
                Badge {
                    Text("${thread.unreadCount}")
                }
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ConversationScreen(
    thread: ChatThreadDto,
    messages: List<MessageDto>,
    isLoading: Boolean,
    onBack: () -> Unit,
    onSendMessage: (String) -> Unit
) {
    var messageText by remember { mutableStateOf("") }
    val listState = rememberLazyListState()
    
    // Scroll to bottom when new messages arrive
    LaunchedEffect(messages.size) {
        if (messages.isNotEmpty()) {
            listState.animateScrollToItem(messages.size - 1)
        }
    }
    
    Column(
        modifier = Modifier.fillMaxSize()
    ) {
        // Header
        TopAppBar(
            title = {
                Column {
                    Text(
                        text = thread.participantName,
                        style = MaterialTheme.typography.titleMedium
                    )
                    Text(
                        text = "@${thread.participantHandle}",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
            },
            navigationIcon = {
                IconButton(onClick = onBack) {
                    Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Back")
                }
            }
        )
        
        // Messages
        if (isLoading && messages.isEmpty()) {
            Box(
                modifier = Modifier
                    .weight(1f)
                    .fillMaxWidth(),
                contentAlignment = Alignment.Center
            ) {
                CircularProgressIndicator()
            }
        } else if (messages.isEmpty()) {
            Box(
                modifier = Modifier
                    .weight(1f)
                    .fillMaxWidth()
                    .padding(32.dp),
                contentAlignment = Alignment.Center
            ) {
                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                    Icon(
                        Icons.Default.Forum,
                        contentDescription = null,
                        modifier = Modifier.size(64.dp),
                        tint = MaterialTheme.colorScheme.primary
                    )
                    Spacer(modifier = Modifier.height(16.dp))
                    Text(
                        text = "Start the conversation!",
                        style = MaterialTheme.typography.titleMedium
                    )
                    Text(
                        text = "Send a message to ${thread.participantName}",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
            }
        } else {
            LazyColumn(
                modifier = Modifier
                    .weight(1f)
                    .fillMaxWidth()
                    .padding(horizontal = 16.dp),
                state = listState,
                verticalArrangement = Arrangement.spacedBy(8.dp)
            ) {
                items(messages) { message ->
                    MessageBubble(
                        message = message,
                        isFromCurrentUser = message.senderId == "player"
                    )
                }
            }
        }
        
        // Input
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
                    maxLines = 4
                )
                Spacer(modifier = Modifier.width(8.dp))
                FilledIconButton(
                    onClick = {
                        if (messageText.isNotBlank()) {
                            onSendMessage(messageText)
                            messageText = ""
                        }
                    },
                    enabled = messageText.isNotBlank()
                ) {
                    Icon(Icons.AutoMirrored.Filled.Send, contentDescription = "Send")
                }
            }
        }
    }
}

@Composable
fun MessageBubble(
    message: MessageDto,
    isFromCurrentUser: Boolean
) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        horizontalArrangement = if (isFromCurrentUser) Arrangement.End else Arrangement.Start
    ) {
        Card(
            modifier = Modifier.widthIn(max = 280.dp),
            colors = CardDefaults.cardColors(
                containerColor = if (isFromCurrentUser)
                    MaterialTheme.colorScheme.primaryContainer
                else
                    MaterialTheme.colorScheme.surfaceVariant
            )
        ) {
            Column(
                modifier = Modifier.padding(12.dp)
            ) {
                if (!isFromCurrentUser) {
                    Text(
                        text = message.sender?.displayName ?: "Unknown",
                        style = MaterialTheme.typography.labelSmall,
                        color = MaterialTheme.colorScheme.primary
                    )
                    Spacer(modifier = Modifier.height(4.dp))
                }
                Text(
                    text = message.content,
                    style = MaterialTheme.typography.bodyMedium
                )
                Spacer(modifier = Modifier.height(4.dp))
                Text(
                    text = formatRelativeTime(message.createdAt),
                    style = MaterialTheme.typography.labelSmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
        }
    }
}

private fun formatRelativeTime(timestamp: String): String {
    // Simple relative time formatting
    // In production, use a proper date library
    return try {
        val time = timestamp.substringAfter("T").substringBefore("+")
        val parts = time.split(":")
        if (parts.size >= 2) {
            "${parts[0]}:${parts[1]}"
        } else {
            timestamp.substring(0, minOf(16, timestamp.length))
        }
    } catch (e: Exception) {
        timestamp.substring(0, minOf(16, timestamp.length))
    }
}
