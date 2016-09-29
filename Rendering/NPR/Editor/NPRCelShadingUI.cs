﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MGFX
{

    public class NPRCelShadingUI : ShaderGUIBase
	{
		[MaterialProperty("_MainTex")]
		MaterialProperty m_MainTex;

        [MaterialProperty("_FadeOut")]
        MaterialProperty m_FadeOut;

        [MaterialProperty("_TextureFadeOutOn", "_TEXTURE_FADE_OUT_ON")]
        MaterialProperty m_TextureFadeOutOn;

        [MaterialProperty("_IrradianceOn", "_IRRADIANCE_ON")]
        MaterialProperty m_IrradianceOn;

        [MaterialProperty("_IrradianceBoost")]
        MaterialProperty m_IrradianceBoost;

        [MaterialProperty("_DarkenBackfacesOn", "_DARKEN_BACKFACES_ON")]
        MaterialProperty m_DarkenBackfacesOn;

        [MaterialProperty("_BayerTex")]
        MaterialProperty m_BayerTex;

		[MaterialProperty("_DimOn", "_DIM_ON")]
		MaterialProperty m_DimOn;

		[MaterialProperty("_DimTex")]
		MaterialProperty m_DimTex;

		[MaterialProperty("_NormalMapOn", "_NORMAL_MAP_ON")]
		MaterialProperty m_NormalMapOn;

		[MaterialProperty("_NormalMapTex")]
		MaterialProperty m_NormalMapTex;

		[MaterialProperty("_RimOn", "_RIM_ON")]
		MaterialProperty m_RimOn;

        [MaterialProperty("_RimLUTTex")]
        MaterialProperty m_RimLUTTex;

		[MaterialProperty("_RimIntensity")]
		MaterialProperty m_RimIntensity;

        [MaterialProperty("_MatCapOn", "_MATCAP_ON")]
        MaterialProperty m_MatCapOn;

        [MaterialProperty("_MatCapPlanarOn", "_MATCAP_PLANAR_ON")]
        MaterialProperty m_MatCapPlanarOn;

        [MaterialProperty("_MatCapAlbedoOn", "_MATCAP_ALBEDO_ON")]
        MaterialProperty m_MatCapAlbedoOn;

        [MaterialProperty("_MatCapTex")]
        MaterialProperty m_MatCapTex;

        [MaterialProperty("_MatCapIntensity")]
        MaterialProperty m_MapCapIntensity;

		[MaterialProperty("_OverlayOn", "_OVERLAY_ON")]
		MaterialProperty m_OverlayOn;

		[MaterialProperty("_OverlayTex")]
		MaterialProperty m_OverlayTex;

		[MaterialProperty("_DiffuseLUTOn", "_DIFFUSE_LUT_ON")]
		MaterialProperty m_DiffuseLUTOn;

		[MaterialProperty("_DiffuseLUTTex")]
		MaterialProperty m_DiffuseLUTTex;
		
		public override void OnGUI(MaterialEditor _materialEditor, MaterialProperty[] _properties)
		{
			FindProperties(this, _properties);

			DoGeneral(_materialEditor);
			DoNormalMap(_materialEditor);
			DoOverlay(_materialEditor);
			DoRim(_materialEditor);
            DoMatCap(_materialEditor);
			DoDiffuseLUT(_materialEditor);
		}

		private void DoGeneral(MaterialEditor _materialEditor)
		{
			if (!BeginGroup("General"))
				return;

			_materialEditor.TextureProperty(m_MainTex, "Main Texture (RGB)");

            _materialEditor.TextureProperty(m_BayerTex, "Differ Matrix");

            _materialEditor.ShaderProperty(m_FadeOut, "Fade Out");

            DoKeyword(_materialEditor, m_TextureFadeOutOn, "Fade Out (Tex Alpha)");

            if (DoKeyword(_materialEditor, m_IrradianceOn, "Use Irradiance"))
            {
                _materialEditor.ShaderProperty(m_IrradianceBoost, "Irradiance Boost");
            }

            DoKeyword(_materialEditor, m_DarkenBackfacesOn, "Use Darken Backfaces");

			if (DoKeyword (_materialEditor, m_DimOn, "Use Dim Texture"))
			{
				_materialEditor.TextureProperty (m_DimTex, "Dim Texture (RGB)");
			}

			EndGroup();
		}

		private void DoNormalMap(MaterialEditor _materialEditor)
		{
			if (!BeginGroup("Normal Map"))
				return;
			
			if (DoKeyword(_materialEditor, m_NormalMapOn, "Use Normal Map"))
			{
				_materialEditor.TextureProperty(m_NormalMapTex, "Normal Map");
			}

			EndGroup();
		}

		private void DoOverlay(MaterialEditor _materialEditor)
		{
			if (!BeginGroup("Overlay"))
				return;
			
			if (DoKeyword(_materialEditor, m_OverlayOn, "Use Overlay Texture"))
			{
				_materialEditor.TextureProperty(m_OverlayTex, "Overlay Texture (RGBA)");
			}

			EndGroup();
		}

		private void DoRim(MaterialEditor _materialEditor)
		{
			if (!BeginGroup("Rim"))
				return;
			
			if (DoKeyword(_materialEditor, m_RimOn, "Use Rim"))
			{
                _materialEditor.ShaderProperty(m_RimLUTTex, "Rim LUT (Grayscale)");
				_materialEditor.ShaderProperty(m_RimIntensity, "Rim Intensity");
			}

			EndGroup();
		}

        private void DoMatCap(MaterialEditor _materialEditor)
        {
            if (!BeginGroup("MatCap"))
                return;

            if (DoKeyword(_materialEditor, m_MatCapOn, "Use MatCap"))
            {
                _materialEditor.TextureProperty(m_MatCapTex, "MatCap");
                _materialEditor.ShaderProperty(m_MapCapIntensity, "MatCap Intensity");
                DoKeyword(_materialEditor, m_MatCapPlanarOn, "MatCap Planar");
                DoKeyword(_materialEditor, m_MatCapAlbedoOn, "MatCap Albedo");
            }

            EndGroup();
        }

		private void DoDiffuseLUT(MaterialEditor _materialEditor)
		{
			if (!BeginGroup("Diffuse LUT"))
				return;
			
			if (DoKeyword(_materialEditor, m_DiffuseLUTOn, "Use Diffuse LUT"))
			{
				_materialEditor.TextureProperty(m_DiffuseLUTTex, "Diffuse LUT (Grayscale)");
			}

			EndGroup();
		}
	}
}