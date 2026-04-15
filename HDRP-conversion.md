# HDRP Conversion Guide — 3D Space Shooter Tutorial

## Project Overview

| Property | Value |
|---|---|
| Unity Version | 2022.3.38f1 (LTS) |
| Current Render Pipeline | Built-in Render Pipeline |
| Target Render Pipeline | High Definition Render Pipeline (HDRP) |
| Single Scene | `Assets/_project/Scenes/Main.unity` |

---

## 1. Current Project Analysis

### 1.1 Render Pipeline

The project uses Unity's **Built-in Render Pipeline** with no Scriptable Render Pipeline (SRP) asset assigned (`m_CustomRenderPipeline: {fileID: 0}`). Graphics settings reference built-in deferred and screen-space shadow shaders only.

### 1.2 Installed Packages (relevant to rendering)

| Package | Version | HDRP Impact |
|---|---|---|
| `com.unity.cinemachine` | 2.10.1 | Cinemachine 2.x is incompatible with HDRP volumes; must upgrade to 3.x |
| `com.unity.textmeshpro` | 3.0.6 | Compatible — TMP shaders are SRP-aware |
| `com.unity.timeline` | 1.7.6 | Compatible |
| `com.unity.inputsystem` | 1.7.0 | Compatible |

### 1.3 Materials and Shaders

All materials in the project use **built-in shaders** identified by their built-in fileID. None will work in HDRP without conversion.

#### Material Groups by Shader

| Built-in Shader (fileID) | Shader Name | Affected Materials | HDRP Replacement |
|---|---|---|---|
| `46` | Standard (Metallic) | All ship body parts (cockpits, wings, engines, main bodies), missiles, turret chunks, shield, projectiles, asteroid rocks | `HDRP/Lit` |
| `45` | Diffuse (Legacy) | `Sci-fi turrets/Models/Turrets.mat` | `HDRP/Lit` |
| `104` | Skybox/6-Sided | `Purple_2K_Resolution.mat`, `Green_2K_Resolution.mat`, `Pink_2K_Resolution.mat` | HDRP Visual Environment component (HDRI Sky or cubemap sky) |
| `200` | Particles/Additive (Legacy) | Detonator fireballs (FireballA/B/C), shockwave, glow, sparks | `HDRP/Unlit` or VFX Graph |
| `203` | Particles/Alpha Blended (Legacy) | Detonator smoke materials (SmokeANew, SmokeBNew, SmokeMIx) | `HDRP/Unlit` |
| `202` | Particles/VertexLit Blended (Legacy) | `Missile/Materials/smoke_material.mat` | `HDRP/Unlit` |
| Custom | `HeatDistort` (GrabPass) | Detonator heat wave effect | Custom HDRP shader (see §4.3) |

**Key texture features in use:**
- Normal maps (`_NORMALMAP`) — directly supported by `HDRP/Lit`
- Metallic/gloss maps (`_METALLICGLOSSMAP`) — supported; channel packing may need adjustment (HDRP uses a mask map: R=Metallic, G=AO, B=Detail, A=Smoothness)
- Emission (`_EMISSION`) — supported by `HDRP/Lit`; must enable HDR emission and Bloom post-process
- Alpha premultiply / transparency — supported with `HDRP/Lit` surface type set to Transparent

### 1.4 Lighting

| Light | Type | Details |
|---|---|---|
| Directional Light | Directional | Warm white (1, 0.957, 0.839), intensity 1, soft shadows enabled |
| Point lights | Point | Used for shield explosions and projectile glow (`ShieldExplosion.cs` drives range/intensity dynamically) |
| Spot lights | Spot | Various scene accents |
| Ambient | Skybox mode | Set to skybox-based ambient (`m_AmbientMode: 0`) |
| Reflection probes | Realtime | `m_ReflectionProbeUsage: 1` on ship meshes |
| Baked lightmaps | Enabled | `m_EnableBakedLightmaps: 1` (no lightmap data baked yet) |
| Fog | Disabled | `m_Fog: 0` |

HDRP uses physical light units (Lux for directional, Lumen/Candela for punctual lights) by default. Existing intensity values will need re-tuning.

### 1.5 Cameras

Three Cinemachine Virtual Cameras managed by `CameraManager.cs`:

- `CockpitCamera` — first-person cockpit view
- `FollowCamera` — third-person chase view
- `EnemyFollowCamera` — spectator follow view

Plus a standard `BackgroundCamera` for parallax/background rendering.

No post-processing volumes or camera stacks exist; all visual enhancements must be added from scratch.

### 1.6 Particle Systems & VFX

The project uses legacy **Detonator** explosion components (C# MonoBehaviours driving `ParticleSystem`) and several standalone particle prefabs:

- `Detonator-Insanity.prefab`, `Detonator-Ignitor.prefab`, `Detonator-Tiny.prefab`
- `EngineDamagePS.prefab`, `SpaceDustParticles.prefab`, `SmokeTrail.prefab`, `Burninating.prefab`
- `ShieldExplosion.prefab`

All particle materials use legacy particle shaders (Additive, Alpha Blended) which are incompatible with HDRP.

### 1.7 Skyboxes

Three 6-sided cubemap skyboxes (`Skybox/6 Sided` shader) — Purple, Green, Pink space scenes. HDRP does not support the `Skybox/6 Sided` shader. The skyboxes need to be recreated as HDRI or Procedural skies.

### 1.8 Custom Shader: HeatDistort

The `HeatDistort.shader` (used by `DetonatorHeatwave`) uses `GrabPass` — a Built-in RP mechanism to capture the framebuffer. **GrabPass does not exist in HDRP.** This is the most technically complex item to convert.

---

## 2. Pre-Conversion Checklist

Before starting:

- [ ] Back up the entire project (Git branch or archive)
- [ ] Record all current material property values and texture assignments
- [ ] Take screenshots of the running game for visual reference
- [ ] Note current light intensity values for re-tuning
- [ ] Upgrade Cinemachine from 2.x → 3.x (breaking API changes; see §4.5)

---

## 3. Step-by-Step Conversion

### Step 1: Add the HDRP Package

1. Open **Window → Package Manager**.
2. Click **+ → Add package by name** and enter `com.unity.render-pipelines.high-definition`.
3. Install version **14.x** (the 2022 LTS-compatible HDRP version).
4. Unity will prompt to import HDRP default resources — accept.

### Step 2: Create the HDRP Asset

1. In the Project window, right-click → **Create → Rendering → HDRP Asset**.
2. Name it `HDRPAsset`.
3. Open **Edit → Project Settings → Graphics** and assign `HDRPAsset` to **Scriptable Render Pipeline Settings**.
4. Open **Edit → Project Settings → Quality** and also assign `HDRPAsset` to each quality level.

### Step 3: Create an HDRP Default Frame Settings Asset (optional but recommended)

Use **Edit → Project Settings → HDRP Default Settings** to configure shadow resolution, MSAA, post-processing features, and reflection probes globally.

### Step 4: Run the HDRP Material Upgrade Wizard

1. Open **Edit → Rendering → Materials → Convert All Built-in Materials to HDRP**.
2. This tool automatically migrates Standard → `HDRP/Lit` and rewires texture slots.

> **Important caveats:**
> - The wizard does **not** handle particle shader materials (Additive, Alpha Blended, VertexLit) — convert those manually.
> - The `Turrets.mat` using the legacy Diffuse shader should be verified after conversion.
> - Transparency materials (shield, glass cockpits) need manual adjustment of Surface Type and Blending Mode in HDRP/Lit.

### Step 5: Fix Metallic/Gloss Map Channel Packing

Standard shader uses:
- `_MetallicGlossMap`: R = Metallic, A = Smoothness

HDRP/Lit uses a **Mask Map**:
- R = Metallic, G = Ambient Occlusion, B = Detail Mask, A = Smoothness

For each material that had `_METALLICGLOSSMAP` enabled (all ship body, cockpit, engine, wing, and blaster materials), create a new Mask Map texture by channel-packing the existing metallic/gloss textures. This can be done with a tool or in a photo editor.

Affected materials:
- All `MainBodies/`, `Cockpits/`, `Engines/`, `Wings/`, `Materials/Weapons/Blaster.mat`, `BigLauncher.mat`

### Step 6: Fix Skyboxes

1. In **Edit → Project Settings → HDRP Default Settings**, configure the **Sky** tab.
2. Add an **HDRI Sky** or **Gradient Sky** component to a **Volume** in the scene (see §3.8).
3. Convert each of the three 6-sided skybox textures to an HDRI Cubemap:
   - Import the six faces as a **Cubemap** texture (set Texture Shape = Cube in the texture importer).
   - Assign to the **HDRI Sky → HDRI Cubemap** field in the Volume.
4. The `SkyboxSetter.cs` script assigns `Material` objects to a `Skybox` component — rewrite it to use HDRP Volume overrides to swap `HDRISky` profiles at runtime instead.

### Step 7: Set Up the HDRP Scene Volume

HDRP uses a **Volume** system for all environment and post-processing settings.

1. Create a new empty GameObject named `Global Volume`.
2. Add a **Volume** component, set **Is Global = true**, and create a new **Volume Profile**.
3. In the profile, add and configure:

| Override | Purpose | Recommended Settings |
|---|---|---|
| **Visual Environment** | Sky type | HDRI Sky (or Gradient Sky) |
| **HDRI Sky** | Space skybox | Assign converted cubemap; Exposure ~0 |
| **Ambient Occlusion** | Ground-truth AO | Intensity 0.5–1.0 |
| **Bloom** | Glow on emissive materials/projectiles | Threshold 0.9, Intensity 0.5 |
| **Color Grading** | Overall look | White balance, contrast |
| **Exposure** | Physical camera exposure | Mode: Fixed, EV 10–13 for space |
| **Vignette** | Cockpit feel | Optional |
| **Fog** | Space atmosphere | Leave disabled (matches current) |

### Step 8: Configure Lights for HDRP

HDRP lights use physical units. The existing lights will default to incorrect intensities.

| Light | Built-in Setting | HDRP Equivalent |
|---|---|---|
| Directional Light (sun) | Intensity 1 | ~100,000 Lux (or use EV compensation) |
| Point lights (shield, projectile glow) | Range/Intensity driven by scripts | Candela values; update `ShieldExplosion.cs` |

1. Select the **Directional Light** → in the Light component, set **Mode = Realtime**, **Intensity** to ~80,000 Lux (tune with Exposure override).
2. Enable **Contact Shadows** and **Screen Space Shadows** in the HDRP Asset for higher quality.
3. Point lights driven by `ShieldExplosion.cs` will still work; only units change. Adjust `_pointLight.intensity` and `_pointLight.range` multiplier values in the script.

### Step 9: Fix Particle System Materials

Manually replace shader references for all particle materials:

| Legacy Material | Legacy Shader | HDRP Replacement |
|---|---|---|
| `FireballA.mat`, `FireballB.mat`, `FireballC.mat` | Particles/Additive | `HDRP/Unlit`, Surface Type: Transparent, Blending: Additive |
| `ShockWave.mat`, `Glow.mat`, `Sparks.mat` | Particles/Additive | `HDRP/Unlit`, Transparent, Additive |
| `SmokeANew.mat`, `SmokeBNew.mat`, `SmokeMIx.mat` | Particles/Alpha Blended | `HDRP/Unlit`, Transparent, Alpha |
| `smoke_material.mat` (missile) | Particles/VertexLit Blended | `HDRP/Unlit`, Transparent, Alpha |

For each material:
1. Change the shader to `HDRP/Unlit`.
2. Set **Surface Type → Transparent**.
3. Set **Blending Mode → Additive** (fire/glow/sparks) or **Alpha** (smoke).
4. Re-assign the existing texture to `_BaseColorMap`.
5. Enable **Use Soft Particle** (requires Depth Pyramid in HDRP settings).

### Step 10: Fix the HeatDistort (GrabPass) Shader

The `HeatDistort.shader` uses `GrabPass` which is not available in HDRP. Two options:

**Option A — Use HDRP Distortion (Recommended)**

HDRP/Lit and HDRP/Unlit support a built-in **Distortion** pass:
1. Create a new material using `HDRP/Unlit`.
2. Enable **Distortion** and **Distortion Blur** in the material's surface options.
3. Assign the normal map to the **Distortion Vector Map** field.
4. Control intensity via the **Distortion Scale** and **Distortion Blur Scale** properties.
5. In `DetonatorHeatwave.cs`, replace `Shader.Find("HeatDistort")` with `Shader.Find("HDRP/Unlit")` and update material property setters to use `_DistortionScale` instead of `_BumpAmt`.

**Option B — Custom Fullscreen Shader**

Write a custom HDRP shader using `SHADERGRAPH` with a **Custom Pass** Volume component for screen-space distortion. This is more powerful but significantly more complex.

### Step 11: Upgrade Cinemachine (2.x → 3.x)

Cinemachine 3.x is required for full HDRP compatibility (Volume-based post-processing per-camera):

1. In **Package Manager**, upgrade `com.unity.cinemachine` to 3.x.
2. Cinemachine 3.x has breaking API changes — `CinemachineVirtualCamera` is replaced by `CinemachineCamera`.
3. Update `CameraManager.cs`:
   - Change the `List<CinemachineVirtualCamera>` field to `List<CinemachineCamera>`.
   - The `gameObject.SetActive(true/false)` priority-switching pattern remains the same.

### Step 12: Configure the HDRP Asset

Open the **HDRP Asset** (`HDRPAsset`) and verify the following for a space-shooter project:

| Setting | Recommendation |
|---|---|
| **Rendering → Lit Shader Mode** | Both (Deferred for opaque, Forward for transparency) |
| **Shadows → Max Shadow Distance** | 500–1000 m |
| **Shadows → Shadow Cascades** | 4 cascades |
| **Reflections → Screen Space Reflections** | Enable |
| **Ambient Occlusion** | Enable |
| **Post-Processing** | Enable |
| **Decals** | Can disable (unused) |
| **Lens Flares** | Enable (for thrusters/projectile glow) |

---

## 4. Technical Details by System

### 4.1 Ship Materials (Standard → HDRP/Lit)

The automatic upgrade wizard handles most of this. Manually verify after conversion:

- **Emission**: HDRP/Lit requires toggling **Emission** on and choosing a color in HDR. Multiplied emissive values power the Bloom post-process.
- **Transparency (glass cockpits, shield)**: Set Surface Type = Transparent. For the shield (`ShieldMaterial.mat`), use Blending Mode = Premultiply to match the original `_ALPHAPREMULTIPLY_ON` behavior.
- **Normals**: Normal intensity is driven by `_NormalScale` in HDRP/Lit (equivalent to `_BumpScale`).
- **Smoothness**: HDRP uses `_Smoothness` globally or reads from Mask Map alpha. Verify the `_GlossMapScale` value is carried over.

### 4.2 Asteroid Materials

All six asteroid rock materials use `_NORMALMAP` and `_PARALLAXMAP`. HDRP/Lit supports parallax occlusion mapping via the **Height Map** slot. Re-assign the parallax texture to **Height Map** and set the amplitude.

### 4.3 HeatDistort Shader Migration Detail

Current shader code uses:
```hlsl
GrabPass { Name "BASE" Tags { "LightMode" = "Always" } }
sampler2D _GrabTexture;
half4 col = tex2D( _GrabTexture, uv );
```

HDRP replacement using HDRP/Unlit Distortion:
```
Material Properties:
  _DistortionEnable = 1
  _DistortionOnly = 0
  _DistortionBlurEnable = 1
  _DistortionVectorMap = [normal map texture]
  _DistortionScale = <mapped from _BumpAmt / 128>
  _DistortionBlurScale = 1
```

Update `DetonatorHeatwave.cs` line:
```csharp
// Before
_material = new Material(Shader.Find("HeatDistort"));
_heatwave.GetComponent<Renderer>().material.SetFloat("_BumpAmt", ((1-_normalizedTime) * distortion));

// After
_material = new Material(Shader.Find("HDRP/Unlit"));
_material.SetFloat("_DistortionScale", (1 - _normalizedTime) * distortion * 0.015f);
_material.SetFloat("_DistortionBlurScale", 1f);
```

### 4.4 Skybox Migration Detail

`SkyboxSetter.cs` currently assigns `Material` objects to a `Skybox` component:
```csharp
_skybox.material = _skyboxMaterials[skyBox];
```

In HDRP, rewrite to swap sky profiles via a local Volume:
```csharp
// Add [SerializeField] List<VolumeProfile> _skyProfiles;
// Add Volume _localVolume;
_localVolume.profile = _skyProfiles[skyBox];
```

Each profile contains an **HDRI Sky** override pointing to a different converted cubemap.

### 4.5 Cinemachine Migration Detail

`CameraManager.cs` changes required:

```csharp
// Before (Cinemachine 2.x)
using Cinemachine;
List<CinemachineVirtualCamera> _virtualCameras;

// After (Cinemachine 3.x)
using Unity.Cinemachine;
List<CinemachineCamera> _virtualCameras;
```

The `gameObject.SetActive(true/false)` priority approach works the same way in Cinemachine 3.x.

### 4.6 ShieldExplosion Light Intensity

`ShieldExplosion.cs` decrements `_pointLight.intensity` and `_pointLight.range` each frame. In HDRP, intensity is in Candela (much higher values). Update the script constants:

```csharp
// Before (Built-in, arbitrary intensity scale)
_pointLight.intensity -= 1 * Time.deltaTime;

// After (HDRP Candela — tune to match visual)
_pointLight.intensity -= 500 * Time.deltaTime;
```

Enable **Physical Light Units** toggle on the Light component so that Unity displays the value in Candela.

---

## 5. Post-Processing Opportunities

Since the project has no existing post-processing, the HDRP conversion is a good opportunity to add visual enhancements that suit a space shooter:

| Effect | Benefit | Priority |
|---|---|---|
| **Bloom** | Makes emissive projectiles and engine thrusters glow | High |
| **Lens Flare (SRP)** | Star/sun flare effect in space | Medium |
| **Chromatic Aberration** | Hit-impact camera feedback | Low |
| **Vignette** | Cockpit immersion | Low |
| **Motion Blur** | Ship speed feel | Medium |
| **Film Grain** | Sci-fi aesthetic | Optional |
| **Color Grading** | Overall colour tone | High |
| **Ambient Occlusion (GTAO)** | Ship crevice shading | Medium |

---

## 6. Known Incompatibilities Summary

| Issue | Severity | Action Required |
|---|---|---|
| All Standard/Legacy materials | Critical | Run upgrade wizard + manual fixes |
| `Skybox/6 Sided` shader | Critical | Convert cubemaps, use HDRI Sky Volume |
| `HeatDistort` GrabPass shader | High | Rewrite using HDRP Distortion (§4.3) |
| Legacy particle shaders (Additive, Alpha Blended) | High | Manual conversion to HDRP/Unlit |
| Cinemachine 2.x | Medium | Upgrade to 3.x, update `CameraManager.cs` |
| Physical light units (intensity scale) | Medium | Re-tune all light intensities |
| `ShieldExplosion.cs` light values | Low | Update intensity decrement constants |
| `SkyboxSetter.cs` material-based swap | Low | Rewrite to use Volume profiles |
| Baked lightmaps | Low | Re-bake after materials are converted |
| `SystemInfo.supportsImageEffects` check | Low | Always returns `true` in HDRP; review if guard is still needed |

---

## 7. Recommended Conversion Order

1. **Add HDRP package and create HDRP Asset** — the scene will turn pink/broken; that is expected.
2. **Run the automatic material upgrade wizard** — resolves the majority of Standard materials.
3. **Manually fix particle materials** (Additive/Alpha Blended → HDRP/Unlit).
4. **Fix the HeatDistort shader** (GrabPass → HDRP Distortion).
5. **Convert skyboxes** and set up the Global Volume with Visual Environment.
6. **Re-tune lighting** (physical units, directional intensity, point light candela).
7. **Upgrade Cinemachine** and update `CameraManager.cs`.
8. **Configure post-processing** in the Volume Profile (Bloom, Color Grading, Exposure).
9. **Fix skybox switcher** (`SkyboxSetter.cs`) to use Volume profiles.
10. **Re-bake lightmaps** if needed.
11. **Visual QA** — compare against reference screenshots, iterate.

---

## 8. References

- [Unity HDRP Documentation (2022 LTS)](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/index.html)
- [Unity — Upgrading from Built-in to HDRP](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Upgrading-To-HDRP.html)
- [HDRP Material Conversion Guide](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Material-Conversion.html)
- [HDRP Lit Shader Reference](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Lit-Shader.html)
- [HDRP Unlit Shader Reference](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Unlit-Shader.html)
- [HDRP Distortion](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Distortion.html)
- [Cinemachine 3 Migration Guide](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.0/manual/CinemachineUpgradeFrom2.html)
- [HDRP Physical Light Units](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Physical-Light-Units.html)
- [HDRP HDRI Sky](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Override-HDRI-Sky.html)
