using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LMNT {

public static class LMNTLoader {
    public static string LoadApiKey() {
        var currentKey = Resources.Load<TextAsset>("LMNT_API_Key");
        if (currentKey != null) {
            return currentKey.ToString();
        }
        return null;
    }

    public static void StoreApiKey(string apiKey) {
        SaveTextResource("LMNT_API_Key", apiKey);
    }

    public static List<Voice> LoadVoices() {
        List<Voice> voiceList = new List<Voice>();
        var voicesFile = Resources.Load<TextAsset>("LMNT_Voices");
        if (voicesFile == null) {
            return voiceList;
        }

        string[] lines = voicesFile.ToString().Split('\n');
        foreach (string line in lines) {
            if (line.Length == 0) {
                continue;
            }
            string[] fields = line.Split('|');
            voiceList.Add(new Voice(fields[0], fields[1]));
        }
        return voiceList;
    }

    public static void StoreVoices(List<Voice> voiceList) {
        StringBuilder sb = new StringBuilder();
        foreach (Voice v in voiceList) {
            sb.Append($"{v.id}|{v.name}\n");
        }
        SaveTextResource("LMNT_Voices", sb.ToString());
    }

    private static void SaveTextResource(string name, string value) {
#if UNITY_EDITOR
		if (!AssetDatabase.IsValidFolder("Assets/Resources")) {
			AssetDatabase.CreateFolder("Assets", "Resources");
		}

		string path = Application.dataPath + $"/Resources/{name}.txt";
		using (FileStream fs = new FileStream(path, FileMode.Create)) {
			using (StreamWriter writer = new StreamWriter(fs)) {
				writer.Write(value);
			}
		}
		AssetDatabase.Refresh();
#endif
    }
}

}
