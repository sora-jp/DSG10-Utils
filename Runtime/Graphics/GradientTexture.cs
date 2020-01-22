using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Gradient Texture", menuName = "Gradient Texture")]
public class GradientTexture : ScriptableObject
{
    public Gradient gradient;
    public int resolution = 512;
    [HideInInspector] public Texture2D generatedTexture;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (generatedTexture != null && generatedTexture.width == resolution) return;
        
        if (generatedTexture != null) AssetDatabase.RemoveObjectFromAsset(generatedTexture);
        generatedTexture = new Texture2D(resolution, 1)
        {
            name = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this))
        };
        AssetDatabase.AddObjectToAsset(generatedTexture, this);
        AssetDatabase.SaveAssets();
    }
#endif

    void OnEnable()
    {
        var colors = new Color[resolution];
        for (var i = 0; i < resolution; i++)
        {
            colors[i] = gradient.Evaluate((float)i / (resolution - 1));
        }
        generatedTexture.SetPixels(colors);
        generatedTexture.Apply(true);
    }

    public static implicit operator Texture2D(GradientTexture tex) => tex.generatedTexture;
    public static implicit operator Texture(GradientTexture tex) => tex.generatedTexture;
}
