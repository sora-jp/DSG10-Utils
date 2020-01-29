using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEditor.Rendering.PostProcessing;

[PostProcessEditor(typeof(SimpleStackEffect))]
public sealed class SimpleStackEffectEditor : PostProcessEffectEditor<SimpleStackEffect>
{
    SerializedParameterOverride m_shader;
    List<SerializedParameterOverride> m_params;

    /// <inheritdoc />
    public override void OnEnable()
    {
        m_shader = FindParameterOverride(x => x.shader);

        m_params = new List<SerializedParameterOverride>();

        var fields = GetType().GetProperty("target", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this).GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(t => t.FieldType.IsSubclassOf(typeof(ParameterOverride)) && t.Name != "enabled")
            .Where(t =>
                (t.IsPublic && t.GetCustomAttributes(typeof(NonSerializedAttribute), false).Length == 0)
                || (t.GetCustomAttributes(typeof(UnityEngine.SerializeField), false).Length > 0)
            )
            .ToList();

        foreach (var field in fields)
        {
            var property = ((SerializedObject)GetType().GetProperty("serializedObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).FindProperty(field.Name);
            var attributes = field.GetCustomAttributes(false).Cast<Attribute>().ToArray();

            Debug.Log(field.Name + " - " + (property == null));

            var parameter = (SerializedParameterOverride)Activator.CreateInstance(typeof(SerializedParameterOverride), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance, null, new object[] {property, attributes}, null);
            m_params.Add(parameter);
        }
    }

    /// <inheritdoc />
    public override void OnInspectorGUI()
    {
        foreach (var parameter in m_params)
            PropertyField(parameter);

        //var s = (Shader) m_shader.value.objectReferenceValue;
        //if (s == null) return;
    }
}