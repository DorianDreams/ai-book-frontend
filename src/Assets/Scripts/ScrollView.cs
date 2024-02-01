using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class ScrollView : MonoBehaviour
{
    private int initialPrompts = 11;
    private ArrayList initialPromptList;
    private string[] initialPromptArray;
    // Start is called before the first frame update
    void Start()
    {
        initialPromptList = new ArrayList();
        StringTable table = LocalizationSettings.StringDatabase.GetTable("Translations");
        for (int i = 0; i < initialPrompts; i++)
        {
            initialPromptList.Add(table.GetEntry("InitialPrompt" + i));
            Debug.Log(initialPromptList[i]);

        }
        initialPromptArray = (string[])initialPromptList.ToArray(typeof(string));

    }

    void reshuffle(string[] texts)
    {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < texts.Length; t++)
        {
            string tmp = texts[t];
            int r = Random.Range(t, texts.Length);
            texts[t] = texts[r];
            texts[r] = tmp;
        }
    }

    public string GetRandomPrompt() {         
        reshuffle(initialPromptArray);
        return initialPromptArray[0];
       }
}
