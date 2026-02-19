# Folder Index

This index gives a quick map of `D:/Unity/CTIN532` and the main gameplay code areas.

## Top-level

- `Assets/`
- `Packages/`
- `ProjectSettings/`
- `.vscode/`
- `.gitignore`

## Assets overview

- `Assets/Scripts/`: gameplay and systems code
- `Assets/Profab/`: prefab assets (note: folder name is likely a typo of `Prefab`)
  - `Character/`
  - `Enemy/`
  - `Weapon/`
  - `Room/`
- `Assets/Scenes/`: Unity scenes
- `Assets/Art/`: art packs and visual assets
- `Assets/TileMap/`: tilemap resources
- `Assets/Animator/`: animation controllers
- `Assets/ScriptableObject/`: ScriptableObject assets
- `Assets/music/`: audio assets
- `Assets/Settings/`: project/runtime settings assets
- `Assets/TextMesh Pro/`: TMP resources

## Script feature map (`Assets/Scripts`)

- `Enemy/`: enemy AI, state machines, movement/combat, enemy variants
- `角色/`: player character data and character state logic
- `武器/`: weapon base classes, gun/shotgun/bullet behavior
- `Buff/`: buff types, buff management, buff UI updates
- `关卡/`: level theme and room progression logic
- `管理器/`: game-level managers (level, character, enemy, pools, camera, weapon)
- `UI/`: HP bars, checkout page, scene transitions
- `输入/`: input controller and input data
- `游戏对象通用方法/`: shared object movement/rotation/effects helpers
- `插件/`: utility plugins (timers, state machine base, gizmos, inspector helpers)
- `Editor/`: editor-only tooling and property drawers

## Noted naming inconsistencies

- `Assets/Profab/` vs standard naming `Assets/Prefab/`
- Mixed Chinese/English folder and file naming conventions
- Mixed transliteration/English naming for similar concepts
