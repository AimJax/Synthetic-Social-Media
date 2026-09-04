# Initialize Android Project
# Usage: .\init-android.ps1

$ErrorActionPreference = "Stop"
$ProjectRoot = "D:\SyntheticSocialWorld"
$AndroidPath = "$ProjectRoot\src\Android\SyntheticSocialWorld"

Write-Host "=== Initializing Android Project ===" -ForegroundColor Cyan

# Check for Java
$java = Get-Command java -ErrorAction SilentlyContinue
if (-not $java) {
    Write-Host "Java not found. Please install JDK 17 or later." -ForegroundColor Red
    exit 1
}

# Check for Android SDK
$androidSdk = $env:ANDROID_HOME
if (-not $androidSdk) {
    $androidSdk = $env:ANDROID_SDK_ROOT
}
if (-not $androidSdk) {
    Write-Host "Android SDK not found. Set ANDROID_HOME environment variable." -ForegroundColor Red
    exit 1
}
Write-Host "Android SDK: $androidSdk" -ForegroundColor Gray

# Create project directory
if (-not (Test-Path $AndroidPath)) {
    New-Item -ItemType Directory -Force -Path $AndroidPath | Out-Null
}

Set-Location $AndroidPath

# Create settings.gradle
Write-Host "Creating settings.gradle..." -ForegroundColor Yellow
@"
pluginManagement {
    repositories {
        google()
        mavenCentral()
        gradlePluginPortal()
    }
}
dependencyResolutionManagement {
    repositoriesMode.set(RepositoriesMode.FAIL_ON_PROJECT_REPOS)
    repositories {
        google()
        mavenCentral()
    }
}

rootProject.name = "SyntheticSocialWorld"
include(":app")
"@ | Out-File -FilePath "$AndroidPath\settings.gradle.kts" -Encoding UTF8

# Create root build.gradle.kts
Write-Host "Creating root build.gradle.kts..." -ForegroundColor Yellow
@"
plugins {
    id("com.android.application") version "8.2.0" apply false
    id("org.jetbrains.kotlin.android") version "1.9.20" apply false
    id("com.google.dagger.hilt.android") version "2.48" apply false
}
"@ | Out-File -FilePath "$AndroidPath\build.gradle.kts" -Encoding UTF8

# Create gradle.properties
Write-Host "Creating gradle.properties..." -ForegroundColor Yellow
@"
android.useAndroidX=true
android.enableJetifier=true
kotlin.code.style=official
org.gradle.jvmargs=-Xmx2048m -Dfile.encoding=UTF-8
org.gradle.parallel=true
org.gradle.caching=true
"@ | Out-File -FilePath "$AndroidPath\gradle.properties" -Encoding UTF8

# Create gradle-wrapper.properties
Write-Host "Creating gradle-wrapper.properties..." -ForegroundColor Yellow
$wrapperPath = "$AndroidPath\gradle\wrapper"
New-Item -ItemType Directory -Force -Path $wrapperPath | Out-Null
@"
distributionBase=GRADLE_USER_HOME
distributionPath=wrapper/dists
distributionUrl=https\://services.gradle.org/distributions/gradle-8.2-bin.zip
networkTimeout=10000
zipStoreBase=GRADLE_USER_HOME
zipStorePath=wrapper/dists
"@ | Out-File -FilePath "$wrapperPath\gradle-wrapper.properties" -Encoding UTF8

# Create app directory structure
Write-Host "Creating app directory structure..." -ForegroundColor Yellow
$appPath = "$AndroidPath\app"
New-Item -ItemType Directory -Force -Path "$appPath\src\main\java\com\syntheticsocialworld\app" | Out-Null
New-Item -ItemType Directory -Force -Path "$appPath\src\main\res\values" | Out-Null
New-Item -ItemType Directory -Force -Path "$appPath\src\main\res\drawable" | Out-Null

# Create app build.gradle.kts
Write-Host "Creating app/build.gradle.kts..." -ForegroundColor Yellow
@"
plugins {
    id("com.android.application")
    id("org.jetbrains.kotlin.android")
    id("com.google.dagger.hilt.android")
    kotlin("kapt")
}

android {
    namespace = "com.syntheticsocialworld.app"
    compileSdk = 34

    defaultConfig {
        applicationId = "com.syntheticsocialworld.app"
        minSdk = 26
        targetSdk = 34
        versionCode = 1
        versionName = "0.1.0"

        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
        vectorDrawables {
            useSupportLibrary = true
        }
    }

    buildTypes {
        release {
            isMinifyEnabled = false
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
        }
    }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }
    kotlinOptions {
        jvmTarget = "17"
    }
    buildFeatures {
        compose = true
    }
    composeOptions {
        kotlinCompilerExtensionVersion = "1.5.5"
    }
    packaging {
        resources {
            excludes += "/META-INF/{AL2.0,LGPL2.1}"
        }
    }
}

dependencies {
    // Core Android
    implementation("androidx.core:core-ktx:1.12.0")
    implementation("androidx.lifecycle:lifecycle-runtime-ktx:2.6.2")
    implementation("androidx.activity:activity-compose:1.8.1")

    // Compose
    implementation(platform("androidx.compose:compose-bom:2023.10.01"))
    implementation("androidx.compose.ui:ui")
    implementation("androidx.compose.ui:ui-graphics")
    implementation("androidx.compose.ui:ui-tooling-preview")
    implementation("androidx.compose.material3:material3")
    implementation("androidx.compose.material:material-icons-extended")

    // Navigation
    implementation("androidx.navigation:navigation-compose:2.7.5")

    // ViewModel
    implementation("androidx.lifecycle:lifecycle-viewmodel-compose:2.6.2")
    implementation("androidx.lifecycle:lifecycle-runtime-compose:2.6.2")

    // Hilt
    implementation("com.google.dagger:hilt-android:2.48")
    kapt("com.google.dagger:hilt-android-compiler:2.48")
    implementation("androidx.hilt:hilt-navigation-compose:1.1.0")

    // Networking
    implementation("com.squareup.retrofit2:retrofit:2.9.0")
    implementation("com.squareup.retrofit2:converter-gson:2.9.0")
    implementation("com.squareup.okhttp3:okhttp:4.12.0")
    implementation("com.squareup.okhttp3:logging-interceptor:4.12.0")

    // Image loading
    implementation("io.coil-kt:coil-compose:2.5.0")

    // Coroutines
    implementation("org.jetbrains.kotlinx:kotlinx-coroutines-android:1.7.3")

    // Testing
    testImplementation("junit:junit:4.13.2")
    androidTestImplementation("androidx.test.ext:junit:1.1.5")
    androidTestImplementation("androidx.test.espresso:espresso-core:3.5.1")
    androidTestImplementation(platform("androidx.compose:compose-bom:2023.10.01"))
    androidTestImplementation("androidx.compose.ui:ui-test-junit4")
    debugImplementation("androidx.compose.ui:ui-tooling")
    debugImplementation("androidx.compose.ui:ui-test-manifest")
}

kapt {
    correctErrorTypes = true
}
"@ | Out-File -FilePath "$appPath\build.gradle.kts" -Encoding UTF8

# Create AndroidManifest.xml
Write-Host "Creating AndroidManifest.xml..." -ForegroundColor Yellow
@"
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android"
    xmlns:tools="http://schemas.android.com/tools">

    <uses-permission android:name="android.permission.INTERNET" />
    <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />

    <application
        android:name=".SyntheticSocialWorldApp"
        android:allowBackup="true"
        android:icon="@mipmap/ic_launcher"
        android:label="@string/app_name"
        android:roundIcon="@mipmap/ic_launcher_round"
        android:supportsRtl="true"
        android:theme="@style/Theme.SyntheticSocialWorld"
        android:usesCleartextTraffic="true"
        tools:targetApi="31">
        <activity
            android:name=".MainActivity"
            android:exported="true"
            android:theme="@style/Theme.SyntheticSocialWorld">
            <intent-filter>
                <action android:name="android.intent.action.MAIN" />
                <category android:name="android.intent.category.LAUNCHER" />
            </intent-filter>
        </activity>
    </application>

</manifest>
"@ | Out-File -FilePath "$appPath\src\main\AndroidManifest.xml" -Encoding UTF8

# Create strings.xml
Write-Host "Creating strings.xml..." -ForegroundColor Yellow
@"
<resources>
    <string name="app_name">Synthetic Social World</string>
</resources>
"@ | Out-File -FilePath "$appPath\src\main\res\values\strings.xml" -Encoding UTF8

# Create themes.xml
Write-Host "Creating themes.xml..." -ForegroundColor Yellow
@"
<?xml version="1.0" encoding="utf-8"?>
<resources>
    <style name="Theme.SyntheticSocialWorld" parent="android:Theme.Material.Light.NoActionBar">
        <item name="android:statusBarColor">@android:color/black</item>
    </style>
</resources>
"@ | Out-File -FilePath "$appPath\src\main\res\values\themes.xml" -Encoding UTF8

# Create colors.xml
Write-Host "Creating colors.xml..." -ForegroundColor Yellow
@"
<?xml version="1.0" encoding="utf-8"?>
<resources>
    <color name="purple_200">#FFBB86FC</color>
    <color name="purple_500">#FF6200EE</color>
    <color name="purple_700">#FF3700B3</color>
    <color name="teal_200">#FF03DAC5</color>
    <color name="teal_700">#FF018786</color>
    <color name="black">#FF000000</color>
    <color name="white">#FFFFFFFF</color>
</resources>
"@ | Out-File -FilePath "$appPath\src\main\res\values\colors.xml" -Encoding UTF8

# Create Application class
Write-Host "Creating Application class..." -ForegroundColor Yellow
$appPackagePath = "$appPath\src\main\java\com\syntheticsocialworld\app"
@"
package com.syntheticsocialworld.app

import android.app.Application
import dagger.hilt.android.HiltAndroidApp

@HiltAndroidApp
class SyntheticSocialWorldApp : Application()
"@ | Out-File -FilePath "$appPackagePath\SyntheticSocialWorldApp.kt" -Encoding UTF8

# Create MainActivity
Write-Host "Creating MainActivity..." -ForegroundColor Yellow
@"
package com.syntheticsocialworld.app

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.ui.Modifier
import com.syntheticsocialworld.app.ui.theme.SyntheticSocialWorldTheme
import com.syntheticsocialworld.app.ui.screens.MainScreen
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
                    MainScreen()
                }
            }
        }
    }
}
"@ | Out-File -FilePath "$appPackagePath\MainActivity.kt" -Encoding UTF8

# Create Theme
Write-Host "Creating Theme..." -ForegroundColor Yellow
$themePath = "$appPackagePath\ui\theme"
New-Item -ItemType Directory -Force -Path "$themePath" | Out-Null

@"
package com.syntheticsocialworld.app.ui.theme

import androidx.compose.ui.graphics.Color

val Purple80 = Color(0xFFD0BCFF)
val PurpleGrey80 = Color(0xFFCCC2DC)
val Pink80 = Color(0xFFEFB8C8)

val Purple40 = Color(0xFF6650a4)
val PurpleGrey40 = Color(0xFF625b71)
val Pink40 = Color(0xFF7D5260)
"@ | Out-File -FilePath "$themePath\Color.kt" -Encoding UTF8

@"
package com.syntheticsocialworld.app.ui.theme

import android.app.Activity
import android.os.Build
import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.dynamicDarkColorScheme
import androidx.compose.material3.dynamicLightColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.SideEffect
import androidx.compose.ui.graphics.toArgb
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalView
import androidx.core.view.WindowCompat

private val DarkColorScheme = darkColorScheme(
    primary = Purple80,
    secondary = PurpleGrey80,
    tertiary = Pink80
)

private val LightColorScheme = lightColorScheme(
    primary = Purple40,
    secondary = PurpleGrey40,
    tertiary = Pink40
)

@Composable
fun SyntheticSocialWorldTheme(
    darkTheme: Boolean = isSystemInDarkTheme(),
    dynamicColor: Boolean = true,
    content: @Composable () -> Unit
) {
    val colorScheme = when {
        dynamicColor && Build.VERSION.SDK_INT >= Build.VERSION_CODES.S -> {
            val context = LocalContext.current
            if (darkTheme) dynamicDarkColorScheme(context) else dynamicLightColorScheme(context)
        }
        darkTheme -> DarkColorScheme
        else -> LightColorScheme
    }
    val view = LocalView.current
    if (!view.isInEditMode) {
        SideEffect {
            val window = (view.context as Activity).window
            window.statusBarColor = colorScheme.primary.toArgb()
            WindowCompat.getInsetsController(window, view).isAppearanceLightStatusBars = darkTheme
        }
    }

    MaterialTheme(
        colorScheme = colorScheme,
        typography = Typography,
        content = content
    )
}
"@ | Out-File -FilePath "$themePath\Theme.kt" -Encoding UTF8

@"
package com.syntheticsocialworld.app.ui.theme

import androidx.compose.material3.Typography
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.sp

val Typography = Typography(
    bodyLarge = TextStyle(
        fontFamily = FontFamily.Default,
        fontWeight = FontWeight.Normal,
        fontSize = 16.sp,
        lineHeight = 24.sp,
        letterSpacing = 0.5.sp
    ),
    titleLarge = TextStyle(
        fontFamily = FontFamily.Default,
        fontWeight = FontWeight.Normal,
        fontSize = 22.sp,
        lineHeight = 28.sp,
        letterSpacing = 0.sp
    ),
    labelSmall = TextStyle(
        fontFamily = FontFamily.Default,
        fontWeight = FontWeight.Medium,
        fontSize = 11.sp,
        lineHeight = 16.sp,
        letterSpacing = 0.5.sp
    )
)
"@ | Out-File -FilePath "$themePath\Type.kt" -Encoding UTF8

# Create MainScreen
Write-Host "Creating MainScreen..." -ForegroundColor Yellow
$screensPath = "$appPackagePath\ui\screens"
New-Item -ItemType Directory -Force -Path "$screensPath" | Out-Null

@"
package com.syntheticsocialworld.app.ui.screens

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MainScreen() {
    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(\"Synthetic Social World\") },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.primaryContainer,
                    titleContentColor = MaterialTheme.colorScheme.onPrimaryContainer
                )
            )
        }
    ) { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .padding(16.dp)
        ) {
            Text(
                text = \"Welcome to Synthetic Social World\",
                style = MaterialTheme.typography.headlineMedium
            )
            Spacer(modifier = Modifier.height(8.dp))
            Text(
                text = \"A persistent AI social network simulation\",
                style = MaterialTheme.typography.bodyLarge,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
            Spacer(modifier = Modifier.height(24.dp))
            
            // Placeholder for feed
            Card(
                modifier = Modifier.fillMaxWidth()
            ) {
                Text(
                    text = \"Feed coming soon...\",
                    modifier = Modifier.padding(16.dp),
                    style = MaterialTheme.typography.bodyMedium
                )
            }
        }
    }
}
"@ | Out-File -FilePath "$screensPath\MainScreen.kt" -Encoding UTF8

Write-Host "`nAndroid project created successfully!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Download gradle wrapper: cd $AndroidPath; gradle wrapper" -ForegroundColor Gray
Write-Host "2. Build APK: .\scripts\build-android.ps1" -ForegroundColor Gray
Write-Host "3. Deploy: .\scripts\deploy-android.ps1" -ForegroundColor Gray
