//
// Kino/Obscurance - SSAO (screen-space ambient obscurance) effect for Unity
//
// Copyright (C) 2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using UnityEngine;
using UnityEngine.Rendering;

namespace Mud
{
	[ExecuteInEditMode]
	[AddComponentMenu("Mud/Rendering/Features/SSAO")]
	public class RenderFeatureSSAO : RenderFeatureBase
	{
		#region Material Identifiers

		public static HashID MTL_SSAO = new HashID("Hidden/Mud/SSAO");
		
		#endregion

		#region Public Properties

		/// Degree of darkness produced by the effect.
		public float intensity
		{
			get { return _intensity; }
			set { _intensity = value; }
		}

		[SerializeField, Range(0, 4), Tooltip(
			"Degree of darkness produced by the effect.")]
		float _intensity = 1;

		/// Radius of sample points, which affects extent of darkened areas.
		public float radius
		{
			get { return Mathf.Max(_radius, 1e-4f); }
			set { _radius = value; }
		}

		[SerializeField, Tooltip(
			"Radius of sample points, which affects extent of darkened areas.")]
		float _radius = 0.3f;

		/// Degree of darkness produced by the effect.
		public float sharpness
		{
			get { return _sharpness; }
			set { _sharpness = value; }
		}

		[SerializeField, Range(0, 1), Tooltip(
			"Degree of sharpness produced by the effect.")]
		float _sharpness = 0;

		/// Number of sample points, which affects quality and performance.
		public SampleCount sampleCount
		{
			get { return _sampleCount; }
			set { _sampleCount = value; }
		}

		public enum SampleCount { Lowest, Low, Medium, High, Variable }

		[SerializeField, Tooltip(
			"Number of sample points, which affects quality and performance.")]
		SampleCount _sampleCount = SampleCount.Medium;

		/// Determines the sample count when SampleCount.Variable is used.
		/// In other cases, it returns the preset value of the current setting.
		public int sampleCountValue
		{
			get
			{
				switch (_sampleCount)
				{
					case SampleCount.Lowest: return 3;
					case SampleCount.Low: return 6;
					case SampleCount.Medium: return 12;
					case SampleCount.High: return 20;
				}
				return Mathf.Clamp(_sampleCountValue, 1, 256);
			}
			set { _sampleCountValue = value; }
		}

		[SerializeField]
		int _sampleCountValue = 24;

		/// Number of iterations of blur filter.
		public int blurIterations
		{
			get { return _blurIterations; }
			set { _blurIterations = value; }
		}

		[SerializeField, Range(0, 4), Tooltip(
			"Number of iterations of the blur filter.")]
		int _blurIterations = 2;

		/// Halves the resolution of the effect to increase performance.
		public bool downsampling
		{
			get { return _downsampling; }
			set { _downsampling = value; }
		}

		[SerializeField, Tooltip(
			"Halves the resolution of the effect to increase performance.")]
		bool _downsampling = false;

		
		#endregion

		#region Private Properties

		// AO shader material
		Material aoMaterial
		{
			get
			{
				return GetMaterial(MTL_SSAO);
			}
		}

		#endregion

		#region Effect Passes

		// Build commands for the AO pass (used in the ambient-only mode).
		void BuildAOCommands(Camera _cam, CommandBuffer _cmdBuf, RenderSystem _system)
		{
			var tw = _cam.pixelWidth;
			var th = _cam.pixelHeight;
			var format = RenderTextureFormat.R8;
			var rwMode = RenderTextureReadWrite.Linear;

			if (downsampling)
			{
				tw /= 2;
				th /= 2;
			}

			// AO buffer
			var _m = aoMaterial;
			var _idMask = Shader.PropertyToID("_ObscuranceTexture");
			var _idCurr = Shader.PropertyToID("_CurrTexture");
			
			_cmdBuf.GetTemporaryRT(_idMask, tw, th, 0, FilterMode.Bilinear, format, rwMode);
			_cmdBuf.GetTemporaryRT(_idCurr, -1, -1);

			_cmdBuf.Blit(BuiltinRenderTextureType.CameraTarget, _idCurr);

			// AO estimation
			_cmdBuf.Blit(BuiltinRenderTextureType.None, _idMask, _m, 0);

			if (blurIterations > 0)
			{
				// Blur buffer
				var rtBlur = Shader.PropertyToID("_ObscuranceBlurTexture");
				_cmdBuf.GetTemporaryRT(
					rtBlur, tw, th, 0, FilterMode.Bilinear, format, rwMode
				);

				var _blurRight = new Vector2(1.0f / tw, 0);
				var _blurUp = new Vector2(0, 1.0f / th);

				// Blur iterations
				for (var i = 0; i < blurIterations; i++)
				{
					_cmdBuf.SetGlobalVector("_BlurVector", _blurRight);
					_cmdBuf.Blit(_idMask, rtBlur, _m, 1);

					_cmdBuf.SetGlobalVector("_BlurVector", _blurUp);
					_cmdBuf.Blit(rtBlur, _idMask, _m, 1);
				}

				_cmdBuf.ReleaseTemporaryRT(rtBlur);
			}

			//_cmdBuf.SetGlobalTexture("_MudAlbedoBuffer", _system.GetAlbedoBufferForCamera(_cam));
			_cmdBuf.SetGlobalTexture("_MudAlbedoBuffer", _idCurr);
			_cmdBuf.SetGlobalVector("_MudAlbedoBuffer_TexelSize", new Vector4(1, -1, 1, 1)); // currently hardcoded to flipped the y
			_cmdBuf.Blit(_idMask, BuiltinRenderTextureType.CameraTarget, _m, 2);

			_cmdBuf.ReleaseTemporaryRT(_idMask);
			_cmdBuf.ReleaseTemporaryRT(_idCurr);
		}
		
		// Update the common material properties.
		void UpdateMaterialProperties(bool _useGBuffer)
		{
			var _mtl = aoMaterial;
			_mtl.shaderKeywords = null;

			_mtl.SetFloat("_Intensity", intensity);
			_mtl.SetFloat("_Radius", radius);
			_mtl.SetFloat("_TargetScale", downsampling ? 0.5f : 1);

			float _cutoff = _sharpness * 0.5f;
			_mtl.SetVector("_DynamicRange", new Vector2(_cutoff, 1.0f - _cutoff));

			// Use G-buffer if available.
			//if (IsGBufferAvailable)
			//m.EnableKeyword("_SOURCE_GBUFFER");

			// Sample count
			if (sampleCount == SampleCount.Lowest)
				_mtl.EnableKeyword("_SAMPLECOUNT_LOWEST");
			else
				_mtl.SetInt("_SampleCount", sampleCountValue);
		}
		

		#endregion

		#region MonoBehaviour Functions

		public override void OnEnable()
		{
			base.OnEnable();

			LoadMaterial(MTL_SSAO);
		}
		
		public override void SetupCameraEvents(Camera _cam, RenderSystem _system)
		{
			// update command buffers
			var _cmdbuf = GetCommandBufferForEvent(_cam, CameraEvent.AfterForwardOpaque, "Mud.SSAO");
			_cmdbuf.Clear();

			UpdateMaterialProperties(true);

			BuildAOCommands(_cam, _cmdbuf, _system);
		}

		#endregion
	}
}