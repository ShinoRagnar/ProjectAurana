using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Spawner : MonoBehaviour {

	public void SpawnMech(GameUnit player, MechNames nam, string name, string layer)
    {
        //GameObject
        Transform playerNode = new GameObject(name).transform;
        playerNode.parent = this.transform;
        playerNode.position = this.transform.position;

        //Player Placeholder
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);

        if (DevelopmentSettings.SHOW_PLAYER_BOUNDS)
        {
            body.GetComponent<Renderer>().material = Global.Resources[MaterialNames.Glass]; //Global.instance.MAT_GLASS;
        }
        else
        {
            body.GetComponent<Renderer>().enabled = false;
        }

        body.name = name + Global.NAME_BODY;
        Transform playerBody = body.transform;
        playerBody.parent = playerNode;
        playerBody.position = this.transform.position;

        //Shield
        Transform playerShield = Instantiate(Global.Resources[PrefabNames.ForceShield], playerBody);

        //Add mech components
        player.RegisterBodyForMech(playerBody,playerShield.GetComponent<Forge3D.Forcefield>());

        //Mech creation
        player.mech = Global.Resources[nam, player, name];


        playerShield.name = name + Global.NAME_SHIELD;
        playerShield.localScale += new Vector3(0.2f, 0.2f, 0.2f); //Add some extra radius to be safe
        playerShield.localPosition = Vector3.zero;
        playerShield.gameObject.AddComponent<GameUnitBodyComponent>().owner = player;

        //Layer
        Global.SetLayerOfThisAndChildren(layer, playerNode.gameObject);
        Global.SetLayerOfThisAndChildren(Global.LAYER_SHIELDS, playerShield.gameObject);

        //Stats
        player.stats.AddStat(Stat.JumpForce, 1);

    }
}
