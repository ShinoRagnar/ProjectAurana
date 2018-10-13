using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/*public interface GameUI
{
    Fade Fade(Fade fade);
    void Hide(bool fade);
    void Show(bool fade);
}*/
public class UIContainer {

    //private static readonly float FADE_DURATION = MenuSystem.MENU_TRANSITION_FADE_TIME;

    public static readonly string SPRITE_NODE_NAME = "Image";

    //The panels that the prefabs reside in
    private Transform[] panels;
    //The original prefabs
    public Transform[] prefabs;
    //The instantiated prefabs
    private Transform[] visibles;

    private Sprite[] sprites;

    //The positions of the instantiated prefabs
    private RectTransform[] positions;
    //The original prefab distances
    private Vector3[] originalPositions;
    //The last position we moved to (used by fade)
    //private Vector3 lastPosition;

    //Duration slider
    private Transform[] prefabsContainingDuration;
    private Transform[] visiblesContainingDuration;
    private RectTransform[] durations;
    private string[] durationNames;

    //Text
    private Transform[] prefabsContainingText;
    private Transform[] visiblesContainingText;
    private TextMeshProUGUI[] text;

    public float currentFadeDuration = 0;

    //
    public FadeDirection fadeDirection;
    public MenuGroup menu;
    public string name;

    //Instantiated
    public bool instantiated = false;
    //Active = false
    public bool hidden = false;
    //Fade
    /*public Fade fade
    {
        get
        {
            if(currentFadeDuration == FADE_DURATION) { 

                return global::Fade.FadeOut;

            }else if (currentFadeDuration == 0)
            {
                return global::Fade.FadeIn;
            }
            return global::Fade.Fading;
        }
        set
        {
            if(value == global::Fade.FadeIn)
            {
                currentFadeDuration = 0;
                if (!wasHidden && instantiated)
                {
                    this.Show(true);
                }
            }
            else if((value == global::Fade.FadeOut))
            {
                currentFadeDuration = FADE_DURATION;
                Hide(true);
            }
            else if ((value == global::Fade.Fading))
            {
                if (!wasHidden && instantiated)
                {
                    this.Show(true);
                }
            }
        }
    }*/
    //Used by fade to restore
    public bool wasHidden = false;


    public Transform this[int i]
    {
        get { return visibles[i]; }
    }

    public UIContainer(
        string nameVal,
        Transform panel,
        Transform prefab,
        /*Sprite sprites,*/
        FadeDirection fadeDirectionVal = FadeDirection.NoFade,
        MenuGroup belongsToMenu = MenuGroup.NoMenu
     ) : this(nameVal, new Transform[] { panel }, new Transform[] { prefab }, new Sprite[] { }, null,null,null,fadeDirectionVal,belongsToMenu){ }

    public UIContainer(
        //The two below must be the same length
        string nameVal,
        Transform[] panelsVal, 
        Transform[] prefabsVal,
        Sprite[] spritesVal,
        //The two below must be same length
        Transform[] prefabContainingDurationVal = null, 
        string[] durationNameVal = null, 
        //Any length
        Transform[] prefabContainingTextVal = null,
        //Fade is optional
        FadeDirection fadeDirectionVal = FadeDirection.NoFade,
        MenuGroup belongsToMenu = MenuGroup.NoMenu)
    {
        this.sprites = spritesVal;
        this.name = nameVal;
        this.menu = belongsToMenu;
        this.fadeDirection = fadeDirectionVal;
        this.panels = panelsVal;
        this.prefabs = prefabsVal;
        this.visibles = new Transform[panelsVal.Length];
        this.positions = new RectTransform[panelsVal.Length];
        this.originalPositions = new Vector3[panelsVal.Length];
        this.prefabsContainingDuration = prefabContainingDurationVal;
        this.durationNames = durationNameVal;
        this.prefabsContainingText = prefabContainingTextVal;

        if(prefabsContainingDuration != null) { 
            this.visiblesContainingDuration = new Transform[prefabsContainingDuration.Length];
            this.durations = new RectTransform[prefabsContainingDuration.Length];
        }

        if (prefabContainingTextVal != null)
        {
            this.visiblesContainingText = new Transform[prefabContainingTextVal.Length];
            this.text = new TextMeshProUGUI[prefabContainingTextVal.Length];
        }

        //Register this component so we can fade it in and out with our menusystem
        
    }
    public T GetComponent<T>(string name, int num = 0)
    {
        return Global.FindDeepChild(visibles[num], name).GetComponent<T>();
    }
   

    public TextMeshProUGUI GetText(int i = 0)
    {
        return text[i];
    }
    public void ScaleDuration(float length, float prcntg, int scaleIgnore, bool yAxis, int scaleNum = 0)
    {
        int durPosition = ((int)(prcntg * length)) / scaleIgnore;
        int oldVal = 0;
        if (yAxis)
        {
            oldVal = ((int)durations[scaleNum].offsetMin.y) / scaleIgnore;
        }
        else
        {
            oldVal = ((int)durations[scaleNum].offsetMin.x) / scaleIgnore;
        }

        if (oldVal != durPosition)
        {
            if (yAxis) { 
                durations[scaleNum].offsetMin = new Vector2(durations[scaleNum].offsetMin.x, (float)durPosition * scaleIgnore);
            }
            else
            {
                durations[scaleNum].offsetMin = new Vector2((float)durPosition * scaleIgnore, durations[scaleNum].offsetMin.y);
            }
        }
    }

    public void MoveX(float x)// bool fade = false)
    {
        Move(new Vector3(x, Mathf.NegativeInfinity));//, fade);
    }
    public void MoveY(float y)//, bool fade = false)
    {
        Move(new Vector3(Mathf.NegativeInfinity, y));//, fade);
    }
   
    private void Move(Vector3 move)//, bool fade = false)
    {
        for(int i = 0; i < positions.Length; i++)
        {
            RectTransform rt = positions[i];

            if(rt == null)
            {
                Debug.Log(instantiated);
            }
            Vector3 moveTo = new Vector3(rt.localPosition.x, rt.localPosition.y, rt.localPosition.z);

            if (move.x != Mathf.NegativeInfinity)
            {
                moveTo.x = originalPositions[i].x+move.x;
            }
            if (move.y != Mathf.NegativeInfinity)
            {
                moveTo.y = originalPositions[i].y+move.y;
            }
            if (move.z != Mathf.NegativeInfinity)
            {
                moveTo.z = originalPositions[i].z+move.z;

            }
           // Vector3 pos = new Vector3(x, y, z);

            /*if (!fade)
            {
                lastPosition = moveTo-originalPositions[i];
            }*/
            rt.localPosition = moveTo;
        }
    }
    public void Hide()//bool fade = false)
    {
        if (instantiated && !hidden)
        {
            foreach (Transform h in visibles)
            {
                h.gameObject.SetActive(false);
            }
            hidden = true;
           /* if (!fade)
            {
                wasHidden = true;
            }*/
        }
    }

   /* public Fade Fade(Fade f = global::Fade.FadeOut)
    {
        if (fadeDirection == FadeDirection.NoFade)
        {
            fade = f;
            return fade;
        }
        else
        {
            float t = currentFadeDuration / FADE_DURATION;
            t = t * t * t * (t * (6f * t - 15f) + 10f); //1f - Mathf.Cos(t * Mathf.PI * 0.5f); // t * t * t * (t * (6f * t - 15f) + 10f);

            if (f == global::Fade.FadeOut)
            {
                currentFadeDuration += Time.unscaledDeltaTime; //Global.globalTime;
            }
            else if (f == global::Fade.FadeIn)
            {
                currentFadeDuration -= Time.unscaledDeltaTime;
            }

            currentFadeDuration = Mathf.Min(Mathf.Max(currentFadeDuration, 0), FADE_DURATION);
            //Debug.Log(currentFadeDuration);


            if (fadeDirection == FadeDirection.FadeLeft)
            {
                MoveX(Mathf.Lerp(lastPosition.x, -200, t), true);
            }
            if (fadeDirection == FadeDirection.FadeUp)
            {
                MoveY(Mathf.Lerp(lastPosition.y, 200, t), true);
            }

            //See get/set in fade variable
            if (f == fade)
            {
                //Debug.Log("Ewuals");
                fade = f;
            }
            else
            {
                fade = global::Fade.Fading;
            }

            return fade;
        }
    }*/

    


    public void Show(bool fad = false)
    {
        if (!instantiated) {
            for(int i = 0; i < prefabs.Length; i++)
            {
                //Debug.Log("Called by: "+caller+" Creating ["+i+"]: for " + name);
                Transform visi = Global.Create(prefabs[i], panels[i]);
                if(i < sprites.Length)
                {
                    Global.FindDeepChild(visi, SPRITE_NODE_NAME).GetComponent<Image>().sprite = sprites[i];
                }
                visibles[i] = visi;
                positions[i] = visi.GetComponent<RectTransform>();
                originalPositions[i] = prefabs[i].localPosition;
                //lastPosition = positions[i].localPosition;

                //Match against duration prefabs
                //Match against duration prefabs
                if(prefabsContainingDuration != null) {
                    for (int j = 0; j < prefabsContainingDuration.Length; j++)
                    {
                        Transform prDur = prefabsContainingDuration[j];
                        if (prefabs[i] == prDur)
                        {
                            visiblesContainingDuration[j] = visi;
                            durations[j] = Global.FindDeepChild(visi, durationNames[j]).GetComponent<RectTransform>();
                        }
                    }
                }
                //Match against text prefabs
                if(prefabsContainingText != null)
                {
                    for (int j = 0; j < prefabsContainingText.Length; j++)
                    {
                        Transform prText = prefabsContainingText[j];
                        if (prefabs[i] == prText)
                        {
                            visiblesContainingText[j] = visi;
                            text[j] = visi.GetComponentInChildren<TextMeshProUGUI>();
                        }
                    }
                }
            }
            //Global.instance.menues.Register(this);
            instantiated = true;
        }
        else
        {
            if (hidden){// && (fad || fade != global::Fade.FadeOut) ) {
                foreach (Transform h in visibles)
                {
                    h.gameObject.SetActive(true);
                }
               /* if (!fad)
                {
                    wasHidden = false;
                }*/
                hidden = false;
            }
        }
    }


}
