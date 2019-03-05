using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace Minimalist
{
	public class MinimalistSkyEditor : ShaderGUI
	{
		private MaterialProperty _Color1;
		private MaterialProperty _Color2;
		private MaterialProperty _Intensity;
		private MaterialProperty _Exponent;
		private MaterialProperty _DirX;
		private MaterialProperty _DirY;
		private MaterialProperty _UpVector;

		private void InitializeMatProps(MaterialProperty[] _props)
		{
			_Color1 = FindProperty("_Color1", _props);
			_Color2 = FindProperty("_Color2", _props);
			_Intensity = FindProperty("_Intensity", _props);
			_Exponent = FindProperty("_Exponent", _props);
			_DirX = FindProperty("_DirX", _props);
			_DirY = FindProperty("_DirY", _props);
		}

		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			InitializeMatProps(properties);
			EditorGUI.BeginChangeCheck();
			{
				materialEditor.ColorProperty(_Color1, "Color 1");
				materialEditor.ColorProperty(_Color2, "Color 2");

				materialEditor.FloatProperty(_Intensity, "Intensity");
				materialEditor.FloatProperty(_Exponent, "Exponent");

				EditorGUI.BeginDisabledGroup(true);
				{
					materialEditor.RangeProperty(_DirY, "Pitch");
					materialEditor.RangeProperty(_DirX, "Yaw");
				}
				EditorGUI.EndDisabledGroup();
				
				EditorGUILayout.HelpBox("Some Features are not available in the free version of Minimalist", MessageType.Warning);
            
				if (GUILayout.Button("Get the full fersion of Minimalist"))
				{
					Application.OpenURL("https://assetstore.unity.com/packages/vfx/shaders/minimalist-lowpoly-flat-gradient-shader-91366");
				}
            
            
				EditorGUILayout.BeginHorizontal();
				{
                
					if (GUILayout.Button("Forum"))
					{
						Application.OpenURL("https://forum.unity.com/threads/minimalist-lowpoly-gradient-shader.478507");
					}
                
					if (GUILayout.Button("Email"))
					{
						Application.OpenURL("mailto://isfaqrahman98@gmail.com");
					}
                
					if (GUILayout.Button("Rate/Review"))
					{
						Application.OpenURL("https://assetstore.unity.com/packages/vfx/shaders/minimalist-free-lowpoly-flat-gradient-shader-96148");
					}
				}
            
				EditorGUILayout.EndHorizontal();
			}
			if (EditorGUI.EndChangeCheck())
			{
				InitializeMatProps(properties);
			}
		}
	}
}