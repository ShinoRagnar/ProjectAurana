using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
[RequireComponent(typeof(ShaderTerrain))]
public class ShaderTerrainSampler : MonoBehaviour {

    private static readonly string ALBEDO = "_MainTex";
    private static readonly string BUMP = "_BumpMap";
    private static readonly string HEIGHT = "_ParallaxMap";
    private static readonly string OCCLUSION = "_OcclusionMap";
    private static readonly string DETAIL_ALBEDO = "_DetailAlbedoMap";
    private static readonly string DETAIL_BUMP = "_DetailNormalMap";


    public bool update = false;
    public Material shader = null;
    public Material fallback = null;
    public Material colors = null;

    public Texture2D test = null;

    [Header("")]
    public ShaderTerrain terrain = null;
    public float[] heights;

    [Header("Texture settings")]
    [Range(0.01f, 1)]
    public float fallbackHeight = 0.2f;
    [Range(0, 2)]
    public float textureScale = 0.2f;
    [Range(0, 2)]
    public float detailScale = 1.5f;
    [Range(-2, 2)]
    public float bumpScale = 1;
    [Range(0.02f, 0.08f)]
    public float parallaxHeight = 0.08f;
    [Range(0.0f, 1)]
    public float colorGlow = 0.2f;

    [Header("Materials")]
    public int size = 2048;

    public Material[] albedoMat;
    public TextureFormat albedoFormat;
    private Texture2DArray albedo;

    public Material[] bumpMat;
    public TextureFormat bumpFormat;
    private Texture2DArray bump;

    public Material[] heightMat;
    public TextureFormat heightFormat;
    private Texture2DArray height;

    public Material[] occlusionMat;
    public TextureFormat occlusionFormat;
    private Texture2DArray occlusion;

    [Header("Materials Details")]
    public int sizeDetails = 1024;

    public Material[] detailAlbedoMat;
    public TextureFormat detailAlbedoFormat;
    private Texture2DArray detailAlbedo;

    public Material[] detailBumpMat;
    public TextureFormat detailBumpFormat;
    private Texture2DArray detailBump;

    

    public Material GetShadedMaterial() {

        Material mat = new Material(shader);

        mat.SetFloat("_TextureScale", textureScale);
        mat.SetFloat("_DetailScale", detailScale);

        mat.SetFloat("_SamplerCount", Count);
        mat.SetFloat("_Parallax", parallaxHeight);


        mat.SetFloatArray("_HeightScales", heights);
        mat.SetTexture("_Albedo", albedo);
        mat.SetTexture("_Bump", bump);
        mat.SetTexture("_Height", height);
        mat.SetTexture("_Occlusion", occlusion);
        mat.SetTexture("_DetailAlbedo", detailAlbedo);
        mat.SetTexture("_DetailBump", detailBump);

        mat.SetTexture("_BumpMapColor", colors.GetTexture("_BumpMap"));

       // Texture2D tex = new Texture2D(size, size, TextureFormat.DXT5, true);

      //  Graphics.CopyTexture(fallback.mainTexture,0,tex,0);


       // mat.SetTexture("_Test", tex);


        return mat;
    }


    public int Count {
        get { return albedoMat.Length;  }
    }
    public Dictionary<Material, int> lookup;

    public void OnValidate()

    {
        if (update)
        {

            ExecuteUpdate();
        }


    }

    public void ExecuteUpdate() {

        if (fallback == null)
        {
            Debug.Log("Fallback material not set!!!");
        }
        else {

            if (terrain == null) {
                terrain = GetComponent<ShaderTerrain>();
            }

            Debug.Log("Materials found: " + terrain.materials.Count+" + fallback");

            int count = terrain.materials.Count + 1;

            heights = new float[count];
            albedoMat = new Material[count];
            bumpMat = new Material[count];
            heightMat = new Material[count];
            occlusionMat = new Material[count];
            detailAlbedoMat = new Material[count];
            detailBumpMat = new Material[count];

            albedoFormat = ((Texture2D)fallback.mainTexture).format;
            bumpFormat = ((Texture2D)fallback.GetTexture(BUMP)).format;
            heightFormat = ((Texture2D)fallback.GetTexture(HEIGHT)).format;
            occlusionFormat = ((Texture2D)fallback.GetTexture(OCCLUSION)).format;
            detailAlbedoFormat = ((Texture2D)fallback.GetTexture(DETAIL_ALBEDO)).format;
            detailBumpFormat = ((Texture2D)fallback.GetTexture(DETAIL_BUMP)).format;

            albedo = new Texture2DArray(size, size, count, TextureFormat.RGBA32, false);

           // albedo = new Texture2DArray(size,size, count, albedoFormat, true);
            bump = new Texture2DArray(size, size, count, bumpFormat, true);
            height = new Texture2DArray(size, size, count, heightFormat, true);
            occlusion = new Texture2DArray(size, size, count, occlusionFormat, true);

            detailAlbedo = new Texture2DArray(sizeDetails, sizeDetails, count, detailAlbedoFormat, true);
            detailBump = new Texture2DArray(sizeDetails, sizeDetails, count, detailBumpFormat, true);

            //Debug.Log("Fallback format: " + ((Texture2D)fallback.mainTexture).format.ToString());
            //test = new Texture2D(2048,2048,TextureFormat.DXT5,true);
            //Graphics.CopyTexture(fallback.mainTexture, 0, test, 0);
            Texture2D fallb = (Texture2D)fallback.mainTexture;

            albedo.SetPixels(fallb.GetPixels(), 0);
            
            //Texture2D fallb = (Texture2D)fallback.mainTexture;

            //Graphics.CopyTexture(fallb, 0, albedo,0);


            Graphics.CopyTexture(fallback.GetTexture(BUMP), 0, bump, 0);
            Graphics.CopyTexture(fallback.GetTexture(HEIGHT), 0, height, 0);
            Graphics.CopyTexture(fallback.GetTexture(OCCLUSION), 0, occlusion, 0);
            Graphics.CopyTexture(fallback.GetTexture(DETAIL_ALBEDO), 0, detailAlbedo, 0);
            Graphics.CopyTexture(fallback.GetTexture(DETAIL_BUMP), 0, detailBump, 0);

            lookup = new Dictionary<Material, int>();

            lookup.Add(fallback, 0);

            heights[0] = fallbackHeight;
            albedoMat[0] = fallback;
            bumpMat[0] = fallback;
            heightMat[0] = fallback;
            occlusionMat[0] = fallback;
            detailAlbedoMat[0] = fallback;
            detailBumpMat[0] = fallback;

            int index = 1;
            foreach (Material m in terrain.materials) {

                lookup.Add(m, index);
                heights[index] = terrain.materials[m];

                //CopyTexture(m, fallback, albedoMat, index, albedo, albedoFormat, ALBEDO);
                CopyTexture(m, fallback, bumpMat, index, bump, bumpFormat, BUMP);
                CopyTexture(m, fallback, heightMat, index, height, heightFormat, HEIGHT);
                CopyTexture(m, fallback, occlusionMat, index, occlusion, occlusionFormat, OCCLUSION);
                CopyTexture(m, fallback, detailAlbedoMat, index, detailAlbedo, detailAlbedoFormat, DETAIL_ALBEDO);
                CopyTexture(m, fallback, detailBumpMat, index, detailBump, detailBumpFormat, DETAIL_BUMP);
                
                index++;
            }

            //foreach (Material m in terrain.materials) {


            //}
        }
    }

    private static void CopyTexture(
        Material m, 
        Material fallback, 
        Material[] show, 
        int index, 
        Texture2DArray target, 
        TextureFormat tf,
        string textureName
        ) {

        bool copied = false;

        if (m.HasProperty(textureName))
        {
            Texture tex = m.GetTexture(textureName);

            if (((Texture2D)tex).format == tf)
            {

                Graphics.CopyTexture(tex, 0, target, index);
                show[index] = m;
                copied = true;
            }
            else {
                Debug.Log("Invalid texture format for material: " + m.name + " using format: " + ((Texture2D)tex).format.ToString() + " which does not match format: " + tf.ToString());
            }
        }

        //Use fallback texture
        if (!copied) {
            Graphics.CopyTexture(fallback.GetTexture(textureName), 0, target, index);
            show[index] = fallback;
            
        }
    }
}
*/