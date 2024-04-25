using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// Pattern based on this forum answer https://discussions.unity.com/t/how-do-i-return-a-value-from-a-coroutine/6438/4
public class CoroutineWithData
{
    public Coroutine coroutine { get; private set; }
    public object result;
    private IEnumerator target;

    public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
    {
        this.target = target;
        this.coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
    }
}
