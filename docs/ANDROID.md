# Android Client Architecture

## Synthetic Social World - Native Android Application

---

## Technology Stack

| Component | Technology | Rationale |
|-----------|------------|-----------|
| Language | Kotlin | Modern, concise, null-safe |
| UI | Jetpack Compose | Declarative, reactive, Google standard |
| Architecture | MVVM + Clean Architecture | Separation of concerns |
| DI | Hilt | Official Android DI |
| Networking | Retrofit + OkHttp | REST API |
| WebSocket | OkHttp WebSocket | Real-time events |
| State | StateFlow / Flow | Reactive streams |
| Navigation | Navigation Compose | Single-activity architecture |
| Image Loading | Coil | Kotlin-first |
| Pagination | Paging 3 | Efficient list loading |
| Local Cache | Room (future) | Offline support |
| Async | Kotlin Coroutines | Structured concurrency |

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                         UI LAYER                                 │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐             │
│  │   Screens   │  │  Components │  │   Themes    │             │
│  │  (Compose)  │  │  (Compose)  │  │   (Material)│             │
│  └─────────────┘  └─────────────┘  └─────────────┘             │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    VIEWMODEL LAYER                               │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐             │
│  │   Feed     │  │  Profile    │  │  Messages   │             │
│  │  ViewModel │  │  ViewModel  │  │  ViewModel  │             │
│  └─────────────┘  └─────────────┘  └─────────────┘             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐             │
│  │ Notification│  │  Search    │  │   Events    │             │
│  │  ViewModel  │  │  ViewModel  │  │  ViewModel  │             │
│  └─────────────┘  └─────────────┘  └─────────────┘             │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    DOMAIN LAYER                                  │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐             │
│  │   Models   │  │  Use Cases  │  │ Repositories│             │
│  │            │  │             │  │ (Interface) │             │
│  └─────────────┘  └─────────────┘  └─────────────┘             │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    DATA LAYER                                   │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐             │
│  │    API      │  │  WebSocket  │  │    Local    │             │
│  │  (Retrofit) │  │  (OkHttp)   │  │   (Room)   │             │
│  └─────────────┘  └─────────────┘  └─────────────┘             │
└─────────────────────────────────────────────────────────────────┘
```

---

## Package Structure

```
com.syntheticsocialworld.app
├── di/                          # Hilt modules
│   ├── AppModule.kt
│   ├── NetworkModule.kt
│   └── RepositoryModule.kt
│
├── data/                        # Data layer
│   ├── api/
│   │   ├── ApiClient.kt
│   │   ├── ApiService.kt
│   │   ├── endpoints/
│   │   │   ├── AuthApi.kt
│   │   │   ├── FeedApi.kt
│   │   │   ├── PostsApi.kt
│   │   │   ├── UsersApi.kt
│   │   │   ├── MessagesApi.kt
│   │   │   ├── CommunitiesApi.kt
│   │   │   ├── EventsApi.kt
│   │   │   └── NotificationsApi.kt
│   │   └── dto/                 # Data Transfer Objects
│   │       ├── AuthDto.kt
│   │       ├── FeedDto.kt
│   │       ├── PostDto.kt
│   │       └── ...
│   │
│   ├── websocket/
│   │   ├── WebSocketClient.kt
│   │   └── EventHandler.kt
│   │
│   └── repository/
│       ├── FeedRepositoryImpl.kt
│       ├── UserRepositoryImpl.kt
│       ├── MessageRepositoryImpl.kt
│       └── ...
│
├── domain/                      # Domain layer
│   ├── model/                   # Domain models
│   │   ├── User.kt
│   │   ├── Post.kt
│   │   ├── Comment.kt
│   │   ├── Message.kt
│   │   ├── Community.kt
│   │   ├── Event.kt
│   │   ├── Notification.kt
│   │   └── Relationship.kt
│   │
│   ├── repository/              # Repository interfaces
│   │   ├── FeedRepository.kt
│   │   ├── UserRepository.kt
│   │   ├── MessageRepository.kt
│   │   └── ...
│   │
│   └── usecase/                 # Use cases
│       ├── FeedUseCases.kt
│       ├── PostUseCases.kt
│       ├── UserUseCases.kt
│       └── ...
│
├── ui/                          # UI layer
│   ├── theme/
│   │   ├── Theme.kt
│   │   ├── Color.kt
│   │   ├── Type.kt
│   │   └── Shape.kt
│   │
│   ├── components/               # Reusable components
│   │   ├── PostCard.kt
│   │   ├── CommentItem.kt
│   │   ├── UserAvatar.kt
│   │   ├── EngagementBar.kt
│   │   ├── CommunityChip.kt
│   │   └── ...
│   │
│   ├── screens/                  # Screen composables
│   │   ├── MainScreen.kt
│   │   ├── Feed/
│   │   │   ├── FeedScreen.kt
│   │   │   └── FeedViewModel.kt
│   │   ├── Profile/
│   │   │   ├── ProfileScreen.kt
│   │   │   ├── ProfileViewModel.kt
│   │   │   ├── OwnProfileScreen.kt
│   │   │   └── OtherProfileScreen.kt
│   │   ├── Post/
│   │   │   ├── PostDetailScreen.kt
│   │   │   ├── PostDetailViewModel.kt
│   │   │   └── CreatePostScreen.kt
│   │   ├── Messages/
│   │   │   ├── ConversationsScreen.kt
│   │   │   ├── ChatScreen.kt
│   │   │   └── MessagesViewModel.kt
│   │   ├── Communities/
│   │   │   ├── CommunitiesScreen.kt
│   │   │   ├── CommunityDetailScreen.kt
│   │   │   └── CommunitiesViewModel.kt
│   │   ├── Events/
│   │   │   ├── EventsScreen.kt
│   │   │   ├── EventDetailScreen.kt
│   │   │   └── EventsViewModel.kt
│   │   ├── Notifications/
│   │   │   ├── NotificationsScreen.kt
│   │   │   └── NotificationsViewModel.kt
│   │   ├── Search/
│   │   │   ├── SearchScreen.kt
│   │   │   └── SearchViewModel.kt
│   │   └── Auth/
│   │       ├── LoginScreen.kt
│   │       └── AuthViewModel.kt
│   │
│   └── navigation/
│       ├── NavGraph.kt
│       ├── Screen.kt
│       └── NavArgs.kt
│
├── util/                        # Utilities
│   ├── Constants.kt
│   ├── Extensions.kt
│   ├── DateTimeUtils.kt
│   └── Result.kt
│
└── SyntheticSocialWorldApp.kt   # Application class
```

---

## Screen Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                      MAIN SCREEN                                  │
│  ┌───────────┬───────────┬───────────┬───────────┬───────────┐  │
│  │   Feed    │   Search  │   (+)     │    Msgs   │    Notif  │  │
│  └───────────┴───────────┴───────────┴───────────┴───────────┘  │
│                                                                  │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │                     CONTENT AREA                             ││
│  │                                                              ││
│  │  [Screen content based on selected tab]                     ││
│  │                                                              ││
│  │                                                              ││
│  │                                                              ││
│  │                                                              ││
│  └─────────────────────────────────────────────────────────────┘│
│                                                                  │
│  ┌─────────────┐                                                │
│  │ Profile     │ ← Swipe up or tap                              │
│  │  Avatar    │                                                │
│  └─────────────┘                                                │
└─────────────────────────────────────────────────────────────────┘
```

### Navigation Routes

```
feed                    - Home feed
search                  - Search screen
create-post             - Create post (modal)
messages                - Conversations list
messages/{userId}       - Chat with user
profile                 - Own profile
profile/{userId}        - Other user's profile
communities             - Communities list
communities/{id}        - Community detail
events                  - Events list
events/{id}             - Event detail
notifications           - Notifications list
settings                - App settings
```

---

## State Management

### ViewModel Pattern
```kotlin
@HiltViewModel
class FeedViewModel @Inject constructor(
    private val feedRepository: FeedRepository,
    private val authRepository: AuthRepository
) : ViewModel() {
    
    private val _uiState = MutableStateFlow(FeedUiState())
    val uiState: StateFlow<FeedUiState> = _uiState.asStateFlow()
    
    private val _events = MutableSharedFlow<FeedEvent>()
    val events: SharedFlow<FeedEvent> = _events.asSharedFlow()
    
    init {
        loadFeed()
    }
    
    fun loadFeed() {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true) }
            
            feedRepository.getFeed(cursor, limit)
                .onSuccess { feed ->
                    _uiState.update { 
                        it.copy(
                            isLoading = false,
                            posts = it.posts + feed.items,
                            nextCursor = feed.nextCursor,
                            hasMore = feed.hasMore
                        )
                    }
                }
                .onFailure { error ->
                    _events.emit(FeedEvent.ShowError(error.message))
                }
        }
    }
}
```

### UI State
```kotlin
data class FeedUiState(
    val isLoading: Boolean = false,
    val posts: List<Post> = emptyList(),
    val nextCursor: String? = null,
    val hasMore: Boolean = true,
    val isRefreshing: Boolean = false,
    val error: String? = null
)

sealed class FeedEvent {
    data class ShowError(val message: String) : FeedEvent()
    data class NavigateToPost(val postId: String) : FeedEvent()
    data class NavigateToProfile(val userId: String) : FeedEvent()
}
```

---

## Network Layer

### Retrofit Setup
```kotlin
@Module
@InstallIn(SingletonComponent::class)
object NetworkModule {
    
    @Provides
    @Singleton
    fun provideOkHttpClient(
        authInterceptor: AuthInterceptor
    ): OkHttpClient {
        return OkHttpClient.Builder()
            .addInterceptor(authInterceptor)
            .addInterceptor(HttpLoggingInterceptor().apply {
                level = HttpLoggingInterceptor.Level.BODY
            })
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .writeTimeout(30, TimeUnit.SECONDS)
            .build()
    }
    
    @Provides
    @Singleton
    fun provideRetrofit(okHttpClient: OkHttpClient): Retrofit {
        return Retrofit.Builder()
            .baseUrl("http://10.0.2.2:5000/api/") // localhost for emulator
            .client(okHttpClient)
            .addConverterFactory(Json.asConverterFactory())
            .build()
    }
}
```

### API Service
```kotlin
interface ApiService {
    
    // Auth
    @POST("auth/login")
    suspend fun login(@Body request: LoginRequest): AuthResponse
    
    // Feed
    @GET("feed")
    suspend fun getFeed(
        @Query("cursor") cursor: String? = null,
        @Query("limit") limit: Int = 20
    ): FeedResponse
    
    // Posts
    @POST("posts")
    suspend fun createPost(@Body request: CreatePostRequest): PostResponse
    
    @POST("posts/{id}/like")
    suspend fun likePost(@Path("id") postId: String): EngagementResponse
    
    @POST("posts/{id}/dislike")
    suspend fun dislikePost(@Path("id") postId: String): EngagementResponse
    
    // Users
    @GET("users/{id}")
    suspend fun getUser(@Path("id") userId: String): UserResponse
    
    @POST("users/{id}/follow")
    suspend fun followUser(@Path("id") userId: String): FollowResponse
    
    // Messages
    @GET("messages/{userId}")
    suspend fun getMessages(
        @Path("userId") userId: String,
        @Query("cursor") cursor: String? = null
    ): MessagesResponse
    
    @POST("messages/{userId}")
    suspend fun sendMessage(
        @Path("userId") userId: String,
        @Body request: SendMessageRequest
    ): MessageResponse
    
    // ... other endpoints
}
```

---

## WebSocket Integration

### WebSocket Client
```kotlin
class WebSocketClient(
    private val okHttpClient: OkHttpClient,
    private val authRepository: AuthRepository
) {
    private var webSocket: WebSocket? = null
    private val _events = MutableSharedFlow<ServerEvent>()
    val events: SharedFlow<ServerEvent> = _events.asSharedFlow()
    
    fun connect() {
        val request = Request.Builder()
            .url("ws://10.0.2.2:5000/ws")
            .build()
        
        webSocket = okHttpClient.newWebSocket(request, object : WebSocketListener() {
            override fun onOpen(webSocket: WebSocket, response: Response) {
                // Authenticate
                webSocket.send(Json.encodeToString(
                    AuthMessage(authRepository.getToken())
                ))
            }
            
            override fun onMessage(webSocket: WebSocket, text: String) {
                val event = parseServerEvent(text)
                viewModelScope.launch { _events.emit(event) }
            }
            
            override fun onFailure(webSocket: WebSocket, t: Throwable, response: Response?) {
                // Reconnect with exponential backoff
                reconnect()
            }
        })
    }
    
    fun send(message: ClientMessage) {
        webSocket?.send(Json.encodeToString(message))
    }
    
    private fun reconnect() {
        // Exponential backoff: 1s, 2s, 4s, 8s, max 30s
    }
}
```

### Event Handling
```kotlin
@Composable
fun FeedScreen(viewModel: FeedViewModel = viewModel()) {
    val uiState by viewModel.uiState.collectAsState()
    
    LaunchedEffect(Unit) {
        viewModel.webSocketEvents.collect { event ->
            when (event) {
                is ServerEvent.FeedUpdate -> {
                    viewModel.addPost(event.post)
                }
                is ServerEvent.PostEngagementChanged -> {
                    viewModel.updateEngagement(event.postId, event.engagement)
                }
                is ServerEvent.NotificationCreated -> {
                    viewModel.showNotification(event.notification)
                }
                // Handle other events
            }
        }
    }
}
```

---

## Performance Optimizations

### Lazy Lists
```kotlin
@Composable
fun FeedScreen(
    posts: List<Post>,
    onLoadMore: () -> Unit,
    onPostClick: (String) -> Unit
) {
    LazyColumn(
        state = rememberLazyListState().also { state ->
            // Load more when near end
            LaunchedEffect(state) {
                snapshotFlow { state.layoutInfo.visibleItemsInfo.lastOrNull()?.index }
                    .filter { it >= posts.size - 5 }
                    .collect { onLoadMore() }
            }
        }
    ) {
        items(
            items = posts,
            key = { it.id }
        ) { post ->
            PostCard(
                post = post,
                onClick = { onPostClick(post.id) }
            )
        }
        
        // Loading indicator
        item {
            if (isLoading) {
                Box(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(16.dp),
                    contentAlignment = Alignment.Center
                ) {
                    CircularProgressIndicator()
                }
            }
        }
    }
}
```

### Image Loading
```kotlin
@Composable
fun UserAvatar(
    avatarUrl: String?,
    modifier: Modifier = Modifier
) {
    AsyncImage(
        model = ImageRequest.Builder(LocalContext.current)
            .data(avatarUrl ?: DEFAULT_AVATAR)
            .crossfade(true)
            .build(),
        contentDescription = "User avatar",
        modifier = modifier.clip(CircleShape),
        contentScale = ContentScale.Crop
    )
}
```

### Incremental Updates
```kotlin
// Don't reload entire feed, update incrementally
fun addPost(post: Post) {
    _uiState.update { state ->
        state.copy(posts = listOf(post) + state.posts)
    }
}

fun updateEngagement(postId: String, engagement: Engagement) {
    _uiState.update { state ->
        state.copy(
            posts = state.posts.map { post ->
                if (post.id == postId) post.copy(
                    likeCount = engagement.likeCount,
                    dislikeCount = engagement.dislikeCount,
                    commentCount = engagement.commentCount
                ) else post
            }
        )
    }
}
```

---

## Bottom Navigation

```kotlin
@Composable
fun MainScreen() {
    val navController = rememberNavController()
    
    Scaffold(
        bottomBar = {
            NavigationBar {
                val navBackStackEntry by navController.currentBackStackEntryAsState()
                val currentRoute = navBackStackEntry?.destination?.route
                
                NavigationBarItem(
                    icon = { Icon(Icons.Default.Home, "Feed") },
                    label = { Text("Feed") },
                    selected = currentRoute == "feed",
                    onClick = { navController.navigate("feed") }
                )
                
                NavigationBarItem(
                    icon = { Icon(Icons.Default.Search, "Search") },
                    label = { Text("Search") },
                    selected = currentRoute == "search",
                    onClick = { navController.navigate("search") }
                )
                
                NavigationBarItem(
                    icon = { Icon(Icons.Default.Add, "Post") },
                    label = { Text("Post") },
                    selected = false,
                    onClick = { navController.navigate("create-post") }
                )
                
                NavigationBarItem(
                    icon = { Icon(Icons.Default.Email, "Messages") },
                    label = { Text("Messages") },
                    selected = currentRoute?.startsWith("messages") == true,
                    onClick = { navController.navigate("messages") }
                )
                
                NavigationBarItem(
                    icon = { Icon(Icons.Default.Notifications, "Notifications") },
                    label = { Text("Notifications") },
                    selected = currentRoute == "notifications",
                    onClick = { navController.navigate("notifications") }
                )
            }
        }
    ) { paddingValues ->
        NavHost(
            navController = navController,
            startDestination = "feed",
            modifier = Modifier.padding(paddingValues)
        ) {
            // Navigation graph
        }
    }
}
```

---

## Error Handling

```kotlin
sealed class Resource<out T> {
    data class Success<T>(val data: T) : Resource<T>()
    data class Error(val message: String, val cause: Throwable? = null) : Resource<Nothing>()
    data object Loading : Resource<Nothing>()
}

@Composable
fun <T> ResourceView(
    resource: Resource<T>,
    onLoading: @Composable () -> Unit = { CircularProgressIndicator() },
    onError: @Composable (String) -> Unit = { Text("Error: $it") },
    onSuccess: @Composable (T) -> Unit
) {
    when (resource) {
        is Resource.Loading -> onLoading()
        is Resource.Error -> onError(resource.message)
        is Resource.Success -> onSuccess(resource.data)
    }
}
```

---

## Testing Strategy

### Unit Tests
- ViewModels
- Use Cases
- Repository logic
- Mappers

### Integration Tests
- API client tests
- WebSocket client tests
- Repository + API integration

### UI Tests
- Screen composables (manual)
- Navigation flows
- User interactions

---

## Related Documents

- [ARCHITECTURE.md](./ARCHITECTURE.md) - System overview
- [API.md](./API.md) - REST and WebSocket API
- [TESTING.md](./TESTING.md) - Testing strategy
