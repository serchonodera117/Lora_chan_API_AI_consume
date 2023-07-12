using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using SpeechLib;
using VRM;
using JetBrains.Annotations;
using LMNT;
using System.Runtime.CompilerServices;
using System;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

public class Animation_Script : MonoBehaviour
{
    private Animator anim;
    private bool isListening = false;
    private bool isTalking = false;
    private SpVoice voice = new SpVoice();
    private bool isDictationActive = false;
    private bool isComandActive = false;

    private Dictionary<string, Action> fristCommand;
    private KeywordRecognizer voicerecognicer;
    private DictationRecognizer dictationRecognizer;

    private float secondsTak = 0f;
    private float speechTimmer = 0f;


    public SkinnedMeshRenderer skinnedMeshRenderer;
    public Sprite listenigAbled;
    public Sprite listenigDisabled;
    public Button btnListening;
    public TextMeshProUGUI listeningMessage;
    public TMP_InputField  writingTextBox; 

    // Start is called before the first frame update
    void Start()
    {
        fristCommand= new Dictionary<string, Action>();
        fristCommand.Add("lora chan", prepareTotakeInstructions);
        fristCommand.Add("lora", prepareTotakeInstructions);



        voicerecognicer = new KeywordRecognizer(fristCommand.Keys.ToArray());
        voicerecognicer.OnPhraseRecognized += onRecognizeLORAcommand;

        dictationRecognizer = new DictationRecognizer();
        dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;

        anim = GetComponent<Animator>();
        StartCoroutine(no_loop_bodyAimation("greeting", "Standing_Greeting")); //call the function with timmer to count aniamtion seconds
        anim.SetTrigger("face_greeting");
    }

    // Update is called once per frame
    void Update()
    {
        if (isTalking)
        {
          speechTimmer += Time.deltaTime;
           if(speechTimmer >= secondsTak) { stopTalking(speechTimmer, secondsTak); speechTimmer = 0; }
        }
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
        Talk("Hola buen día Lora Chan a su servicio, ¿Que haremos hoy?");
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
            loop_FaceExpressions("face_listening");
            isComandActive = true;
            //voicerecognicer.Start();
            dictationRecognizer.Start();
        }
        else
        {
            listeningMessage.text = "Not Listening";
            btnListening.image.sprite = listenigDisabled;
            writingTextBox.readOnly = false;
            loop_bodyAnimation("idle");
            loop_FaceExpressions("face_base");
            isComandActive = false;
            //voicerecognicer.Stop();
            dictationRecognizer.Stop();
        }
    }

   public  void Talk(string theSpeech)
    {
        loop_FaceExpressions("face_talking");
        isTalking = true;
        float charactersPerSecond = 15f;
        float duration = theSpeech.Length / charactersPerSecond;
       secondsTak =  duration;
        voice.Speak(theSpeech, SpeechVoiceSpeakFlags.SVSFlagsAsync);
    }

    public void stopTalking(float timmer, float speechDuration)//Stop Talking
    {
        isTalking = false;
        timmer = 0f;
        speechDuration = 0f;
        secondsTak = 0f;
        loop_FaceExpressions("face_base");
    }
    public void onRecognizeLORAcommand(PhraseRecognizedEventArgs word)//executes prepareTotakeInstructions
    {
        Debug.Log(word);
        fristCommand[word.text].Invoke();
        if (isComandActive) { voicerecognicer.Stop(); isComandActive = false; }
    }
    public void prepareTotakeInstructions()//listening commands 
    {
        loop_bodyAnimation("idle_listening");

       // PhraseRecognitionSystem.Shutdown();

        //if (voicerecognicer.IsRunning) { voicerecognicer.Stop(); isComandActive = false; }
        //if (!isDictationActive) { dictationRecognizer.Start(); isDictationActive = true; }
    }



    //-----------------------------------------------------dictation recognizer
    private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {

        writingTextBox.text = text;
        //dictationRecognizer.Stop();
        //isDictationActive = false;

        //if (!voicerecognicer.IsRunning)
        //{
        //    voicerecognicer.Start();
        //    isComandActive = true;
        //}
    }
}

