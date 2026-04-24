# Images Inventory

Recommended images and sprite assets for the graphics improvements. Columns: name, format, width, height, tool needed to make it, description, and purpose.

| Name | Format | Width | Height | Tool needed | Description | Purpose |
|---|---:|---:|---:|---|---|---|
| Terrain Tile (base) | PNG (RGBA) | 128 | 128 | Aseprite / Photoshop / Krita | Square ground tile variants (plain, forest, fort, mountain, wall). | Core map tiles for rendering. |
| Terrain Decoration — Tree | PNG (RGBA) | 128 | 128 | Aseprite / Photoshop | Decorative tree prop with transparent background. | Visual variety & cover. |
| Terrain Decoration — Rock/Bush | PNG (RGBA) | 64 | 64 | Aseprite / Photoshop | Small props (rocks, bushes). | Visual variety & minor obstacles. |
| Terrain Tileset Atlas | PNG (RGBA) | 2048 | 2048 | TexturePacker / Unity Sprite Atlas / Photoshop | Packed atlas containing all tile variants and decorations. | Efficient batching and import. |
| Unit Sprite Frame (per-frame) | PNG (RGBA) | 128 | 128 | Aseprite / Spine / Photoshop | Single animation frame for a unit (walk/idle/attack/hurt). | Base frame size for character animations. |
| Unit Sprite Sheet (example) | PNG (RGBA) | 1024 | 512 | Aseprite / TexturePacker | Sprite sheet (frame=128x128; layout example: 8x4). | Import into Unity as sprite sheet. |
| Character Portrait — Full | PNG (RGBA) | 512 | 512 | Photoshop / Krita | Detailed portrait for combat/dialog screens. | Combat/dialog UI and closeups. |
| Character Portrait — Thumb | PNG (RGBA) | 128 | 128 | Photoshop / Krita | Small portrait used in HUD and lists. | Selected-unit HUD display. |
| Weapon Icon | PNG (RGBA) | 64 | 64 | Figma / Illustrator | Small icon for each weapon type (sword, axe, lance, bow, staff). | Inventory, HUD, and menus. |
| Item Icon | PNG (RGBA) | 64 | 64 | Figma / Illustrator | Icons for consumables and items (potion, key, etc.). | Inventory UI. |
| Status Effect Icon | PNG (RGBA) | 64 | 64 | Figma / Illustrator | Icons for Poison, Sleep, Stun, etc. | Above-unit indicators and HUD. |
| HP Bar — Fill | PNG (RGBA) | 256 | 32 | Figma / Photoshop | Foreground fill sprite for HP bars (horizontally stretched). | Show current HP. |
| HP Bar — Track | PNG (RGBA) | 256 | 32 | Figma / Photoshop | Background track for HP bar (9-slice friendly). | HP bar background. |
| UI Panel Background (9-slice) | PNG (RGBA) | 512 | 256 | Figma / Photoshop | Scalable panel background art for dialogs/menus. | UI windows and panels. |
| Button (Primary) | PNG (RGBA) | 256 | 64 | Figma / Photoshop | Primary action button artwork (provide 9-slice guides). | On-screen buttons. |
| Controller Button Icons | PNG (RGBA) | 64 | 64 | Figma / Illustrator | A/B/X/Y and shoulder icons for prompts. | On-screen control hints (gamepad). |
| Cursor Sprite | PNG (RGBA) | 64 | 64 | Aseprite / Photoshop | Pulsating cursor used by mouse and gamepad. | Map selection and movement. |
| Selection Ring | PNG (RGBA) | 128 | 128 | Aseprite / Photoshop | Semi-transparent ring to indicate selected unit/tile. | Selection and target highlighting. |
| Movement Range Tile Overlay | PNG (RGBA) | 128 | 128 | Aseprite / Photoshop | Semi-transparent blue overlay tile for movement range. | Show reachable tiles. |
| Attack Range Tile Overlay | PNG (RGBA) | 128 | 128 | Aseprite / Photoshop | Semi-transparent red overlay tile for attack range. | Show attackable tiles. |
| Skill / Attack VFX Sheet | PNG (RGBA) | 1024 | 1024 | Aseprite / After Effects / Spine | Animated effect frames (frame=128x128; example 8x8 grid). | Combat visual effects (spells, slashes). |
| Particle Sprite (small) | PNG (RGBA) | 32 | 32 | Aseprite | Small particle used for hits, sparks, heal pips. | Particle systems & VFX. |
| Map Background (full-screen) | PNG (RGBA) | 1920 | 1080 | Photoshop / Krita | Parallax or background art used behind the map. | Visual depth and ambience. |
| Minimap Icon | PNG (RGBA) | 32 | 32 | Figma / Illustrator | Small icon for player, enemy, POIs on minimap. | Minimap display. |
| Icon Source (vector) | SVG | — | — | Figma / Illustrator | Vector source for small icons; export to PNG at required sizes. | Scalable source for UI/icons. |

## Export & naming conventions

- Raster export: use PNG-24 (RGBA) for sprites requiring alpha. Provide layered source files (`.aseprite`, `.psd`, `.xcf`) in a `Assets/Source/` folder.
- Use power-of-two sizes where reasonable (32, 64, 128, 256, 512, 1024, 2048) for GPU-friendly textures.
- Provide sprite sheets + metadata (Aseprite `.json` or TexturePacker `.atlas`) for animations. Use Unity Sprite Atlas for runtime packing.
- UI icons: keep a vector (`.svg`) master and export PNGs at 1x and 2x (or produce separate high-DPI assets) for crisp UI on different displays.
- For scalable UI panels/buttons use 9-slice-friendly art and note the safe margins in source files.

If you want, I can: (1) adjust sizes for a specific target (mobile/low-end/4K), (2) produce a suggested folder layout under `Assets/Graphics/`, or (3) generate example Aseprite/PSD templates.
