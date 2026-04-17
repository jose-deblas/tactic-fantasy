# Plan for Graphics: From 2D Procedural to 3D Isometric

> **Goal**: Transform the current procedural 2D rendering (colored quads and circles) into a 3D isometric game that visually evokes Fire Emblem: Radiant Dawn.
>
> **Architecture Advantage**: Thanks to Hexagonal Architecture, **only the Adapters layer changes**. All 30 Domain files and 16+ test suites remain completely untouched. The entire graphics transition is confined to `Assets/Scripts/Adapters/` and new asset files.
>
> **Current Rendering State**: Zero external assets. Everything is procedurally generated at runtime -- `SpriteRenderer` quads for tiles, procedural circle textures for units, world-space Canvas for HP bars and UI. Orthographic top-down camera on the XY plane.

---

## Tool Stack (All Free)

| Tool | Purpose | Where to Get It |
|------|---------|-----------------|
| **Unity 6 URP** | Render pipeline (already installed) | Package Manager |
| **ProBuilder** | In-engine 3D tile mesh creation | Package Manager: `com.unity.probuilder` |
| **Shader Graph** | Procedural textures, effects, highlights | Included with URP |
| **Blender 4.x** | Character modeling, UV, texturing, animation | blender.org (free) |
| **Mixamo** | Auto-rigging + base humanoid animations | mixamo.com (free, Adobe account) |
| **Cinemachine** | Smart camera management, combat camera | Package Manager: `com.unity.cinemachine` |
| **DOTween** | Smooth programmatic animations | Unity Asset Store (free version) |
| **Aseprite** | Pixel art sprites (if using billboard approach) | aseprite.org ($20) or compile from source |
| **BFXR / SFXR** | Retro sound effect generation | bfxr.net (free, browser-based) |
| **Audacity** | Audio editing | audacityteam.org (free) |

---

## Phase 1: Camera + 3D Tile Foundation

**Objective**: Get the game running in 3D isometric with primitive 3D shapes. No art assets yet -- just transform from 2D to 3D coordinate space.

### Step 1: Isometric Camera Setup

**What Radiant Dawn does**: Fixed isometric camera at ~30-degree pitch. Rotatable in 90-degree snaps (4 cardinal views). Zoom in/out. Follows selected unit or cursor.

**What to build**:

1. Create `Adapters/CameraController.cs`:
   ```
   Camera type: Orthographic (not perspective -- cleaner isometric look)
   Rotation: (30, 45, 0) for default isometric angle
   Size: 10 (adjustable with zoom)
   
   Structure:
   - Empty "CameraPivot" at map center
     - Camera as child, offset backward
   - Rotating CameraPivot gives 4 views
   ```

2. Controls:
   - **Q / E keys** or **shoulder buttons**: Rotate 90 degrees (smooth lerp, 0.3s)
   - **Scroll wheel / triggers**: Zoom in/out (size 6 to 14)
   - **Auto-follow**: Camera smoothly tracks selected unit or cursor

3. Install Cinemachine for smooth camera transitions:
   ```
   Package Manager -> com.unity.cinemachine
   ```
   Use `CinemachineVirtualCamera` with `CinemachineTransposer` for follow behavior.

**Coordinate system change**:
- Current: XY plane (Z = depth sorting)
- New: **XZ plane** (Y = height). This is Unity's standard 3D convention
- All position calculations: `new Vector3(x, height, y)` instead of `new Vector3(x, y, z)`

### Step 2: 3D Tile Grid

**What to change in `MapRenderer.cs`** (complete rewrite):

Current:
```csharp
// Creates 2D SpriteRenderer + BoxCollider2D per tile
var sprite = tileGO.AddComponent<SpriteRenderer>();
sprite.color = GetTerrainColor(terrain);
```

New:
```csharp
// Creates 3D cube + BoxCollider per tile
var meshRenderer = tileGO.AddComponent<MeshRenderer>();
var meshFilter = tileGO.AddComponent<MeshFilter>();
meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
tileGO.AddComponent<BoxCollider>(); // 3D collider for raycasting

// Height variation by terrain
float height = GetTerrainHeight(terrain); // Plain=0.2, Forest=0.3, Fort=0.5, Mountain=1.0
tileGO.transform.localScale = new Vector3(0.95f, height, 0.95f); // 0.95 gap between tiles
tileGO.transform.position = new Vector3(x, height / 2f, y);

// Use MaterialPropertyBlock for efficient per-tile coloring
var block = new MaterialPropertyBlock();
block.SetColor("_BaseColor", GetTerrainColor(terrain));
meshRenderer.SetPropertyBlock(block);
```

**Terrain height values**:

| Terrain | Height | Visual |
|---------|--------|--------|
| Plain | 0.2 | Flat slab |
| Forest | 0.4 | Slightly raised + tree props later |
| Fort | 0.6 | Raised platform |
| Mountain | 1.2 | Tall block |
| Wall | 1.5 | Full-height block |

### Step 3: Unit Placeholder 3D Objects

**What to change in `UnitRenderer.cs`** (complete rewrite):

Replace circle sprites with Unity primitive capsules:
```csharp
// Create 3D capsule for each unit
var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
capsule.transform.localScale = new Vector3(0.4f, 0.6f, 0.4f);

// Position on top of tile
float tileHeight = GetTerrainHeight(terrain);
capsule.transform.position = new Vector3(x, tileHeight + 0.6f, y);

// Team color via MaterialPropertyBlock
Color teamColor = unit.Team == Team.PlayerTeam 
    ? new Color(0.2f, 0.4f, 1f)   // Blue
    : new Color(1f, 0.2f, 0.2f);  // Red
```

HP bars become world-space Canvas elements that billboard toward the camera:
```csharp
var hpCanvas = new GameObject("HPCanvas");
var canvas = hpCanvas.AddComponent<Canvas>();
canvas.renderMode = RenderMode.WorldSpace;
// Billboard: hpCanvas.transform.LookAt(Camera.main.transform);
```

### Step 4: 3D Input Raycasting

**What to change in `InputHandler.cs`**:

Current (2D):
```csharp
Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
int x = Mathf.RoundToInt(worldPos.x);
int y = Mathf.RoundToInt(worldPos.y);
```

New (3D):
```csharp
Ray ray = Camera.main.ScreenPointToRay(mousePos);
if (Physics.Raycast(ray, out RaycastHit hit, 100f))
{
    int x = Mathf.RoundToInt(hit.point.x);
    int y = Mathf.RoundToInt(hit.point.z); // Z in 3D = Y in game logic
}
```

### Deliverable

Playable game with 3D isometric view, colored cubes for terrain with height variation, colored capsules for units, rotatable camera. Still no art assets -- purely Unity primitives. All domain logic and tests untouched.

---

## Phase 2: Terrain Art with ProBuilder + Shader Graph

**Objective**: Replace primitive cubes with styled terrain tiles using procedural textures. No external texture files needed.

### Step 5: Install ProBuilder

```
Package Manager -> Unity Registry -> ProBuilder
```

ProBuilder lets you create custom 3D meshes directly inside Unity Editor. Create one prefab per terrain type:

**Terrain tile prefabs to create:**

| Terrain | ProBuilder Mesh Design |
|---------|----------------------|
| **Plain** | Flat slab (1x0.2x1) with beveled top edges. Grass-green material |
| **Forest** | Raised slab (1x0.3x1) + 2-3 cylinder+cone child objects as simple trees |
| **Fort** | Platform (1x0.5x1) with small parapet walls on edges (4 thin boxes) |
| **Mountain** | Irregular tall shape (1x1.2x1). Use ProBuilder's extrude tool to make a rough peak |
| **Wall** | Solid block (1x1.5x1) with stone texture |

**Workflow:**
1. Open ProBuilder window (Tools > ProBuilder > ProBuilder Window)
2. Create a new ProBuilder cube
3. Select faces and extrude/inset to shape each terrain type
4. Apply URP Lit material
5. Save as prefab in `Assets/Prefabs/Terrain/`
6. MapRenderer instantiates correct prefab per tile

### Step 6: Procedural Textures with Shader Graph

Create materials that look good without any texture files:

**Grass shader** (for Plains):
1. Open Shader Graph (Create > Shader Graph > URP > Lit Shader Graph)
2. Add `Gradient Noise` node (scale 15) -> multiply with base green color
3. Add `Voronoi` node (scale 8, low angle offset) for subtle cell pattern
4. Combine and output to `Base Color`
5. Result: organic-looking green surface

**Stone shader** (for Walls, Forts, Mountains):
1. `Simple Noise` (scale 5) -> remap to gray range (0.3 to 0.7)
2. `Voronoi` (scale 3) for crack-like patterns
3. Multiply together -> output to `Base Color`
4. Add `Normal From Height` node for fake 3D cracks

**Dirt shader** (for paths, Fort floor):
1. `Gradient Noise` (scale 10) mixed with brown base
2. Small `Voronoi` for pebble texture

**Water shader** (for future Bridge/Water terrain):
1. `Time` node -> `UV Offset` for animation
2. Blue base + `Simple Noise` for ripples
3. Set `Surface Type` to Transparent, alpha 0.7

**How to apply:**
- Create materials in `Assets/Materials/Terrain/`
- Each ProBuilder prefab gets the corresponding material
- MaterialPropertyBlock still works for tinting variations (lighter/darker per tile)

### Step 7: Grid Overlay

Create a subtle grid overlay so players can clearly see tile boundaries:

**Option A: Decal projector** (URP feature) -- project a grid texture from above
**Option B: Shader-based** -- add thin white lines at tile edges in the terrain shader using `Frac(UV)` comparison

Recommended: Shader approach (no extra GameObjects, zero performance cost):
```
In terrain Shader Graph:
- Take world position XZ
- Frac() each axis
- If < 0.02 or > 0.98 → blend in a subtle dark line (alpha 0.15)
```

### Deliverable

Distinct, visually appealing terrain types with procedural textures. No external files. Grid lines visible. ProBuilder prefabs for each terrain type.

---

## Phase 3: Character Models

You have two options. Choose based on your art skills and time budget.

### Option A: Sprite Billboards (2-3 weeks, easier)

**Best if**: You're comfortable with 2D pixel art or want fast results (Disgaea/Tactics Ogre style).

**Process:**
1. Create sprite sheets for each class using **Aseprite** or **Piskel** (free):
   - 4 directional frames (N, S, E, W) for idle
   - 4 frames for walk animation
   - 2-3 frames for attack
   - 1 frame for damaged/death
   - Sprite size: 32x32 or 64x64 pixels
2. Import sprite sheets into Unity's Sprite Editor (slice into individual frames)
3. Create a billboard script:
   ```csharp
   public class BillboardSprite : MonoBehaviour
   {
       void LateUpdate()
       {
           // Face camera but stay upright
           transform.rotation = Quaternion.Euler(
               Camera.main.transform.eulerAngles.x, 
               Camera.main.transform.eulerAngles.y, 
               0);
       }
   }
   ```
4. Each unit gets a `SpriteRenderer` on a quad with the billboard script
5. Animate using Unity's `Animator` with sprite swap

**Asset count**: 14 classes x 1 sprite sheet each = 14 sprite sheets

### Option B: Low-Poly 3D Models (6-8 weeks, closer to RD)

**Best if**: You want the full Radiant Dawn look and are willing to learn Blender.

**Process per character class:**

**1. Model base body in Blender** (Day 1-2 per class, faster after first)
- Target: 1500-3000 triangles per model
- T-pose for rigging
- Style: chibi/low-poly anime (like early 3DS Fire Emblem or Into the Breach)
- **Optimization**: Create ONE base male body and ONE base female body. Differentiate classes by swapping armor/weapon meshes and recoloring. This cuts modeling time by 70%.

**2. UV unwrap and texture** (Day 2-3)
- UV unwrap in Blender (Smart UV Project for quick results)
- Hand-paint texture in Blender's Texture Paint mode or use Krita (free)
- Resolution: 256x256 or 512x512 atlas per character
- Style: flat/cel-shaded colors with painted shadows

**3. Auto-rig with Mixamo** (30 minutes per model)
1. Export base mesh as FBX from Blender
2. Upload to mixamo.com
3. Auto-rig (select character type, adjust skeleton)
4. Download rigged model + animations:
   - Idle (breathing)
   - Walking
   - Running
5. Result: fully rigged FBX with humanoid skeleton

**4. Custom attack animations in Blender** (Day 3-4)
- Import Mixamo-rigged FBX back into Blender
- Create these animation clips:
  - Sword slash (1 second)
  - Lance thrust (1 second)
  - Axe swing (1.2 seconds)
  - Bow draw + release (1.5 seconds)
  - Magic cast (hand raise + particle point) (1.5 seconds)
  - Staff wave (1 second)
  - Hit reaction (flinch, 0.5 seconds)
  - Death (fall, 1.5 seconds)
- Export each animation as separate FBX or all in one NLA-strip file

**5. Import into Unity** (1 hour per class)
1. Drag FBX into `Assets/Models/Characters/`
2. In Import Settings:
   - Rig tab: `Humanoid` (matches Mixamo skeleton)
   - Animation tab: define clips (split if combined)
3. Create `Animator Controller` per class:
   ```
   States: Idle -> Walk -> Attack -> Hit -> Death
   Transitions:
     Idle -> Walk: "isMoving" = true
     Walk -> Idle: "isMoving" = false
     Any -> Attack: "attack" trigger
     Any -> Hit: "hit" trigger
     Any -> Death: "isDead" = true
   ```
4. Create prefab: Model + Animator + UnitBillboard tag

**Adapter changes for both options:**

- **`UnitRenderer.cs`** -- Replace capsule creation with prefab instantiation:
  ```csharp
  // Load prefab based on class name
  var prefab = Resources.Load<GameObject>($"Prefabs/Units/{unit.Class.Name}");
  var unitGO = Instantiate(prefab, worldPos, Quaternion.identity);
  ```

- **New file: `Adapters/UnitAnimationController.cs`** -- Bridges domain events to Animator:
  ```csharp
  public void PlayMoveAnimation(IUnit unit, List<(int,int)> path) { ... }
  public void PlayAttackAnimation(IUnit attacker, IUnit defender) { ... }
  public void PlayHitAnimation(IUnit unit) { ... }
  public void PlayDeathAnimation(IUnit unit) { ... }
  ```

### Deliverable

Animated characters (sprites or 3D models) on the isometric map. Movement triggers walk animation. Idle animation when stationary. Team identification via color/material.

---

## Phase 4: Combat View

**Objective**: Replicate RD's zoomed-in combat scene where attacker and defender face each other with full attack animations.

### Step 8: Combat Camera

**What Radiant Dawn does**: When combat initiates, the map camera fades out and a side-view combat camera activates. Two combatants face each other. Attack animations play. Damage numbers appear. Camera returns to map after combat.

**Implementation:**

1. **New file: `Adapters/CombatCameraController.cs`**
   - Secondary `Camera` or `CinemachineVirtualCamera`
   - Positioned to frame both combatants from the side
   - Activated on combat start, deactivated after
   - Smooth transition: main camera fades out (0.3s) -> combat camera fades in

2. **New file: `Adapters/CombatAnimationSequencer.cs`**
   - Takes a `CombatResult` (already computed by domain) and choreographs the visual sequence:
   ```
   Sequence for a hit:
   1. Attacker runs forward (0.3s)
   2. Attacker plays attack animation (0.5s)
   3. Hit VFX on defender (0.2s)
   4. Damage number floats up from defender (0.5s)
   5. Defender HP bar drains
   6. Attacker returns to position (0.3s)
   
   If counter-attack:
   7. Defender runs forward (0.3s)
   8. Defender plays attack animation (0.5s)
   9. Hit/miss VFX
   10. Damage number from attacker
   11. Return to positions
   
   If double attack:
   12. Repeat attacker sequence
   
   Total: 2-4 seconds depending on hits/counters/doubles
   ```

3. **Changes to `GameController.cs`**:
   ```csharp
   // Before: immediate combat
   var result = _combatResolver.ResolveCombat(attacker, defender, _gameMap);
   defender.TakeDamage(result.Damage);
   
   // After: combat with animation
   var result = _combatResolver.ResolveCombat(attacker, defender, _gameMap);
   yield return _combatSequencer.PlayCombatAnimation(attacker, defender, result);
   // Damage already applied by domain; animation is purely visual
   ```

4. **Combat backdrop**: Simple colored plane behind combatants matching the terrain type. No complex background needed initially.

### Step 9: Skip Button

RD lets you skip combat animations. Add:
- Press B/Space during combat animation to skip to result
- `CombatAnimationSequencer` checks for skip input each frame and fast-forwards if pressed

### Deliverable

Combat initiates a zoomed-in side view with attack animations, damage numbers, and HP drain. Skippable. Map camera returns after combat.

---

## Phase 5: VFX and Particles

**Objective**: Add visual effects for combat, magic, skills, level-ups, and status.

### Step 10: Particle Systems

Create VFX prefabs in `Assets/Prefabs/VFX/`:

| Effect | Implementation |
|--------|---------------|
| **Sword slash** | Trail Renderer on weapon swing arc. White/blue color |
| **Lance thrust** | Short burst of particles forward from weapon tip |
| **Axe swing** | Wider trail renderer, red tint |
| **Arrow flight** | Line Renderer from bow to target with arrow head |
| **Fire magic** | Particle System: orange/red flames rising at target position (50 particles, 1.5s lifetime, upward velocity, size-over-lifetime shrink) |
| **Thunder magic** | Line Renderer zigzag from sky to target + white flash (full-screen overlay, 0.1s) |
| **Wind magic** | Green spiraling particles around target (orbital velocity) |
| **Heal** | Green sparkles rising from unit (30 particles, 2s lifetime, slow upward drift) |
| **Critical hit** | Screen flash (white overlay 0.1s) + zoom punch (Cinemachine Impulse) + slow-motion frame (Time.timeScale = 0.3 for 0.2s) + larger weapon trail |
| **Skill activation** | Text popup ("Astra!", "Sol!") floating above unit + blue flash |
| **Level up** | Blue pillar of light (cylinder with emissive Shader Graph material, scale up over 1s) + stat popup |
| **Unit death** | Fade out (material alpha lerp to 0 over 1s) + dissolve particles |
| **Status applied** | Colored ring pulse: purple (Poison), blue (Sleep), yellow (Stun) |
| **Terrain healing** | Green plus sign floating up from fort tile |

**New file: `Adapters/VFXManager.cs`**:
```csharp
public class VFXManager : MonoBehaviour
{
    // Prefab references (assign in inspector or load from Resources)
    public void PlayEffect(VFXType type, Vector3 position) { ... }
    public void PlayDamageNumber(int damage, Vector3 position, bool isCritical) { ... }
    public void PlaySkillActivation(string skillName, Vector3 position) { ... }
    
    // Damage numbers: TextMeshPro in world space, float up + fade out
}
```

**Shader Graph effects:**

- **Dissolve shader**: For unit death. Use `Noise` node compared against a `_DissolveAmount` float (animated 0->1). Pixels below threshold are clipped. Edge glow via `Step` with slight offset
- **Highlight shader**: For selected tile/unit. Emissive pulse using `Sin(Time)` on emission intensity
- **Outline shader**: For hovered units. Fresnel effect or inverted hull method

### Deliverable

Impactful visual feedback for all combat actions. Magic spells look distinct. Critical hits feel powerful. Level-ups are celebratory.

---

## Phase 6: UI Overhaul

**Objective**: Replace programmatically created Canvas UI with styled panels matching the Fire Emblem aesthetic.

### Step 11: FE-Style UI Panels

**UI Framework choice**: Use Unity's Canvas + Image/Text (current approach, well-understood) but with designed sprites and layouts instead of programmatic creation. Alternatively, switch to UI Toolkit for more CSS-like styling.

**Panels to redesign:**

#### A. Turn/Phase Banner
- Full-width banner that slides in from left on phase change
- "Player Phase" in gold text on dark blue ribbon (3 seconds, auto-dismiss)
- "Enemy Phase" in red text on dark ribbon
- Include subtle sword/shield icon

#### B. Unit Stat Panel (Left Side)
- Slides in from left when unit is selected
- **Character portrait** at top (placeholder: colored silhouette per class)
- Name, Level, Class below portrait
- HP bar (styled, not just colored rect)
- Stat columns: STR/MAG | SKL/SPD | LCK/DEF/RES
- Equipped weapon with icon
- Skill icons (small icons in a row)
- XP bar at bottom

#### C. Combat Forecast (Center Bottom)
- Appears when hovering over attackable enemy
- Side-by-side comparison:
  ```
  [Attacker Portrait]         [Defender Portrait]
  Weapon: Iron Sword          Weapon: Iron Lance
  HP: 24                      HP: 18
  Damage: 8                   Damage: 5
  Hit: 87%                    Hit: 72%
  Crit: 12%                   Crit: 3%
  x2 (doubles)                -- (no double)
  ```
- Color coding: green = advantage, red = disadvantage

#### D. Terrain Info (Bottom Right)
- Small tooltip that appears on tile hover
- Terrain name + icon
- Movement cost, DEF bonus, AVO bonus
- Minimal, doesn't block view

#### E. Game Over Screen
- Full overlay with dramatic text
- "Victory" with golden particles
- "Defeat" with red tint
- Stats summary: turns, units lost, BEXP earned

#### F. Character Portraits
Options for creating portraits:
1. **Silhouettes**: Create colored silhouettes per class in any image editor (simplest)
2. **Pixel art**: Draw 64x64 or 96x96 portraits in Aseprite
3. **AI-generated**: Use image generation tools for anime-style character portraits, then clean up
4. **Commission**: Hire an artist for consistent style

Place portraits in `Assets/Sprites/Portraits/`.

### Step 12: Animated UI Elements

- HP bars drain smoothly (lerp over 0.5s, not instant)
- Stat changes flash yellow when modified (support bonus, skill activation)
- Buttons have hover/press animations (scale punch to 1.05x on hover)
- Panel slide-in/out using DOTween:
  ```csharp
  panel.DOAnchorPosX(0, 0.3f).SetEase(Ease.OutBack); // Slide in
  panel.DOAnchorPosX(-300, 0.2f).SetEase(Ease.InBack); // Slide out
  ```

### Deliverable

Polished, Fire Emblem-style UI that feels like a finished game. Information is clear, accessible, and aesthetically consistent.

---

## Phase 7: Audio Integration

**Objective**: Add sound effects and music to bring the game to life.

### Step 13: Sound Effects

**Create using BFXR** (bfxr.net, browser-based, instant):

| Category | Sounds | BFXR Preset |
|----------|--------|-------------|
| **UI** | Cursor move, confirm, cancel, menu open/close | "Blip", "Pickup" |
| **Combat** | Sword slash, lance thrust, axe swing | "Hit/Hurt" |
| **Ranged** | Arrow fire, arrow impact | "Shoot", "Hit" |
| **Magic** | Fire whoosh, thunder crack, wind gust, heal chime | "Explosion", "Powerup" |
| **Impact** | Hit land, critical hit, miss whoosh | "Hit/Hurt", custom |
| **System** | Level up fanfare, phase transition | "Powerup" |
| **Death** | Unit death sound | "Explosion" (low pitch) |
| **Movement** | Footsteps on grass/stone (optional) | Custom |

Export as WAV, place in `Assets/Audio/SFX/`.

### Step 14: Music

**Options (free or cheap):**

1. **Create with LMMS** (free DAW, lmms.io):
   - Player phase: Upbeat, strategic (tempo 120 BPM, major key)
   - Enemy phase: Tense, minor key (tempo 100 BPM)
   - Combat: Fast, intense (tempo 140+ BPM)
   - Base/shop: Calm, peaceful
   - Boss battle: Epic, driving
   - Victory: Short fanfare (5 seconds)
   - Defeat: Somber dirge (5 seconds)

2. **Bosca Ceoil** (free, boscaceoil.net): Chiptune-style music, very easy to use

3. **License royalty-free music**: OpenGameArt.org has free tracks. Unity Asset Store has packs ($10-50)

4. **Commission**: Hire a composer for custom tracks ($50-200 per track)

Place in `Assets/Audio/Music/`.

### Step 15: Audio Manager

**New file: `Adapters/AudioManager.cs`**:
```csharp
public class AudioManager : MonoBehaviour
{
    private AudioSource _musicSource;   // For BGM (loop)
    private AudioSource _sfxSource;     // For one-shot SFX
    
    public void PlaySFX(SFXType type) { ... }
    public void PlayMusic(MusicTrack track) { ... }
    public void StopMusic() { ... }
    public void CrossfadeMusic(MusicTrack newTrack, float duration) { ... }
    public void SetMusicVolume(float vol) { ... }
    public void SetSFXVolume(float vol) { ... }
}
```

**Integration points in `GameController.cs`**:
```
Unit selected          -> PlaySFX(CursorConfirm)
Unit moved             -> PlaySFX(Movement)
Combat initiated       -> CrossfadeMusic(Combat, 0.5f)
Hit landed             -> PlaySFX(Hit)
Critical hit           -> PlaySFX(Critical)
Miss                   -> PlaySFX(Miss)
Combat ended           -> CrossfadeMusic(previousPhaseMusic, 0.5f)
Phase change (Player)  -> CrossfadeMusic(PlayerPhase, 1f)
Phase change (Enemy)   -> CrossfadeMusic(EnemyPhase, 1f)
Level up               -> PlaySFX(LevelUp)
Unit death             -> PlaySFX(Death)
Game over (win)        -> PlayMusic(Victory)
Game over (lose)       -> PlayMusic(Defeat)
```

### Deliverable

Fully audible game. Combat has impact. Phase changes have musical identity. UI feels responsive with click/cursor sounds.

---

## Phase 8: Polish and Performance

**Objective**: Post-processing, lighting, optimization, and final visual quality.

### Step 16: Lighting Setup

1. **Directional Light** (Sun):
   - Rotation: (50, -30, 0) for warm angled light
   - Color: Warm white (1, 0.95, 0.9)
   - Intensity: 1.2
   - Shadow: Soft shadows, resolution 2048

2. **Ambient Light**:
   - Skybox or Gradient ambient
   - Color: Subtle blue-gray (0.3, 0.35, 0.45)
   - Intensity: 0.5

3. **Per-terrain lighting**:
   - Forts: Warm point light (torch glow, orange, range 2)
   - Forest: Slightly darker (tree shadow from cookie texture on directional light)

### Step 17: Post-Processing (URP)

Add a Global Volume with these effects:

| Effect | Setting | Purpose |
|--------|---------|---------|
| **Bloom** | Threshold 0.9, Intensity 0.3 | Magic effects and emissive materials glow |
| **Color Grading** | Warm filter, slight contrast boost | FE's warm fantasy palette |
| **Ambient Occlusion** | Intensity 0.5, Radius 0.3 | Depth between tiles and units |
| **Vignette** | Intensity 0.2 | Subtle focus on center |
| **Depth of Field** | Off by default, enable for menu/dialogue | Cinematic blur |

### Step 18: Performance Optimization

| Technique | What to Do |
|-----------|-----------|
| **GPU Instancing** | Enable on all terrain materials. Tiles of the same type share one draw call |
| **MaterialPropertyBlock** | Already using for per-tile colors. Avoids material copies |
| **Static Batching** | Mark all tile GameObjects as static (they never move) |
| **Object Pooling** | Pool VFX particle systems. Pool damage number text objects |
| **LOD Groups** | Only needed if character models exceed 5000 tris (unlikely at low-poly) |
| **Occlusion Culling** | Enable in Unity's Occlusion settings. Useful when camera rotates and tiles are behind mountains |
| **Draw Call Budget** | Target: < 200 draw calls. 16x16 tiles with instancing = ~5 calls. 8 units = ~8 calls. UI = ~20 calls |

### Step 19: Movement Path Visualization

When a unit is selected and player hovers over a reachable tile:
- Show movement path as arrow segments on tiles
- Blue arrow sprites/meshes on each tile in the path
- Final tile has a larger arrow head pointing in movement direction
- Implementation: `MapRenderer` draws path quads with arrow texture on top of tiles

### Step 20: Selection and Hover Effects

- **Selected unit**: Glowing ring on tile (emissive circle decal or ring mesh)
- **Hovered tile**: Subtle pulse (scale oscillation 0.98-1.02 on the tile mesh)
- **Movement range**: Blue transparent overlay on reachable tiles (shader alpha 0.3)
- **Attack range**: Red transparent overlay (shader alpha 0.25)
- **Danger zone** (enemy attack range): Red gradient overlay, more subtle than attack range

### Deliverable

A visually polished game with proper lighting, post-processing, smooth performance, and clear visual communication of game state.

---

## Architecture: What Changes vs. What Stays

### UNCHANGED (Domain Layer + Tests)

All 30 domain files remain untouched:
```
Domain/AI/AIController.cs                    -- No changes
Domain/Combat/CombatResolver.cs              -- No changes
Domain/Combat/CombatResult.cs                -- No changes
Domain/Combat/CombatForecast.cs              -- No changes
Domain/Combat/CombatForecastService.cs       -- No changes
Domain/Combat/CombatXp.cs                    -- No changes
Domain/Combat/WeaponTriangle.cs              -- No changes
Domain/Map/GameMap.cs                        -- No changes
Domain/Map/PathFinder.cs                     -- No changes
Domain/Map/TerrainType.cs                    -- No changes
Domain/Map/Tile.cs                           -- No changes
Domain/Save/GameSaveService.cs               -- No changes
Domain/Save/GameSnapshot.cs                  -- No changes
Domain/Save/IGameRepository.cs               -- No changes
Domain/Save/InMemoryGameRepository.cs        -- No changes
Domain/Save/UnitSnapshot.cs                  -- No changes
Domain/Turn/TurnManager.cs                   -- No changes
Domain/Turn/Phase.cs                         -- No changes
Domain/Turn/VictoryCondition.cs              -- No changes
Domain/Units/Unit.cs                         -- No changes
Domain/Units/CharacterStats.cs               -- No changes
Domain/Units/ClassData.cs                    -- No changes
Domain/Units/ClassPromotionService.cs        -- No changes
Domain/Units/MoveType.cs                     -- No changes
Domain/Units/StatusEffect.cs                 -- No changes
Domain/Units/Team.cs                         -- No changes
Domain/Units/UnitDisplayFormatter.cs         -- No changes
Domain/Units/UnitFactory.cs                  -- No changes
Domain/Weapons/DamageType.cs                 -- No changes
Domain/Weapons/Weapon.cs                     -- No changes
Domain/Weapons/WeaponType.cs                 -- No changes
```

All 16 test suites continue to pass without modification.

### MODIFIED (Adapters Layer)

| File | Change Level | Description |
|------|-------------|-------------|
| `MapRenderer.cs` | **Complete rewrite** | 2D SpriteRenderer -> 3D meshes/prefabs |
| `UnitRenderer.cs` | **Complete rewrite** | 2D circles -> 3D models/billboards |
| `CursorRenderer.cs` | **Major update** | 2D quad -> 3D tile highlight ring |
| `InputHandler.cs` | **Moderate update** | 2D raycast -> 3D raycast |
| `UIManager.cs` | **Complete rewrite** | Programmatic UI -> prefab-based styled panels |
| `GameController.cs` | **Moderate update** | Add animation/audio hooks, combat sequencing |
| `GameSceneSetup.cs` | **Minor update** | Add new component references |

### NEW (Adapters Layer)

| New File | Phase | Purpose |
|----------|-------|---------|
| `CameraController.cs` | 1 | Isometric camera with rotation/zoom |
| `UnitAnimationController.cs` | 3 | Bridges domain events to Animator triggers |
| `CombatCameraController.cs` | 4 | Secondary camera for combat view |
| `CombatAnimationSequencer.cs` | 4 | Choreographs combat animation from CombatResult |
| `VFXManager.cs` | 5 | Particle/effect management and pooling |
| `AudioManager.cs` | 7 | Sound/music management |

### NEW (Assets)

```
Assets/
  Prefabs/
    Terrain/          -- ProBuilder tile prefabs (Phase 2)
    Units/            -- Character model prefabs (Phase 3)
    VFX/              -- Particle system prefabs (Phase 5)
    UI/               -- UI panel prefabs (Phase 6)
  Materials/
    Terrain/          -- Shader Graph materials (Phase 2)
    Characters/       -- Character materials (Phase 3)
    VFX/              -- Effect materials (Phase 5)
  Models/
    Characters/       -- FBX models from Blender (Phase 3, Option B only)
  Sprites/
    Portraits/        -- Character portraits (Phase 6)
    UI/               -- UI icons, frames, buttons (Phase 6)
    Units/            -- Sprite sheets (Phase 3, Option A only)
  Shaders/
    Terrain.shadergraph    -- Grass, stone, dirt shaders (Phase 2)
    Dissolve.shadergraph   -- Death effect (Phase 5)
    Highlight.shadergraph  -- Selection glow (Phase 5)
  Audio/
    SFX/              -- Sound effects (Phase 7)
    Music/            -- Background music (Phase 7)
  Animations/
    Characters/       -- Animator Controllers (Phase 3)
```

---

## Suggested Learning Path

If you're new to 3D graphics in Unity, tackle these tutorials in order:

1. **Unity's URP setup**: unity.com/learn -> "Introduction to URP" (1 hour)
2. **ProBuilder basics**: youtube search "Unity ProBuilder tutorial" (1 hour)
3. **Shader Graph fundamentals**: unity.com/learn -> "Introduction to Shader Graph" (2 hours)
4. **Blender for beginners**: youtube "Blender Donut Tutorial by Blender Guru" (4 hours, covers all basics)
5. **Mixamo workflow**: youtube "Mixamo to Unity tutorial" (30 min)
6. **Cinemachine**: unity.com/learn -> "Using Cinemachine" (1 hour)
7. **Particle System**: unity.com/learn -> "Introduction to Particle Systems" (1 hour)

---

**Last Updated**: 2026-04-15
**Current State**: Procedural 2D (zero assets)
**Target State**: 3D isometric with Radiant Dawn visual identity
**Total Estimated Timeline**: 18-20 weeks (part-time) or 8-10 weeks (full-time)
