# Model Catalog Guide

This guide explains how to maintain the `catalog.json` file for the CustomCars mod.

## Overview

The `catalog.json` file is the central registry of all downloadable car models. Users can browse and download models directly through the in-game model browser (press F7).

## File Location

The catalog must be located at the root of your GitHub repository:
```
https://raw.githubusercontent.com/JosFa1/CustomCars/main/catalog.json
```

## JSON Structure

```json
{
  "models": [
    {
      "name": "modelname",
      "displayName": "Display Name",
      "description": "Description of the model",
      "author": "Author Name",
      "version": "1.0",
      "downloadUrl": "https://raw.githubusercontent.com/JosFa1/CustomCars/main/Models/modelname",
      "previewUrl": "",
      "fileSize": "0.5"
    }
  ]
}
```

## Field Descriptions

### Required Fields

- **name** (string): Internal identifier for the model
  - Must match the asset bundle filename
  - Use lowercase, no spaces
  - Example: `"cybertruck"`, `"sportscar"`, `"racecar"`

- **displayName** (string): User-friendly name shown in the browser
  - Can use any characters, spaces, capitals
  - Example: `"Cybertruck"`, `"Sports Car"`, `"Race Car"`

- **description** (string): Brief description of the car model
  - Keep it concise (1-2 sentences)
  - Describe the visual style or inspiration

- **author** (string): Creator's name
  - Your username or real name
  - Can be modified if you create commissioned work

- **version** (string): Model version number
  - Use semantic versioning: `"1.0"`, `"1.1"`, `"2.0"`
  - Update when you release model updates

- **downloadUrl** (string): Direct download link to the asset bundle
  - Must be a raw GitHub URL
  - Format: `https://raw.githubusercontent.com/[user]/[repo]/[branch]/Models/[name]`
  - **Important**: No file extension in the URL

- **fileSize** (string): File size in megabytes
  - String representation of a decimal number
  - Example: `"0.5"` for 500KB, `"2.3"` for 2.3MB
  - Helps users understand download size

### Optional Fields

- **previewUrl** (string): URL to a preview image
  - Not currently displayed but reserved for future use
  - Can be left empty: `""`
  - Should be a direct link to an image (PNG, JPG)

## Adding a New Model

1. Create and test your car model asset bundle
2. Upload the asset bundle to `/Models/` in your repository
3. Edit `catalog.json`
4. Add a new entry to the `models` array:

```json
{
  "name": "newcar",
  "displayName": "New Car",
  "description": "An amazing new car model",
  "author": "YourName",
  "version": "1.0",
  "downloadUrl": "https://raw.githubusercontent.com/JosFa1/CustomCars/main/Models/newcar",
  "previewUrl": "",
  "fileSize": "0.8"
}
```

5. Commit and push to GitHub
6. Users will see it in the browser on next launch (if auto-check is enabled) or when they press F7

## Updating an Existing Model

To update a model without breaking existing installations:

### Minor Update (same filename)
1. Replace the asset bundle file in `/Models/`
2. Update the `version` field in catalog.json
3. Users will see "Installed" but can re-download

### Major Update (new filename)
1. Upload new asset bundle with different name
2. Add as a new entry to catalog.json
3. Optionally remove old entry or keep both

## Testing Your Catalog

Test your catalog locally before pushing:

1. Host catalog.json on a local web server or GitHub gist
2. Temporarily edit `ModelCatalog.cs` to point to your test URL
3. Build and test the mod
4. Verify all models download correctly
5. Restore the original URL before releasing

## Common Mistakes

### Wrong URL Format
❌ Wrong: `https://github.com/JosFa1/CustomCars/blob/main/Models/car`
✅ Correct: `https://raw.githubusercontent.com/JosFa1/CustomCars/main/Models/car`

### Missing Comma
```json
{
  "models": [
    {"name": "car1", ...}   // ❌ Missing comma
    {"name": "car2", ...}
  ]
}
```

Should be:
```json
{
  "models": [
    {"name": "car1", ...},  // ✅ Comma present
    {"name": "car2", ...}
  ]
}
```

### Name Mismatch
If your asset bundle file is `cybertruck` (no extension), then:
- ✅ `"name": "cybertruck"`
- ❌ `"name": "Cybertruck"` (wrong case)
- ❌ `"name": "cybertruck.bundle"` (no extension)

## Example: Complete Catalog

```json
{
  "models": [
    {
      "name": "cybertruck",
      "displayName": "Cybertruck",
      "description": "A futuristic angular electric truck design",
      "author": "JosFa1",
      "version": "1.0",
      "downloadUrl": "https://raw.githubusercontent.com/JosFa1/CustomCars/main/Models/cybertruck",
      "previewUrl": "",
      "fileSize": "0.5"
    },
    {
      "name": "cozycoupe",
      "displayName": "Cozy Coupe",
      "description": "Classic children's car with a timeless design",
      "author": "JosFa1",
      "version": "1.0",
      "downloadUrl": "https://raw.githubusercontent.com/JosFa1/CustomCars/main/Models/cozycoupe",
      "previewUrl": "",
      "fileSize": "0.3"
    },
    {
      "name": "keitruck",
      "displayName": "Kei Truck",
      "description": "Compact Japanese utility vehicle",
      "author": "JosFa1",
      "version": "1.0",
      "downloadUrl": "https://raw.githubusercontent.com/JosFa1/CustomCars/main/Models/keitruck",
      "previewUrl": "",
      "fileSize": "0.4"
    }
  ]
}
```

## For Commissioned Models

If you're creating models for others:

1. Add model to `/Models/` as usual
2. In the catalog entry, set:
   - `"author"`: Commissioner's name or "Commission for [Name]"
   - `"description"`: Add "(Private)" if they don't want it public
3. Consider creating a separate branch for private models

## Validation

Before pushing changes:

1. Validate JSON syntax: https://jsonlint.com
2. Test all download URLs in a browser
3. Verify file sizes are accurate
4. Check that all names match asset bundle filenames

## Maintenance Tips

- Keep models organized in alphabetical order
- Archive old/outdated models by removing from catalog (but keep files for legacy users)
- Update descriptions if you improve the model
- Increment version numbers for updates
- Consider adding a changelog in the GitHub releases
