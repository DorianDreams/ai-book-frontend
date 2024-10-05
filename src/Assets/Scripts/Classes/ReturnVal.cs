using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnVal
{
    public string id;
    public string storybook_id;
    public string image;

    public ReturnVal(string id, string storybook_id, string image_url)
    {
        this.id = id;
        this.storybook_id = storybook_id;
        this.image = image_url;
    }
}
