using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum OffMeshLinkMoveMethod
{
    Teleport,
    NormalSpeed,
    Parabola
}

[RequireComponent(typeof(NavMeshAgent))]
public class CharacterLinkMover : GameUnitBodyComponent
{
    public OffMeshLinkMoveMethod method = OffMeshLinkMoveMethod.Parabola;
    public float speed = 6;
    public float height = 2;
    // Owner
   // public GameUnit owner;
    

    IEnumerator Start()
    {
        JetPack characterJetpack = (JetPack) owner.itemEquiper.GetFirstItemOfType(typeof(JetPack));
        NavMeshAgent agent = owner.navMeshAgent;
        agent.autoTraverseOffMeshLink = false;
        


        while (true)
        {
            if (agent.isOnOffMeshLink)
            {
                if(characterJetpack != null)
                {
                    //Debug.Log("JETPACK!!");
                    characterJetpack.Enable();
                }
                if (method == OffMeshLinkMoveMethod.NormalSpeed)
                {
                    yield return StartCoroutine(NormalSpeed(agent));
                }
                else if (method == OffMeshLinkMoveMethod.Parabola)
                {
                    float absoluteSpeed = Vector3.Distance(agent.currentOffMeshLinkData.startPos, agent.currentOffMeshLinkData.endPos) / speed;
                    yield return StartCoroutine(Parabola(agent, height, absoluteSpeed));
                }
                if (characterJetpack != null)
                {
                    characterJetpack.Disable();
                }
                agent.CompleteOffMeshLink();
            }
            yield return null;
        }
    }

    IEnumerator NormalSpeed(NavMeshAgent agent)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
        while (agent.transform.position != endPos)
        {
            agent.transform.position = Vector3.MoveTowards(agent.transform.position, endPos, agent.speed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator Parabola(NavMeshAgent agent, float height, float duration)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
        float normalizedTime = 0.0f;
        while (normalizedTime < 1.0f)
        {
            float yOffset = height * 4.0f * (normalizedTime - normalizedTime * normalizedTime);
            agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }
    }
}