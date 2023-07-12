using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LMNT {

[CustomEditor(typeof(LMNTSpeech))]
public class LMNTSpeechInspector : Editor {
	private List<Voice> voiceList;

	public void OnEnable() {
		voiceList = LMNTLoader.LoadVoices();
	}

	public override VisualElement CreateInspectorGUI() {
		VisualElement inspector = new VisualElement();

		VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Constants.LMNT_SPEECH_INSPECTOR_XML);
		visualTree.CloneTree(inspector);

		DropdownField voicesElement = inspector.Query<DropdownField>("voice");
		voicesElement.choices = voiceList.Select(v => v.name).ToList<string>();
		return inspector;
	}
}

}
