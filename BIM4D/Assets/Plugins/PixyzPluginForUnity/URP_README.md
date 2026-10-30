# Pixyz 2021.1.1.5 — Unity 6.3 URP patch

This is a local rendering compatibility patch for the supplied Pixyz package.
Validated with Unity 6000.3.17f1, Universal RP 17.3.0 and Direct3D 11.

## Install in each project

1. Use a Unity 6.3 project with Universal RP installed and a URP asset assigned
   in Project Settings > Graphics (and any active Quality override).
2. For a new installation, choose Assets > Import Package > Custom Package and
   import `PixyzPluginForUnity-2021.1.1.5-Unity6.3-URP.unitypackage`.
3. If this exact Pixyz 2021.1.1.5 package is already installed, import the smaller
   `PixyzPluginForUnity-2021.1.1.5-Unity6.3-URP-Patch.unitypackage` instead.
   Include all patch assets and allow the listed files to be updated.
4. In Pixyz import settings, leave Override Shader empty, or select
   Universal Render Pipeline/Lit. New imported materials use URP automatically.

Existing CAD materials already saved in a project are not rewritten by this
package. Reimport the CAD source using these settings, or use Unity's Render
Pipeline Converter for existing Standard materials. Existing custom VRED Shader
Graphs need their own compatible URP versions.

## Included changes

- Resolve the active render pipeline, including Quality overrides, and select
  Universal Render Pipeline/Lit for new imports and missing/replacement materials.
- Add a URP material path to both ImportSDK and OptimizeSDK: base colors/maps,
  alpha blending and cutout, depth/shadow state, metallic/smoothness, normal maps,
  occlusion and emission keywords.
- Pack metallic in red and inverted roughness in alpha using linear textures;
  use coefficients correctly when only one channel comes from a texture.
- Combine opacity with textured or color-only albedo without changing shared
  source textures, and preserve tiling/offset.
- Read visible URP BaseColor/BaseMap/Smoothness/Surface inputs when converting
  Unity materials into Pixyz; the hidden legacy aliases no longer take priority.
- Add explicit URP subshaders to Simple Lines, Thick Lines, Splats and Vertex
  Colors, retaining their original Built-in subshaders and shader names/GUIDs.
- Convert the plugin's Checker material to URP Lit.
- Correct two invalid struct serialization attributes rejected by Unity 6.
- Detect the active pipeline correctly in the original baking warnings.

## Validation and limits

The complete plugin compiled in a separate test project. Thirty-seven material,
render-state and shader checks passed, including both import and optimization
conversion paths. A URP render request rendered synthetic CAD-style materials,
lines and points without pink error pixels. The original package and existing
Unity projects were not edited.

The test used synthetic material definitions; a licensed native CAD-file import,
all Toolbox operations, player builds, other graphics APIs and XR were not tested.
The native Pixyz libraries and licensing behavior remain those of the original
2021 package. This is not an official Pixyz upgrade or a guarantee of complete
Unity 6 support.

Pixyz's legacy impostor shader, billboard/impostor baking and Built-in texture
baking were not ported. Their existing Built-in-only limitations remain.
Thick Lines and Splats use geometry shaders and retain the original platform
restrictions. When two maps need different UV transforms, the packed map uses
the selected source mapping and logs the specific mismatch.

Unity 6 may show obsolete API warnings from the remaining 2021 code and asset
reimport warnings from the original native-library postprocessor.

## Reference

[Unity 6.3 custom shader migration guide](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/urp-shaders/birp-urp-custom-shader-upgrade-guide.html)

[URP Lit shader and material properties](https://github.com/Unity-Technologies/Graphics/blob/6000.3/staging/Packages/com.unity.render-pipelines.universal/Shaders/Lit.shader)
