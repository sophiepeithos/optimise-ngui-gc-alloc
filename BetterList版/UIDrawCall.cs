public class UIDrawCall : MonoBehaviour
{
	public void UpdateGeometry ()
    {
        int count = verts.size;

		// Safety check to ensure we get valid values
		if (count > 0 && (count == uvs.size && count == cols.size) && (count % 4) == 0)
		{
			// Cache all components
			if (mFilter == null) mFilter = gameObject.GetComponent<MeshFilter>();
			if (mFilter == null) mFilter = gameObject.AddComponent<MeshFilter>();

			if (verts.size < 65000)
			{
				// Populate the index buffer
				int indexCount = (count >> 1) * 3;
				bool setIndices = (mIndices == null || mIndices.Length != indexCount);

				// Create the mesh
				if (mMesh == null)
				{
					mMesh = new Mesh();
					mMesh.hideFlags = HideFlags.DontSave;
					mMesh.name = (mMaterial != null) ? mMaterial.name : "Mesh";
					mMesh.MarkDynamic();
					setIndices = true;
				}
#if !UNITY_FLASH
				// If the buffer length doesn't match, we need to trim all buffers
				bool trim = (uvs.buffer.Length != verts.buffer.Length) ||
					(cols.buffer.Length != verts.buffer.Length) ||
					(norms.buffer != null && norms.buffer.Length != verts.buffer.Length) ||
					(tans.buffer != null && tans.buffer.Length != verts.buffer.Length);

				// Non-automatic render queues rely on Z position, so it's a good idea to trim everything
                //if (!trim && panel.renderQueue != UIPanel.RenderQueue.Automatic)
                //    trim = (mMesh == null || mMesh.vertexCount != verts.buffer.Length);

				// NOTE: Apparently there is a bug with Adreno devices:
				// http://www.tasharen.com/forum/index.php?topic=8415.0
#if !UNITY_ANDROID
				// If the number of vertices in the buffer is less than half of the full buffer, trim it
				if (!trim && (verts.size << 1) < verts.buffer.Length) trim = true;
#endif
				mTriangles = (verts.size >> 1);

				if (trim || verts.buffer.Length > 65000)
				{
					if (trim || mMesh.vertexCount != verts.size)
					{
						mMesh.Clear();
						setIndices = true;
					}

					mMesh.vertices = verts.ToArray();
					mMesh.uv = uvs.ToArray();
					mMesh.colors32 = cols.ToArray();

					if (norms != null) mMesh.normals = norms.ToArray();
					if (tans != null) mMesh.tangents = tans.ToArray();
				}
				else
				{
					if (mMesh.vertexCount != verts.buffer.Length)
					{
						mMesh.Clear();
						setIndices = true;
					}

					mMesh.vertices = verts.buffer;
					mMesh.uv = uvs.buffer;
					mMesh.colors32 = cols.buffer;

					if (norms != null) mMesh.normals = norms.buffer;
					if (tans != null) mMesh.tangents = tans.buffer;
				}
#else
				mTriangles = (verts.size >> 1);

				if (mMesh.vertexCount != verts.size)
				{
					mMesh.Clear();
					setIndices = true;
				}

				mMesh.vertices = verts.ToArray();
				mMesh.uv = uvs.ToArray();
				mMesh.colors32 = cols.ToArray();

				if (norms != null) mMesh.normals = norms.ToArray();
				if (tans != null) mMesh.tangents = tans.ToArray();
#endif
				if (setIndices)
				{
#if OPTIMISE_NGUI_GC_ALLOC
                    GenerateCachedIndexBuffer(count, indexCount, a =>
                    {
                        mIndices = a;
                        mMesh.triangles = mIndices;
                    });
#else
                    mIndices = GenerateCachedIndexBuffer(count, indexCount);
                    mMesh.triangles = mIndices;
#endif
                }

#if !UNITY_FLASH
				if (trim || !alwaysOnScreen)
#endif
					mMesh.RecalculateBounds();

				mFilter.mesh = mMesh;
			}
			else
			{
				mTriangles = 0;
				if (mFilter.mesh != null) mFilter.mesh.Clear();
				Debug.LogError("Too many vertices on one panel: " + verts.size);
			}

			if (mRenderer == null) mRenderer = gameObject.GetComponent<MeshRenderer>();

			if (mRenderer == null)
			{
				mRenderer = gameObject.AddComponent<MeshRenderer>();
#if UNITY_EDITOR
				mRenderer.enabled = isActive;
#endif
			}
			UpdateMaterials();
		}
		else
		{
			if (mFilter.mesh != null) mFilter.mesh.Clear();
			Debug.LogError("UIWidgets must fill the buffer with 4 vertices per quad. Found " + count);
		}

		verts.Clear();
		uvs.Clear();
		cols.Clear();
		norms.Clear();
		tans.Clear();
	}

	const int maxIndexBufferCache = 10;

#if UNITY_FLASH
	List<int[]> mCache = new List<int[]>(maxIndexBufferCache);
#else
#if !OPTIMISE_NGUI_GC_ALLOC
    static List<int[]> mCache = new List<int[]>(maxIndexBufferCache);
#else
    public static Nordeus.DataStructures.VaryingIntList mCache = new Nordeus.DataStructures.VaryingIntList();
#endif
#endif

    /// <summary>
    /// Generates a new index buffer for the specified number of vertices (or reuses an existing one).
    /// </summary>

#if OPTIMISE_NGUI_GC_ALLOC
    void GenerateCachedIndexBuffer(int vertexCount, int indexCount, Nordeus.DataStructures.VaryingIntList.ArrayAction action)
    {
        if (mCache.size < indexCount)
        {
            mCache.MakeLargerThan(indexCount);
            for (int i = mCache.size / 3 * 2; i < vertexCount; i += 4)
            {
                mCache.Add(i);
                mCache.Add(i + 1);
                mCache.Add(i + 2);

                mCache.Add(i + 2);
                mCache.Add(i + 3);
                mCache.Add(i);
            }
        }
        mCache.AsArrayOfLength(indexCount, action);
    }
#else
    int[] GenerateCachedIndexBuffer (int vertexCount, int indexCount)
	{
		for (int i = 0, imax = mCache.Count; i < imax; ++i)
		{
			int[] ids = mCache[i];
			if (ids != null && ids.Length == indexCount)
				return ids;
		}

		int[] rv = new int[indexCount];
		int index = 0;

		for (int i = 0; i < vertexCount; i += 4)
		{
			rv[index++] = i;
			rv[index++] = i + 1;
			rv[index++] = i + 2;

			rv[index++] = i + 2;
			rv[index++] = i + 3;
			rv[index++] = i;
		}

		if (mCache.Count > maxIndexBufferCache) mCache.RemoveAt(0);
		mCache.Add(rv);
		return rv;
	}
#endif

	static public void Destroy (UIDrawCall dc)
	{
		if (dc)
		{
			if (Application.isPlaying)
			{
				if (mActiveList.Remove(dc))
				{
					NGUITools.SetActive(dc.gameObject, false);
					mInactiveList.Add(dc);
#if OPTIMISE_NGUI_GC_ALLOC
                    CachedGeometries.PushToCachedGeometries(dc.verts, dc.uvs, dc.cols);
                    dc.verts = new BetterList<Vector3>();
                    dc.uvs = new BetterList<Vector2>();
                    dc.cols = new BetterList<Color32>();
#endif
                }
            }
			else
			{
				mActiveList.Remove(dc);
#if SHOW_HIDDEN_OBJECTS && UNITY_EDITOR
				if (UnityEditor.Selection.activeGameObject == dc.gameObject)
					UnityEditor.Selection.activeGameObject = null;
#endif
                NGUITools.DestroyImmediate(dc.gameObject);
			}
		}
	}
}
