using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class Textboxes : MonoBehaviour
{

    public GameObject TextBoxPrefab;

    private int initialPrompts = 11;
    private ArrayList initialPromptList;
    private string[] initialPromptArray;
    private ArrayList instantiatedTextBoxes;

    // Start is called before the first frame update
    void Start()
    {
        getInitialPrompts();
        reshuffle(initialPromptArray);
        EventSystem.instance.ChangeLocale += OnChangeLocale;
        instantiatedTextBoxes = new ArrayList();

        for (int i = 0; i < initialPrompts; i++)
        {
            GameObject textBox = Instantiate(TextBoxPrefab);
            textBox.transform.SetParent(this.transform, false);
            textBox.GetComponentInChildren<TextMeshProUGUI>().SetText(initialPromptArray[i]);
            instantiatedTextBoxes.Add(textBox);
        }
    }

    void getInitialPrompts()
    {
        initialPromptList = new ArrayList();
        StringTable table = LocalizationSettings.StringDatabase.GetTable("Translations");
        for (int i = 1; i < initialPrompts + 1; i++)
        {
            initialPromptList.Add(table.GetEntry("InitialPrompt" + i).LocalizedValue);

        }
        initialPromptArray = (string[])initialPromptList.ToArray(typeof(string));
        
    }

    void OnChangeLocale()
    {
        getInitialPrompts();
        int i = 0;
        foreach (GameObject textBox in instantiatedTextBoxes) {
            textBox.GetComponentInChildren<TextMeshProUGUI>().SetText(initialPromptArray[i]);
            i++;
         }
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
}
