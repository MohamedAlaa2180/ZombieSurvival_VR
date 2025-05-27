using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T>
{
    public readonly Stack<T> pool;
    private readonly Func<T> createFunc;
    private readonly Action<T> onGet;
    private readonly Action<T> onRelease;
    private readonly Action<T> onDestroy;

    private int waitedObjects = 0;

    public ObjectPool(Func<T> createFunc, Action<T> onGet, Action<T> onRelease, Action<T> onDestroy = null)
    {
        pool = new Stack<T>();
        
        this.createFunc = createFunc;
        this.onGet = onGet;
        this.onRelease = onRelease;
        this.onDestroy = onDestroy;
        Debug.Log($"New pool created with count {pool.Count}");
    }

    public T Get()
    {       
        var obj = pool.Count > 0 ? pool.Pop() : createFunc();
        onGet(obj);
        waitedObjects++;
        return obj;
    }

    public void Release(T obj)
    {
        onRelease(obj);
        pool.Push(obj);
        waitedObjects--;
    }

    public async void Clear()
    {
        await UniTask.WaitUntil(() => waitedObjects <= 0);

        if (onDestroy != null)
        {
            foreach (var obj in pool)
                onDestroy(obj);
        }
        pool.Clear();
    }
}
