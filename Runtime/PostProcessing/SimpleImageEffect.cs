using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable 649

[ExecuteInEditMode]
public class SimpleImageEffect : MonoBehaviour
{
    // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
    public Material EffectMaterial => m_effectMat;

    [HideInInspector]
    [SerializeField]
    Material m_effectMat;
    
    [HideInInspector]
    [SerializeField]
    Shader m_shader;

    protected virtual void OnValidate()
    {
        if (m_shader == null)
        {
            m_effectMat = null;
            return;
        }

        if (m_effectMat == null) m_effectMat = new Material(m_shader);
        m_effectMat.shader = m_shader;
    }

    protected virtual void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (m_effectMat == null) Graphics.Blit(src, dst);
        else Graphics.Blit(src, dst, m_effectMat);
    }
}