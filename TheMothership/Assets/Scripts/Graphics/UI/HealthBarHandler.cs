using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarHandler : MonoBehaviour{

    public static readonly string NAME_SELF = "Healthbar";

    private static readonly string ENERGY_LINE = "EnergyLine";
    private static readonly string HEALTH_LINE = "HealthLine";
    private static readonly string DEATH = "Death";
    private static readonly string EYE_ONE = "EyeOne";
    private static readonly string EYE_TWO = "EyeTwo";
    private static readonly string DIVIDER = "Dividor";
    private static readonly string HEALTH_TEXT = "HealthText";
    private static readonly string SHIELD_TEXT = "ShieldText";

//    private static readonly float DAMAGE_INTERVAL = 3f;
//    private static readonly float DAMAGE_DECREASE_AFTER = 2.7f;
    private static readonly float MAX = 1;
    private static readonly float energyOffset = -353;
    private static readonly float height = 380f;
    private static readonly float width = 70f;
    private static readonly float energy_fade_offset = 5;

    //Save variables to see if we actually need to update something
    private float currentHealth = -1;
    private float currentEnergy = -1;
    private float currentMaxHealth = -1;
    private float currentMaxEnergy = -1;

    //public Transform healthBar;
    public GameUnit owner;

    private Image death;
    private Image eyeOne;
    private Image eyeTwo;

    private RectTransform healthLine;
    private RectTransform energyLine;
    private RectTransform divider;

    private TextMeshProUGUI healthText;
    private TextMeshProUGUI shieldText;

    public UIContainer ui;

    public void Start()
    {
        //Find all components needed
        death = ui.GetComponent<Image>(DEATH); //Global.FindDeepChild(healthBar, DEATH).GetComponent<Image>();
        eyeOne = ui.GetComponent<Image>(EYE_ONE); //Global.FindDeepChild(healthBar, EYE_ONE).GetComponent<Image>();
        eyeTwo = ui.GetComponent<Image>(EYE_TWO);// Global.FindDeepChild(healthBar, EYE_TWO).GetComponent<Image>();
        healthLine = ui.GetComponent<RectTransform>(HEALTH_LINE);// Global.FindDeepChild(healthBar, HEALTH_LINE).GetComponent<RectTransform>();
        energyLine = ui.GetComponent<RectTransform>(ENERGY_LINE); //Global.FindDeepChild(healthBar, ENERGY_LINE).GetComponent<RectTransform>();
        divider = ui.GetComponent<RectTransform>(DIVIDER);// Global.FindDeepChild(healthBar, DIVIDER).GetComponent<RectTransform>();
        healthText = ui.GetComponent<TextMeshProUGUI>(HEALTH_TEXT); //Global.FindDeepChild(healthBar, HEALTH_TEXT).GetComponent<TextMeshProUGUI>();
        shieldText = ui.GetComponent<TextMeshProUGUI>(SHIELD_TEXT);// Global.FindDeepChild(healthBar, SHIELD_TEXT).GetComponent<TextMeshProUGUI>();
    }
    
    public void Update()
    {
        if(ui != null && owner != null)
        {
            float newMaxHealth = owner.stats.GetStat(Stat.Health); //health.maxHealth;
            float newMaxEnergy = owner.stats.GetStat(Stat.Shield); // health.maxEnergy;
            float newHealth = owner.stats.GetValuePercentage(Stat.Health); //(health.currentHealth / (float) health.maxHealth);
            float newEnergy = owner.stats.GetValuePercentage(Stat.Shield); //(health.currentEnergy / (float) health.maxEnergy);

            //Only update when something changes
            bool healthChanged = newMaxHealth != currentMaxHealth || newMaxEnergy != currentMaxEnergy || currentHealth != newHealth;
            bool energyChanged = newMaxEnergy != currentMaxEnergy || newMaxHealth != currentMaxHealth || currentEnergy != newEnergy;
            bool anythingChanged = healthChanged || energyChanged;

            if (DevelopmentSettings.SHOW_HEALTH_NUMBERS)
            {
                if (anythingChanged)
                {
                    healthText.text = "Health: " + ((int)(newHealth * newMaxHealth)) + "/" + ((int)newMaxHealth);
                    shieldText.text = "Shield: " + ((int)(newEnergy * newMaxEnergy)) + "/" + ((int)newMaxEnergy);
                }
            }

            float dividorPos = MAX/2- newMaxHealth / (newMaxHealth + newMaxEnergy);
            float healthLength = height - dividorPos * (height * 2);
            float energyLength = height + dividorPos * (height * 2);

            //Position divider
            if(newMaxHealth != currentMaxHealth || newMaxEnergy != currentMaxEnergy) { 
                divider.localPosition = new Vector3(
                                        divider.localPosition.x,
                                        -dividorPos*(height*2), 
                                        divider.localPosition.z);
            }
            //Health update
            if (healthChanged)
            {
                //Change when needed
                currentHealth = newHealth;
                healthLine.localPosition = new Vector3(
                    healthLine.localPosition.x,
                     divider.localPosition.y - healthLength/2,
                    healthLine.localPosition.z);
                healthLine.sizeDelta = new Vector3(width, healthLength);
                Global.Resources[MaterialNames.Health].SetFloat(OrbVariable.FILL, newHealth); //Global.instance.MATERIAL_HEALTH
            }
            //Energy update
            if (energyChanged)
            {
                //Change when needed
                currentEnergy = newEnergy;
                energyLine.localPosition = new Vector3(
                    energyLine.localPosition.x,
                    divider.localPosition.y+energyLength / 2 - energy_fade_offset, //energyOffset - dividorPos * (height * 2), 
                    energyLine.localPosition.z);
                energyLine.sizeDelta = new Vector3(width, energyLength);
               Global.Resources[MaterialNames.Shield].SetFloat(OrbVariable.FILL, newEnergy); // Global.instance.MATERIAL_SHIELD
            }
            
            //Make skull less transparent
            if (anythingChanged)
            {
                float deathmtr = 1 - (currentHealth + currentEnergy / (MAX * 2));
                SetColor(death, deathmtr);
                SetColor(eyeOne, deathmtr);
                SetColor(eyeTwo, deathmtr);
            }

            currentMaxEnergy = newMaxEnergy;
            currentMaxHealth = newMaxHealth;

        }
    }
    private void SetColor(Image g, float alpha)
    {
        var cll = g.color;
        cll.a = alpha;
        g.color = cll;
    }
}
