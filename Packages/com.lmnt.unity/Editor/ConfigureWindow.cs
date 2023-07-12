using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TinyJson;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.VSAttribution.LMNT;
using UnityEngine;
using UnityEngine.UIElements;

namespace LMNT {

using VoicesJson = Dictionary<string, Dictionary<string, Dictionary<string, string>>>;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

public class ConfigureWindow : EditorWindow {
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

	private TextField txtApiKey;
	private Button btnSave;
	private Button btnGetKey;
	private Label lblError;

    [MenuItem("LMNT/Configure API key...")]
    public static void OnConfigure()
    {
        ConfigureWindow wnd = GetWindow<ConfigureWindow>();
        wnd.titleContent = new GUIContent("Welcome to LMNT");
		wnd.minSize = new Vector2(400, 350);
    }

	[MenuItem("LMNT/Update voice list")]
	public static async void OnUpdateVoices() {
		string apiKey = LMNTLoader.LoadApiKey();
		(string json, HttpStatusCode code, bool requiresAttribution) = await FetchVoices(apiKey);
		if ((int)code >= 400) {
			EditorUtility.DisplayDialog("Error updating voices", $"There was an error updating the voice list. Status code: {(int)code}", "That's too bad, but OK");
			return;
		}

		var parsedJson = JSONParser.FromJson<VoicesJson>(json);
		var voices = parsedJson["voices"];
		List<Voice> voiceList = new List<Voice>();
		foreach (var entry in voices) {
			voiceList.Add(new Voice(entry.Key, entry.Value["name"]));
		}
		LMNTLoader.StoreVoices(voiceList);
	}

	[MenuItem("LMNT/Update voice list", true)]
	public static bool OnUpdateVoicesEnabled() {
		return LMNTLoader.LoadApiKey() != null;
	}

    public void CreateGUI() {
        VisualElement root = rootVisualElement;

		Image logo = new Image();
		logo.image = AssetDatabase.LoadAssetAtPath<Texture>(Constants.LMNT_LOGO_PATH);
		logo.style.height = 80;
		logo.style.marginBottom = 40;
		logo.style.marginTop = 20;
		logo.style.marginLeft = 10;
		logo.style.marginRight = 10;

        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();

		root.Add(logo);
        root.Add(labelFromUXML);

		txtApiKey = root.Query<TextField>("textApiKey");
		txtApiKey.RegisterCallback<ChangeEvent<string>>(OnKeyChange);

		btnSave = root.Query<Button>("btnSave");
		btnSave.SetEnabled(false);
		btnSave.clicked += OnSaveClicked;

		btnGetKey = root.Query<Button>("btnGetKey");
		btnGetKey.clicked += OnGetKeyClicked;

		lblError = root.Query<Label>("lblError");

		string apiKey = LMNTLoader.LoadApiKey();
		if (apiKey != null) {
			txtApiKey.value = apiKey;
		}
    }

	private void OnKeyChange(ChangeEvent<string> evt) {
		btnSave.SetEnabled(evt.newValue.Length == 32);
	}

	private async void OnSaveClicked() {
		string apiKey = txtApiKey.text;
		(string json, HttpStatusCode code, bool requiresAttribution) = await FetchVoices(apiKey);
		if ((int)code >= 400) {
			lblError.text = "Invalid API key";
			return;
		}

		var parsedJson = JSONParser.FromJson<VoicesJson>(json);
		var voices = parsedJson["voices"];
		List<Voice> voiceList = new List<Voice>();
		foreach (var entry in voices) {
			voiceList.Add(new Voice(entry.Key, entry.Value["name"]));
		}
		LMNTLoader.StoreVoices(voiceList);
		LMNTLoader.StoreApiKey(apiKey);
		if (requiresAttribution) {
			VSAttribution.SendAttributionEvent("login", "LMNT", apiKey);
		}
		Close();
	}

	private void OnGetKeyClicked() {
		Application.OpenURL(Constants.LMNT_SIGNUP_URL);
	}

	private static async Task<(string, HttpStatusCode, bool)> FetchVoices(string apiKey) {
                var packageInfo = PackageInfo.FindForAssetPath(Constants.LMNT_PACKAGE_PATH);
		var request = WebRequest.Create(Constants.LMNT_VOICES_URL);
		request.Method = "get";
		request.Headers.Add("X-API-Key", apiKey);
                request.Headers.Add("X-Client", $"unity/{packageInfo.version}");
		try {
			using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse) {
				using (Stream streamResponse = response.GetResponseStream()) {
					using (StreamReader reader = new StreamReader(streamResponse)) {
                                                bool requiresAttribution = response.Headers["X-Attribution"] == "true" || response.Headers["X-Attribution"] == "unity";
						return (reader.ReadToEnd(), HttpStatusCode.OK, requiresAttribution);
					}
				}
			}
		} catch (WebException e) {
			return (null, ((HttpWebResponse)e.Response).StatusCode, false);
		}
	}
}

}
