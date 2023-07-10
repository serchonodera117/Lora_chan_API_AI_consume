using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRM;

public class Animation_Script : MonoBehaviour
{
    private Animator anim;
    public SkinnedMeshRenderer skinnedMeshRenderer;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();


        StartCoroutine(no_loop_bodyAimation("greeting", "Standing_Greeting")); //call the function with timmer to count aniamtion seconds
        anim.SetTrigger("face_greeting");
    }

    // Update is called once per frame
    void Update()
    {

    }


    public IEnumerator no_loop_bodyAimation(string nameExpression, string nameAnimation)
    {
        anim.SetTrigger(nameExpression);
        AnimationClip[] animationLenght = anim.runtimeAnimatorController.animationClips;
        float clipLength = 0f;
        foreach (AnimationClip clip in animationLenght)
        {
            if (clip.name == nameAnimation)
            {
                clipLength = clip.length;
            }
        }

        yield return new WaitForSeconds(clipLength);
        anim.SetTrigger("idle");
        if(nameAnimation=="Standing_Greeting"){anim.SetTrigger("face_base");}//after frist greeting, return to nomral smile
    }

}

