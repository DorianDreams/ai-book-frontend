using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;


public class Request : MonoBehaviour
{
    // StableDiffusionXLTurbo Call
    public static IEnumerator GetImageGeneration(string caption, float strength, byte[] screenshot)
    {
        string json = "{\"strength" + "\":" + strength.ToString("0.0", CultureInfo.InvariantCulture) + "}";
        Debug.Log(json);
        string prompt = Metadata.Instance.consistencyPrompt + Metadata.Instance.currentPrompt + caption;
        string url = "http://127.0.0.1:8000/api/images/" + Metadata.Instance.storyBookId
                                              + "?prompt=" + prompt
                                              + "&parameters=" + json;
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", screenshot);
        form.headers["Content-Type"] = "multipart/form-data";
        Debug.Log(form.ToString());

        UnityWebRequest request = UnityWebRequest.Post(url, form);

        yield return request.SendWebRequest();
        Dictionary<string, object> returnVal = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.downloadHandler.text);
        yield return returnVal;
    }

    // Language Model Inference Calls 
    public static IEnumerator GetSentenceCompletion(byte[] bytes, string sentence, float temperature)
    {
        string url = "http://127.0.0.1:8000/api/chat/chapters?prompt=" + sentence + "&temperature=" + 
            temperature.ToString("0.0", CultureInfo.InvariantCulture) + "&ch_index=" + Metadata.Instance.currentChapter;
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", bytes);
        form.headers["Content-Type"] = "multipart/form-data";

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        yield return request.SendWebRequest();

        Dictionary<string, string> returnVal = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
        string completed_sentence = returnVal["generated_description"].ToString();
        yield return completed_sentence;
    }

    public static IEnumerator GetNextprompt(string previous_story)
    {
        string url = "http://127.0.0.1:8000/api/chat/nextprompts?prev_story=" + previous_story;
        WWWForm form = new WWWForm();
        UnityWebRequest request = UnityWebRequest.Post(url, form);
        yield return request.SendWebRequest();
        
        Dictionary<string, string> returnVal = JsonConvert.DeserializeObject
            <Dictionary<string, string>>(request.downloadHandler.text);
        string next_prompt = returnVal["next_prompt"].ToString();
        yield return next_prompt;
    }

    // Call Tiny-Llama for Title Creation
    public static IEnumerator CreateTitle(string alltext)
    {
        string json = "{ \"user_input\":" + "\"" + alltext + "\"" + "}";

        using (UnityWebRequest request = UnityWebRequest.Post("http://127.0.0.1:8000/api/chat/titles", json, "application/json"))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                Dictionary<string, object> returnVal = JsonConvert.DeserializeObject
                    <Dictionary<string, object>>(request.downloadHandler.text);


                string title = returnVal["generated_text"].ToString();
                var sb2 = new StringBuilder(title.Length);

                foreach (char i in title)
                    if (i != '"')
                        sb2.Append(i);
                title = sb2.ToString();
                yield return title;

            }
        }
    }

    // Call Seamless for Language Translation
    public static IEnumerator TranslateSentence(string sentence)
    {
        string json = "{ \"tgt_lang\":" + "\"" + "deu" + "\"," + "\"user_input\":" + "\"" + sentence + "\"" + "}";

        using (UnityWebRequest request = UnityWebRequest.Post("http://127.0.0.1:8000/api/chat/translations", json, "application/json"))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                Dictionary<string, object> returnVal = JsonConvert.DeserializeObject
                    <Dictionary<string, object>>(request.downloadHandler.text);


                string translation = returnVal["generated_text"].ToString();
                yield return translation;

            }
        }
    }



    // Unused for now
    public static IEnumerator GetChapterStories(string completion)
    {
        string url = "http://127.0.0.1:8000/api/chat/chapterstories?ch_index=" + Metadata.Instance.currentChapter + "&prompt=" + completion;
        WWWForm form = new WWWForm();
        //form.headers["Content-Type"] = "multipart/form-data";
        UnityWebRequest request = UnityWebRequest.Post(url, form);
        yield return request.SendWebRequest();
       
        Dictionary<string, string> returnVal = JsonConvert.DeserializeObject
            <Dictionary<string, string>>(request.downloadHandler.text);
        string generated_description = returnVal["generated_description"].ToString();
        yield return generated_description;
        

    }

    // BLIP Inference Call
    public static IEnumerator GetImageCaption(byte[] bytes)
    {
        string url = "http://127.0.0.1:8000/api/chat/captions";
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", bytes);
        form.headers["Content-Type"] = "multipart/form-data";
        Debug.Log(form.ToString());

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            int count = 0;
            while (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
                request = UnityWebRequest.Post(url, form);
                yield return request.SendWebRequest();
                count++;
                if (count > 10)
                {
                    EventSystem.instance.RestartSceneEvent();
                }
            }
            Dictionary<string, string> returnVal = JsonConvert.DeserializeObject
                <Dictionary<string, string>>(request.downloadHandler.text);
            string caption = returnVal["caption"].ToString();
            yield return caption;
        }
        else
        {
            Dictionary<string, string> returnVal = JsonConvert.DeserializeObject
                <Dictionary<string, string>>(request.downloadHandler.text);
            string caption = returnVal["caption"].ToString();
            yield return caption;
        }
    }



    // CRUD Calls
    public static IEnumerator CreateStoryBook()
    {
        StoryBook storyBook = new StoryBook(Metadata.Instance.startingPrompt, false, false);
        Metadata.Instance.storyBook = storyBook;
        string json = JsonUtility.ToJson(storyBook);
        string url = "http://127.0.0.1:8000/api/storybooks";

        using (UnityWebRequest request = UnityWebRequest.Post(url, json, "application/json"))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                Dictionary<string, object> returnVal = JsonConvert.DeserializeObject
                    <Dictionary<string, object>>(request.downloadHandler.text);


                Metadata.Instance.storyBookId = returnVal["id"].ToString();
            }
        }
    }

    public static IEnumerator PostImageDescription(string description, string image_id)
    {
        string url = "http://127.0.0.1:8000/api/descriptions/" + Metadata.Instance.storyBookId + "/" + image_id;

        var sb = new StringBuilder(description.Length);

        foreach (char i in description)
            if (i != '\n' && i != '\r' && i != '\t' && i != '"')
                sb.Append(i);
        description = sb.ToString();


        string json = "{ \"description\":" + "\"" + description + "\"" + "}";

        using (UnityWebRequest request = UnityWebRequest.Post(url, json, "application/json"))
        {
            yield return request.SendWebRequest();
        }
    }


}