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
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEditor.VersionControl;
using System.IO;
using Unity.VisualScripting;
using TMPro.EditorUtilities;


public class Animation_Script : MonoBehaviour
{
    private const string APIKey = ""; //your API KEY
    private const string ChatGPTEndpoint = "https://api.openai.com/v1/chat/completions";
    private const string organization = ""; //your Organization ID


    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private Animator anim;
    private bool isListening = false;
    private bool isTalking = false;
    private SpVoice voice = new SpVoice();
    private bool isDictationActive = false;
    private bool isComandActive = false;
    private bool isComandSettinsActive = false;
   

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
    public TMP_InputField writingTextBox;
    public GameObject panelCommands;
    public Transform ScrollListCommand;
    public GameObject CommandElement;
    public TMP_InputField commandToADD;

    //----comand voice variable string
    private string nameAppToOpen = "";
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        panelCommands.SetActive(false);//ocultar

        writingTextBox.text = "";
        fristCommand = new Dictionary<string, Action>();
        fristCommand.Add("lora chan", prepareTotakeInstructions);
        fristCommand.Add("lora", prepareTotakeInstructions);
        LoadSavedCommnads();

        voicerecognicer = new KeywordRecognizer(fristCommand.Keys.ToArray());
        voicerecognicer.OnPhraseRecognized += onRecognizeLORAcommand;

        dictationRecognizer = new DictationRecognizer();
        dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;

        PhraseRecognitionSystem.Restart();

        StartCoroutine(no_loop_bodyAimation("greeting", "Standing_Greeting")); //call the function with timmer to count aniamtion seconds
        anim.SetTrigger("face_greeting");
    }

    // Update is called once per frame
    void Update()
    {
        if (isTalking)
        {
            speechTimmer += Time.deltaTime;
            if (speechTimmer >= secondsTak) { stopTalking(speechTimmer, secondsTak); speechTimmer = 0; }
        }
    }


    //---------------------------------------------------------------------------------------Animations

    public IEnumerator no_loop_bodyAimation(string nameExpression, string nameAnimation)//beining 
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
    public IEnumerator no_loop_bodyGENERICANIM(string nameExpression, string nameAnimation)//generic
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
    }
    public void loop_bodyAnimation(string nameExpression)
    {
        anim.SetTrigger(nameExpression);
    }

    public void loop_FaceExpressions(string nameExpression)
    {
        anim.SetTrigger(nameExpression);
    }

    //-------------------------Turn on and off Command recognize
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
            voicerecognicer.Start();
            //dictationRecognizer.Start();
            //nameAppToOpen = "brave";
            //OpenWordApp();
            //addCommmand();
        }
        else
        {
            listeningMessage.text = "Not Listening";
            btnListening.image.sprite = listenigDisabled;
            writingTextBox.readOnly = false;
            loop_bodyAnimation("idle");
            loop_FaceExpressions("face_base");
            isComandActive = false;
            voicerecognicer.Stop();

            if (isDictationActive)
            {
            isDictationActive = false;
            dictationRecognizer.Stop();
            dictationRecognizer.Dispose();
            }
        }
    }

    //-------------------------------------------------------Talking
    public void Talk(string theSpeech)
    {
        loop_FaceExpressions("face_talking");
        isTalking = true;
        float charactersPerSecond = 16f;
        float duration = theSpeech.Length / charactersPerSecond;
        secondsTak = duration;

        UnityEngine.Debug.Log(secondsTak);

        voice.Speak(theSpeech, SpeechVoiceSpeakFlags.SVSFlagsAsync);
    }

    public void stopTalking(float timmer, float speechDuration)
    {
        isTalking = false;
        timmer = 0f;
        speechDuration = 0f;
        secondsTak = 0f;
        loop_FaceExpressions("face_base");
    }
    public void onRecognizeLORAcommand(PhraseRecognizedEventArgs word)//executes prepareTotakeInstructions
    {
        string[] wordArray = word.text.Split(' ');
        nameAppToOpen = wordArray[1];
        fristCommand[word.text].Invoke();
        UnityEngine.Debug.Log(nameAppToOpen);
    }
    public void prepareTotakeInstructions()//listening commands 
    {
        loop_bodyAnimation("idle_listening");
        PhraseRecognitionSystem.Shutdown();

        if (voicerecognicer.IsRunning) {
            voicerecognicer.Stop();
            isComandActive = false;
        }
        if (!isDictationActive) {
            dictationRecognizer = new DictationRecognizer();
            dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;
            dictationRecognizer.Start();
            isDictationActive = true;
        }
    }

    public void loraThabks()
    {
        //StartCoroutine(no_loop_bodyAimation("thinking", "Thinking"));
    }

    //-----------------------------------------------------dictation recognizer
    private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {

        writingTextBox.text = text;
        dictationRecognizer.Stop();
        dictationRecognizer.Dispose();
        isDictationActive = false;



        if (!voicerecognicer.IsRunning)
        {
            PhraseRecognitionSystem.Restart();
            voicerecognicer.Start();
            isComandActive = true;
        }

        StartCoroutine(readLoraResponse(text));
       // StartCoroutine(openNotepad(text));

    }
    //request of buttom
    public void requestByButton()
    {
        string textBoxContent = writingTextBox.text.Trim();
        if (!string.IsNullOrEmpty(textBoxContent)) {
                StartCoroutine(readLoraResponse(writingTextBox.text));
        }
        writingTextBox.text = "";
    }

    //----------------------------------------http request to  Open AI API
    private async Task<string> SendChatGptRequest(string message)
    {
        using (HttpClient client = new HttpClient())
        {
            loop_bodyAnimation("thinking");

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {APIKey}");
           client.DefaultRequestHeaders.Add("OpenAI-Organization", organization);
            var requestData = new ChatGptRequestData
            {
                model = "gpt-3.5-turbo",
                messages = new ChatGptMessage[]
                {
                    new ChatGptMessage
                    {
                        role="system",
                        content="You are a helpful assistant known as Lora-Chan"
                    },
                    new ChatGptMessage
                    {
                      role="user",
                      content = message
                    }
                }
            };
            var json = JsonUtility.ToJson(requestData);
            UnityEngine.Debug.Log(message);
            UnityEngine.Debug.Log("requested data" + requestData);
            UnityEngine.Debug.Log("json to string" + json.ToString());

            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(ChatGPTEndpoint, data);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }
            else
            {
                UnityEngine.Debug.Log($"Chat GPT API request failed with status code: {response.StatusCode}");
                UnityEngine.Debug.Log(response.ToSafeString());

                if (response.StatusCode .ToString() == "TooManyRequests")
                { 
                Talk("Lo lamento, ahora mismo ya no me encuentro disponible para responder peticiones, intenta más tarde");
                }
                else
                {
                    Talk("Me disculpo, algo ha salido mal, y no he podido procesar tu petición");

                }
                return null;
            }
        }
    }

   public async Task<string> AI_chatBotAPI_REQUEST(string message)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://chat-gpt-ai-bot.p.rapidapi.com/GenerateAIWritter?prompt={message}"),
            Headers =
                {
                    { "X-RapidAPI-Key", "" },
                    { "X-RapidAPI-Host", "chat-gpt-ai-bot.p.rapidapi.com" },
                },
        };

        try
        {
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                UnityEngine.Debug.Log(body);
                if (body != null) { return body; }
                else return null;
            }
        }
        catch(Exception e){
            Talk(e.Message.ToString() + " por el momento ya no me encuentro disponible, vuelve más tarde");
            return null;
        }
 
    }

    private IEnumerator readLoraResponse(string message)
    {
        loop_bodyAnimation("thinking");
        string input = message;
        // Task<string> responseTask = SendChatGptRequest(input); 
         Task<string> responseTask = AI_chatBotAPI_REQUEST(input); 

        yield return new WaitUntil(()=> responseTask.IsCompleted); 
        
        string response = responseTask.Result;

        if (response!=null)
        {
            string responseText = "";
            string code = "no";
            int characterCounter = 0;
            int codeFristIndex = response.IndexOf("```");

            

            for(int i = 0; i < response.Length; i++)
            {
                responseText += response[i].ToString();
                if (characterCounter > 77)
                {
                    response += "\n";
                    characterCounter = 0;
                }
                characterCounter++;
            }

            Talk(response.ToSafeString());
            StartCoroutine(openNotepad(responseText));
            UnityEngine.Debug.Log(response);
        }
    }
    //--------------------------------Open and Write Noteblock
    private IEnumerator openNotepad(string writedMessage){
        StartCoroutine(no_loop_bodyGENERICANIM("open_app", "open_app"));
        string tempFilepath = Path.GetTempFileName();
        File.WriteAllText(tempFilepath, writedMessage);

        Process process = Process.Start("notepad.exe", tempFilepath);
        
        yield return new WaitForSeconds(1);//wait a little time to ensure that is already open

        File.Delete(tempFilepath);
    }


    //------------------------------------------------------------------------------------------------------------Functions for commands
    //-----Show or hide command settings
    public void controllerContainerCommand()
    {
        isComandSettinsActive = !isComandSettinsActive;
        panelCommands.SetActive(isComandSettinsActive);
    }
    //-----Load Commands
    public void LoadSavedCommnads()
    {
        string ruta = Path.Combine(Application.persistentDataPath, "LoraCommandsData.json");
        if(File.Exists(ruta))
        {
            string json = File.ReadAllText(ruta);
            CommandData data = JsonUtility.FromJson<CommandData>(json);
            CommandElement.gameObject.SetActive(true);
            data.commandList.ForEach(command =>
            {
                if (!fristCommand.ContainsKey(command.nombre))
                {
                    fristCommand.Add($"abre {command.nombre}", OpenWordApp);
                    GameObject item = Instantiate(CommandElement, ScrollListCommand);
                    TextMeshProUGUI nameCommand = item.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                    nameCommand.text = command.nombre;
                }
            });
            CommandElement.gameObject.SetActive(false);
                    //UnityEngine.GameObject.Destroy(CommandElement);
        }
    }
    //-----generic command open
    public void OpenWordApp()
    {
        try
        {
            StartCoroutine(no_loop_bodyGENERICANIM("open_app","open_app"));
            ProcessStartInfo theProgram = new ProcessStartInfo();
            string file = nameAppToOpen + ".exe";

            theProgram.FileName = file;
            theProgram.Arguments = DateTime.Now.ToString("yyyyMMddHHmmss"); //generate unique argument
            Process.Start(theProgram);

        }catch (Exception e)
        {
            UnityEngine.Debug.Log(e.Message);
           string speechFailToOpen = $"Lo siento, no he podido abrir {nameAppToOpen}, verifique que el nombre de la aplicación sea el correcto observando la ubicación del archivo en cuesrión";
            Talk(speechFailToOpen);
        }
    }
    //write json
    public void addCommmand()
    {
        string ruta = Path.Combine(Application.persistentDataPath, "LoraCommandsData.json");

        if (File.Exists(ruta) && !string.IsNullOrEmpty(commandToADD.text))
        {
            string json = File.ReadAllText(ruta);
            CommandData data = JsonUtility.FromJson<CommandData>(json);
           
            if (!data.commandList.Exists(item => item.nombre == commandToADD.text))
            {
                data.commandList.Add(
                        new Commando
                        {
                            nombre = commandToADD.text
                        }
                    ); 
               var updatedJson = JsonUtility.ToJson(data);
                File.WriteAllText(ruta, updatedJson);
                commandToADD.text = "";
            }
        }
        else if(!string.IsNullOrEmpty(commandToADD.text))
        {
            UnityEngine.Debug.Log("File DOES NOT EXIST");

            var DataCommand = new CommandData
            {
                commandList = new List<Commando>
                {
                    new Commando
                    {
                        nombre = commandToADD.text
                    }
                }
            };
            var json = JsonUtility.ToJson(DataCommand);
            File.WriteAllText(ruta, json);
            commandToADD.text = "";
        }

        UpdateCommandList();
    }

    public void deleteCommand(GameObject gameOBJ){
        string ruta = Path.Combine(Application.persistentDataPath, "LoraCommandsData.json");
        
       
        TextMeshProUGUI name = gameOBJ.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        UnityEngine.Debug.Log(name.text);
        if(name != null)
        {
            string json = File.ReadAllText(ruta);
            CommandData data = JsonUtility.FromJson<CommandData>(json);
            
            var elemenToDelete = data.commandList.Find(a => a.nombre == name.text);
            data.commandList.Remove(elemenToDelete);

            var updatedJson = JsonUtility.ToJson(data);
            File.WriteAllText(ruta, updatedJson);

            UnityEngine.GameObject.Destroy(gameOBJ);
            UpdateCommandList();
        }
    }

    public void UpdateCommandList()
    {
        fristCommand.Clear();
        for(int i = ScrollListCommand.childCount-1; i > 0; i--)
        {
            Transform child = ScrollListCommand.GetChild(i);
              UnityEngine.GameObject.Destroy(child.gameObject);
        }
        fristCommand.Add("lora chan", prepareTotakeInstructions);
        fristCommand.Add("lora", prepareTotakeInstructions);
        
        LoadSavedCommnads();

        voicerecognicer.Stop();
        voicerecognicer.Dispose();
        voicerecognicer = new KeywordRecognizer(fristCommand.Keys.ToArray());
        voicerecognicer.OnPhraseRecognized += onRecognizeLORAcommand;
        
        if(isListening) voicerecognicer.Start();
    }

}

//-------------------------Classes to convert to json
[System.Serializable]
public class Commando
{
    public string nombre;
}
[System.Serializable]

public class CommandData
{
    public List<Commando> commandList;
    public string OrganizationCredentials;
    public string ApiKey;
}

[System.Serializable]
public class ChatGptRequestData
{
    public ChatGptMessage[] messages;
    public string model;
}

[System.Serializable]
public class ChatGptMessage
{
    public string role;
    public string content;
}
