using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MenuGroup
{
    NoMenu,
    PauseMenu,
    StartMenu,
    Inventory,
    Equipment,
}
public class MenuSystem : MonoBehaviour {

    public static readonly float MENU_TRANSITION_FADE_TIME = 0.25f;
    public static readonly float MENU_TRANSITION_TOTAL_TIME = MENU_TRANSITION_FADE_TIME * 2;

    private DictionaryList<int, List<GamePanel>> panels = new DictionaryList<int, List<GamePanel>>();

    public CameraMovement camMov;

    public ListHash<int> currentMenues = new ListHash<int>();

    //Makes you able to use multiple menues at once
    public DictionaryList<int, HashSet<int>> menuCompatability = new DictionaryList<int, HashSet<int>>()
    {
        {(int)MenuGroup.Inventory,new HashSet<int>(){ (int)MenuGroup.Equipment} },
        {(int)MenuGroup.Equipment,new HashSet<int>(){ (int)MenuGroup.Inventory} }
    };

    public delegate void MenuChangeDelegate(MenuGroup m, bool show);
    public event MenuChangeDelegate menuChange;

   // bool isFading = false;

   // private float currentZoomTime = 0;




    public MenuSystem()
    {
        currentMenues.Add((int)MenuGroup.NoMenu);
        panels.Add((int)MenuGroup.PauseMenu, new List<GamePanel>());
        panels.Add((int)MenuGroup.Inventory, new List<GamePanel>());
        panels.Add((int)MenuGroup.Equipment, new List<GamePanel>());
    }
    public void Start()
    {
        camMov = GameObject.FindObjectOfType<CameraMovement>();
    }

    public void Show(MenuGroup m)
    {
        if (!currentMenues.Contains((int)m))
        {
            bool pause = m == MenuGroup.PauseMenu;
            StartCoroutine(Switch(m, pause));
            /*if(m == Menu.Inventory)
            {
                StartCoroutine(Zoom(Global.CAMERA_DISTANCE,camMov.GetInventoryOffset()));

            }else if(currentMenu == Menu.Inventory)
            {
                StartCoroutine(Zoom(camMov.GetInventoryOffset(), Global.CAMERA_DISTANCE));
            }*/

        }
    }
    IEnumerator Switch(MenuGroup to, bool pause)
    {
       // isFading = true;
        if (pause)
        {
            Global.instance.Pause();
            Debug.Log("Paused");
        }

        //Alert menu change
        foreach (MenuGroup m in currentMenues)
        {
            if (!IsCompatible(m, to))
            {
                if (menuChange != null)
                {
                    menuChange(m, false);
                }
            }
        }

        bool allFaded = false;
        //Fade out incompatible menues
        while (!allFaded)
        {
            allFaded = true;
            foreach (MenuGroup m in currentMenues)
            {
                if (!IsCompatible(m,to))
                {
                    if (!FadeOut(m))
                    {
                        allFaded = false;
                    }
                }
            }
            yield return null;
        }

        //Alert menu added
        if (menuChange != null)
        {
            menuChange(to, true);
        }
        //Fade in new menu
        while (!FadeIn(to))
        {
            yield return null;
        }


        //Remove menue
        foreach (MenuGroup m in currentMenues)
        {
            if (!IsCompatible(m, to))
            {
                currentMenues.RemoveLater((int)m);
            }
        }
        currentMenues.Remove();

       
        if (!pause)
        {
            Global.instance.UnPause();
            //Debug.Log("Unpaused");
        }

        //Add menu
        currentMenues.Add((int)to);
        //Debug.Log("Faded in: " + to);

        //currentMenu = to;
        //isFading = false;
    }
   /*IEnumerator Zoom(Vector3 from, Vector3 to)
    {
        while(currentZoomTime < MENU_TRANSITION_TOTAL_TIME)
        {
            currentZoomTime += Time.unscaledDeltaTime;
            float t = currentZoomTime / MENU_TRANSITION_TOTAL_TIME;
            t = t * t * t * (t * (6f * t - 15f) + 10f);
            camMov.offset = Vector3.Lerp(from, to, t);
            yield return null;
        }
        currentZoomTime = 0;
    }*/
    private bool IsCompatible(MenuGroup m, MenuGroup to)
    {
        bool compatible = true;
        if (menuCompatability.Contains((int)m))
        {
            if (!menuCompatability[(int)m].Contains((int)to))
            {
                compatible = false;
            }
        }
        else
        {
            compatible = false;
        }
        return compatible;
    }
    private bool FadeOut(MenuGroup m)
    {
        bool allHaveFadedOut = true;
        foreach(GamePanel ui in panels[(int)m])
        {
            bool fad = ui.Fade(Fade.FadeOut);
            if(fad == false)
            {
                allHaveFadedOut = false;
            }
        }
        return allHaveFadedOut;
    }
    private bool FadeIn(MenuGroup m)
    {
        bool allHaveFadedIn = true;
        foreach (GamePanel ui in panels[(int)m])
        {
            bool fad = ui.Fade(Fade.FadeIn);
            if (fad == false)
            {
                allHaveFadedIn = false;
            }
        }
        return allHaveFadedIn;
    }

    public void Update()
    {
        if (!Global.Console.Showing)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentMenues.Contains((int)MenuGroup.PauseMenu))
                {
                    Show(MenuGroup.NoMenu);
                }
                else
                {
                    Show(MenuGroup.PauseMenu);
                }
            }
            else if (Input.GetKeyDown(KeyCode.I))
            {
                if (currentMenues.Contains((int)MenuGroup.Inventory))
                {
                    Show(MenuGroup.NoMenu);
                }
                else
                {
                    Show(MenuGroup.Inventory);
                }
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                if (currentMenues.Contains((int)MenuGroup.Equipment))
                {
                    Show(MenuGroup.NoMenu);
                }
                else
                {
                    Show(MenuGroup.Equipment);
                }
            }
        }
    }

    public void Register(GamePanel register)
    {
        Register(MenuGroup.NoMenu, register);
    }
    public void Register(MenuGroup m, GamePanel register)
    {
        if (!panels.Contains((int)m))
        {
            panels.Add((int)m, new List<GamePanel>());
        }
        panels[(int)m].Add(register);
    }

}
