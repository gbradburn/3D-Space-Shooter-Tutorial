# URP to HDRP Conversion Plan — 3D Space Shooter Tutorial

## Project Analysis

### Current State
| Property | Value |
|---|---|
| Unity Version | 2022.3.38f1 (target upgrade: Unity 6 / 6000.x) |
| Render Pipeline | **Built-in Render Pipeline (BRP)** — *not* URP |
| Scene | `Assets/_project/Scenes/Main.unity` |
| Total Materials | ~54 (41 in `_project/Materials` + missile, turret, asteroid, cockpit, skybox mats) |
| Custom Shaders | 1 (`HeatDistort.shader` — uses `GrabPass`) |
| Lighting | 1 Directional Light, Point Lights via script (ShieldExplosion, DetonatorLight) |
| Cameras | Main Camera, BackgroundCamera, 3× Cinemachine Virtual Cameras |
| Particle Systems | Legacy particle shaders (Particles/Additive, Particles/Alpha Blended) |
| Third-party Assets | HiRez Spaceships Creator Free (includes HDRP upgrade package), Detonator FX |

> **Important note:** The `GraphicsSettings.asset` shows `m_CustomRenderPipeline: {fileID: 0}` and `manifest.json` contains no `com.unity.render-pipelines.universal` package — the project is currently on the **Built-in Render Pipeline**, not URP. This plan covers conversion from BRP directly to HDRP in Unity 6.

---

## Material Inventory by Shader

| Built-in Shader (fileID) | Count | HDRP Target Shader |
|---|---|---|
| `Standard` (46) | 38 | `HDRP/Lit` |
| `Particles/Additive` (200) | 6 | `HDRP/Unlit` + Additive blend |
| `Particles/Alpha Blended` (202) | 1 | `HDRP/Unlit` |
| `Particles/Multiply` (203) | 3 | `HDRP/Unlit` |
| `Skybox/6 Sided` (104) | 3 | HDRP Visual Environment (HDRI Sky) or custom sky |
| `HeatDistort` (custom) | 1 | Custom HDRP Shader Graph (Scene Color node) |

---

## Phase 1 — Pre-Conversion Preparation

### 1.1 Source Control Checkpoint
- Commit all current changes and tag the commit: `git tag pre-hdrp-conversion`
- Create a new branch: `feature/hdrp-conversion`
- Verify a clean working state before proceeding

### 1.2 Unity Version Upgrade
- Upgrade from **Unity 2022.3.38f1** to **Unity 6 (6000.x / 6.3)** via Unity Hub
- Open the project in Unity 6 and accept all automatic script and API upgrade prompts
- Resolve any compilation errors from deprecated Unity APIs (e.g., `Input.GetKeyDown` → Input System, which is already installed)
- Verify the project opens, compiles, and the scene loads without errors before proceeding

### 1.3 Document Current Lighting Values
- Note all Light intensities in the scene (currently using arbitrary units); they will need to be rescaled to physical units (Lux / Lumen / Candela) in HDRP
- Record environment lighting settings from `LightmapSettings` in `Main.unity`
- Screenshot the scene in Play mode for visual reference during validation

---

## Phase 2 — Install HDRP Package

### 2.1 Add HDRP to manifest.json
In **Window → Package Manager**, install:
```
com.unity.render-pipelines.high-definition
```
For Unity 6.3, use the HDRP package version **17.x** (bundled with Unity 6).

Remove any URP package if present (currently none, so this step can be skipped). Final relevant entry in `Packages/manifest.json`:
```json
"com.unity.render-pipelines.high-definition": "17.x.x"
```

### 2.2 Run HDRP Wizard
- Open **Edit → Rendering → HDRP Wizard**
- Click **Fix All** for the project section
- Click **Fix All** for the default scene section
- This will:
  - Create the **HDRP Default Settings** asset
  - Create an **HDRP Pipeline Asset**
  - Assign the pipeline asset in **Edit → Project Settings → Graphics**
  - Update Quality Settings to use the HDRP pipeline asset

---

## Phase 3 — HDRP Asset Configuration

### 3.1 HDRP Global Settings
Create or configure `HDRPDefaultSettings.asset` at `Assets/_project/Settings/`:
- Enable **Diffusion Profile** support (for SSS if needed on cockpit glass)
- Enable **Realtime Reflection Probes**
- Set **Frame Settings** defaults appropriate for a space shooter (no terrain features needed)

### 3.2 HDRP Pipeline Asset Quality Levels
Configure three HDRP Pipeline Asset variants (Low / Medium / High) to replace the existing 6-tier quality settings:

**All Tiers — Shared Settings:**
- Shadow Resolution: 2048 (Medium), 4096 (High)
- Enable Screen Space Reflections
- Enable Ambient Occlusion
- Enable Volumetric Lighting (for engine exhausts and explosion glow)

**High Tier — Additional Features:**
- Screen Space Global Illumination
- Ray-Traced Shadows (optional, requires DXR-capable GPU)
- Temporal Anti-Aliasing (TAA) or DLSS if targeting PC

---

## Phase 4 — Material Conversion

### 4.1 Automatic Conversion (Run First)
Use the built-in upgrade tool:
**Edit → Rendering → Materials → Convert All Built-in Materials to HDRP**

This will automatically convert the 38 `Standard` shader materials to `HDRP/Lit`. Review each conversion but the following categories should convert cleanly:
- Asteroid materials (`Rock31`, `Rock012`, etc.) — opaque PBR, minimal post-fix needed
- Cockpit materials (`Cockpit3Grey`, `Cockpit3Red`, etc.)
- Ship body / wing / engine materials
- Turret material (`Sci-fi turrets/Models/Turrets.mat`)
- Missile object material (`Missile/Object/Materials/missileMat.mat`)

### 4.2 Transparent / Emission Materials (Manual Review Required)

**ShieldMaterial.mat** (`_project/Materials/ShieldMaterial.mat`):
- Currently: `Standard` with `_ALPHAPREMULTIPLY_ON`, `_EMISSION`, Transparent render queue (3000)
- In HDRP: Set to `HDRP/Lit`, Surface Type = **Transparent**, Blend Mode = **Pre-Multiply Alpha**
- Enable **Emission** and set emission color
- Adjust **Smoothness** and **Metallic** for a holographic energy-field look
- Consider enabling **Refraction** mode for a glass-like distortion effect

**Weapon projectile materials** (`Blaster.mat`, `PlayerProjectile.mat`, `EnemyProjectile.mat`, `WeakPlayerProjectile.mat`, `WeakEnemyProjectile.mat`, `BigLauncher.mat`):
- Currently: `Standard` with `_EMISSION` enabled, solid blue/cyan/red colors
- In HDRP: `HDRP/Unlit` or `HDRP/Lit` with high emission intensity
- For glowing projectiles: Enable **Emission** with HDR values above 1.0 (e.g., 3–10×) to trigger **Bloom**
- Since HDRP uses physical units, adjust emission from LDR `(0, 0.95, 1)` to HDR values

**Cockpit glass materials** (`Cockpit3Grey_Glass.mat`, `Cockpit3Red_Glass.mat`):
- Currently: `Standard` with `_ALPHAPREMULTIPLY_ON`
- In HDRP: `HDRP/Lit`, Surface Type = **Transparent**, enable **Refraction** (Sphere mode or Box mode)
- Set IOR (Index of Refraction) ~1.5 for glass

**Radar/Cockpit screen materials** (`RadarBlip.mat`, `CockpitEquipments_Screens.mat`, `CockpitEquipments_TargetScreens.mat`):
- Currently: `Standard` with emission
- In HDRP: `HDRP/Unlit` for screen elements, or `HDRP/Lit` with high emission
- Enable **Unlit** mode to avoid lighting influence on simulated screen output

### 4.3 Particle Materials (Manual Conversion Required)
The automatic converter does not handle legacy particle shaders. Each must be manually reassigned:

**Detonator Fireball materials** (`FireballA.mat`, `FireballB.mat`, `FireballC.mat`, `Glow.mat`, `Sparks.mat`, `ShockWave.mat`):
- Currently: `Particles/Additive` (fileID 200) — additive blending, no lighting
- In HDRP: Change shader to `HDRP/Unlit`, Surface Type = **Transparent**, Blend Mode = **Additive**
- Preserve `_TintColor` → map to `_UnlitColor` in HDRP/Unlit
- Preserve texture assignments

**Detonator Smoke materials** (`SmokeMIx.mat`, `SmokeANew.mat`, `SmokeBNew.mat`):
- Currently: `Particles/Multiply` (fileID 203)
- In HDRP: `HDRP/Unlit`, Blend Mode = **Multiply** or **Alpha** depending on visual result
- Smoke in HDRP looks best with **Alpha blending** and a soft-particle depth fade

**Missile smoke material** (`Missile/Materials/smoke_material.mat`):
- Currently: `Particles/Alpha Blended` (fileID 202)
- In HDRP: `HDRP/Unlit`, Blend Mode = **Alpha**

**Missile materials** (`missile_material_green.mat`, `missie_material.mat`):
- Currently: `Standard` — should auto-convert via the tool

### 4.4 Skybox Materials (Replace Approach)
The `Skybox/6 Sided` shader is not available in HDRP. Three skybox materials need handling:
- `Purple_2K_Resolution.mat`
- `Green_2K_Resoution.mat`
- `Pink_2K_Resolution.mat`

**Option A — HDRI Sky (Recommended):**
- Combine the six 2K face textures into a single equirectangular HDRI texture per skybox (use an external tool such as [Panorama Studio](https://panoramastudio.de) or Blender's environment rendering)
- In HDRP, configure the **Sky and Fog Volume** with `Sky Type = HDRI Sky` and assign the HDRI texture
- Update `SkyboxSetter.cs` to swap HDRI textures on the Volume component instead of using the `Skybox` component (see Phase 7)

**Option B — Physically Based Sky:**
- Replace all three skyboxes with HDRP's built-in Physically Based Sky
- Adjust space sky color, star density, and ambient light to approximate the original cubemap look

### 4.5 HiRez Spaceships Creator Free — HDRP Materials
The `HiRezSpaceshipsCreatorFree` asset already includes an HDRP upgrade package:
- `Assets/HiRezSpaceshipsCreatorFree/UpgradeHDRP/HDRP_2019.4+.unitypackage`
- `Assets/HiRezSpaceshipsCreatorFree/UpgradeHDRP/HDRP_2020.2+.unitypackage`

Import the more recent `HDRP_2020.2+.unitypackage` into the project. This replaces the asset's built-in materials with HDRP-compatible equivalents. Verify that all ship meshes display correctly after import.

---

## Phase 5 — Custom Shader Conversion (HeatDistort)

This is the most complex and highest-effort item in the conversion.

### Current Implementation
`Assets/_project/Resources/Detonator/Textures/HeatDistort.shader`:
- Uses `GrabPass {}` to capture the screen behind the object
- Applies a normal-map-based distortion offset to the grabbed texture
- Renders in the Transparent queue with `LightMode = Always`

### Problem
`GrabPass` is **not supported** in HDRP. It was a legacy Shader Model 2 feature specific to the Built-in RP and URP (where it was also deprecated). HDRP uses a physically correct rendering path and does not support arbitrary screen-space grabs per object.

### Solution — HDRP Refraction + Distortion
**Step 1:** Create a new Shader Graph asset: `Assets/_project/Shaders/HeatDistortHDRP.shadergraph`

**Step 2:** In the Shader Graph:
- Set Surface Type = **Transparent**
- Enable **Distortion** (under HDRP Shader Graph Distortion settings): this replaces GrabPass with HDRP's built-in screen-space distortion buffer
- Add a **Normal Map** node using the `_BumpMap` texture
- Route the normal map output into the **Distortion** output with the `_BumpAmt` scalar as a multiplier
- Connect `_MainTex` tint color to the **Base Color** with alpha blending

**Step 3:** Create a new material `HeatDistortHDRP.mat` using this Shader Graph.

**Step 4:** Update `DetonatorHeatwave.cs` or the prefab to reference the new material.

> **Alternative:** If distortion quality is insufficient, implement a Custom Pass that renders the heat-wave objects into a separate distortion buffer. This requires creating a `CustomPassVolume` in the scene and a custom `FullScreenCustomPass` script. This approach is more complex but gives exact control.

---

## Phase 6 — Lighting Conversion

### 6.1 Directional Light
- The scene contains one Directional Light (`Main.unity`)
- In HDRP, add (or confirm auto-added) the `HDAdditionalLightData` component
- Set **Intensity Mode** to `Lux`
- Equivalent BRP intensity `1.0` → approximately `100,000 Lux` for full daylight, but for a space scene use `50,000–80,000 Lux` and tune visually
- Disable **Affect Volumetrics** unless intentional fog is added

### 6.2 Point Lights (Script-Driven)
Two systems create point lights at runtime:

**ShieldExplosion.cs:**
- Creates a `Light` component and fades `intensity` and `range` over time
- In HDRP, point light intensity is in **Candela** (not arbitrary units)
- Multiply the fade values by ~100–500× and adjust the starting intensity. For a shield explosion effect, starting at `~5000 cd` with fast decay is a good baseline
- Add `HDAdditionalLightData` via `gameObject.AddComponent<HDAdditionalLightData>()` in `Awake` to ensure HDRP registers the light

**DetonatorLight.cs:**
- Similarly creates a `Light` component and fades over duration
- `_lightComponent.intensity = intensity` — this value will need to be in Candela for HDRP
- `_lightComponent.range = size * 50f` — range behavior is similar but verify visually
- Add `HDAdditionalLightData` component when creating the light dynamically

**Code change for both scripts:**
```csharp
// After _lightComponent = _light.AddComponent<Light>();
#if HDRP_PRESENT
var hdLightData = _light.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>();
hdLightData.SetIntensity(intensity * 500f, UnityEngine.Rendering.HighDefinition.LightUnit.Candela);
#endif
```
Use `com.unity.render-pipelines.high-definition` scripting define or a `#if` guard for portability.

### 6.3 Environment Lighting
- Replace the scene's default skybox reference with a **Volume** containing `Sky and Fog` override
- In `LightingSettings`, ensure **Mixed Lighting** mode matches HDRP's supported modes (Shadowmask is supported in HDRP)
- Re-bake lightmaps after all materials are converted (baking with pink materials produces incorrect results)

---

## Phase 7 — Camera Configuration

### 7.1 HDAdditionalCameraData
HDRP requires `HDAdditionalCameraData` on every camera. The HDRP Wizard (Phase 2.2) will add these automatically. Verify on:
- `Main Camera` (tagged `MainCamera`)
- `BackgroundCamera`
- `EnemyShipFollowCamera`
- `FollowCamera`

### 7.2 Background Camera
The scene uses a `BackgroundCamera` with a custom `Skybox` component (`SkyboxSetter.cs`) to render a 6-sided cubemap skybox. This pattern is unnecessary in HDRP, which handles sky rendering through the Volume system.

**Recommended change:**
- Remove or disable `BackgroundCamera`
- Configure the sky in the scene's **Sky and Fog Volume** instead (see Phase 6.3)
- Delete the `Skybox` component from the Main Camera (HDRP ignores it)

### 7.3 SkyboxSetter.cs — Update for HDRP
The `SkyboxSetter.cs` script sets a material on a `Skybox` component. This has no effect in HDRP.

**Replacement approach:**
```csharp
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(Volume))]
public class SkyboxSetter : MonoBehaviour
{
    [SerializeField] List<Texture> _hdriTextures; // Replace Material list with HDRI Texture list

    Volume _volume;
    HDRISky _hdriSky;

    void Awake()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _hdriSky);
    }

    void OnEnable() => ChangeSkybox(0);

    void ChangeSkybox(int index)
    {
        if (_hdriSky != null && index >= 0 && index < _hdriTextures.Count)
            _hdriSky.hdriSky.value = _hdriTextures[index];
    }
}
```

### 7.4 Cinemachine Compatibility
The project uses `com.unity.cinemachine 2.10.1`. In Unity 6, Cinemachine 3.x is the default. Key differences:
- `CinemachineVirtualCamera` → `CinemachineCamera` in Cinemachine 3
- `CameraManager.cs` references `CinemachineVirtualCamera` — update type references after upgrading the package
- HDRP-specific post-processing overrides on Cinemachine cameras must be done via **Volume Profiles** on the virtual cameras

Upgrade path: In Package Manager, update `com.unity.cinemachine` to **3.x** (the version shipped with Unity 6).

---

## Phase 8 — Post-Processing (HDRP Volume Framework)

The current project has no post-processing. HDRP's Volume system enables significant visual improvements suitable for a space shooter. Add a **Global Volume** to the scene with:

| Effect | Recommended Settings | Purpose |
|---|---|---|
| **Bloom** | Intensity: 0.5, Threshold: 0.9 | Makes emissive projectiles and engine exhausts glow |
| **Tonemapping** | Mode: ACES or Neutral | Converts HDR scene to LDR display |
| **Color Adjustments** | Contrast +10, Saturation +5 | Enhances sci-fi color vibrancy |
| **Ambient Occlusion** | Intensity: 0.6 | Adds depth to cockpit and ship crevices |
| **Depth of Field** | Focus Mode: Physical | Cockpit interior camera depth blur |
| **Chromatic Aberration** | Intensity: 0.1 | Subtle lens distortion for impact effects |
| **Lens Distortion** | Intensity: −0.1 | Subtle barrel distortion for cockpit view |
| **Film Grain** | Intensity: 0.05 | Subtle cinematic grain |
| **Vignette** | Intensity: 0.3 | Cockpit camera framing |

**Per-Camera Volumes:** Use local volumes or `HDAdditionalCameraData.volumeLayerMask` to apply different post-processing per camera (e.g., stronger depth of field on cockpit camera, no vignette on enemy-follow camera).

---

## Phase 9 — HDRP-Specific Visual Enhancements

After the base conversion is stable, these HDRP features will significantly improve the game's visuals:

### 9.1 Volumetric Lighting & Fog
- Add **Fog** override to the Global Volume
- In a space environment, use very low fog density (`Mean Free Path = 10000+`) for subtle nebula-like atmosphere
- Engine exhaust trails and explosion fireballs will interact with volumetric fog

### 9.2 Screen Space Reflections (SSR)
- Enable in the HDRP Pipeline Asset
- Improves reflections on ship hulls and cockpit glass without requiring additional reflection probes

### 9.3 Lens Flares (HDRP)
- Add **HDRP Lens Flare** component to the Directional Light for a star/sun flare
- Add lens flare to engine exhaust lights for a sci-fi engine glow effect

### 9.4 Light Layers
- Use **Rendering Layers** in HDRP to isolate lighting (e.g., shield explosion point lights affect only the player ship, not background asteroids)

### 9.5 Decals (Optional)
- Add explosion scorch marks to asteroids using HDRP Decal Projectors

---

## Phase 10 — Detonator FX System Assessment

The `Detonator` explosion system (`Assets/_project/Scripts/Effects/Detonator/`) is a legacy particle-based system originally written for Unity 3.x. It uses:
- Legacy `ParticleEmitter`-style scripted spawning
- `Camera.main` references for facing direction
- Legacy particle materials

After HDRP conversion, evaluate replacing `Detonator` with one of:
1. **Unity VFX Graph** — GPU-driven particle effects, fully HDRP-compatible, ideal for large explosion counts
2. **Unity Particle System (Shuriken)** with HDRP-compatible materials — simpler, faster to implement
3. **Keep Detonator** with updated materials — lowest effort if visual quality is acceptable after material conversion

For a production-quality result, migrating to VFX Graph is recommended but is out of scope for a minimal HDRP conversion.

---

## Phase 11 — Testing & Validation

### 11.1 Per-Phase Validation
After each phase, verify in the Unity Editor:
- No pink/magenta materials (indicates missing or unConverted shaders)
- No console errors related to render pipeline
- Scene renders in the Game View

### 11.2 Material Visual Comparison
- Enter Play mode and compare each material against pre-conversion screenshots
- Priority check list: Ship hulls, cockpit glass, projectile emission, explosion particles, skybox

### 11.3 Lighting Validation
- Verify Directional Light provides correct scene illumination (not over/underexposed)
- Test ShieldExplosion point light by hitting player ship with enemy fire
- Test Detonator explosion light flash (`DetonatorLight`) by destroying an enemy

### 11.4 Shader Effect Validation
- Trigger a Detonator explosion containing the heat-wave effect to validate the new `HeatDistortHDRP` shader
- Verify distortion effect is visible and does not cause GPU errors

### 11.5 Performance Profiling
- Use **Unity Profiler** and **HDRP Render Pipeline Debugger** (`Window → Rendering → Render Pipeline Debugger`)
- Target: maintain original frame rate at equivalent quality settings
- HDRP has higher minimum GPU cost than BRP; expect ~10–20% baseline overhead on low-end GPUs

---

## Phase 12 — Cleanup

- Remove the now-unused `HeatDistort.shader` (or archive it)
- Delete the `Skybox` components from cameras
- Remove unused Quality Settings tiers (consolidate to Low/Medium/High for HDRP)
- If `BackgroundCamera` is removed, delete its GameObject from the scene
- Update README.md to reflect Unity 6 and HDRP requirements

---

## Risk Summary

| Risk | Impact | Mitigation |
|---|---|---|
| `HeatDistort.shader` uses `GrabPass` — not supported in HDRP | High | Rewrite as HDRP Shader Graph with Distortion output |
| Legacy particle shaders (Particles/Additive etc.) not auto-converted | Medium | Manual shader reassignment; ~10 materials |
| Detonator system uses `Camera.main` and legacy particle API | Medium | Keep as-is initially; `Camera.main` works in HDRP |
| Light intensity units change from arbitrary to physical (Lux/Candela) | Medium | Script-side multiplier; tune visually |
| Cinemachine 2.x → 3.x API breaking changes | Medium | Update `CameraManager.cs` type references |
| `SkyboxSetter.cs` uses `Skybox` component which is ignored in HDRP | Medium | Rewrite to use HDRP Volume + HDRISky |
| HiRez Spaceships HDRP packages target 2019.4/2020.2, not Unity 6 | Low–Medium | Test compatibility; re-convert materials if needed |
| Baked lightmaps may need re-baking after material conversion | Low | Re-bake lightmaps in Phase 11 |

---

## Conversion Checklist

- [ ] **Phase 1** — Create git tag + branch, upgrade to Unity 6
- [ ] **Phase 2** — Install HDRP package, run HDRP Wizard
- [ ] **Phase 3** — Configure HDRP Global Settings and Pipeline Asset quality tiers
- [ ] **Phase 4.1** — Run automatic BRP-to-HDRP material converter
- [ ] **Phase 4.2** — Manually fix ShieldMaterial, weapon projectile, and cockpit glass materials
- [ ] **Phase 4.3** — Manually convert all particle/Detonator materials to HDRP/Unlit
- [ ] **Phase 4.4** — Convert skybox materials to HDRI Sky or Physically Based Sky
- [ ] **Phase 4.5** — Import HiRez Spaceships HDRP upgrade package
- [ ] **Phase 5** — Rewrite `HeatDistort.shader` as HDRP Shader Graph with Distortion
- [ ] **Phase 6** — Adjust Directional Light to Lux, update runtime point light scripts for Candela
- [ ] **Phase 7** — Verify HDAdditionalCameraData on all cameras, update `SkyboxSetter.cs`, update Cinemachine
- [ ] **Phase 8** — Add Global Volume with Bloom, Tonemapping, AO, Color Adjustments
- [ ] **Phase 9** — Enable SSR, Volumetric Fog, Lens Flares
- [ ] **Phase 10** — Evaluate Detonator system; convert materials or replace with VFX Graph
- [ ] **Phase 11** — Visual validation, lighting validation, shader effect validation, performance profiling
- [ ] **Phase 12** — Cleanup, documentation update
