using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StoryBook
{
    public string title;
    public string starting_sentence;
    public string decision_of_authorship;
    public bool finished_playthrough;
    public bool signed_the_book;
    public Drawing drawing;

    public StoryBook(string starting_sentence, bool finished_playthrough, bool signed_the_book)
    {
        this.title = "to be defined";
        this.starting_sentence = starting_sentence;
        this.decision_of_authorship = "to be defined";
        this.finished_playthrough = finished_playthrough;
        this.signed_the_book = signed_the_book;
        this.drawing = new Drawing();
    }
}
