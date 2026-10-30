using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pixyz.Commons.Utilities
{
	public static class ShaderUtilities
	{
		public static Shader GetPixyzLineShader()
		{
			Shader shader = Shader.Find("Pixyz/Simple Lines");
			if (shader != null)
				return shader;

			return GetDefaultShader();
		}
		public static Shader GetPixyzSplatsShader()
		{
			Shader shader = Shader.Find("Pixyz/Splats");
			if (shader != null)
				return shader;

			return GetDefaultShader();
		}

        public static RenderPipelineAsset GetActiveRenderPipeline()
        {
            // currentRenderPipeline accounts for the active Quality override.
            return GraphicsSettings.currentRenderPipeline;
        }

        public static bool IsUniversalRenderPipeline()
        {
            var pipeline = GetActiveRenderPipeline();
            if (pipeline == null) return false;
            for (var type = pipeline.GetType(); type != null; type = type.BaseType)
                if (type.FullName == "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset")
                    return true;
            return false;
        }

        public static bool IsUniversalLitMaterial(UnityEngine.Material material)
        {
            return material != null && material.shader != null &&
                material.shader.name == "Universal Render Pipeline/Lit";
        }

        public static Shader GetDefaultShader()
        {
            if (IsUniversalRenderPipeline())
            {
                var lit = Shader.Find("Universal Render Pipeline/Lit");
                if (lit != null) return lit;
                throw new System.InvalidOperationException(
                    "Pixyz: URP Lit shader is missing. Install Universal RP in Package Manager.");
            }
            var pipeline = GetActiveRenderPipeline();
            var shader = pipeline != null ? pipeline.defaultShader : null;
            return shader != null ? shader : Shader.Find("Standard");
        }


		private static Dictionary<string, string> VredToShaderGraph = new Dictionary<string, string>()
		{
			{ "UMetallicPaintMaterial", "Shader Graphs/CarPaint" },
			{ "UBrushedMetalMaterial", "Shader Graphs/Metal" },
			{ "UEnvironmentSwitchMaterial", "" },
			{ "USphereEnvironmentMaterial", "" },
			{ "UCarbonMaterial", "Shader Graphs/Metal" },
			{ "UCarbonMaterial2D", "Shader Graphs/CarbonFiber" },
			{ "UChromeMaterial", "Shader Graphs/Metal" },
			{ "UFlipflopMaterial", "Shader Graphs/CarPaint" },
			{ "UGlassMaterial", "Shader Graphs/GlassSimple" },
			{ "UPhongMaterial", "Shader Graphs/CarPaint" },
			{ "UPlasticMaterial", "Shader Graphs/PlasticTransparentSimple" },
			{ "UReflectivePlasticMaterial", "Shader Graphs/PlasticNormal" },
			{ "UTireMaterial", "Shader Graphs/Rubber" },
			{ "UUnicolorPaintMaterial", "Shader Graphs/CarPaint" },
			{ "UVelvetMaterial", "Shader Graphs/FabricSuede" },
			{ "UWovenClothMaterial", "Shader Graphs/FabricSuede" },
			{ "UTriplanarMaterial", "Shader Graphs/FabricSuede" },
			{ "UShadowMaterial", "" },
		};
		private static Dictionary<string, Dictionary<string, string>> VredPropToKeyword = new Dictionary<string, Dictionary<string, string>>()
		{
			{  "Shader Graphs/CarPaint", new Dictionary<string, string>()
			{
				{ "baseColor", "Color_F141EB95" },
				{ "flakeColor", "Color_CF7B3627" }
			}
			},
			{  "Shader Graphs/Rubber", new Dictionary<string, string>()
			{
				{ "ambienceColor", "Color_9A170B2D" },
			}
			},
			{  "Shader Graphs/PlasticTransparentSimple", new Dictionary<string, string>()
			{
				{ "ambienceColor", "Color_38879BCF" },
				{ "seeThrough", "Color_38879BCF_alpha" }
			}
			},
			{  "Shader Graphs/GlassSimple", new Dictionary<string, string>()
			{
				{ "ambienceColor", "Color_DCF1FFF3" },
			}
			},
			{  "Shader Graphs/CarbonFiber", new Dictionary<string, string>()
			{
				{ "ambienceColor", "Color_DCF1FFF3" },
			}
			},
			{  "Shader Graphs/Metal", new Dictionary<string, string>()
			{
				{ "ambienceColor", "Color_AD8F67CE" },
			}
			},
			{  "Shader Graphs/FabricSuede", new Dictionary<string, string>()
			{
				{ "ambienceColor", "Color_4F4ED3A9" },
				{ "diffuseComponent_image", "Texture2D_D9FF89B8" }
			}
			},
			{  "Shader Graphs/Leather", new Dictionary<string, string>()
			{
				{ "ambienceColor", "Color_7A8D5E5E" },
				{ "diffuseComponent_image", "Texture2D_4D3C9E50" }
			}
			}
		};
		public static Shader GetCustomShader(string name)
		{
			Shader shader = null;
			if (VredToShaderGraph.ContainsKey(name))
			{
				shader = Shader.Find(VredToShaderGraph[name]);
			}
			return shader;
		}

		public static string GetShaderKeyword(string name, string property)
		{
			VredPropToKeyword.TryGetValue(name, out Dictionary<string, string> maps);
			if (maps != null)
			{
				maps.TryGetValue(property, out string keyword);
				return keyword;
			}
			return null;
		}
	}
}