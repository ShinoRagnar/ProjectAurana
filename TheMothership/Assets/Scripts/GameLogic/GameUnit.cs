using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class GameUnitBodyComponent : MonoBehaviour
{
    public GameUnit owner;
}
public class GameUnit{

    public struct DamageResponse
    {
        public bool killed;
        public bool damagedHealth;
        public float healthDamageDealt;
        public float shieldDamageDealt;
        public float shakeAdded;
        public float impactAdded;
        public ListHash<Rigidbody> detachedItems;

        public DamageResponse(bool killed, bool damagedHealth, ListHash<Rigidbody> detached, float healthDamageDealt, float shieldDamageDealt)
        {
            this.killed = killed;
            this.damagedHealth = damagedHealth;
            this.detachedItems = detached;
            this.healthDamageDealt = healthDamageDealt;
            this.shieldDamageDealt = shieldDamageDealt;
            this.shakeAdded = 0;
            this.impactAdded = 0;
        }
}

    protected static DictionaryList<string, int> activeUnits = new DictionaryList<string, int>();
    public static DictionaryList<Faction, ListHash<GameUnit>> unitsByFaction = new DictionaryList<Faction, ListHash<GameUnit>>();

    // Used by Agents
    public Character character;
    public NavMeshAgent navMeshAgent;
   // public Animator animator;
    public Rigidbody rigid;
    public CapsuleCollider collider;
    public CharacterLinkMover characterLinkMover;
    public Squad squad;

    // Used by turrets
    public NavMeshObstacle obstacle;

    // Used by Mechs
    public Mech mech;
    public DictionaryList<MechItem,Vector2> inventory = new DictionaryList<MechItem,Vector2>();
    public LegMovement movement;

    // Common
    public Equipper itemEquiper;
    public Faction belongsToFaction;
    public Senses senses;
    public Transform body;
    public BuffHandler buffHandler;
    public ImpactReceiver impact;
    public Stats stats;
    public CharacterController controller;
    public AIBasic ai;
    public Forge3D.Forcefield shield;

    //Names
    public string unitName;
    public string uniqueName;

    //Player?
    public bool isPlayer;
    public bool isActive;

    //Acrtive effects
    public ListHash<OnHit.ActiveEffect> activeEffecs = new ListHash<OnHit.ActiveEffect>();

    //By default gameunits are not active
    public GameUnit(string unitNameVal, Faction f, Senses s, Stats stat, bool isPlayerVal, bool isActive = false)
    {
        this.isActive = isActive;
        this.stats = stat;
        this.belongsToFaction = f;
        this.senses = s;
        this.unitName = unitNameVal;

        //Active units
        if (isActive)
        {
            if (activeUnits.Contains(unitName))
            {
                activeUnits[unitName]++;
            }
            else { activeUnits.Add(unitName, 0); }
            uniqueName = unitName + activeUnits[unitName];
            if (!unitsByFaction.Contains(f))
            {
                unitsByFaction.Add(f, new ListHash<GameUnit>());
            }
            unitsByFaction[f].AddIfNotContains(this);
        }

        this.senses.owner = this;
        this.stats.owner = this;
        isPlayer = isPlayerVal;
    }
    public void AddAI<T>() where T : AIBasic
    {
        this.ai = AddBodyComponent<T>();
        //this.ai = this.body.gameObject.AddComponent<T>();
        //this.ai.owner = this;
    }
    public T AddBodyComponent<T>() where T: GameUnitBodyComponent
    {
        T t = this.body.gameObject.AddComponent<T>();
        t.owner = this;
        return t;
    }
    public void Flash(Material m, float time)
    {
        if(mech != null)
        {
            mech.Flash(m,time);
        }
    }

    //Clones are always active
    public GameUnit Clone()
    {
        return new GameUnit(unitName, belongsToFaction, senses.Clone(), stats.Clone(), isPlayer, true);
    }
    public void AddToInventory(MechItem mi, Vector2 vec)
    {
        itemEquiper.Equip(mi);
        inventory.Add(mi, vec);
    }
    public Vector3 GetCenterPos()
    {
        if(mech == null)
        {
            return new Vector3(body.position.x, body.position.y + collider.height / 2, body.position.z);
        }
        else
        {
            return body.position;
        }
    }
    public Vector3 GetHeadPos()
    {
        if (mech == null)
        {
            return new Vector3(body.position.x, body.position.y + collider.height, body.position.z);
        }
        else
        {
            return GetCenterPos();
        }
    }
    public Vector3 GetFootPos()
    {
        if (mech == null)
        {
            return new Vector3(body.position.x, body.position.y, body.position.z);
        }
        else
        {
            return body.position-new Vector3(0,body.localScale.y);
        }
    }

    public Vector3 GetScale()
    {
        if(mech != null)
        {
            return body.localScale;
        }
        return new Vector3(body.localScale.x,body.localScale.y*collider.height,body.localScale.z);
    }

    public float GetMaxScale()
    {
        Vector3 scale = GetScale();
        return Mathf.Max(Mathf.Max(scale.x, scale.y), scale.z);
    }

    public bool IsGrounded()
    {
        if (mech != null)
        {
            return controller.isGrounded;
        }
        else
        {
            return character.IsGrounded();
        }
    }

    public Ground GetLastOnGround()
    {
        if (mech != null)
        {
            return movement.lastWalkedOn;
        }
        else
        {
            return null;
        }
    }

    public void RegisterBodyForMech(Transform body, Forge3D.Forcefield forc)
    {
        this.body = body;
        if (forc == null)
        {
            Debug.Log("Shield not found: " + uniqueName);
        }
        shield = forc;
        buffHandler = AddBodyComponent<BuffHandler>(); //this.body.gameObject.AddComponent<BuffHandler>();
        //buffHandler.owner = this;
        AddBodyComponent<GameUnitBodyComponent>();
        //ColliderOwner col = this.body.gameObject.AddComponent<ColliderOwner>();
        //ColliderOwner col = this.body.gameObject.AddComponent<ColliderOwner>();
        //col.owner = this;
        if(stats != null)
        {
            float weight = stats.GetStat(Stat.Weight);
            if(weight != 0) {
                impact = AddBodyComponent<ImpactReceiver>();
                    //this.body.gameObject.AddComponent<ImpactReceiver>();
                impact.mass = weight;
                //impact.owner = this;
            }
            else
            {
                Debug.Log("mech is missing weight: " + weight);
            }
        }
        controller = this.body.gameObject.AddComponent<CharacterController>();
        obstacle = this.body.gameObject.AddComponent<NavMeshObstacle>();
        movement = new LegMovement(this);

    }
    // Return true if health was subtracted
    public DamageResponse Damage(float anyDamage, float empDamage, float corruptionDamage, GameUnit source, bool isOnHitDamage)
    {
        float damageReceived = stats.GetStat(Stat.DamageReceived);
        float onHitDamageReceived = (isOnHitDamage ? (1 - stats.GetStat(Stat.OnHitDamageReceived)) : 0);

        if (empDamage > 0) { empDamage *= damageReceived + (1 - stats.GetStat(Stat.EMPDamageReceived)) + onHitDamageReceived;  }
        if (corruptionDamage > 0) { corruptionDamage *= damageReceived + (1 - stats.GetStat(Stat.CorruptionDamageDealt)) + onHitDamageReceived; }
        
        anyDamage *= damageReceived + onHitDamageReceived;

        float spill = Mathf.Max(stats.Damage(Stat.Shield, anyDamage+empDamage) - empDamage, 0);

        if(spill > 0 || (stats.GetValuePercentage(Stat.Shield) == 0 && corruptionDamage > 0))
        {
            ListHash<Rigidbody> detached = null;
            stats.Damage(Stat.Health, Mathf.Max(0,spill)+corruptionDamage);
            if (stats.GetValuePercentage(Stat.Health) == 0){
                detached = Kill(source);
            }
            //Debug.Log("Remaining health:" + stats.GetCurrentValue(Stat.Health));
            return new DamageResponse(detached != null, true, detached, spill+corruptionDamage, anyDamage - spill);
        }
        //Debug.Log("Remaining shield:" + stats.GetCurrentValue(Stat.Shield));
        return new DamageResponse(false,false, null,0,anyDamage+empDamage);
    }

    public ListHash<Rigidbody> Kill(GameUnit killedby)
    {
       
        if (isActive)
        {
            Debug.Log(uniqueName + " was killed by; " + killedby.uniqueName);

            if (!isPlayer)
            {
                isActive = false;
                activeUnits[unitName] -= 1;
                unitsByFaction[belongsToFaction].Remove(this);

                //Return effects to make them end
                foreach(OnHit.ActiveEffect ae in activeEffecs)
                {
                    ae.source.ReturnEffect(this, ae.toReturn, ae.effect);
                }
                activeEffecs.Clear();
                
                ListHash<Rigidbody> ret;

                if (mech != null)
                {

                    Global.Destroy(ai);
                    movement.Kill();
                    ret = mech.Kill();
                }
                else 
                {
                    ret = character.Kill();
                }

                if (squad != null)
                {
                    squad.members.Remove(this);
                }

                Global.Destroy(body.gameObject);

                return ret;
            }
        }
        else
        {
            Debug.Log(uniqueName + " has already been killed!! Was killed again by " + killedby.uniqueName);
        }
        return null;
    }
    /*public void RegisterTurrentTypeAgent(Transform bodyVal)
    {
        this.body = bodyVal;

        this.itemEquiper = new ItemEquipper(this); //this.body.gameObject.AddComponent<ItemEquipper>();
        //this.itemEquiper.owner = this;
        this.obstacle = this.body.gameObject.AddComponent<NavMeshObstacle>();
    }*/

    public void RegisterBodyForSoldier(Transform bodyVal, Forge3D.Forcefield forc)
    {
        this.body = bodyVal;
        if(forc == null)
        {
            Debug.Log("Shield not found: " + uniqueName);
        }
        this.shield = forc;

        //Sets
        this.navMeshAgent = this.body.gameObject.AddComponent<NavMeshAgent>();
        this.navMeshAgent.stoppingDistance = 1f;
        this.navMeshAgent.speed = 7;
        buffHandler = AddBodyComponent<BuffHandler>();
        /*this.itemEquiper = this.body.gameObject.AddComponent<ItemEquiper>();
        this.itemEquiper.owner = this;*/
        this.character = AddBodyComponent<Character>(); //this.body.gameObject.AddComponent<Character>();
       // this.character.owner = this;
        this.character.animator = this.body.gameObject.GetComponent<Animator>();
        this.itemEquiper = this.character;
        this.characterLinkMover = AddBodyComponent<CharacterLinkMover>();//this.body.gameObject.AddComponent<CharacterLinkMover>();
      //  this.characterLinkMover.owner = this;

        //Gets (if it has them)
       // this.animator = this.body.gameObject.GetComponent<Animator>();
        this.rigid = this.body.gameObject.GetComponent<Rigidbody>();
        this.collider = this.body.gameObject.GetComponent<CapsuleCollider>();
        AddBodyComponent<GameUnitBodyComponent>();

    }
}
