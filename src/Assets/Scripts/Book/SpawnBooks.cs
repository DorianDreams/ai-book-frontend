using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpawnBooks : MonoBehaviour
{
    public GameObject Anchor;
    public GameObject BookPrefab;

    private Texture2D[] textures;

    // Start is called before the first frame update
    void Start()
    {
        textures = Resources.LoadAll<Texture2D>("BookCovers");
        Shuffle(textures);

        spawn();
    }

    //https://gist.github.com/jasonmarziani/7b4769673d0b593457609b392536e9f9
    void Shuffle(Texture2D[] a)
    {
        // Loops through array
        for (int i = a.Length - 1; i > 0; i--)
        {
            // Randomize a number between 0 and i (so that the range decreases each time)
            int rnd = UnityEngine.Random.Range(0, i);

            // Save the value of the current i, otherwise it'll overright when we swap the values
            Texture2D temp = a[i];

            // Swap the new and old values
            a[i] = a[rnd];
            a[rnd] = temp;
        }

        // Print
        for (int i = 0; i < a.Length; i++)
        {
            Debug.Log(a[i]);
        }
    }

    void spawn()
    {
        int k = 0;
        for (int i = 1; i < 6; i++)
        {
           
            for (int j = 0; j < 3; j++)
            {
                GameObject book = Instantiate(BookPrefab, new Vector3(Anchor.transform.position.x + i * 0.4f,
                                                Anchor.transform.position.y,
                                                Anchor.transform.position.z - j * 0.4f),
                                                Quaternion.Euler(0, 180, 0));

                book.GetComponent<Renderer>().material = new Material(Shader.Find("Mobile/Diffuse"));
                book.GetComponent<Renderer>().material.mainTexture = textures[k];
                k++;

            }
    }
    }

        
}
