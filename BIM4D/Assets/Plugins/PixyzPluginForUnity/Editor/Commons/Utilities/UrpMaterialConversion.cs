using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Native = Pixyz.Material.Native;

namespace Pixyz.Commons.Utilities
{
    // No URP assembly dependency: this compiles before URP is installed.
    public static class UrpMaterialConversion
    {
        public delegate Texture2D TextureResolver(Native.Texture texture, ref Dictionary<uint, Texture2D> map);

        private static void Keyword(UnityEngine.Material material, string keyword, bool enabled)
        {
            if (enabled) material.EnableKeyword(keyword);
            else material.DisableKeyword(keyword);
        }

        public static void ConfigureSurface(UnityEngine.Material material, bool transparent, bool cutout)
        {
            material.SetFloat("_Surface", transparent ? 1 : 0);
            material.SetFloat("_Blend", 0);
            material.SetFloat("_BlendModePreserveSpecular", 0);
            material.SetFloat("_AlphaClip", cutout ? 1 : 0);
            material.SetFloat("_Cutoff", 0.5f);
            material.SetFloat("_AlphaToMask", cutout && !transparent ? 1 : 0);
            material.SetInt("_SrcBlend", (int)(transparent ? BlendMode.SrcAlpha : BlendMode.One));
            material.SetInt("_DstBlend", (int)(transparent ? BlendMode.OneMinusSrcAlpha : BlendMode.Zero));
            material.SetInt("_SrcBlendAlpha", (int)BlendMode.One);
            material.SetInt("_DstBlendAlpha", (int)(transparent ? BlendMode.OneMinusSrcAlpha : BlendMode.Zero));
            material.SetInt("_ZWrite", transparent ? 0 : 1);
            material.SetFloat("_QueueOffset", 0);
            material.SetOverrideTag("RenderType", transparent ? "Transparent" : cutout ? "TransparentCutout" : "Opaque");
            material.renderQueue = (int)(transparent ? RenderQueue.Transparent : cutout ? RenderQueue.AlphaTest : RenderQueue.Geometry);
            Keyword(material, "_SURFACE_TYPE_TRANSPARENT", transparent);
            Keyword(material, "_ALPHATEST_ON", cutout);
            material.DisableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.DisableKeyword("_ALPHAMODULATE_ON");
            material.SetShaderPassEnabled("ShadowCaster", !transparent);
            material.SetShaderPassEnabled("DepthOnly", !transparent);
        }

        private static void SetTexture(UnityEngine.Material material, string property, Texture2D image, Native.Texture mapping)
        {
            material.SetTexture(property, image);
            if (mapping == null) return;
            material.SetTextureScale(property, new Vector2((float)mapping.tilling.x, (float)mapping.tilling.y));
            material.SetTextureOffset(property, new Vector2((float)mapping.offset.x, (float)mapping.offset.y));
        }

        private static Color Sample(Texture2D image, int x, int y, int width, int height, Color fallback)
        {
            if (image == null) return fallback;
            if (image.width == width && image.height == height) return image.GetPixel(x, y);
            return image.GetPixelBilinear((x + 0.5f) / width, (y + 0.5f) / height);
        }

        private static bool SameMapping(Native.Texture a, Native.Texture b)
        {
            return a == null || b == null ||
                (a.offset.x == b.offset.x && a.offset.y == b.offset.y &&
                 a.tilling.x == b.tilling.x && a.tilling.y == b.tilling.y);
        }

        public static void Apply(Native.MaterialDefinition source, UnityEngine.Material material,
            ref Dictionary<uint, Texture2D> textures, TextureResolver resolve)
        {
            material.name = source.name;
            material.SetFloat("_WorkflowMode", 1);
            material.DisableKeyword("_SPECULAR_SETUP");
            material.DisableKeyword("_METALLICGLOSSMAP");
            material.DisableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
            material.SetFloat("_SmoothnessTextureChannel", 0);

            float alpha = source.opacity._type == Native.CoeffOrTexture.Type.COEFF
                ? Mathf.Clamp01((float)source.opacity.coeff) : 1;
            bool cutout = source.opacity._type == Native.CoeffOrTexture.Type.TEXTURE;
            ConfigureSurface(material, !cutout && alpha < 1, cutout);
            Color color = source.albedo._type == Native.ColorOrTexture.Type.COLOR
                ? new Color((float)source.albedo.color.r, (float)source.albedo.color.g, (float)source.albedo.color.b, alpha)
                : new Color(1, 1, 1, alpha);
            material.SetColor("_BaseColor", color);
            material.SetColor("_Color", color);

            Native.Texture albedoMapping = source.albedo._type == Native.ColorOrTexture.Type.TEXTURE ? source.albedo.texture : null;
            Texture2D albedo = albedoMapping != null ? resolve(albedoMapping, ref textures) : null;
            if (cutout)
            {
                var opacity = resolve(source.opacity.texture, ref textures);
                if (opacity != null)
                {
                    if (!SameMapping(albedoMapping, source.opacity.texture))
                        Debug.LogWarning("Pixyz URP: albedo and opacity use different UV transforms on " + source.name + ". The packed map uses the albedo transform.");
                    int width = albedo != null ? albedo.width : opacity.width;
                    int height = albedo != null ? albedo.height : opacity.height;
                    var packed = new Texture2D(width, height, TextureFormat.RGBA32, false);
                    packed.name = source.name + "_BaseOpacity";
                    var pixels = new Color[width * height];
                    for (int y = 0; y < height; y++)
                        for (int x = 0; x < width; x++)
                        {
                            Color pixel = Sample(albedo, x, y, width, height, Color.white);
                            pixel.a = Sample(opacity, x, y, width, height, Color.white).r;
                            pixels[y * width + x] = pixel;
                        }
                    packed.SetPixels(pixels);
                    packed.Apply();
                    albedo = packed;
                    if (albedoMapping == null) albedoMapping = source.opacity.texture;
                }
            }
            SetTexture(material, "_BaseMap", albedo, albedoMapping);
            SetTexture(material, "_MainTex", albedo, albedoMapping);

            Texture2D normal = source.normal._type == Native.ColorOrTexture.Type.TEXTURE
                ? resolve(source.normal.texture, ref textures) : null;
            SetTexture(material, "_BumpMap", normal, source.normal.texture);
            Keyword(material, "_NORMALMAP", normal != null);

            float metallic = source.metallic._type == Native.CoeffOrTexture.Type.COEFF ? Mathf.Clamp01((float)source.metallic.coeff) : 1;
            float smoothness = source.roughness._type == Native.CoeffOrTexture.Type.COEFF ? 1 - Mathf.Clamp01((float)source.roughness.coeff) : 1;
            Texture2D metalMap = source.metallic._type == Native.CoeffOrTexture.Type.TEXTURE ? resolve(source.metallic.texture, ref textures) : null;
            Texture2D roughMap = source.roughness._type == Native.CoeffOrTexture.Type.TEXTURE ? resolve(source.roughness.texture, ref textures) : null;
            Texture2D metalSmooth = null;
            Native.Texture metalMapping = roughMap != null ? source.roughness.texture : source.metallic.texture;
            if (metalMap != null || roughMap != null)
            {
                if (!SameMapping(source.metallic.texture, source.roughness.texture))
                    Debug.LogWarning("Pixyz URP: metallic and roughness use different UV transforms on " + source.name + ". The packed map uses the roughness transform.");
                Texture2D sizeSource = roughMap != null ? roughMap : metalMap;
                int width = sizeSource.width, height = sizeSource.height;
                metalSmooth = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
                metalSmooth.name = source.name + "_MetallicSmoothness";
                var pixels = new Color[width * height];
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        float red = Sample(metalMap, x, y, width, height, new Color(metallic, 0, 0, 1)).r;
                        float gloss = roughMap != null ? 1 - Sample(roughMap, x, y, width, height, Color.black).r : smoothness;
                        pixels[y * width + x] = new Color(red, 0, 0, gloss);
                    }
                metalSmooth.SetPixels(pixels);
                metalSmooth.Apply();
                smoothness = 1;
            }
            SetTexture(material, "_MetallicGlossMap", metalSmooth, metalMapping);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Glossiness", smoothness);
            Keyword(material, "_METALLICSPECGLOSSMAP", metalSmooth != null);

            Texture2D occlusion = source.ao._type == Native.CoeffOrTexture.Type.TEXTURE ? resolve(source.ao.texture, ref textures) : null;
            SetTexture(material, "_OcclusionMap", occlusion, source.ao.texture);
            material.SetFloat("_OcclusionStrength", occlusion != null ? 1 : Mathf.Clamp01((float)source.ao.coeff));
            Keyword(material, "_OCCLUSIONMAP", occlusion != null);

            Texture2D emission = source.emissive._type == Native.ColorOrTexture.Type.TEXTURE ? resolve(source.emissive.texture, ref textures) : null;
            Color emissionColor = emission != null ? Color.white :
                new Color((float)source.emissive.color.r, (float)source.emissive.color.g, (float)source.emissive.color.b, 1);
            SetTexture(material, "_EmissionMap", emission, source.emissive.texture);
            material.SetColor("_EmissionColor", emissionColor);
            bool emissive = emission != null || emissionColor.maxColorComponent > 0;
            Keyword(material, "_EMISSION", emissive);
            material.globalIlluminationFlags = emissive ? MaterialGlobalIlluminationFlags.BakedEmissive : MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }
    }
}
