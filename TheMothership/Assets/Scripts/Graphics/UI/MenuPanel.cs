using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FadeDirection
{
    NoFade,
    FadeLeft,
    // FadeRight,
    FadeUp,
    FadeDown
}
public enum Fade
{
    FadeOut,
    FadeIn,
   // Fading
}
public interface GamePanel
{
    bool Fade(Fade fade);
    /*void Hide(bool fade);
    void Show(bool fade);*/
}
public class MenuPanel : MonoBehaviour, GamePanel, Initiates
{

    public MenuGroup menu;
    public FadeDirection direction;
    public bool hideOnStartup = false;
    public float hideDistance = 200;

   // private RectTransform self;
   // private Vector3 originalPosition;
    private float currentFadeDuration;

    public ListDictionary<Transform, Vector3> originalPositions = new ListDictionary<Transform, Vector3>();

    public void Start()
    {
        Initiate();
    }

    public void Initiate() {

        if (Global.IsAwake)
        {
            //Register all positions of children
            for (int i = 0; i < this.transform.childCount; i++)
            {
                originalPositions.Add(this.transform.GetChild(i), this.transform.GetChild(i).localPosition);
            }
            if (hideOnStartup)
            {
                Hide();
                //self.transform.gameObject.SetActive(false);
            }

            Global.instance.menues.Register(menu, this);
        }
        else
        {
            Global.initiates.AddIfNotContains(this);
        }

    }
    

    public void Hide()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            if (this.transform.GetChild(i).gameObject.activeSelf)
            {
                this.transform.GetChild(i).gameObject.SetActive(false);
            }  
        }
    }
    public void Show()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            if (!this.transform.GetChild(i).gameObject.activeSelf)
            {
                this.transform.GetChild(i).gameObject.SetActive(true);
            }

        }
    }
    private void Move(float t, float distanceX = 0, float distanceY = 0)
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            Transform child = this.transform.GetChild(i);
            Vector3 originalPosition = originalPositions[this.transform.GetChild(i)];
            child.localPosition = Vector3.Lerp(originalPosition, new Vector3(originalPosition.x + distanceX, originalPosition.y+ distanceY, originalPosition.z), t);
        }
    }

    public bool Fade(Fade f)
    {
        if (f == global::Fade.FadeOut)
        {
            currentFadeDuration += Time.unscaledDeltaTime; //Global.globalTime;
            if(direction == FadeDirection.NoFade)
            {
                currentFadeDuration = MenuSystem.MENU_TRANSITION_FADE_TIME;
            }
        }
        else if (f == global::Fade.FadeIn)
        {
            currentFadeDuration -= Time.unscaledDeltaTime;
            Show();
            if (direction == FadeDirection.NoFade)
            {
                currentFadeDuration = 0;
            }
        }

        currentFadeDuration = Mathf.Min(Mathf.Max(currentFadeDuration, 0), MenuSystem.MENU_TRANSITION_FADE_TIME);
        //Debug.Log(currentFadeDuration);
        float t = currentFadeDuration / MenuSystem.MENU_TRANSITION_FADE_TIME;
        t = t * t * t * (t * (6f * t - 15f) + 10f);


        if (direction == FadeDirection.FadeLeft)
        {
            Move(t, -hideDistance);
            //self.localPosition = Vector3.Lerp(originalPosition, new Vector3(originalPosition.x- hideDistance, originalPosition.y,originalPosition.z), t);
            //MoveX(Mathf.Lerp(lastPosition.x, -200, t), true);
        }
        if (direction == FadeDirection.FadeUp)
        {
            Move(t, 0,hideDistance);
            //self.localPosition = Vector3.Lerp(originalPosition, new Vector3(originalPosition.x,originalPosition.y+ hideDistance, originalPosition.z), t);
            //MoveY(Mathf.Lerp(lastPosition.y, 200, t), true);
        }
        if (direction == FadeDirection.FadeDown)
        {
            Move(t, 0, -hideDistance);
            //self.localPosition = Vector3.Lerp(originalPosition, new Vector3(originalPosition.x,originalPosition.y+ hideDistance, originalPosition.z), t);
            //MoveY(Mathf.Lerp(lastPosition.y, 200, t), true);
        }
        if (currentFadeDuration == MenuSystem.MENU_TRANSITION_FADE_TIME && f == global::Fade.FadeOut)
        {
            Hide();
            //self.transform.gameObject.SetActive(false);
            return true;
        }else if (currentFadeDuration == 0 && f == global::Fade.FadeIn)
        {
            return true;
        }
        return false;
    }
}
