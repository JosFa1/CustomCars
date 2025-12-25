# Release Notes Template

Use this template when creating new releases on GitHub.

## Release Checklist

1. [ ] Update version number in `CustomCars.csproj`
2. [ ] Build the project in Release mode
3. [ ] Test the DLL with the game
4. [ ] Commit and push changes
5. [ ] Create GitHub release with proper tag format (v1.x.x)
6. [ ] Upload DLL and any new car models
7. [ ] Fill out release notes using template below

---

## Release Notes Template

Copy and paste this into your GitHub release description:

```markdown
## What's New

- Add new features here
- List major changes

## Bug Fixes

- List bug fixes
- Include issue numbers if applicable (#123)

## New Car Models

- List any new car models added
- Include screenshots if available

## Installation

1. Download `CustomCars.dll` from the Assets section below
2. Place in `BepInEx/plugins/CustomCars/` folder
3. Add any new car models to `BepInEx/plugins/CustomCars/AssetBundles/`
4. Update your configuration if needed

## Compatibility

- BepInEx: 5.4.23.2+
- Unity: 2022.3.39f1 (for creating custom models)
- Game: New Star GP

## Known Issues

- List any known issues
- Link to issue tracker

## Full Changelog

Link to compare view: https://github.com/JosFa1/CustomCars/compare/v1.0.0...v1.0.1
```

---

## Version Numbering Guide

Follow semantic versioning (MAJOR.MINOR.PATCH):

- **MAJOR** (1.0.0 → 2.0.0): Breaking changes, major rewrites
- **MINOR** (1.0.0 → 1.1.0): New features, non-breaking changes
- **PATCH** (1.0.0 → 1.0.1): Bug fixes, small improvements

Examples:
- `v1.0.1` - Bug fix release
- `v1.1.0` - Added new feature (e.g., update checker)
- `v2.0.0` - Complete rewrite or breaking API changes

---

## Example Release

**Tag:** `v1.0.1`

**Title:** CustomCars v1.0.1 - Update Checker

**Description:**

```markdown
## What's New

- Added automatic update checker on launch
- Users are notified when new versions are available
- Option to silence update notifications in config

## Bug Fixes

- Fixed issue with car models not loading on first race
- Improved error logging for asset bundle loading

## Installation

Download `CustomCars.dll` from the Assets section below and place it in your `BepInEx/plugins/CustomCars/` folder.

## Compatibility

- BepInEx 5.4.23.2+
- Tested on Windows and Steam Deck

## Configuration

New config options in `BepInEx/config/CustomCars.cfg`:

```ini
[Update Checker]
CheckForUpdates = true
SilenceUpdateNotifications = false
```

## Full Changelog

https://github.com/JosFa1/CustomCars/compare/v1.0.0...v1.0.1
```

**Assets:**
- CustomCars.dll (upload your built DLL)
