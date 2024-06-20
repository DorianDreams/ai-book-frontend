using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using Unity.Collections;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

// Based on ScreenRendering: https://docs.unity3d.com/ScriptReference/Texture2D.ReadPixels.html
public class BookCovers : MonoBehaviour
{
    public Image Picture;
    public Image Cover;
    public Image Spine;
    public TextMeshProUGUI Text;
    public TextMeshProUGUI AuthorshipText;
    public Texture RenderTexture;

    private string[] colors = { "#FFFF00", "#F59C00", "#FF00E9", "#FF0000", "#0000FF", "#743B0A", "#008000", "#5FD2CE" };
    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("S key was pressed.");
            SaveTextureToFile(RenderTexture, "E:\\thesis\\unity\\AI-Book-Frontend\\src\\Assets\\Resources\\BookCovers\\" + System.DateTime.Now.Ticks + ".jpg", 1024, 819);
        } */
    }

    void Start()
    {
        EventSystem.instance.SaveCurrentCover += OnSaveCurrentCover;
        EventSystem.instance.ChooseCoverImage += OnChooseCoverImage;
        EventSystem.instance.ChooseCoverAuthor += OnChooseAuthorship;
        int xcount = Random.Range(0, 8);
        string currentColor = colors[xcount];
        UnityEngine.Color newCol;
        if (UnityEngine.ColorUtility.TryParseHtmlString(currentColor, out newCol))
        {
            Cover.GetComponent<Image>().color = newCol;
            Spine.GetComponent<Image>().color = newCol;
        }
    }

    void OnChooseAuthorship(string decision)
    {
        AuthorshipText.text = "By "+decision;
    }
    void OnSaveCurrentCover()
    {
        Debug.Log("Save current cover");
        SaveTextureToFile(RenderTexture, "E:\\thesis\\unity\\AI-Book-Frontend\\src\\Assets\\Resources\\BookCovers\\" + System.DateTime.Now.Ticks + ".jpg", 1024, 819);
    }

    void OnChooseCoverImage(byte[] newcover)
    {
        Texture2D tex = new Texture2D(512, 512);
        tex.LoadImage(newcover);
        tex.Apply();
        Picture.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }






    public enum SaveTextureFileFormat
    {
        EXR, JPG, PNG, TGA
    };


    // Convert the render texture to an image: https://forum.unity.com/threads/save-rendertexture-or-texture2d-as-image-file-utility.1325130/#post-8387577
    static public void SaveTextureToFile(Texture source,
                                         string filePath,
                                         int width,
                                         int height,
                                         SaveTextureFileFormat fileFormat = SaveTextureFileFormat.PNG,
                                         int jpgQuality = 95,
                                         bool asynchronous = true,
                                         System.Action<bool> done = null)
    {
        Debug.Log("Saving texture to file: " + filePath);
        // check that the input we're getting is something we can handle:
        if (!(source is Texture2D || source is RenderTexture))
        {
            done?.Invoke(false);
            return;
        }

        // use the original texture size in case the input is negative:
        if (width < 0 || height < 0)
        {
            width = source.width;
            height = source.height;
        }

        // resize the original image:
        var resizeRT = UnityEngine.RenderTexture.GetTemporary(width, height, 0);
        UnityEngine.Graphics.Blit(source, resizeRT);

        // create a native array to receive data from the GPU:
        var narray = new NativeArray<byte>(width * height * 4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        // request the texture data back from the GPU:
        var request = AsyncGPUReadback.RequestIntoNativeArray(ref narray, resizeRT, 0, (AsyncGPUReadbackRequest request) =>
        {
            // if the readback was successful, encode and write the results to disk
            if (!request.hasError)
            {
                NativeArray<byte> encoded;

                switch (fileFormat)
                {
                    case SaveTextureFileFormat.EXR:
                        encoded = ImageConversion.EncodeNativeArrayToEXR(narray, resizeRT.graphicsFormat, (uint)width, (uint)height);
                        break;
                    case SaveTextureFileFormat.JPG:
                        encoded = ImageConversion.EncodeNativeArrayToJPG(narray, resizeRT.graphicsFormat, (uint)width, (uint)height, 0, jpgQuality);
                        break;
                    case SaveTextureFileFormat.TGA:
                        encoded = ImageConversion.EncodeNativeArrayToTGA(narray, resizeRT.graphicsFormat, (uint)width, (uint)height);
                        break;
                    default:
                        encoded = ImageConversion.EncodeNativeArrayToPNG(narray, resizeRT.graphicsFormat, (uint)width, (uint)height);
                        break;
                }

                System.IO.File.WriteAllBytes(filePath, encoded.ToArray());
                encoded.Dispose();
            }

            narray.Dispose();

            // notify the user that the operation is done, and its outcome.
            done?.Invoke(!request.hasError);
        });

        if (!asynchronous)
            request.WaitForCompletion();
    }


} 
