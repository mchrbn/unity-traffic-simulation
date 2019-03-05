using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.Rendering;

namespace Minimalist
{

    [CanEditMultipleObjects]
    public class MinimalistStandardEditor : ShaderGUI
    {
        #region enums 

        public enum ShadingMode
        {
            VertexColor,
            SolidColor,
            GradientProOnly
        }

        public enum GradientSettings
        {
            UseGlobalGradientSettings,
            DefineCustomGradientSettings
        }

        public enum GradientSpace
        {
            WorldSpace,
            LocalSpace
        }

        public enum AOuv
        {
            uv0,
            uv1
        }

        public enum LightMapBlendingMode
        {
            Add,
            Multiply,
            UseAsAO
        }

        public enum HandleProfile
        {
            frontGradient,
            frontRotation
        }

        public enum Cull
        {
            Off,
            Front,
            Back
        }

        public enum Mode
        {
            Opaque,
            Transparent
        }

        #endregion

        #region Constants

        const string FRONT = "Front";
        const string BACK = "Back";
        const string TOP = "Top";
        const string DOWN = "Down";
        const string LEFT = "Left";
        const string RIGHT = "Right";

        #endregion

        #region boleans

        bool showFrontShading;
        bool showBackShading;
        bool showLeftShading;
        bool showRightShading;
        bool showTopShading;
        bool showBottomShading;

        private bool ShowTexture
        {
            get
            {
                float x = _ShowTexture.floatValue;
                if (x == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (value == true)
                {
                    _ShowTexture.floatValue = 1;
                }
                else
                {
                    _ShowTexture.floatValue = 0;
                }
            }
        }

        private bool ShowCustomShading
        {
            get
            {
                if (_ShowCustomShading.floatValue == 0) return false;
                else return true;
            }
            set
            {
                if (value == true) _ShowCustomShading.floatValue = 1;
                else _ShowCustomShading.floatValue = 0;
            }
        }

        private bool ShowOtherSettings
        {
            get
            {
                float x = _OtherSettings.floatValue;
                if (x == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (value == true)
                {
                    _OtherSettings.floatValue = 1;
                }
                else
                {
                    _OtherSettings.floatValue = 0;
                }
            }
        }

        private bool ShowGlobalGradientSettings
        {
            get
            {
                float x = _ShowGlobalGradientSettings.floatValue;
                if (x == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (value == true)
                {
                    _ShowGlobalGradientSettings.floatValue = 1;
                }
                else
                {
                    _ShowGlobalGradientSettings.floatValue = 0;
                }
            }
        }

        private bool ShowAmbientSettings
        {
            get
            {
                float x = _ShowAmbientSettings.floatValue;
                if (x == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (value == true)
                {
                    _ShowAmbientSettings.floatValue = 1;
                }
                else
                {
                    _ShowAmbientSettings.floatValue = 0;
                }
            }
        }

        private bool realtimeShadow
        {
            get
            {
                float x = _RealtimeShadow.floatValue;
                if (x == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (value == true)
                {
                    _RealtimeShadow.floatValue = 1;
                }
                else
                {
                    _RealtimeShadow.floatValue = 0;
                }
            }
        }

        private bool dontMix
        {
            get
            {
                float x = _DontMix.floatValue;
                if (x == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (value == true)
                {
                    _DontMix.floatValue = 1;
                }
                else
                {
                    _DontMix.floatValue = 0;
                }
            }
        }

        private bool ShowAOSettings
        {
            get
            {
                float x = _ShowAO.floatValue;
                if (x == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (value == true)
                {
                    _ShowAO.floatValue = 1;
                }
                else
                {
                    _ShowAO.floatValue = 0;
                }
            }
        }

        private bool EnableAO
        {
            get
            {
                float x = _AOEnable.floatValue;
                if (x == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (value == true)
                {
                    _AOEnable.floatValue = 1;
                }
                else
                {
                    _AOEnable.floatValue = 0;
                }
            }
        }

        private bool ShowlMapSettings
        {
            get
            {
                float x = _ShowLMap.floatValue;
                if (x == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (value == true)
                {
                    _ShowLMap.floatValue = 1;
                }
                else
                {
                    _ShowLMap.floatValue = 0;
                }
            }
        }

        private bool EnableLmap
        {
            get
            {
                float x = _LmapEnable.floatValue;
                if (x == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (value == true)
                {
                    _LmapEnable.floatValue = 1;
                }
                else
                {
                    _LmapEnable.floatValue = 0;
                }
            }
        }

        private bool ShowFogSettigns
        {
            get
            {
                float x = _ShowFog.floatValue;
                if (x == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (value == true)
                {
                    _ShowFog.floatValue = 1;
                }
                else
                {
                    _ShowFog.floatValue = 0;
                }
            }
        }

        private bool EnableUnityFog
        {
            get
            {
                float x = _UnityFogEnable.floatValue;
                if (x == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (value == true)
                {
                    _UnityFogEnable.floatValue = 1;
                }
                else
                {
                    _UnityFogEnable.floatValue = 0;
                }
            }
        }

        private bool EnableHFog
        {
            get
            {
                float x = _HFogEnable.floatValue;
                if (x == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (value == true)
                {
                    _HFogEnable.floatValue = 1;
                }
                else
                {
                    _HFogEnable.floatValue = 0;
                }
            }
        }

        private bool ColorCorrectionEnable
        {
            get
            {
                float x = _ColorCorrectionEnable.floatValue;
                if (x == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (value == true)
                {
                    _ColorCorrectionEnable.floatValue = 1;
                }
                else
                {
                    _ColorCorrectionEnable.floatValue = 0;
                }
            }
        }

        private bool RimEnable
        {
            get
            {
                float x = _RimEnable.floatValue;
                if (x == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (value == true)
                {
                    _RimEnable.floatValue = 1;
                }
                else
                {
                    _RimEnable.floatValue = 0;
                }
            }
        }

        private bool ShowColorCorrectionSettings
        {
            get
            {
                float x = _ShowColorCorrection.floatValue;
                if (x == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (value == true)
                {
                    _ShowColorCorrection.floatValue = 1;
                }
                else
                {
                    _ShowColorCorrection.floatValue = 0;
                }
            }
        }

        #endregion

        #region enum props

        ShadingMode frontShadingMode;
        ShadingMode backShadingMode;
        ShadingMode leftShadingMode;
        ShadingMode rightShadingMode;
        ShadingMode topShadingMode;
        ShadingMode bottomShadingMode;

        private Cull cullmode
        {
            get { return (Cull) _Cull.floatValue; }
            set { _Cull.floatValue = (float) value; }
        }

        private Mode blendingMode
        {
            get { return (Mode) _Mode.floatValue; }
            set { _Mode.floatValue = (float) value; }
        }

        GradientSpace gSpace
        {
            get { return (GradientSpace) _GradientSpace.floatValue; }
            set
            {
                if (value == GradientSpace.WorldSpace)
                {
                    _GradientSpace.floatValue = 0;
                }
                else
                {
                    _GradientSpace.floatValue = 1;
                }
            }
        }

        AOuv aouv
        {
            get { return (AOuv) _AOuv.floatValue; }
            set
            {
                if (value == AOuv.uv0)
                {
                    _AOuv.floatValue = 0;
                }
                else
                {
                    _AOuv.floatValue = 1;
                }
            }
        }

        LightMapBlendingMode lMapBlendMode
        {
            get { return (LightMapBlendingMode) _LmapBlendingMode.floatValue; }
            set
            {
                if (value == LightMapBlendingMode.Add)
                {
                    _LmapBlendingMode.floatValue = 0;
                }
                else if (value == LightMapBlendingMode.Multiply)
                {
                    _LmapBlendingMode.floatValue = 1;
                }
                else
                {
                    _LmapBlendingMode.floatValue = 2;
                }
            }
        }

        #endregion

        #region gradientSettings

        GradientSettings frontGradientSettings;
        GradientSettings backGradientSettings;
        GradientSettings leftGradientSettings;
        GradientSettings rightGradientSettings;
        GradientSettings topGradientSettings;
        GradientSettings bottomGradientSettings;

        #endregion

        #region  ShaderProperties

        MaterialProperty _ShowTexture, _MainTexture, _MainTexturePower;

        MaterialProperty _ShowCustomShading;

        MaterialProperty _ShowFront,
            _Shading_F,
            _Color1_F,
            _Color2_F,
            _GradSettings_F,
            _GradientYStartPos_F,
            _GradientHeight_F,
            _Rotation_F,
            _GizmoPosition_F;

        MaterialProperty _ShowBack,
            _Shading_B,
            _Color1_B,
            _Color2_B,
            _GradSettings_B,
            _GradientYStartPos_B,
            _GradientHeight_B,
            _Rotation_B,
            _GizmoPosition_B;

        MaterialProperty _ShowLeft,
            _Shading_L,
            _Color1_L,
            _Color2_L,
            _GradSettings_L,
            _GradientYStartPos_L,
            _GradientHeight_L,
            _Rotation_L,
            _GizmoPosition_L;

        MaterialProperty _ShowRight,
            _Shading_R,
            _Color1_R,
            _Color2_R,
            _GradSettings_R,
            _GradientYStartPos_R,
            _GradientHeight_R,
            _Rotation_R,
            _GizmoPosition_R;

        MaterialProperty _ShowTop,
            _Shading_T,
            _Color1_T,
            _Color2_T,
            _GradSettings_T,
            _GradientXStartPos_T,
            _GradientHeight_T,
            _Rotation_T,
            _GizmoPosition_T;

        MaterialProperty _ShowBottom,
            _Shading_D,
            _Color1_D,
            _Color2_D,
            _GradSettings_D,
            _GradientXStartPos_D,
            _GradientHeight_D,
            _Rotation_D,
            _GizmoPosition_D;

        MaterialProperty _ShowAO, _AOEnable, _AOTexture, _AOColor, _AOPower, _AOuv;
        MaterialProperty _ShowLMap, _LmapEnable, _LmapBlendingMode, _LMColor, _LMPower;
        MaterialProperty _ShowFog, _UnityFogEnable, _HFogEnable, _Color_Fog, _FogYStartPos, _FogHeight;

        MaterialProperty _ShowColorCorrection,
            _ColorCorrectionEnable,
            _RimEnable,
            _RimColor,
            _RimPower,
            _TintColor,
            _Saturation,
            _Brightness;

        MaterialProperty _ShowGlobalGradientSettings, _GradientHeight_G, _GradPivot_G, _Rotation_G, _OtherSettings;
        MaterialProperty _ShowAmbientSettings, _AmbientColor, _AmbientPower;
        MaterialProperty _RealtimeShadow, _ShadowColor;
        MaterialProperty _GradientSpace, _DontMix, _Cull, _Mode, _Fade;

        #endregion

        Dictionary<string, MaterialProperty> RecorderProps;

        #region GUIStyles

        private readonly GUIStyle Indented = new GUIStyle()
        {
            padding = new RectOffset(10, 0, 0, 0)
        };

        private readonly GUIStyle HeaderStyle = new GUIStyle("box")
        {
            fontSize = EditorStyles.boldLabel.fontSize,
            fontStyle = EditorStyles.boldLabel.fontStyle,
            font = EditorStyles.boldLabel.font,
            alignment = TextAnchor.UpperLeft,
            padding = new RectOffset(10, 0, 2, 0),
        };

        #endregion

        Material targetMat;
        private MaterialEditor matEditor;
        static GradientHandle gradientHandle;
        Transform selectedObject;
        Transform top, bottom;
        bool drawGradintGizmo, drawRotationGizmo, deawFogGizmo;
        Event guiEvent;
        public static MinimalistClipboard MClipboard;

        public void UndoRedoPerformed(MaterialProperty[] _props)
        {
            InitializeMatProps(_props);
            InitializeHelperVars();
        }

        private void InitializeMatProps(MaterialProperty[] _props)
        {
            _ShowTexture = FindProperty("_ShowTexture", _props);
            _MainTexture = FindProperty("_MainTexture", _props);
            _MainTexturePower = FindProperty("_MainTexturePower", _props);

            _ShowCustomShading = FindProperty("_ShowCustomShading", _props);

            _ShowFront = FindProperty("_ShowFront", _props);
            _Shading_F = FindProperty("_Shading_F", _props);
            _Color1_F = FindProperty("_Color1_F", _props);
            _Color2_F = FindProperty("_Color2_F", _props);
            _GradSettings_F = FindProperty("_GradSettings_F", _props);
            _GradientYStartPos_F = FindProperty("_GradientYStartPos_F", _props);
            _GradientHeight_F = FindProperty("_GradientHeight_F", _props);
            _Rotation_F = FindProperty("_Rotation_F", _props);
            _GizmoPosition_F = FindProperty("_GizmoPosition_F", _props);

            _ShowBack = FindProperty("_ShowBack", _props);
            _Shading_B = FindProperty("_Shading_B", _props);
            _Color1_B = FindProperty("_Color1_B", _props);
            _Color2_B = FindProperty("_Color2_B", _props);
            _GradSettings_B = FindProperty("_GradSettings_B", _props);
            _GradientYStartPos_B = FindProperty("_GradientYStartPos_B", _props);
            _GradientHeight_B = FindProperty("_GradientHeight_B", _props);
            _Rotation_B = FindProperty("_Rotation_B", _props);
            _GizmoPosition_B = FindProperty("_GizmoPosition_B", _props);

            _ShowLeft = FindProperty("_ShowLeft", _props);
            _Shading_L = FindProperty("_Shading_L", _props);
            _Color1_L = FindProperty("_Color1_L", _props);
            _Color2_L = FindProperty("_Color2_L", _props);
            _GradSettings_L = FindProperty("_GradSettings_L", _props);
            _GradientYStartPos_L = FindProperty("_GradientYStartPos_L", _props);
            _GradientHeight_L = FindProperty("_GradientHeight_L", _props);
            _Rotation_L = FindProperty("_Rotation_L", _props);
            _GizmoPosition_L = FindProperty("_GizmoPosition_L", _props);

            _ShowRight = FindProperty("_ShowRight", _props);
            _Shading_R = FindProperty("_Shading_R", _props);
            _Color1_R = FindProperty("_Color1_R", _props);
            _Color2_R = FindProperty("_Color2_R", _props);
            _GradSettings_R = FindProperty("_GradSettings_R", _props);
            _GradientYStartPos_R = FindProperty("_GradientYStartPos_R", _props);
            _GradientHeight_R = FindProperty("_GradientHeight_R", _props);
            _Rotation_R = FindProperty("_Rotation_R", _props);
            _GizmoPosition_R = FindProperty("_GizmoPosition_R", _props);


            _ShowTop = FindProperty("_ShowTop", _props);
            _Shading_T = FindProperty("_Shading_T", _props);
            _Color1_T = FindProperty("_Color1_T", _props);
            _Color2_T = FindProperty("_Color2_T", _props);
            _GradSettings_T = FindProperty("_GradSettings_T", _props);
            _GradientXStartPos_T = FindProperty("_GradientXStartPos_T", _props);
            _GradientHeight_T = FindProperty("_GradientHeight_T", _props);
            _Rotation_T = FindProperty("_Rotation_T", _props);
            _GizmoPosition_T = FindProperty("_GizmoPosition_T", _props);

            _ShowBottom = FindProperty("_ShowBottom", _props);
            _Shading_D = FindProperty("_Shading_D", _props);
            _Color1_D = FindProperty("_Color1_D", _props);
            _Color2_D = FindProperty("_Color2_D", _props);
            _GradSettings_D = FindProperty("_GradSettings_D", _props);
            _GradientXStartPos_D = FindProperty("_GradientXStartPos_D", _props);
            _GradientHeight_D = FindProperty("_GradientHeight_D", _props);
            _Rotation_D = FindProperty("_Rotation_D", _props);
            _GizmoPosition_D = FindProperty("_GizmoPosition_D", _props);

            _ShowAO = FindProperty("_ShowAO", _props);
            _AOEnable = FindProperty("_AOEnable", _props);
            _AOTexture = FindProperty("_AOTexture", _props);
            _AOPower = FindProperty("_AOPower", _props);
            _AOColor = FindProperty("_AOColor", _props);
            _AOuv = FindProperty("_AOuv", _props);

            _ShowLMap = FindProperty("_ShowLMap", _props);
            _LmapEnable = FindProperty("_LmapEnable", _props);
            _LmapBlendingMode = FindProperty("_LmapBlendingMode", _props);
            _LMColor = FindProperty("_LMColor", _props);
            _LMPower = FindProperty("_LMPower", _props);

            _ShowFog = FindProperty("_ShowFog", _props);
            _UnityFogEnable = FindProperty("_UnityFogEnable", _props);
            _HFogEnable = FindProperty("_HFogEnable", _props);
            _Color_Fog = FindProperty("_Color_Fog", _props);
            _FogYStartPos = FindProperty("_FogYStartPos", _props);
            _FogHeight = FindProperty("_FogHeight", _props);

            _ShowColorCorrection = FindProperty("_ShowColorCorrection", _props);
            _ColorCorrectionEnable = FindProperty("_ColorCorrectionEnable", _props);
            _RimEnable = FindProperty("_RimEnable", _props);
            _RimColor = FindProperty("_RimColor", _props);
            _RimPower = FindProperty("_RimPower", _props);
            _TintColor = FindProperty("_TintColor", _props);
            _Saturation = FindProperty("_Saturation", _props);
            _Brightness = FindProperty("_Brightness", _props);

            _OtherSettings = FindProperty("_OtherSettings", _props);

            _ShowGlobalGradientSettings = FindProperty("_ShowGlobalGradientSettings", _props);
            _GradientHeight_G = FindProperty("_GradientHeight_G", _props);
            _GradPivot_G = FindProperty("_GradientYStartPos_G", _props);
            _Rotation_G = FindProperty("_Rotation_G", _props);

            _ShowAmbientSettings = FindProperty("_ShowAmbientSettings", _props);
            _AmbientColor = FindProperty("_AmbientColor", _props);
            _AmbientPower = FindProperty("_AmbientPower", _props);

            _RealtimeShadow = FindProperty("_RealtimeShadow", _props);
            _ShadowColor = FindProperty("_ShadowColor", _props);

            _GradientSpace = FindProperty("_GradientSpace", _props);
            _DontMix = FindProperty("_DontMix", _props);
            _Cull = FindProperty("_Cull", _props);
            _Mode = FindProperty("_Mode", _props);
            _Fade = FindProperty("_Fade", _props);

            RecorderProps = new Dictionary<string, MaterialProperty>();
            RecorderProps.Add(_GradientHeight_F.name, _GradientHeight_F);
            RecorderProps.Add(_GradientHeight_B.name, _GradientHeight_B);
            RecorderProps.Add(_GradientHeight_L.name, _GradientHeight_L);
            RecorderProps.Add(_GradientHeight_R.name, _GradientHeight_R);
            RecorderProps.Add(_GradientHeight_T.name, _GradientHeight_T);
            RecorderProps.Add(_GradientHeight_D.name, _GradientHeight_D);

            RecorderProps.Add(_GradientYStartPos_F.name, _GradientYStartPos_F);
            RecorderProps.Add(_GradientYStartPos_B.name, _GradientYStartPos_B);
            RecorderProps.Add(_GradientYStartPos_L.name, _GradientYStartPos_L);
            RecorderProps.Add(_GradientYStartPos_R.name, _GradientYStartPos_R);
            RecorderProps.Add(_GradientXStartPos_T.name, _GradientXStartPos_T);
            RecorderProps.Add(_GradientXStartPos_D.name, _GradientXStartPos_D);

            RecorderProps.Add(_GizmoPosition_F.name, _GizmoPosition_F);
            RecorderProps.Add(_GizmoPosition_B.name, _GizmoPosition_B);
            RecorderProps.Add(_GizmoPosition_L.name, _GizmoPosition_L);
            RecorderProps.Add(_GizmoPosition_R.name, _GizmoPosition_R);
            RecorderProps.Add(_GizmoPosition_T.name, _GizmoPosition_T);
            RecorderProps.Add(_GizmoPosition_D.name, _GizmoPosition_D);

            RecorderProps.Add(_Rotation_F.name, _Rotation_F);
            RecorderProps.Add(_Rotation_B.name, _Rotation_B);
            RecorderProps.Add(_Rotation_L.name, _Rotation_L);
            RecorderProps.Add(_Rotation_R.name, _Rotation_R);
            RecorderProps.Add(_Rotation_T.name, _Rotation_T);
            RecorderProps.Add(_Rotation_D.name, _Rotation_D);


        }

        private void InitializeHelperVars()
        {
            if (_ShowFront.floatValue == 0) showFrontShading = false;
            else if (_ShowFront.floatValue == 1) showFrontShading = true;
            if (_ShowBack.floatValue == 0) showBackShading = false;
            else if (_ShowBack.floatValue == 1) showBackShading = true;
            if (_ShowLeft.floatValue == 0) showLeftShading = false;
            else if (_ShowLeft.floatValue == 1) showLeftShading = true;
            if (_ShowRight.floatValue == 0) showRightShading = false;
            else if (_ShowRight.floatValue == 1) showRightShading = true;
            if (_ShowTop.floatValue == 0) showTopShading = false;
            else if (_ShowTop.floatValue == 1) showTopShading = true;
            if (_ShowBottom.floatValue == 0) showBottomShading = false;
            else if (_ShowBottom.floatValue == 1) showBottomShading = true;

            frontShadingMode = (ShadingMode) _Shading_F.floatValue;
            backShadingMode = (ShadingMode) _Shading_B.floatValue;
            leftShadingMode = (ShadingMode) _Shading_L.floatValue;
            rightShadingMode = (ShadingMode) _Shading_R.floatValue;
            topShadingMode = (ShadingMode) _Shading_T.floatValue;
            bottomShadingMode = (ShadingMode) _Shading_D.floatValue;

            frontGradientSettings = (GradientSettings) _GradSettings_F.floatValue;
            backGradientSettings = (GradientSettings) _GradSettings_B.floatValue;
            leftGradientSettings = (GradientSettings) _GradSettings_L.floatValue;
            rightGradientSettings = (GradientSettings) _GradSettings_R.floatValue;
            topGradientSettings = (GradientSettings) _GradSettings_T.floatValue;
            bottomGradientSettings = (GradientSettings) _GradSettings_D.floatValue;
        }

        void InitializeGradientSpace(Material mat)
        {
            if (mat.shader.name.Contains("LocalSpace"))
            {
                gSpace = GradientSpace.LocalSpace;
            }
            else
            {
                gSpace = GradientSpace.WorldSpace;
            }
        }

        void InitializeClipboard()
        {
            if (MClipboard == null)
            {
                MClipboard = EditorGUIUtility.Load("MinimalistClipboard.asset") as MinimalistClipboard;

                if (MClipboard == null)
                {
                    Debug.Log(
                        "Couldn't find \"Assets/Editor Default Resources/MinimalistClipboard.asset\", creating one.");
                    if (!AssetDatabase.IsValidFolder("Assets/Editor Default Resources"))
                    {
                        AssetDatabase.CreateFolder("Assets", "Editor Default Resources");
                    }

                    MinimalistClipboard clipboard = ScriptableObject.CreateInstance<MinimalistClipboard>();
                    AssetDatabase.CreateAsset(clipboard, "Assets/Editor Default Resources/MinimalistClipboard.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }

        private bool firstTimeApply = true;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            matEditor = materialEditor;
            targetMat = materialEditor.target as Material;
            InitializeMatProps(properties);
            InitializeHelperVars();
            InitializeGradientSpace(targetMat);
            //InitializeClipboard();
            HeaderStyle.normal.background = GUI.skin.GetStyle("ShurikenModuleTitle").normal.background;

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width - 34));
                {
                    //Texture Module
                    ShowTexture = GUILayout.Toggle(ShowTexture, "Main Texture", HeaderStyle, GUILayout.Height(20),
                        GUILayout.ExpandWidth(true));
                    if (ShowTexture) TextureModule();

                    //Shading Module
                    ShowCustomShading = GUILayout.Toggle(ShowCustomShading, "Custom Shading", HeaderStyle,
                        GUILayout.Height(20), GUILayout.ExpandWidth(true));
                    if (ShowCustomShading)
                    {
                        EditorGUILayout.BeginVertical(Indented);
                        CustomShadingModule(ref showFrontShading, ref _ShowFront, FRONT, ref frontShadingMode,
                            ref _Shading_F, ref _Color1_F, ref _Color2_F, ref frontGradientSettings,
                            ref _GradSettings_F, ref _GradientHeight_F, ref _GradientYStartPos_F, ref _Rotation_F,
                            "FRONTGRADIENT", "FRONTSOLID", _GizmoPosition_F);
                        CustomShadingModule(ref showBackShading, ref _ShowBack, BACK, ref backShadingMode,
                            ref _Shading_B, ref _Color1_B, ref _Color2_B, ref backGradientSettings, ref _GradSettings_B,
                            ref _GradientHeight_B, ref _GradientYStartPos_B, ref _Rotation_B, "BACKGRADIENT",
                            "BACKSOLID", _GizmoPosition_B);
                        CustomShadingModule(ref showLeftShading, ref _ShowLeft, LEFT, ref leftShadingMode,
                            ref _Shading_L, ref _Color1_L, ref _Color2_L, ref leftGradientSettings, ref _GradSettings_L,
                            ref _GradientHeight_L, ref _GradientYStartPos_L, ref _Rotation_L, "LEFTGRADIENT",
                            "LEFTSOLID", _GizmoPosition_L);
                        CustomShadingModule(ref showRightShading, ref _ShowRight, RIGHT, ref rightShadingMode,
                            ref _Shading_R, ref _Color1_R, ref _Color2_R, ref rightGradientSettings,
                            ref _GradSettings_R, ref _GradientHeight_R, ref _GradientYStartPos_R, ref _Rotation_R,
                            "RIGHTGRADIENT", "RIGHTSOLID", _GizmoPosition_R);
                        CustomShadingModule(ref showTopShading, ref _ShowTop, TOP, ref topShadingMode, ref _Shading_T,
                            ref _Color1_T, ref _Color2_T, ref topGradientSettings, ref _GradSettings_T,
                            ref _GradientHeight_T, ref _GradientXStartPos_T, ref _Rotation_T, "TOPGRADIENT", "TOPSOLID",
                            _GizmoPosition_T);
                        CustomShadingModule(ref showBottomShading, ref _ShowBottom, DOWN, ref bottomShadingMode,
                            ref _Shading_D, ref _Color1_D, ref _Color2_D, ref bottomGradientSettings,
                            ref _GradSettings_D, ref _GradientHeight_D, ref _GradientXStartPos_D, ref _Rotation_D,
                            "BOTTOMGRADIENT", "BOTTOMSOLID", _GizmoPosition_D);
                        EditorGUILayout.EndVertical();
                    }

                    //Ambient Occlusion
                    ShowAOSettings = GUILayout.Toggle(ShowAOSettings, "Ambient Occlusion", HeaderStyle,
                        GUILayout.Height(20), GUILayout.ExpandWidth(true));
                    if (ShowAOSettings) AOModule();

                    //Lightmap
                    ShowlMapSettings = GUILayout.Toggle(ShowlMapSettings, "Lightmap", HeaderStyle, GUILayout.Height(20),
                        GUILayout.ExpandWidth(true));
                    if (ShowlMapSettings) LightmapModule();

                    //Fog
                    ShowFogSettigns = GUILayout.Toggle(ShowFogSettigns, "Fog", HeaderStyle, GUILayout.Height(20),
                        GUILayout.ExpandWidth(true));
                    if (ShowFogSettigns) FogModule();

                    //Color Correction
                    ShowColorCorrectionSettings = GUILayout.Toggle(ShowColorCorrectionSettings, "Color Correction",
                        HeaderStyle, GUILayout.Height(20), GUILayout.ExpandWidth(true));
                    if (ShowColorCorrectionSettings) ColorCorrectionModule();

                    //Other Settings
                    ShowOtherSettings = GUILayout.Toggle(ShowOtherSettings, "Other Settings", HeaderStyle,
                        GUILayout.Height(20), GUILayout.ExpandWidth(true));
                    if (ShowOtherSettings) OtherSettings();
                }
                EditorGUILayout.EndVertical();


            }
            if (EditorGUI.EndChangeCheck())
            {
                UndoRedoPerformed(properties);
            }
            
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

        private void OnClosed()
        {
            Debug.Log("Closed");
            SceneView.onSceneGUIDelegate -= SceneGUI;
        }

        private void TextureModule()
        {
            Texture tex = matEditor.TextureProperty(_MainTexture, "Texture");
            if (tex != null)
            {
                targetMat.EnableKeyword("TEXTUREMODULE_ON");
                matEditor.RangeProperty(_MainTexturePower, "Power");
            }
            else
            {
                targetMat.DisableKeyword("TEXTUREMODULE_ON");
            }
        }

        private void CustomShadingModule(
            ref bool ShowShading, ref MaterialProperty Show, string ShadingSide,
            ref ShadingMode ShadingType, ref MaterialProperty ShadeMode, ref MaterialProperty Color1,
            ref MaterialProperty Color2, ref GradientSettings GradSettings, ref MaterialProperty GradientSettings,
            ref MaterialProperty GradHeight, ref MaterialProperty GradPivot,
            ref MaterialProperty Rotation, string shaderKeywordG, string shaderKeywordS, MaterialProperty Gizmopos)
        {
            EditorGUILayout.BeginVertical();
            {
                ShowShading = EditorGUILayout.Foldout(ShowShading, ShadingSide);
            }
            EditorGUILayout.EndVertical();
            if (ShowShading)
            {
                Show.floatValue = 1;
                ShadingType = (ShadingMode) EditorGUILayout.EnumPopup("Shading Mode", ShadingType);
                ShadeMode.floatValue = (float) ShadingType;
                if (ShadingType == ShadingMode.VertexColor)
                {
                    targetMat.DisableKeyword(shaderKeywordG);
                    targetMat.DisableKeyword(shaderKeywordS);
                }
                else if (ShadingType == ShadingMode.SolidColor)
                {
                    matEditor.ColorProperty(Color1, "Color");
                    targetMat.DisableKeyword(shaderKeywordG);
                    targetMat.EnableKeyword(shaderKeywordS);
                }
                else if (ShadingType == ShadingMode.GradientProOnly)
                {
                    targetMat.EnableKeyword(shaderKeywordS);
                    EditorGUILayout.BeginHorizontal("Box");
                    {
                        EditorGUILayout.BeginVertical(GUILayout.Width(50));
                        {
                            EditorGUI.BeginDisabledGroup(true);
                                Color1.colorValue = EditorGUILayout.ColorField(Color1.colorValue);
                                Color2.colorValue = EditorGUILayout.ColorField(Color2.colorValue);
                                
                                if (GUILayout.Button("Swap"))
                                {
                                    Color temp = Color1.colorValue;
                                    Color1.colorValue = Color2.colorValue;
                                    Color2.colorValue = temp;
                                }
                            EditorGUI.EndDisabledGroup();
                            Rect R = EditorGUILayout.GetControlRect(GUILayout.Height(50), GUILayout.Width(50));

                            if (ShadingSide == TOP || ShadingSide == DOWN)
                                GUI.DrawTexture(R, GetTexture(Color1.colorValue, Color2.colorValue, true));
                            else
                                GUI.DrawTexture(R, GetTexture(Color1.colorValue, Color2.colorValue, false));
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width - 112));
                        {
                            GradSettings = (GradientSettings) EditorGUILayout.EnumPopup("", GradSettings,
                                GUILayout.Width(Screen.width - 110));
                            GradientSettings.floatValue = (float) GradSettings;
                            
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUI.BeginDisabledGroup(IsGlobal(GradSettings));
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width - 142));
                                    {
                                        if (IsGlobal(GradSettings))
                                        {
                                            GradHeight.floatValue =
                                                EditorGUILayoutExtended.FloatField(new GUIContent("Falloff"),
                                                    _GradientHeight_G.floatValue, 70,
                                                    GUILayout.Width(Screen.width - 142));
                                            EditorGUILayout.LabelField("Pivot", GUILayout.Width(60));
                                            GradPivot.vectorValue = EditorGUILayout.Vector2Field("",
                                                _GradPivot_G.vectorValue, GUILayout.Width(Screen.width - 142));
                                            EditorGUILayout.LabelField("Rotation", GUILayout.Width(60));
                                            Rotation.floatValue = EditorGUILayout.Slider(_Rotation_G.floatValue, 0f,
                                                360f, GUILayout.Width(Screen.width - 142));
                                        }
                                        else
                                        {
                                            GradHeight.floatValue =
                                                EditorGUILayoutExtended.FloatField(new GUIContent("Falloff"),
                                                    GradHeight.floatValue, 70, GUILayout.Width(Screen.width - 142));
                                            EditorGUILayout.LabelField("Pivot", GUILayout.Width(60));
                                            GradPivot.vectorValue = EditorGUILayout.Vector2Field("",
                                                GradPivot.vectorValue, GUILayout.Width(Screen.width - 142));
                                            EditorGUILayout.LabelField("Rotation", GUILayout.Width(60));
                                            Rotation.floatValue = EditorGUILayout.Slider(Rotation.floatValue, 0f, 360f,
                                                GUILayout.Width(Screen.width - 142));
                                        }
                                    }
                                    EditorGUILayout.EndVertical();

                                    EditorGUILayout.BeginVertical();
                                    {
                                        EditorGUI.BeginDisabledGroup(!isAnythingSelected());
                                        {
                                            if (GUILayout.Button(
                                                EditorGUIUtility.IconContent("EditCollider", "Edit in Scene"),
                                                GUILayout.Height(28)))
                                            {
                                                selectedObject = Selection.activeGameObject.transform;
                                                SceneView sc = (SceneView) SceneView.sceneViews[0];

                                                ReDrawHandle:
                                                if (gradientHandle == null)
                                                {
                                                    gradientHandle = new GradientHandle();
                                                    if (ShadingSide == TOP || ShadingSide == DOWN)
                                                    {
                                                        gradientHandle.pivot = new Vector3(GradPivot.vectorValue.x,
                                                            selectedObject.position.y, GradPivot.vectorValue.y);
                                                        gradientHandle.falloff = new Vector3(Gizmopos.vectorValue.z,
                                                            selectedObject.position.y, Gizmopos.vectorValue.w);

                                                        gradientHandle.pivot = new Vector3(GradPivot.vectorValue.x,
                                                            selectedObject.position.y, GradPivot.vectorValue.y);
                                                        gradientHandle.falloff = new Vector3(
                                                            -(GradHeight.floatValue *
                                                              Mathf.Cos(Rotation.floatValue * Mathf.Deg2Rad)),
                                                            selectedObject.position.y,
                                                            -(GradHeight.floatValue *
                                                              Mathf.Sin(Rotation.floatValue * Mathf.Deg2Rad)));

                                                    }
                                                    else if (ShadingSide == FRONT || ShadingSide == BACK)
                                                    {
                                                        gradientHandle.pivot = new Vector3(GradPivot.vectorValue.x,
                                                            GradPivot.vectorValue.y, selectedObject.position.z);
                                                        gradientHandle.falloff = new Vector3(
                                                            -(GradHeight.floatValue *
                                                              Mathf.Cos(Rotation.floatValue * Mathf.Deg2Rad)),
                                                            -(GradHeight.floatValue *
                                                              Mathf.Sin(Rotation.floatValue * Mathf.Deg2Rad)),
                                                            selectedObject.position.z);

                                                    }
                                                    else
                                                    {
                                                        gradientHandle.pivot = new Vector3(selectedObject.position.x,
                                                            GradPivot.vectorValue.y, GradPivot.vectorValue.x);
                                                        gradientHandle.falloff = new Vector3(selectedObject.position.x,
                                                            -(GradHeight.floatValue *
                                                              Mathf.Cos(Rotation.floatValue * Mathf.Deg2Rad)),
                                                            -(GradHeight.floatValue *
                                                              Mathf.Sin(Rotation.floatValue * Mathf.Deg2Rad)));
                                                    }

                                                    gradientHandle.Height = GradHeight.name;
                                                    gradientHandle.Ystart = GradPivot.name;
                                                    gradientHandle.gizmo = Gizmopos.name;
                                                    gradientHandle.rotation = Rotation.name;
                                                    gradientHandle.profile = ShadingSide + "Gradient";

                                                    //Camerawork
                                                    sc.orthographic = true;
                                                    if (ShadingSide == FRONT) sc.rotation = new Quaternion(0, 1, 0, 0);
                                                    if (ShadingSide == BACK) sc.rotation = new Quaternion(0, 0, 0, 1);
                                                    if (ShadingSide == TOP) sc.rotation = new Quaternion(1, 0, 0, 1);
                                                    if (ShadingSide == DOWN) sc.rotation = new Quaternion(-1, 0, 0, 1);
                                                    if (ShadingSide == LEFT) sc.rotation = new Quaternion(0, -1, 0, 1);
                                                    if (ShadingSide == RIGHT) sc.rotation = new Quaternion(0, 1, 0, 1);


                                                    //Start SceneView code
                                                    SceneView.onSceneGUIDelegate -= SceneGUI;
                                                    SceneView.onSceneGUIDelegate += SceneGUI;

                                                }
                                                else
                                                {
                                                    if (gradientHandle.profile == ShadingSide + "Gradient")
                                                    {
                                                        gradientHandle = null;
                                                    }
                                                    else
                                                    {
                                                        gradientHandle = null;
                                                        goto ReDrawHandle;
                                                    }

                                                }
                                            }

                                            if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Duplicate"),
                                                GUILayout.Height(28)))
                                            {
                                                MClipboard.falloff = GradHeight.floatValue;
                                                MClipboard.pivot = GradPivot.vectorValue;
                                                MClipboard.rotation = Rotation.floatValue;
                                            }

                                            EditorGUI.BeginDisabledGroup(!MClipboard);
                                            {
                                                if (GUILayout.Button(EditorGUIUtility.IconContent("Clipboard", "Paste"),
                                                    GUILayout.Height(28)))
                                                {
                                                    if (MClipboard == null)
                                                    {
                                                        Debug.LogError("MClipboard is null, Nothing to paste");
                                                    }
                                                    else
                                                    {
                                                        GradHeight.floatValue = MClipboard.falloff;
                                                        GradPivot.vectorValue = MClipboard.pivot;
                                                        Rotation.floatValue = MClipboard.rotation;
                                                    }

                                                }
                                            }
                                            EditorGUI.EndDisabledGroup();
                                        }
                                        EditorGUI.EndDisabledGroup();
                                    }
                                    EditorGUILayout.EndVertical();
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            EditorGUI.EndDisabledGroup();
                            EditorGUI.EndDisabledGroup();
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    Color1.colorValue = Color.white;
                    Color2.colorValue = Color.white;
                }
            }
            else
            {
                Show.floatValue = 0;
            }
        }

        private void AOModule()
        {
            EnableAO = EditorGUILayout.Toggle("Enable", EnableAO);
            if (EnableAO)
            {
                targetMat.EnableKeyword("AO_ON");
                matEditor.TexturePropertySingleLine(new GUIContent("AO Map"), _AOTexture, _AOPower);
                
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.BeginHorizontal();
                {
                    matEditor.ColorProperty(_AOColor, "AO Color");
                    aouv = (AOuv) EditorGUILayout.EnumPopup(aouv);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                targetMat.DisableKeyword("AO_ON");
            }
        }

        private void LightmapModule()
        {
            EnableLmap = EditorGUILayout.Toggle("Enable", EnableLmap);
            if (EnableLmap)
            {
                lMapBlendMode = (LightMapBlendingMode) EditorGUILayout.EnumPopup("Mode", lMapBlendMode);
                if (lMapBlendMode == LightMapBlendingMode.Add)
                {
                    matEditor.RangeProperty(_LMPower, "Power");
                    targetMat.EnableKeyword("LIGHTMAP_ADD");
                    targetMat.DisableKeyword("LIGHTMAP_MULTIPLY");
                }
                else if (lMapBlendMode == LightMapBlendingMode.Multiply)
                {
                    matEditor.RangeProperty(_LMPower, "Power");
                    targetMat.EnableKeyword("LIGHTMAP_MULTIPLY");
                    targetMat.DisableKeyword("LIGHTMAP_ADD");
                }

                if (lMapBlendMode == LightMapBlendingMode.UseAsAO)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.HelpBox(
                        "Turn of all lights in the scene and bake lightmap with AO turned on and ambient color set to WHITE under the environment lighting settings, in the lightmap baking window.",
                        MessageType.Info);
                    matEditor.ColorProperty(_LMColor, "AO Color");
                    matEditor.RangeProperty(_LMPower, "Power");
                    targetMat.DisableKeyword("LIGHTMAP_ADD");
                    targetMat.DisableKeyword("LIGHTMAP_MULTIPLY");
                    EditorGUI.EndDisabledGroup();
                }
            }
            else
            {
                targetMat.DisableKeyword("LIGHTMAP_AO");
                targetMat.DisableKeyword("LIGHTMAP_ADD");
            }
        }

        private void GlobalGradientSettingsModule()
        {
            
            ShowGlobalGradientSettings =
                EditorGUILayout.Foldout(ShowGlobalGradientSettings, "Global Gradient Settings");
            
            EditorGUI.BeginDisabledGroup(true);
            if (ShowGlobalGradientSettings)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    _GradientHeight_G.floatValue = EditorGUILayoutExtended.FloatField(new GUIContent("Falloff"),
                        _GradientHeight_G.floatValue, 100);

                    _GradPivot_G.vectorValue = EditorGUILayout.Vector2Field("Pivot", _GradPivot_G.vectorValue);
                    EditorGUILayout.LabelField("Rotation");
                    _Rotation_G.floatValue = EditorGUILayout.Slider(_Rotation_G.floatValue, 0f, 360f);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void AmbientSettingsModule()
        {
            ShowAmbientSettings = EditorGUILayout.Foldout(ShowAmbientSettings, "Ambient Settings");
            
            EditorGUI.BeginDisabledGroup(true);
            if (ShowAmbientSettings)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    matEditor.ColorProperty(_AmbientColor, "Color");
                    matEditor.RangeProperty(_AmbientPower, "Power");
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void OtherSettings()
        {
            EditorGUILayout.BeginVertical(Indented);
            GlobalGradientSettingsModule();
            AmbientSettingsModule();
            EditorGUILayout.EndVertical();

            RimEnable = EditorGUILayout.Toggle("Rim", RimEnable);
            EditorGUI.BeginDisabledGroup(true);
            if (RimEnable)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    matEditor.ColorProperty(_RimColor, "Color");
                    matEditor.RangeProperty(_RimPower, "Power");
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUI.EndDisabledGroup();

            realtimeShadow = EditorGUILayout.Toggle("Realtime Shadow", realtimeShadow);
            EditorGUI.BeginDisabledGroup(true);
            if (realtimeShadow)
            {
                matEditor.ColorProperty(_ShadowColor, "Shadow Color");
            }
            else
            {
                targetMat.DisableKeyword("SHADOW_ON");
            }
            EditorGUI.EndDisabledGroup();

            dontMix = EditorGUILayout.Toggle("Don't mix shadings", dontMix);
            if (dontMix) targetMat.EnableKeyword("DONTMIX");
            else targetMat.DisableKeyword("DONTMIX");
            
            EditorGUI.BeginDisabledGroup(true);
            matEditor.RangeProperty(_Fade, "Fade");
            cullmode = (Cull) EditorGUILayout.EnumPopup("Cull", cullmode);
            blendingMode = (Mode) EditorGUILayout.EnumPopup("Blending Mode", blendingMode);
            EditorGUI.EndDisabledGroup();
            SetupMaterialMode(blendingMode, targetMat);
        }

        private void SetupMaterialMode(Mode _blendingMode, Material _material)
        {
            if (_blendingMode == Mode.Opaque)
            {
                _material.SetOverrideTag("RenderType", "Opaque");
                _material.renderQueue = (int) RenderQueue.Geometry;
                _material.SetInt("_SrcBlend", (int) BlendMode.One);
                _material.SetInt("_DstBlend", (int) BlendMode.Zero);
                _material.SetInt("_ZWrite", 1);
            }
            else if (_blendingMode == Mode.Transparent)
            {
                _material.SetOverrideTag("RenderType", "Transparent");
                _material.renderQueue = (int) RenderQueue.Transparent;
                _material.SetInt("_SrcBlend", (int) BlendMode.SrcAlpha);
                _material.SetInt("_DstBlend", (int) BlendMode.OneMinusSrcAlpha);
                _material.SetInt("_ZWrite", 0);
            }
        }

        private void FogModule()
        {
            EnableUnityFog = EditorGUILayout.Toggle("Unity fog", EnableUnityFog);
            if (EnableUnityFog) targetMat.EnableKeyword("UNITY_FOG");
            else targetMat.DisableKeyword("UNITY_FOG");

            EnableHFog = EditorGUILayout.Toggle("Height fog", EnableHFog);
            EditorGUI.BeginDisabledGroup(true);
            if (EnableHFog)
            {
                matEditor.ColorProperty(_Color_Fog, "Color");
                matEditor.FloatProperty(_FogYStartPos, "Height");
                matEditor.FloatProperty(_FogHeight, "Falloff");
            }
            EditorGUI.EndDisabledGroup();
        }

        private void ColorCorrectionModule()
        {
            ColorCorrectionEnable = EditorGUILayout.Toggle("Enable", ColorCorrectionEnable);
            EditorGUI.BeginDisabledGroup(true);
            if (ColorCorrectionEnable)
            {
                
                matEditor.RangeProperty(_Saturation, "Saturation");
                matEditor.RangeProperty(_Brightness, "Brightness");
                matEditor.ColorProperty(_TintColor, "Tint");
            }
            EditorGUI.EndDisabledGroup();
        }


        void SceneGUI(SceneView _sceneview)
        {
            if (!isAnythingSelected()) gradientHandle = null;

            if (gradientHandle != null)
            {
                Tools.hidden = true;
                EditorGUI.BeginChangeCheck();

                Vector3 pivot = new Vector3(), falloff = new Vector3();
                float rotation = 0;

                if (gradientHandle.profile == "LeftGradient" || gradientHandle.profile == "RightGradient")
                {
                    pivot = Handles.PositionHandle(gradientHandle.pivot, Quaternion.identity);
                    gradientHandle.pivot = new Vector3(gradientHandle.pivot.x, pivot.y, pivot.z);
                    falloff = Handles.PositionHandle(gradientHandle.falloff, Quaternion.identity);
                    gradientHandle.falloff = new Vector3(gradientHandle.falloff.x, falloff.y, falloff.z);
                    rotation = Vector3.SignedAngle(falloff - pivot, Vector3.up, Vector3.right);
                }
                else if (gradientHandle.profile == "FrontGradient" || gradientHandle.profile == "BackGradient")
                {
                    pivot = Handles.PositionHandle(gradientHandle.pivot, Quaternion.identity);
                    gradientHandle.pivot = new Vector3(pivot.x, pivot.y, gradientHandle.pivot.z);
                    falloff = Handles.PositionHandle(gradientHandle.falloff, Quaternion.identity);
                    gradientHandle.falloff = new Vector3(falloff.x, falloff.y, gradientHandle.falloff.z);
                    rotation = Vector3.SignedAngle(falloff - pivot, Vector3.up, Vector3.back);
                }
                else
                {
                    pivot = Handles.PositionHandle(gradientHandle.pivot, Quaternion.identity);
                    gradientHandle.pivot = new Vector3(pivot.x, gradientHandle.pivot.y, pivot.z);
                    falloff = Handles.PositionHandle(gradientHandle.falloff, Quaternion.identity);
                    gradientHandle.falloff = new Vector3(falloff.x, gradientHandle.falloff.y, falloff.z);
                    rotation = Vector3.SignedAngle(falloff - pivot, Vector3.forward, Vector3.up);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(targetMat, "Undo gradient values");
                    Undo.RecordObject(gradientHandle, "Undo gradient handles");

                    Vector3 objPos = selectedObject.position;
                    if (gSpace == GradientSpace.WorldSpace) objPos = Vector3.zero;

                    if (gradientHandle.profile == "FrontGradient" || gradientHandle.profile == "BackGradient")
                    {
                        RecorderProps[gradientHandle.Ystart].vectorValue =
                            new Vector2(gradientHandle.pivot.x - objPos.x, gradientHandle.pivot.y - objPos.y);
                    }
                    else if (gradientHandle.profile == "LeftGradient" || gradientHandle.profile == "RightGradient")
                    {
                        RecorderProps[gradientHandle.Ystart].vectorValue =
                            new Vector2(gradientHandle.pivot.z - objPos.z, gradientHandle.pivot.y - objPos.y);
                    }
                    else
                    {
                        RecorderProps[gradientHandle.Ystart].vectorValue =
                            new Vector2(gradientHandle.pivot.z - objPos.x, gradientHandle.pivot.x - objPos.z);
                    }

                    RecorderProps[gradientHandle.Height].floatValue =
                        Vector3.Distance(gradientHandle.pivot, gradientHandle.falloff);
                    RecorderProps[gradientHandle.rotation].floatValue = Remap(rotation, -180, 180, 360, 0);
                    matEditor.Repaint();
                }

                Handles.DrawDottedLine(gradientHandle.pivot, gradientHandle.falloff, 5);
            }
            else
            {
                Tools.hidden = false;
            }
        }

        //Helper Classes and Methods
        bool IsGlobal(GradientSettings settings)
        {
            if (settings == GradientSettings.UseGlobalGradientSettings) return true;
            else return false;
        }

        bool isAnythingSelected()
        {
            if (Selection.activeTransform) return true;
            return false;
        }

        float WrapAngle(float angle)
        {
            if (angle > 360)
            {
                angle = angle - 360;
            }
            else if (angle < 0)
            {
                angle = 360 + angle;
            }

            return angle;
        }

        float Remap(float value, float low1, float high1, float low2, float high2)
        {
            return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
        }

        Texture2D GetTexture(Color color1, Color color2, bool Horizontal)
        {
            Texture2D tex;
            Color[] cols = new Color[64];

            if (Horizontal)
            {
                for (int i = 0; i < 64; i++)
                {
                    cols[i] = Color.Lerp(color1, color2, (float) i / 63);
                }

                tex = new Texture2D(64, 1);
            }
            else
            {
                for (int i = 0; i < 64; i++)
                {
                    cols[i] = Color.Lerp(color2, color1, (float) i / 63);
                }

                tex = new Texture2D(1, 64);
            }

            tex.SetPixels(cols);
            tex.Apply();
            return tex;
        }

        class GradientSettingsHolder
        {
            public ShadingMode Mode;
            public GradientSettings gradSettings;
            public Color color1;
            public Color color2;
            public float gradHeight;
            public float gradYPos;
            public float Rotation;

            public GradientSettingsHolder(ShadingMode _mode, GradientSettings _settings, Color _color1, Color _color2,
                float _gradHeight, float _gradYPos, float _Rotation)
            {
                Mode = _mode;
                gradSettings = _settings;
                color1 = _color1;
                color2 = _color2;
                gradHeight = _gradHeight;
                gradYPos = _gradYPos;
                Rotation = _Rotation;
            }
        }

        class GradientHandle : ScriptableObject
        {
            public Vector3 pivot;
            public Vector3 falloff;
            public string profile;
            public string Ystart;
            public string Height;
            public string gizmo;
            public string rotation;
        }

        public class Style
        {
            public static GUIContent MainTexture = new GUIContent("Texture",
                "Albedo (RGB) and Transparency (A).\nChange Blending Mode to Transparent to use transparency");

            public static GUIContent MainTexturePower = new GUIContent("Power", "Strength of the main texture");

        }
    }
}