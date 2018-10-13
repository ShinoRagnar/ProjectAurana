using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyType
{
    Character,
    Mech
}
public class EnemySpawner : Spawner, Initiates {

    private Global o;

    public EnemyType unitTypeToSpawn = EnemyType.Character;
    public MechNames unitToSpawn;

    public int numberToSpawn = 1;
    public float spawnDistance = 1f;

    private bool spawned;

    // Use this for initialization
    void Start () {
        Initiate();
    }

    public void Initiate()
    {
        if (Global.IsAwake)
        {
            o = Global.instance;
            Spawn();
        }
        else
        {
            Global.initiates.AddIfNotContains(this);
        }
    }


    private void Spawn()
    {
            if (!spawned) { 
            Senses.Hit hit = Senses.SeeGroundBelow(this.transform.position);

            if(hit.pos != null)
            {

                if(unitTypeToSpawn == EnemyType.Character)
                {
                    AISquad ai = SpawnUnitsInFormation(unitTypeToSpawn, Global.Grounds[hit.hit.collider.transform], hit.pos.x);
                    ai.squad.EquipAllMembersWith(
                        Global.Resources[GunNames.StandardRifle]
                        //o.GUN_STANDARD_RIFLE
                        , HumanBodyBones.RightHand);
                    ai.squad.EquipAllMembersWith(o.JETPACK_STANDARD, HumanBodyBones.UpperChest);
                }
                else if(unitTypeToSpawn == EnemyType.Mech)
                {
                    GameUnit turret = SpawnEnemy(unitTypeToSpawn, hit.pos);
                    turret.AddAI<AIMech>();

                    /*TurretBase bas = (TurretBase) turret.itemEquiper.Equip(o.TURRET_BASE_TWO_MOUNTS.Clone());
                    bas.Show(turret.body.transform);

                    //bas.MountAndShow(PointOfInterest.AttachmentSlot1, o.GUN_MORTAR_ONE.Clone());
                    //GunArray ga = o.ARRAY_EIGHT_SLOTS.Clone();
                    //TurretTargeter targeter = bas.MountAndShow(PointOfInterest.AttachmentSlot1, ga);

                    //Gunz
                    
                    //o.GUN_DOUBLE_MORTAR_ONE.Clone());//o.GUN_MORTAR_ONE.Clone());
                    bas.MountAndShow(PointOfInterest.AttachmentSlot2,
                        //o.GUN_MORTAR_ONE.Clone()
                        Global.Resources[GunNames.Mortar]
                        );

                    //bas.ShowMounted();
                    AITurret turr = bas.visualItem.gameObject.AddComponent<AITurret>();
                    turr.owner = turret;
                    turr.turret = bas;*/

                    //turret.itemEquiper.EquipItem()
                }

                spawned = true;
            }
        }
    }

    public AISquad SpawnUnitsInFormation(EnemyType unit, Ground ground, float x)
    {
        AISquad ai = this.gameObject.AddComponent<AISquad>();

        int reserves = ai.squad.currentFormation.ProjectFormationOn(
            ground,
            x,
            spawnDistance,
            numberToSpawn
            );

        foreach (int i in ai.squad.currentFormation.placements)
        {

            ai.squad.currentFormation.Place(i, ai.AddUnit(SpawnEnemy(unit, ai.squad.currentFormation.placements[i])));
        }
        return ai;
    }

    public GameUnit SpawnEnemy(EnemyType unitTypeToSpawn, Vector3 location)
    {
        //GameUnit
        GameUnit enemy;
        Transform enemyBody;

        if (unitTypeToSpawn == EnemyType.Character)
        {
            enemy = Global.ENEMY_SOLDIER_STANDARD.Clone();


            enemyBody = Instantiate(Global.Resources[PrefabNames.AlienSoldier], //o.UNIT_ENEMY_SOLDIER,
            this.transform);
            enemyBody.position = location;


            enemyBody.gameObject.name = enemy.uniqueName;

            //Body components

            //ColliderOwner ccc = enemyBody.gameObject.AddComponent<ColliderOwner>();
            // ccc.owner = enemy;

            //Shield
            Transform enemyShield = Instantiate(Global.Resources[PrefabNames.ForceShield], enemyBody);

            enemy.RegisterBodyForSoldier(enemyBody,enemyShield.GetComponent<Forge3D.Forcefield>());

            Transform t = Instantiate(Global.Resources[PrefabNames.DismemberedTPose], enemyBody);
            t.gameObject.AddComponent<SinkChildrenIntoGround>();
            t.gameObject.SetActive(false);


            enemyShield.name = enemy.uniqueName + Global.NAME_SHIELD;
            //enemyShield.position = location;
            enemyShield.localScale += new Vector3(1f, 1f, 1f);
            //enemyShield.Translate(new Vector3(0, 1, 0));
            //ColliderOwner coShield = enemyShield.gameObject.AddComponent<ColliderOwner>();
            //coShield.owner = enemy;
            enemyShield.gameObject.AddComponent<GameUnitBodyComponent>().owner = enemy;

            //Layer
            Global.SetLayerOfThisAndChildren(Global.LAYER_ENEMY, enemyBody.gameObject);
            Global.SetLayerOfThisAndChildren(Global.LAYER_SHIELDS, enemyShield.gameObject);

            enemyShield.localPosition = new Vector3(0, 1, 0);

            

            return enemy;
        }
        else if(unitTypeToSpawn == EnemyType.Mech)
        {
            enemy = Global.ENEMY_MECH.Clone();
            SpawnMech(enemy, unitToSpawn, enemy.uniqueName, Global.LAYER_ENEMY);
            enemy.body.position = new Vector3(location.x, location.y + enemy.body.localScale.y/2);
            enemy.body.Rotate(0, 180, 0);

            //Layer
            //Global.SetLayerOfThisAndChildren(Global.LAYER_ENEMY, enemy.body.gameObject);
           // Global.SetLayerOfThisAndChildren(Global.LAYER_SHIELDS, enemy.sh.gameObject);

            return enemy;
        }




        return null;
    }
}
