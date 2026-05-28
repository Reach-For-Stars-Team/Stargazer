using System;
using System.Collections.Generic;
using Reactor.Utilities.Attributes;
using UnityEngine;

/// <summary>
/// Used to apply materials to the camera.
/// </summary>
[RegisterInIl2Cpp]
public class CameraFXComponent(IntPtr ptr) : MonoBehaviour(ptr)
{
    public static CameraFXComponent Instance;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        
        Instance = this;
    }

    public List<Material> materials = new List<Material>();

    public void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        if (materials.Count == 0)
        {
            Graphics.Blit(source, dest);
            return;
        }

        RenderTexture current = source;
        RenderTexture temp = null;

        for (int i = 0; i < materials.Count; i++)
        {
            if (i == materials.Count - 1)
            {
                Graphics.Blit(current, dest, materials[i]);
            }
            else
            {
                temp = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
                Graphics.Blit(current, temp, materials[i]);
                if (current != source)
                    RenderTexture.ReleaseTemporary(current);
            
                current = temp;
            }
        }
    }
}