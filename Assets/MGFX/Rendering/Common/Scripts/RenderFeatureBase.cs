﻿using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace MGFX
{

	[ExecuteInEditMode]
	[RequireComponent (typeof(RenderSystem))]
	public class RenderFeatureBase : MonoBehaviour
	{
		#region Material Management

		[System.AttributeUsage (System.AttributeTargets.Field)]
		public class MaterialAttribute : System.Attribute
		{
			public HashID Id;

			public MaterialAttribute (string _name)
			{
				Id = new HashID(_name);
			}
		}

		public static int LoadMaterials (RenderFeatureBase _inst)
		{
			int _cnt = 0;
			var _flags = System.Reflection.BindingFlags.Instance 
				| System.Reflection.BindingFlags.NonPublic
				| System.Reflection.BindingFlags.Public;
			
			foreach (var _field in _inst.GetType ().GetFields (_flags))
			{
				foreach (var _attr in _field.GetCustomAttributes (true))
				{
					var _mtlAttr = _attr as MaterialAttribute;
					if (null != _mtlAttr && _field.FieldType == typeof(Material))
					{
						var _mtl = _inst.LoadMaterial (_mtlAttr.Id);
						_field.SetValue (_inst, _mtl);
						++_cnt;
					}
				}
			}
			return _cnt;
		}

		protected HashDict<Material> m_Materials = new HashDict<Material> ();

		protected Material LoadMaterial (HashID _id)
		{
			var _shaderName = _id.ToString ();

			if (m_Materials.ContainsKey (_id))
			{
				Debug.LogWarningFormat ("possible duplicated LoadMaterial {0}", _shaderName);
				return m_Materials [_id];
			}

			Shader _shader = Shader.Find (_shaderName);
			if (null == _shader)
			{
				Debug.LogErrorFormat ("shader {0} not found", _shaderName);
				return null;
			}

			var _mtl = new Material (_shader);
			_mtl.hideFlags = HideFlags.HideAndDontSave;
			m_Materials.Add (_id, _mtl);
			return _mtl;
		}

		protected Material GetMaterial (HashID _id)
		{
			if (!m_Materials.ContainsKey (_id))
				return null;

			return m_Materials [_id];
		}


		#endregion

		#region Command Buffer Management

		protected class EvtCmdBuf
		{
			public CameraEvent Event;
			public CommandBuffer CommandBuffer;
		}

		protected class EvtCmdBufList :List<EvtCmdBuf>
		{
		}

		protected class EvtCmdBufMapping : Dictionary<Camera, EvtCmdBufList>
		{

		}

		protected EvtCmdBufMapping m_CameraCommands = new EvtCmdBufMapping ();

		protected CommandBuffer GetCommandBufferForEvent (Camera _cam, CameraEvent _event, string _name)
		{

			var _curr = m_CameraCommands [_cam].Find ((_evtbuf) =>
			{
				return _evtbuf.Event == _event;
			});

			if (null != _curr)
				return _curr.CommandBuffer;

			CommandBuffer _cmdBuf = new CommandBuffer ();
			_cmdBuf.name = _name;
			_cam.AddCommandBuffer (_event, _cmdBuf);

			m_CameraCommands [_cam].Add (new EvtCmdBuf { Event = _event, CommandBuffer = _cmdBuf });

			return _cmdBuf;
		}

		#endregion

		#region

		[HideInInspector ()]
		public bool affectSceneCamera = true;

		#endregion

		#region MonoBehaviour related

		public virtual void OnEnable ()
		{
		}

		public void OnDisable ()
		{
			Cleanup ();
		}

		protected virtual void Cleanup ()
		{
			foreach (var _pair in m_CameraCommands)
			{
				foreach (var _evtCmds in _pair.Value)
				{
					if (_pair.Key)
						_pair.Key.RemoveCommandBuffer (_evtCmds.Event, _evtCmds.CommandBuffer);
				}
			}

			m_CameraCommands.Clear ();

			foreach (var _pair in m_Materials)
			{
				Material.DestroyImmediate (_pair.Value);
			}

			m_Materials.Clear ();
		}

		public virtual void OnPreRenderCamera (Camera _cam, RenderSystem _system)
		{
			var _active = gameObject.activeInHierarchy && enabled;
			if (!_active)
			{
				Cleanup ();
				return;
			}

			if (!m_CameraCommands.ContainsKey (_cam))
			{
				var _cmdList = new EvtCmdBufList ();
				m_CameraCommands [_cam] = _cmdList;
			}

			SetupCameraEvents (_cam, _system);
		}

		public virtual void SetupCameraEvents (Camera _cam, RenderSystem _system)
		{

		}

		#endregion

		#region Utils

		protected static void Swap (ref RenderTexture _a, ref RenderTexture _b)
		{
			RenderTexture _t = _a;
			_a = _b;
			_b = _t;
		}

		protected static void Swap (ref int _a, ref int _b)
		{
			int _t = _a;
			_a = _b;
			_b = _t;
		}

		protected static void SetFlip (CommandBuffer _cmdBuf, Camera _cam)
		{
			bool _flip = _cam.targetTexture == null;
			_flip = false;
			_cmdBuf.SetGlobalFloat ("_Flip", (true == _flip) ? -1 : 1);
		}

		#endregion
	}

}
