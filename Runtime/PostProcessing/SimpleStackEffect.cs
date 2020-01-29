using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Shader = UnityEngine.Shader;

[Serializable]
[PostProcess(typeof(SimpleStackEffectRenderer), PostProcessEvent.AfterStack, "Custom/Simple Stack Effect")]
public class SimpleStackEffect : PostProcessEffectSettings
{
    [SerializeField]
    public ShaderParameter shader = new ShaderParameter("Unlit/Texture");
}

public sealed class SimpleStackEffectRenderer : PostProcessEffectRenderer<SimpleStackEffect>
{
    public override void Render(PostProcessRenderContext context)
    {
        //var sheet = context.propertySheets.Get((Shader)null);
        context.command.Blit(context.source, context.destination);
    }
}

[Serializable]
public class ShaderParameter : ParameterOverride<Shader>
{
    [SerializeField]
    string m_name;

    public ShaderParameter(string name)
    {
        m_name = name;
        value = null;
    }

    protected override void OnEnable()
    {
        value = Shader.Find(m_name);
        base.OnEnable();
    }
}