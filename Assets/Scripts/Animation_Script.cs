using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using VRM;
using JetBrains.Annotations;

public class Animation_Script : MonoBehaviour
{
    private Animator anim;
    private bool isListening = false;


    public SkinnedMeshRenderer skinnedMeshRenderer;
    public Sprite listenigAbled;
    public Sprite listenigDisabled;
    public Button btnListening;
    public TextMeshProUGUI listeningMessage;
    public TMP_InputField  writingTextBox; 

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
        anim.SetTrigger("face_base");//after frist greeting, return to nomral smile
    }

    public void loop_bodyAnimation(string nameExpression)
    {
        anim.SetTrigger(nameExpression);
    }

    public void loop_FaceExpressions(string nameExpression)
    {
        anim.SetTrigger(nameExpression);
    }

    public void activate_disactivate_listening()
    {
        isListening = !isListening;

        if (isListening)
        {
           btnListening.image.sprite = listenigAbled;
            listeningMessage.text = "Listening";
            writingTextBox.readOnly = true;
            loop_bodyAnimation("idle_listening");
            loop_FaceExpressions("face_listening");
        }
        else
        {
            listeningMessage.text = "Not Listening";
            btnListening.image.sprite = listenigDisabled;
            writingTextBox.readOnly = false;
            loop_bodyAnimation("idle");
            loop_FaceExpressions("face_base");
        }
    }
}

