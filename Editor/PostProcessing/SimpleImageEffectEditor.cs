using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SimpleImageEffect), true)]
public class SimpleImageEffectEditor : Editor
{
    MaterialEditor m_materialEditor;

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        serializedObject.UpdateIfRequiredOrScript();

        var effectMat = ((SimpleImageEffect) target).EffectMaterial;
        if (effectMat != null && m_materialEditor == null) m_materialEditor = (MaterialEditor)CreateEditor(effectMat, typeof(MaterialEditor));

        DrawDefaultInspector(true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_shader"));
        DrawDefaultInspector(false);

        if (m_materialEditor != null)
        {
            MaterialProperty[] props = MaterialEditor.GetMaterialProperties(new[] {m_materialEditor.target}).Where(p => p.name != "_MainTex").ToArray();

            if (props.Length > 0) GuiLine();

            var foldoutShow = typeof(Editor).GetProperty("firstInspectedEditor",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (foldoutShow != null)
            {
                foldoutShow.SetValue(m_materialEditor, true);
            }
            else
            {
                m_materialEditor.DrawHeader();
            }

            DrawMaterialProps(props);

            ResetMaterialEditor();
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

    void OnDisable()
    {
        ResetMaterialEditor();
    }

    void ResetMaterialEditor()
    {
        if (m_materialEditor == null) return;
        DestroyImmediate(m_materialEditor);
        m_materialEditor = null;
    }

    static void GuiLine(int height = 1)
    {
        var rect = EditorGUILayout.GetControlRect(false, height + 10);
        rect.y += 7f;
        rect.height = height;
        EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.1f, 0.1f, 0.1f, 1) : Color.gray);
    }

    void DrawDefaultInspector(bool script)
    {
        if (script)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), true);
            }

            return;
        }

        var iterator = serializedObject.GetIterator();
        for (var enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
        {
            if (iterator.propertyPath != "m_Script") EditorGUILayout.PropertyField(iterator, true);
        }
    }

    void DrawMaterialProps(IEnumerable<MaterialProperty> props)
    {
        m_materialEditor.SetDefaultGUIWidths();
        var info = typeof(MaterialEditor).GetField("m_InfoMessage", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(m_materialEditor)?.ToString();
        int? ctrlHash = (int?) (typeof(MaterialEditor)
            .GetField("s_ControlHash", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(m_materialEditor));

        if (info != null)
            EditorGUILayout.HelpBox(info, MessageType.Info);
        else if (ctrlHash != null)
            GUIUtility.GetControlID(ctrlHash.Value, FocusType.Passive, new Rect(0.0f, 0.0f, 0.0f, 0.0f));

        foreach (var p in props)
        {
            if ((p.flags & (MaterialProperty.PropFlags.HideInInspector | MaterialProperty.PropFlags.PerRendererData)) == 0)
                m_materialEditor.ShaderProperty(
                    EditorGUILayout.GetControlRect(true, m_materialEditor.GetPropertyHeight(p, p.displayName),
                        EditorStyles.layerMaskField), p, p.displayName);
        }
    }
}
