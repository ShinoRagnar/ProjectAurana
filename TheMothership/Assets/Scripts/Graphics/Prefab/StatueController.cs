using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Animator))]

public class StatueController : MonoBehaviour
{

    protected Animator animator;

    public bool ikActive = false;
    public Transform rightHandObj = null;
    public Transform leftHandObj = null;
    public Transform rightFootObj = null;
    public Transform leftFootObj = null;
   /* public Transform rightHintHandObj = null;
    public Transform leftHintHandObj = null;
    public Transform rightHintFootObj = null;
    public Transform leftHintFootObj = null;*/

    public Transform lookObj = null;
    public Transform headObject = null;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.speed = 1f;
    }

    //a callback for calculating IK
    void OnAnimatorIK()
    {
        if (animator)
        {

            //if the IK is active, set the position and rotation directly to the goal. 
            if (ikActive)
            {

                // Set the look target position, if one has been assigned
                if (lookObj != null)
                {
                    animator.SetLookAtWeight(1);
                    animator.SetLookAtPosition(lookObj.position);
                }

                // Set the right hand target position and rotation, if one has been assigned
                if (rightHandObj != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandObj.rotation);
                }
                // Set the right hand target position and rotation, if one has been assigned
                if (leftHandObj != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandObj.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandObj.rotation);
                }
                // Set the right hand target position and rotation, if one has been assigned
                if (leftFootObj != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
                    animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootObj.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootObj.rotation);
                }
                // Set the right hand target position and rotation, if one has been assigned
                if (rightFootObj != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootObj.position);
                    animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootObj.rotation);
                }
                // HINT!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                /*if (rightHintHandObj != null)
                {
                    animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);
                    animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);
                    animator.SetIKHintPosition(AvatarIKHint.RightElbow, rightHintHandObj.position);
                    //animator.setIKHint(AvatarIKHint.RightElbow, rightHintHandObj.rotation);
                }
                // Set the right hand target position and rotation, if one has been assigned
                if (leftHintHandObj != null)
                {
                    animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);
                    animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);
                    animator.SetIKHintPosition(AvatarIKHint.LeftElbow, leftHintHandObj.position);
                   // animator.SetIKHintPosition(AvatarIKHint.LeftHand, leftHintHandObj.rotation);
                }
                // Set the right hand target position and rotation, if one has been assigned
                if (leftHintFootObj != null)
                {
                    animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 1);
                    animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 1);
                    animator.SetIKHintPosition(AvatarIKHint.LeftKnee, leftHintFootObj.position);
                    //animator.SetIKHintPosition(AvatarIKHint.LeftFoot, leftFootObj.rotation);
                }
                // Set the right hand target position and rotation, if one has been assigned
                if (rightHintFootObj != null)
                {
                    animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 1);
                    animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 1);
                    animator.SetIKHintPosition(AvatarIKHint.RightKnee, rightHintFootObj.position);
                 //   animator.SetIKHintPosition(AvatarIKHint.RightFoot, rightFootObj.rotation);
                }*/

                if (headObject != null && lookObj != null)
                {
                    headObject.LookAt(lookObj);
                }

            }

            //if the IK is not active, set the position and rotation of the hand and head back to the original position
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                animator.SetLookAtWeight(0);
            }
        }
    }
}
