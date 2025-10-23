# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 6 (6000.2.9f1) visual novel game built with:
- Universal Render Pipeline (URP) for 2D graphics
- Pixel Crushers Dialogue System for narrative content
- FMOD for audio management
- Unity Addressables for asset loading
- Unity Input System

The game features a dialogue-driven narrative with character status tracking (Heart/心情, Money/金錢, Energy/活力) and visual novel elements including backgrounds, character art, and comic panels.

## Build and Development Commands

### Opening the Project
- Open the project in Unity 6000.2.9f1 or later
- The main development scene is `Assets/Scenes/MainStoryScene.unity`
- Title screen is in `Assets/Scenes/TitleScene.unity`
- Test scene is `Assets/Scenes/Playground.unity`

### Building the Project
Unity builds are performed through the Unity Editor:
1. File > Build Settings
2. Select target platform
3. Click "Build" or "Build And Run"

Note: This project uses `.slnx` solution files (moments.slnx, moments-game.slnx) for Visual Studio integration.

### Running Tests
- Unity Test Framework is included via `com.unity.test-framework@1.6.0`
- Open Window > General > Test Runner in Unity Editor to run tests
- No custom test files exist in the project yet (only third-party plugin tests)

## Architecture

### Core Systems

**GameManager (Singleton)**
- Located at `Assets/Scripts/GameManager.cs`
- Persists across scenes with DontDestroyOnLoad
- Manages character status values (Heart, Money, Energy) with range 0-100
- Provides event system for status changes: `OnStatusHeartChanged`, `OnStatusMoneyChanged`, `OnStatusEnergyChanged`
- Handles scene transitions and game state

**DialogueEventManager (ScriptableObject)**
- Located at `Assets/Scripts/DialogueEventManager.cs`
- Handles Dialogue System events via `OnConversationLine` and `OnConversationResponseMenu`
- Manages dynamic background and comic image loading using Unity Addressables
- Loads assets via Addressables.LoadAssetAsync<Sprite>() based on dialogue entry custom fields
- Custom fields used: "Background Image", "Comic Image"

**StatusToast System**
- Located at `Assets/Scripts/StatusToast.cs`
- Subscribes to GameManager status change events
- Displays animated notifications when character stats change
- Shows status icon, name, and direction indicator (increasing/decreasing)
- Auto-hides after configurable duration (default 2 seconds)

### UI Components

**UIStatus**
- `Assets/Scripts/UIStatus.cs` - Displays status meters with color-coded icons
- Supports three status types with themed colors: Heart (#FFAC9C), Money (#93D9BF), Energy (#FFE77A)
- Dynamically adjusts meter height based on 0-1 normalized values

**UIArrow**
- `Assets/Scripts/UIArrow.cs` - Custom procedural arrow graphic using Unity UI Graphic API
- Draws arrow with configurable line width, length, wing angle, and rounded caps
- `SetDirection(bool up)` adjusts position and scale: up arrows at Y=-12 (flipped), down arrows at Y=12

**ComicImageFader**
- `Assets/Scripts/ComicImageFader.cs` - Automatic fade transitions for comic panel sprites
- Monitors SpriteRenderer.sprite changes and triggers appropriate transitions
- Fade in: null → sprite, Fade out: sprite → null, Cross-fade: sprite → different sprite
- Default fade duration: 0.5 seconds (configurable)

**UIRadialGradient**
- `Assets/Scripts/UIRadialGradient.cs` - Custom UI shader effect component

**AutoScaleSprites**
- `Assets/Scripts/AutoScaleSize.cs` - Auto-scales background sprites to fit camera orthographic view
- Waits for sprite assignment via coroutine, then calculates scale to cover screen
- Uses Camera.main orthographic size and aspect ratio for calculations
- Maintains aspect ratio using minimum scale factor

### Audio System (FMOD)

**Custom Sequencer Commands for Dialogue System + FMOD Integration**
- `SequencerCommandFMODPlay.cs` - Plays FMOD events from dialogue sequences
  - Usage in dialogue: `FMODPlay(eventName)` where eventName is relative to "event:/" path
- `SequencerCommandFMODWait.cs` (Third Party Support) - FMOD + Dialogue System integration

FMOD Studio integration is configured at `Assets/Plugins/FMOD/`
- FMOD banks are loaded at runtime
- Events are referenced using event paths like "event:/EventName"

### Asset Management

**Addressables System**
- Used for loading sprites dynamically (backgrounds, comic images)
- Asset references are stored as string paths in Dialogue System custom fields
- Load pattern: `Addressables.LoadAssetAsync<Sprite>(path).Completed += handler`

**BackgroundDatabase (ScriptableObject)**
- `Assets/Scripts/BackgroundDatabase.cs` - Manages background sprite lookups by name
- Provides `GetBackgroundSprite(string spriteName)` and `HasSprite(string spriteName)`
- Stores List<BackgroundSprite> with spriteName/sprite pairs
- `BackgroundSprite.cs` defines the serializable data structure

### Dialogue System Integration

The project uses Pixel Crushers Dialogue System extensively:
- Main dialogue manager variant prefab: `Assets/Prefabs/Dialogue Manager Variant.prefab`
- Custom sequencer commands extend dialogue functionality (all in `PixelCrushers.DialogueSystem.SequencerCommands` namespace)

**Custom Sequencer Commands:**
- `SequencerCommandSetBackgroundColor.cs` - Changes background sprite color overlay
  - Usage: `SetBackgroundColor(#RRGGBB)` or `SetBackgroundColor(#RRGGBBAA)`
  - Example: `SetBackgroundColor(#00000012)` for semi-transparent black overlay
  - Modifies the SpriteRenderer.color on GameObject with tag "Background"

- `SequencerCommandUpdateStatus.cs` - Modifies character status values from dialogue
  - Usage: `UpdateStatus(statusType, amount)` where statusType is "Heart", "Money", or "Energy"
  - Example: `UpdateStatus(Heart, 10)` increases heart by 10, `UpdateStatus(Money, -5)` decreases money by 5
  - Calls GameManager.Instance.UpdateStatus{Type}(amount) which triggers status change events

- `SequencerCommandFMODPlay.cs` - Plays FMOD one-shot audio events
  - Usage: `FMODPlay(eventName)` where eventName is appended to "event:/" path
  - Example: `FMODPlay(UI/Click)` plays "event:/UI/Click"

**Event System:**
- DialogueEventManager.OnConversationLine() handles background/comic image changes from custom fields
- ResponseMenuTitle component updates menu title from dialogue entry titles
  - Subscribes to DialogueSystemEvents.conversationEvents.onConversationResponseMenu
  - Updates TextMeshProUGUI with first response's destinationEntry.Title

### Project Structure

```
Assets/
├── Scripts/           - Core game logic
├── Scenes/           - Unity scenes (MainStoryScene, TitleScene, Playground)
├── Prefabs/          - Reusable game objects (Dialogue Manager, Status Toast, UIStatus)
├── Arts/             - Visual assets organized by type
│   ├── UI/           - UI sprites and graphics
│   ├── Characters/   - Character art (Mai, Beigo, Bai)
│   ├── CGs/          - CG images
│   ├── Events/       - Event illustrations
│   ├── Icons/        - Icon graphics
│   └── Logos/        - Logo assets
├── Settings/         - URP and scene template settings
├── Editor/           - Custom editor scripts
├── Materials/        - Materials and shaders
└── Plugins/          - Third-party plugins
    ├── FMOD/         - FMOD audio integration
    └── Pixel Crushers/ - Dialogue System
```

## Important Conventions

### Status System
- All status values are clamped between 0-100
- GameManager uses events to notify listeners of status changes
- UI components normalize status values to 0-1 range for display
- Status types are defined in the `StatusType` enum: Heart, Money, Energy

### Asset References
- Background images and comic panels use Addressables string paths
- Always check AsyncOperationStatus before accessing loaded assets
- Null or empty image paths in dialogue clear the display

### Dialogue Custom Fields
- "Background Image" - Addressable path to background sprite
- "Comic Image" - Addressable path to comic panel sprite
- Custom sequencer commands can access dialogue entry fields

### GameObject Tags and Names
- **Tag "Background"** - Main background sprite renderer (used by multiple systems)
  - Used by: SetBackgroundColor, DialogueEventManager, AutoScaleSprites
- **GameObject.Find names:**
  - "ComicImage" - Comic panel sprite renderer (used by DialogueEventManager)
  - "Response Menu Title" - TextMeshProUGUI for dialogue choice menu title (used by ResponseMenuTitle)

### Language
The codebase contains Chinese comments and UI text. Variable names and function names use English.

## Package Dependencies

Key Unity packages (from `Packages/manifest.json`):
- `com.unity.2d.animation@12.0.2` - 2D animation framework
- `com.unity.2d.aseprite@2.0.2` - Aseprite file import
- `com.unity.addressables@2.7.4` - Asset management
- `com.unity.inputsystem@1.14.2` - New Input System
- `com.unity.render-pipelines.universal@17.2.0` - URP rendering
- `com.unity.timeline@1.8.9` - Timeline for cutscenes
- `com.nobi.roundedcorners` (via git) - Rounded corner UI shader

## Working with This Codebase

### Extending Dialogue Functionality
1. Add custom fields to dialogue entries in the Dialogue System database
2. Handle new fields in `DialogueEventManager.OnConversationLine()`
3. Create custom sequencer commands:
   - Inherit from `SequencerCommand` in namespace `PixelCrushers.DialogueSystem.SequencerCommands`
   - Use `GetParameter(index)`, `GetParameterAsInt(index)`, `GetParameterAsFloat(index)` to read parameters
   - Call `Stop()` when command completes (immediately in Awake() for instant commands)
   - Implement OnDestroy() for cleanup (called even if sequence is cancelled)

### Adding New Status Types
1. Add to `StatusType` enum in UIStatus.cs (e.g., `Happiness`)
2. Add color constant in UIStatus.cs (e.g., `private readonly String happinessBgColor = "#AABBCC"`)
3. Add sprite reference to ComponentReferences class
4. Update `GetStatusColor()` and `GetStatusSprite()` switch statements
5. Add properties and methods in GameManager:
   - Private field: `private int statusHappiness = 50`
   - Public property: `public int StatusHappiness { get => statusHappiness; }`
   - Event: `public event StatusChangedHandler OnStatusHappinessChanged`
   - Update method: `public void UpdateStatusHappiness(int amount)` with clamping and event invocation
6. Add event handler in StatusToast.Start() and create callback method
7. Update SequencerCommandUpdateStatus switch statement

### Working with FMOD
- FMOD events use path format: "event:/EventName"
- Play one-shots via: `FMODUnity.RuntimeManager.PlayOneShot(path)`
- In dialogue sequences: `FMODPlay(EventName)` - the "event:/" prefix is added automatically
- FMOD banks managed through FMOD Studio integration settings at `Assets/Plugins/FMOD/`

### Visual Novel Background System
- Background images loaded via Addressables from dialogue custom fields
- AutoScaleSprites component automatically scales backgrounds to fill orthographic camera view
- Background color overlays applied via `SetBackgroundColor()` sequencer command
- Comic panels fade in/out automatically when sprite changes (ComicImageFader component)

### UI Programming Patterns
- Custom UI graphics inherit from `UnityEngine.UI.Graphic` (see UIArrow, UIRadialGradient)
- Override `OnPopulateMesh(VertexHelper vh)` to procedurally generate geometry
- Use `SetVerticesDirty()` in OnValidate() to refresh in editor
- Status display uses normalized 0-1 values internally, even though GameManager stores 0-100
