using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CachedGeometries
{
    public static SortedDictionary<int, Stack<BetterList<Vector2>>> cachedListsOfVector2List = new SortedDictionary<int, Stack<BetterList<Vector2>>>();
    public static SortedDictionary<int, Stack<BetterList<Vector3>>> cachedListsOfVector3List = new SortedDictionary<int, Stack<BetterList<Vector3>>>();
    public static SortedDictionary<int, Stack<BetterList<Color32>>> cachedListsOfColorList = new SortedDictionary<int, Stack<BetterList<Color32>>>();

    public static void PushToCachedGeometries(UIGeometry geometry)
    {
        PushToCachedGeometries(geometry.verts, geometry.uvs, geometry.cols, geometry.mRtpVerts);
    }

    static void PushToCachedGeometries<T>(SortedDictionary<int, Stack<BetterList<T>>> cache, BetterList<T> source)
    {
        Stack<BetterList<T>> listsOfTList;
        if (!cache.TryGetValue(source.buffer.Length, out listsOfTList))
        {
            cache[source.buffer.Length] = (listsOfTList = new Stack<BetterList<T>>());
        }
        listsOfTList.Push(source);
    }

    public static void PushToCachedGeometries(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols, BetterList<Vector3> mRtpVerts = null)
    {
        PushToCachedGeometries(cachedListsOfVector3List, verts);
        PushToCachedGeometries(cachedListsOfVector2List, uvs);
        PushToCachedGeometries(cachedListsOfColorList, cols);
        if (mRtpVerts != null)
        {
            PushToCachedGeometries(cachedListsOfVector3List, mRtpVerts);
        }
    }

    public static void PullFromCachedGeometries(int vertexCount, UIGeometry geometry)
    {
        PullFromCachedGeometries(vertexCount, ref geometry.verts, ref geometry.uvs, ref geometry.cols, ref geometry.mRtpVerts);
    }

    static void PullFromCachedGeometries<T>(int vertexCount, SortedDictionary<int, Stack<BetterList<T>>> cache, ref BetterList<T> source)
    {
        foreach(var pair in cache)
        {
            if (pair.Key >= vertexCount && pair.Value.Count > 0)
            {
                source = pair.Value.Pop();
                source.Clear();
                return;
            }
        }
        if(source.buffer == null || source.buffer.Length < vertexCount)
        {
            source.buffer = new T[Mathf.Max(vertexCount, 32)];
        }
    }

    public static void PullFromCachedGeometries(int vertexCount, ref BetterList<Vector3> verts, ref BetterList<Vector2> uvs, ref BetterList<Color32> cols, ref BetterList<Vector3> mRtpVerts)
    {
        PullFromCachedGeometries(vertexCount, cachedListsOfVector3List, ref verts);
        if (mRtpVerts != null)
        {
            PullFromCachedGeometries(vertexCount, cachedListsOfVector3List, ref mRtpVerts);
        }
        PullFromCachedGeometries(vertexCount, cachedListsOfVector2List, ref uvs);
        PullFromCachedGeometries(vertexCount, cachedListsOfColorList, ref cols);
    }
}
