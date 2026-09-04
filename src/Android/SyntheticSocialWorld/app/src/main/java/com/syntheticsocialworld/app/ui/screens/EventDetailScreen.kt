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

data class EventDto(
    val id: String,
    val title: String,
    val description: String?,
    val organizerId: String,
    val organizerName: String,
    val communityId: String?,
    val communityName: String?,
    val startTime: String,
    val endTime: String?,
    val location: String?,
    val maxAttendees: Int?,
    val attendeeCount: Int,
    val isPublic: Boolean,
    val createdAt: String
)

@HiltViewModel
class EventDetailViewModel @Inject constructor(
    private val api: SyntheticSocialWorldApi
) : ViewModel() {
    
    var event by mutableStateOf<EventDto?>(null)
        private set
    var isLoading by mutableStateOf(false)
        private set
    var error by mutableStateOf<String?>(null)
        private set
    var isAttending by mutableStateOf(false)
        private set
    
    fun loadEvent(eventId: String) {
        viewModelScope.launch {
            isLoading = true
            error = null
            try {
                // In a real app, we'd have a dedicated GET /api/events/{id} endpoint
                // For now, we create a mock event for demo purposes
                event = EventDto(
                    id = eventId,
                    title = "Community Meetup",
                    description = "Join us for an exciting community gathering! Meet new friends and share your experiences.",
                    organizerId = "org1",
                    organizerName = "Event Organizer",
                    communityId = null,
                    communityName = null,
                    startTime = "2025-01-15T18:00:00Z",
                    endTime = "2025-01-15T21:00:00Z",
                    location = "Community Center",
                    maxAttendees = 50,
                    attendeeCount = 23,
                    isPublic = true,
                    createdAt = "2025-01-01T10:00:00Z"
                )
            } catch (e: Exception) {
                error = e.message ?: "Failed to load event"
            } finally {
                isLoading = false
            }
        }
    }
    
    fun attendEvent(eventId: String, userId: String) {
        viewModelScope.launch {
            try {
                // Would call attend endpoint
                isAttending = true
                event = event?.copy(attendeeCount = event!!.attendeeCount + 1)
            } catch (e: Exception) {
                error = e.message
            }
        }
    }
    
    fun cancelAttendance(eventId: String, userId: String) {
        viewModelScope.launch {
            try {
                // Would call cancel endpoint
                isAttending = false
                event = event?.copy(attendeeCount = maxOf(0, event!!.attendeeCount - 1))
            } catch (e: Exception) {
                error = e.message
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun EventDetailScreen(
    eventId: String,
    viewModel: EventDetailViewModel = hiltViewModel(),
    onBack: () -> Unit = {}
) {
    var showAttendDialog by remember { mutableStateOf(false) }
    
    LaunchedEffect(eventId) {
        viewModel.loadEvent(eventId)
    }
    
    Column(
        modifier = Modifier.fillMaxSize()
    ) {
        // Header
        TopAppBar(
            title = { Text("Event Details") },
            navigationIcon = {
                IconButton(onClick = onBack) {
                    Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Back")
                }
            },
            actions = {
                IconButton(onClick = { /* Share */ }) {
                    Icon(Icons.Default.Share, contentDescription = "Share")
                }
                IconButton(onClick = { /* More options */ }) {
                    Icon(Icons.Default.MoreVert, contentDescription = "More")
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
                        text = viewModel.error ?: "Error loading event",
                        color = MaterialTheme.colorScheme.error
                    )
                    Spacer(modifier = Modifier.height(16.dp))
                    Button(onClick = { viewModel.loadEvent(eventId) }) {
                        Text("Retry")
                    }
                }
            }
        } else {
            viewModel.event?.let { event ->
                LazyColumn(
                    modifier = Modifier.fillMaxSize(),
                    contentPadding = PaddingValues(16.dp),
                    verticalArrangement = Arrangement.spacedBy(16.dp)
                ) {
                    // Event header card
                    item {
                        EventHeaderCard(
                            event = event,
                            isAttending = viewModel.isAttending,
                            onAttendClick = { showAttendDialog = true },
                            onCancelClick = { viewModel.cancelAttendance(eventId, "player") }
                        )
                    }
                    
                    // Date and time
                    item {
                        Card(modifier = Modifier.fillMaxWidth()) {
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(16.dp),
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Icon(
                                    Icons.Default.CalendarMonth,
                                    contentDescription = null,
                                    modifier = Modifier.size(40.dp),
                                    tint = MaterialTheme.colorScheme.primary
                                )
                                Spacer(modifier = Modifier.width(16.dp))
                                Column {
                                    Text(
                                        text = "Date & Time",
                                        style = MaterialTheme.typography.titleMedium
                                    )
                                    Spacer(modifier = Modifier.height(4.dp))
                                    Text(
                                        text = formatEventDateTime(event.startTime),
                                        style = MaterialTheme.typography.bodyMedium
                                    )
                                    event.endTime?.let { endTime ->
                                        Text(
                                            text = "to ${formatEventDateTime(endTime)}",
                                            style = MaterialTheme.typography.bodySmall,
                                            color = MaterialTheme.colorScheme.onSurfaceVariant
                                        )
                                    }
                                }
                            }
                        }
                    }
                    
                    // Location
                    event.location?.let { location ->
                        item {
                            Card(modifier = Modifier.fillMaxWidth()) {
                                Row(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .padding(16.dp),
                                    verticalAlignment = Alignment.CenterVertically
                                ) {
                                    Icon(
                                        Icons.Default.LocationOn,
                                        contentDescription = null,
                                        modifier = Modifier.size(40.dp),
                                        tint = MaterialTheme.colorScheme.primary
                                    )
                                    Spacer(modifier = Modifier.width(16.dp))
                                    Column {
                                        Text(
                                            text = "Location",
                                            style = MaterialTheme.typography.titleMedium
                                        )
                                        Spacer(modifier = Modifier.height(4.dp))
                                        Text(
                                            text = location,
                                            style = MaterialTheme.typography.bodyMedium
                                        )
                                    }
                                }
                            }
                        }
                    }
                    
                    // Description
                    event.description?.let { desc ->
                        item {
                            Card(modifier = Modifier.fillMaxWidth()) {
                                Column(
                                    modifier = Modifier.padding(16.dp)
                                ) {
                                    Text(
                                        text = "About This Event",
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
                    
                    // Organizer
                    item {
                        Card(modifier = Modifier.fillMaxWidth()) {
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(16.dp),
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Surface(
                                    modifier = Modifier.size(48.dp),
                                    shape = MaterialTheme.shapes.medium,
                                    color = MaterialTheme.colorScheme.primaryContainer
                                ) {
                                    Box(contentAlignment = Alignment.Center) {
                                        Icon(
                                            Icons.Default.Person,
                                            contentDescription = null,
                                            tint = MaterialTheme.colorScheme.onPrimaryContainer
                                        )
                                    }
                                }
                                Spacer(modifier = Modifier.width(16.dp))
                                Column {
                                    Text(
                                        text = "Organized by",
                                        style = MaterialTheme.typography.labelMedium,
                                        color = MaterialTheme.colorScheme.onSurfaceVariant
                                    )
                                    Text(
                                        text = event.organizerName,
                                        style = MaterialTheme.typography.titleMedium
                                    )
                                }
                            }
                        }
                    }
                    
                    // Attendees
                    item {
                        Card(modifier = Modifier.fillMaxWidth()) {
                            Column(
                                modifier = Modifier.padding(16.dp)
                            ) {
                                Row(
                                    modifier = Modifier.fillMaxWidth(),
                                    horizontalArrangement = Arrangement.SpaceBetween,
                                    verticalAlignment = Alignment.CenterVertically
                                ) {
                                    Text(
                                        text = "Attendees",
                                        style = MaterialTheme.typography.titleMedium
                                    )
                                    Text(
                                        text = "${event.attendeeCount}${event.maxAttendees?.let { " / $it" } ?: ""}",
                                        style = MaterialTheme.typography.bodyMedium,
                                        color = MaterialTheme.colorScheme.primary
                                    )
                                }
                                
                                Spacer(modifier = Modifier.height(12.dp))
                                
                                // Progress bar
                                event.maxAttendees?.let { max ->
                                    LinearProgressIndicator(
                                        progress = { (event.attendeeCount.toFloat() / max).coerceIn(0f, 1f) },
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .height(8.dp),
                                    )
                                    Spacer(modifier = Modifier.height(8.dp))
                                }
                                
                                // Attendee avatars placeholder
                                Row(
                                    horizontalArrangement = Arrangement.spacedBy((-8).dp)
                                ) {
                                    repeat(minOf(5, event.attendeeCount)) { index ->
                                        Surface(
                                            modifier = Modifier.size(32.dp),
                                            shape = MaterialTheme.shapes.small,
                                            color = MaterialTheme.colorScheme.secondaryContainer
                                        ) {
                                            Box(contentAlignment = Alignment.Center) {
                                                Icon(
                                                    Icons.Default.Person,
                                                    contentDescription = null,
                                                    modifier = Modifier.size(20.dp),
                                                    tint = MaterialTheme.colorScheme.onSecondaryContainer
                                                )
                                            }
                                        }
                                    }
                                    if (event.attendeeCount > 5) {
                                        Surface(
                                            modifier = Modifier.size(32.dp),
                                            shape = MaterialTheme.shapes.small,
                                            color = MaterialTheme.colorScheme.surfaceVariant
                                        ) {
                                            Box(contentAlignment = Alignment.Center) {
                                                Text(
                                                    text = "+${event.attendeeCount - 5}",
                                                    style = MaterialTheme.typography.labelSmall
                                                )
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    
    // Attend dialog
    if (showAttendDialog) {
        AlertDialog(
            onDismissRequest = { showAttendDialog = false },
            icon = { Icon(Icons.Default.EventAvailable, contentDescription = null) },
            title = { Text("Attend Event") },
            text = { Text("Would you like to attend this event? You'll be added to the attendee list.") },
            confirmButton = {
                Button(onClick = {
                    viewModel.attendEvent(eventId, "player")
                    showAttendDialog = false
                }) {
                    Text("Yes, I'll attend!")
                }
            },
            dismissButton = {
                TextButton(onClick = { showAttendDialog = false }) {
                    Text("Cancel")
                }
            }
        )
    }
}

@Composable
fun EventHeaderCard(
    event: EventDto,
    isAttending: Boolean,
    onAttendClick: () -> Unit,
    onCancelClick: () -> Unit
) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        colors = CardDefaults.cardColors(
            containerColor = MaterialTheme.colorScheme.primaryContainer
        )
    ) {
        Column(
            modifier = Modifier.padding(16.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            // Event icon
            Surface(
                modifier = Modifier.size(72.dp),
                shape = MaterialTheme.shapes.large,
                color = MaterialTheme.colorScheme.primary
            ) {
                Box(contentAlignment = Alignment.Center) {
                    Icon(
                        Icons.Default.Event,
                        contentDescription = null,
                        modifier = Modifier.size(40.dp),
                        tint = MaterialTheme.colorScheme.onPrimary
                    )
                }
            }
            
            Spacer(modifier = Modifier.height(16.dp))
            
            Text(
                text = event.title,
                style = MaterialTheme.typography.headlineSmall,
                color = MaterialTheme.colorScheme.onPrimaryContainer
            )
            
            Spacer(modifier = Modifier.height(8.dp))
            
            // Status badge
            if (isAttending) {
                AssistChip(
                    onClick = {},
                    label = { Text("You're attending") },
                    leadingIcon = {
                        Icon(
                            Icons.Default.Check,
                            contentDescription = null,
                            modifier = Modifier.size(18.dp)
                        )
                    },
                    colors = AssistChipDefaults.assistChipColors(
                        containerColor = MaterialTheme.colorScheme.tertiaryContainer
                    )
                )
            }
            
            Spacer(modifier = Modifier.height(16.dp))
            
            if (isAttending) {
                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    OutlinedButton(
                        onClick = onCancelClick,
                        colors = ButtonDefaults.outlinedButtonColors(
                            contentColor = MaterialTheme.colorScheme.error
                        )
                    ) {
                        Icon(Icons.Default.Cancel, contentDescription = null, modifier = Modifier.size(18.dp))
                        Spacer(modifier = Modifier.width(4.dp))
                        Text("Cancel")
                    }
                    FilledTonalButton(onClick = { /* Add to calendar */ }) {
                        Icon(Icons.Default.CalendarMonth, contentDescription = null, modifier = Modifier.size(18.dp))
                        Spacer(modifier = Modifier.width(4.dp))
                        Text("Add to Calendar")
                    }
                }
            } else {
                Button(onClick = onAttendClick) {
                    Icon(Icons.Default.EventAvailable, contentDescription = null, modifier = Modifier.size(18.dp))
                    Spacer(modifier = Modifier.width(4.dp))
                    Text("Attend Event")
                }
            }
        }
    }
}

private fun formatEventDateTime(isoString: String): String {
    // Simple formatting - in production use proper date library
    return try {
        val parts = isoString.replace("Z", "").split("T")
        if (parts.size == 2) {
            val dateParts = parts[0].split("-")
            val timeParts = parts[1].split(":")
            if (dateParts.size == 3 && timeParts.size >= 2) {
                val months = listOf("Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec")
                val month = months.getOrElse(dateParts[1].toInt() - 1) { "???" }
                "$month ${dateParts[2]}, ${dateParts[0]} at ${timeParts[0]}:${timeParts[1]}"
            } else isoString
        } else isoString
    } catch (e: Exception) {
        isoString
    }
}
