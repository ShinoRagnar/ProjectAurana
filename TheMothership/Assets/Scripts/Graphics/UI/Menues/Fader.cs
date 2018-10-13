using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum FadePattern
{
    FadeAndStay,
    FadeInAndOut,
    Pulse
   // InnerPulse //Used internally
}
public enum FadePriority
{
    Visibility,
    Marking,
    StickyMarking,
    Hover,
    Selection,
    MarkSelection,
    Top = 99
}
public class Fader {

    //DictionaryList<int, Fader> faderQueue = new DictionaryList<int, Fader>();
    private static readonly float PULSE_SWITCH_THRESHOLD = 0.6f;
    private static int MAX_LAYER = 999;
   // private static readonly float DEFAULT_PULSE_TIME = (1-PULSE_SWITCH_THRESHOLD)*6;

    private DictionaryList<int,FaderLayer> layers = new DictionaryList<int,FaderLayer>();
    List<FaderTarget> targets = new List<FaderTarget>();
    float currentTime = 0;

    private class FaderLayer
    {
        public Color color;
        public Fade fade;
        public FadePriority prio;
        public FadePattern pattern;
        public float[] time;
        public int timeSlot;

        public FaderLayer(Color c, Fade fad, FadePriority pr, FadePattern f, float[] tim)
        {
            fade = fad;
            color = c;
            prio = pr;
            pattern = f;
            time = tim;
            timeSlot = 0;
        }
    }
    private class FaderTarget
    {
        public RawImage rawImage;
        public Image image;
        public TextMeshProUGUI textmesh;
        //public Color startColorText;

        public Color startColor;
        public Color colorLastFrame;

        public FaderTarget(Image im){
            this.image = im;
            this.startColor = im.color;
            this.colorLastFrame = startColor;
        }
        public FaderTarget(RawImage im)
        {
            this.rawImage = im;
            this.startColor = im.color;
            this.colorLastFrame = startColor;
        }
        public FaderTarget(TextMeshProUGUI textmeshPro) { 
            this.textmesh = textmeshPro;
            this.startColor = textmeshPro.color;
            this.colorLastFrame = startColor;
        }
    }

    /*Image image;
    TextMeshProUGUI textmesh;
    Color startColorText;
    Color startColor;
    Color colorLastFrame;*/


    public bool hasFaded
    {
        get {
            return layers.Count == 0; //pattern == FadePattern.FadeAndStay && fade == Fade.FadeOut && current == 0;
        }
    }

    public Fader(Image im, TextMeshProUGUI textmeshPro = null) 
      : this(new Image[] { im}, textmeshPro == null ? null : new TextMeshProUGUI[] { textmeshPro})
    {
        /*this.textmesh = textmeshPro;
        this.image = im;
        this.startColor = im.color;
        if(textmeshPro != null)
        {
            startColorText = textmeshPro.color;
        }
        this.colorLastFrame = startColor;*/
    }

    public Fader(Image[] images = null, TextMeshProUGUI[] textMeshPros = null, RawImage[] rawImages = null)
    {
        if(images != null)
        {
            int i = 0;
            foreach(Image im in images)
            {
                if(im == null) { Debug.Log(i + " is null"); }
                targets.Add(new FaderTarget(im));
                i++;
            }
        }if(textMeshPros != null)
        {
            foreach (TextMeshProUGUI text in textMeshPros)
            {
                targets.Add(new FaderTarget(text));
            }
        }
        if (rawImages != null)
        {
            foreach (RawImage im in rawImages)
            {
                targets.Add(new FaderTarget(im));
            }
        }
    }

    public void SetFade(FadePriority prio, Fade fad, FadePattern patternVal, float tim, Color col)
    {
        SetFade(prio, fad, patternVal, new float[] { tim }, col);
    }

    public void SetFade(FadePriority prio, Fade fad, FadePattern patternVal, float[] tim, Color col)
    {
        FaderLayer currentTop = GetLayerLessThan(MAX_LAYER);
        bool removed = false;
 
        //Remove fadeoutlayers under current layer
        if (currentTop != null)
        {
            if (prio < currentTop.prio && fad == Fade.FadeOut)
            {
                layers.Remove((int)prio);
                removed = true;
            }
        }
        //If this was not a remove action
        if(!removed)
        {
            if (layers.Contains((int)prio))
            {

                FaderLayer fl = layers[(int)prio];
                fl.color = col;
                fl.pattern = patternVal;
                fl.fade = fad;
                fl.time = tim;
                fl.timeSlot = 0;

            }
            else
            {
                layers.Add((int)prio, new FaderLayer(col, fad, prio, patternVal, tim));
            }

            if (currentTop != null)
            {
                if (prio > currentTop.prio)
                {
                    if (currentTop.prio == FadePriority.Marking)
                    {
                        layers.Remove((int)currentTop.prio);
                    }
                    if (fad == Fade.FadeIn)
                    {
                        currentTime = 0;
                    }
                }
            }
        }
    }

    private FaderLayer GetLayerLessThan(int max)
    {
        FaderLayer layer = null;

        if (layers.Count > 0)
        {
            int maxindex = -1;

            foreach (int layerIndex in layers)
            {
                if (layerIndex > maxindex &&  layerIndex < max)
                {
                    maxindex = layerIndex;
                }
            }
            if (maxindex != -1)
            {
                layer = layers[maxindex];
            }
        }
        return layer;
    }

    private bool Tick(FaderLayer current)
    {
        if (current.fade == Fade.FadeIn)
        {
            this.currentTime += Time.deltaTime/current.time[current.timeSlot];

            if(this.currentTime >= 1 && 
                (current.pattern == FadePattern.Pulse 
                || current.pattern == FadePattern.FadeInAndOut)
                )// || current.pattern == FadePattern.InnerPulse))
            {
                if(current.time.Length > 1)
                {
                    current.timeSlot = 1;
                }
                //current.time = DEFAULT_PULSE_TIME;
                current.fade = Fade.FadeOut;
               // current.pattern = FadePattern.InnerPulse;
            }
            /*else if(this.currentTime >= 1 && current.pattern == FadePattern.FadeInAndOut)
            {
                current.timeSlot = 1;
                current.fade = Fade.FadeOut;
            }*/
        }else if(current.fade == Fade.FadeOut)
        {
            this.currentTime -= Time.deltaTime/current.time[current.timeSlot];

            if (this.currentTime <= PULSE_SWITCH_THRESHOLD && current.pattern == FadePattern.Pulse)
            {

                current.fade = Fade.FadeIn;
            }
            else if(this.currentTime <= 0)
            {
                
                layers.Remove((int)current.prio);
                this.currentTime = 1;

                return false;
            }
        }
        //Clamp
        this.currentTime = Mathf.Min(Mathf.Max(this.currentTime, 0), 1);
        return true;
    }

    public void FadeColor()
    {
        //Top layer
        FaderLayer top = GetLayerLessThan(MAX_LAYER);

        if (top != null)
        {
            if (Tick(top))
            {
                FaderLayer belowTop = GetLayerLessThan((int)top.prio);

                //Tick time
                float t = currentTime;
                t = t * t * t * (t * (6f * t - 15f) + 10f);

                foreach(FaderTarget target in targets)
                {
                    Color newColor;
                    //Color newColorText;

                    if (belowTop != null)
                    {
                        // Debug.Log("Using below top color");
                        newColor = Color.Lerp(belowTop.color, top.color, t);
                       // newColorText = newColor;
                    }
                    else
                    {
                        // Debug.Log("Using base layer");

                        newColor = Color.Lerp(target.startColor, top.color, t);
                        //newColorText = target.textmesh != null ? Color.Lerp(target.startColorText, top.color, t) : Color.black;
                    }

                    //Skip unnecessary updates
                    if (newColor != target.colorLastFrame)
                    {
                        if(target.image != null)
                        {
                            target.image.color = newColor;
                        }
                        if (target.textmesh != null)
                        {
                            target.textmesh.color = newColor;
                        }
                        if (target.rawImage != null)
                        {
                            target.rawImage.color = newColor;
                        }
                    }
                    target.colorLastFrame = newColor;
                }
            }
        }
    }



    public void EndFade()
    {

        layers.Clear();
        foreach (FaderTarget target in targets)
        {
            if (target.image != null)
            {
                target.image.color = target.startColor;
            }
            if (target.textmesh != null)
            {
                target.textmesh.color = target.startColor;
            }
            if (target.rawImage != null)
            {
                target.rawImage.color = target.startColor;
            }
        } 
        currentTime = 0;
        //priority = 0;
    }


}
