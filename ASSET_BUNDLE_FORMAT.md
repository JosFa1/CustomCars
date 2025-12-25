# Custom Cars - Asset Bundle Format

## Prefab Structure

Each car prefab must have the following structure:

```
YourCarName (Root GameObject)
├─ MeshFilter (with your car's 3D mesh)
└─ MeshRenderer (with materials and textures)
```

## Component Requirements

### Root GameObject
- **Name**: Use lowercase for consistency (e.g., `cybertruck`, `sportscar`, `racecar`)
- **Transform**: Position/rotation/scale will be applied to the visual model
- **Required Components**:
  - `MeshFilter`: Contains the 3D mesh for your car body
  - `MeshRenderer`: Contains materials/textures for rendering

## Setup in Unity

### Automatic Setup (Recommended)
1. Select your car model in the hierarchy
2. Right-click → "Setup as Car Prefab"
3. This will automatically:
   - Add required components (MeshFilter, MeshRenderer)
   - Add validation script

### Manual Setup
1. Create a new GameObject with your car's name
2. Add `MeshFilter` and assign your car mesh
3. Add `MeshRenderer` and assign materials

### Asset Bundle Assignment
1. Select your prefab in Project window
2. At the bottom of Inspector, find "AssetBundle" dropdown
3. Click dropdown → "New..." → type your car name (lowercase)
4. Example: `cybertruck`, `sportscar`, `racecar`

## Building Asset Bundles

1. Open Unity editor
2. Menu: **Tools → Car Asset Bundle Builder**
3. Click **"Validate All Car Prefabs"** to check for issues
4. Click **"Build Asset Bundles"**
5. Bundles will be created in `Assets/../AssetBundles/`

## Deploying to Game

1. Copy your asset bundle file (e.g., `cybertruck`) from Unity's `AssetBundles` folder
2. Paste into: `BepInEx/plugins/CustomCars/AssetBundles/`
3. **Do NOT copy** `.manifest` or `.meta` files
4. Launch game - check BepInEx console log for confirmation

## What Gets Applied In-Game

When your car loads, the mod will:

1. **Hide** original car mesh renderers:
   - `Car_chasis` renderer disabled
   - `Car_FrontWing` renderer disabled
   - `Car_backWing` renderer disabled
   - `Car_RearWing` renderer disabled

2. **Apply** your custom visuals:
   - Creates `CustomCarVisual` child under `Car_chasis`
   - Applies your mesh and materials to this child
   - Applies your prefab's position, rotation, and scale

## Multiple Cars in One Bundle

You can have multiple car prefabs in a single asset bundle:

1. Create multiple prefabs (e.g., `sportscar`, `racecar`, `classiccar`)
2. Assign them all to the **same** asset bundle name (e.g., `carcustomizations`)
3. Build once
4. Copy the single bundle file to game
5. All cars will be available in the config

## Troubleshooting

### Car doesn't load
- Check BepInEx log for errors
- Verify prefab has all required components
- Ensure asset bundle name matches config entry (case-insensitive)

### Car rotated wrong
- Adjust prefab's root rotation in Unity
- The prefab's rotation will be applied to the visual

## Example Prefab Setup

```
cybertruck (Root)
├─ Transform: Position(0, 0, 0), Rotation(0, 0, 0), Scale(1, 1, 1)
├─ MeshFilter: [CybertruckMesh]
└─ MeshRenderer: [CybertruckMaterial]
```
├─ WheelPosition_FrontRight: LocalPos(0.55, -0.25, 1.1)
├─ WheelPosition_RearLeft: LocalPos(-0.55, -0.25, -1.1)
└─ WheelPosition_RearRight: LocalPos(0.55, -0.25, -1.1)
```
