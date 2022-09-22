using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class ParallaxLink {
    [SerializeField]
    public GameObject background;
    [SerializeField]
    public GameObject linked;
}

public class ParallaxLinker : MonoBehaviour
{
    private static ParallaxLinker parallaxLinker;

    [Header("Parallax Settings")]
    [SerializeField]
    private ParallaxLink[] backgroundGoLinks;

    private ParallaxLinker() { }

    public static ParallaxLinker instance
    {
        get
        {
            if (!parallaxLinker)
            {
                parallaxLinker = FindObjectOfType(typeof(ParallaxLinker)) as ParallaxLinker;
                if (!parallaxLinker)
                {
                    Debug.LogError("There needs to be one active ParallaxLinker script on a GameObject in your scene.");
                }
                else
                {
                    parallaxLinker.Init();
                    DontDestroyOnLoad(parallaxLinker);
                }
            }
            return parallaxLinker;
        }
    }

    void Init()
    {
        Debug.LogError("Init ParallaxLinker");
    }


    void Start()
    {
        // I want this to be added at launch, if was not yet
        foreach (ParallaxLink link in backgroundGoLinks) {
            if (link.linked.GetComponent<ParallaxBackground>() == null)
            {
                Undo.RecordObject(link.linked, typeof(ParallaxBackground) + " was added to " + link.linked);
                ParallaxBackground t = link.linked.AddComponent<ParallaxBackground>();
                ParallaxBackground p = link.background.GetComponent<ParallaxBackground>();
                t.parallaxMulX = p.parallaxMulX;
                t.parallaxMulY = p.parallaxMulY;
            }
            else {
                ParallaxBackground t = link.linked.GetComponent<ParallaxBackground>();
                ParallaxBackground p = link.background.GetComponent<ParallaxBackground>();
                if (t.parallaxMulX != p.parallaxMulX || t.parallaxMulY != p.parallaxMulY) {
                    Undo.RecordObject(link.linked, typeof(ParallaxBackground) + " modified " + link.linked);
                    t.parallaxMulX = p.parallaxMulX;
                    t.parallaxMulY = p.parallaxMulY;
                }
            }

            if (link.linked.GetComponent<SpriteRenderer>() == null)
            {
                Undo.RecordObject(link.linked, typeof(SpriteRenderer) + " was added to " + link.linked);
                SpriteRenderer t = link.linked.AddComponent<SpriteRenderer>();
                SpriteRenderer p = link.background.GetComponent<SpriteRenderer>();
                t.sprite = p.sprite;
            }
            else{
                SpriteRenderer t = link.linked.GetComponent<SpriteRenderer>();
                SpriteRenderer p = link.background.GetComponent<SpriteRenderer>();
                if (t.sprite != p.sprite)
                {
                    Undo.RecordObject(link.linked, typeof(SpriteRenderer) + " modified " + link.linked);
                    t.sprite = p.sprite;
                }

            }
        }
    }
}
