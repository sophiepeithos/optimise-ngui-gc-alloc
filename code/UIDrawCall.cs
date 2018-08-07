public class UIDrawCall : MonoBehaviour
{
	public void UpdateGeometry (int widgetCount)
	{
		this.widgetCount = widgetCount;
		int vertexCount = verts.Count;

		// Safety check to ensure we get valid values
		if (vertexCount > 0 && (vertexCount == uvs.Count && vertexCount == cols.Count) && (vertexCount % 4) == 0)
		{
			if (mColorSpace == ColorSpace.Uninitialized)
				mColorSpace = QualitySettings.activeColorSpace;

			if (mColorSpace == ColorSpace.Linear)
			{
				for (int i = 0; i < vertexCount; ++i)
				{
					var c = cols[i];
					c.r = Mathf.GammaToLinearSpace(c.r);
					c.g = Mathf.GammaToLinearSpace(c.g);
					c.b = Mathf.GammaToLinearSpace(c.b);
					c.a = Mathf.GammaToLinearSpace(c.a);
					cols[i] = c;
				}
			}

			// Cache all components
			if (mFilter == null) mFilter = gameObject.GetComponent<MeshFilter>();
			if (mFilter == null) mFilter = gameObject.AddComponent<MeshFilter>();

			if (vertexCount < 65000)
			{
				// Populate the index buffer
				int indexCount = (vertexCount >> 1) * 3;
				bool setIndices = (mIndices == null || mIndices.Length != indexCount);

				// Create the mesh
				if (mMesh == null)
				{
					mMesh = new Mesh();
					mMesh.hideFlags = HideFlags.DontSave;
					mMesh.name = (mMaterial != null) ? "[NGUI] " + mMaterial.name : "[NGUI] Mesh";
					if (dx9BugWorkaround == 0) mMesh.MarkDynamic();
					setIndices = true;
				}
#if !UNITY_FLASH
				// If the buffer length doesn't match, we need to trim all buffers
				bool trim = uvs.Count != vertexCount || cols.Count != vertexCount || uv2.Count != vertexCount || norms.Count != vertexCount || tans.Count != vertexCount;

				// Non-automatic render queues rely on Z position, so it's a good idea to trim everything
				if (!trim && panel != null && panel.renderQueue != UIPanel.RenderQueue.Automatic)
					trim = (mMesh == null || mMesh.vertexCount != verts.Count);

				// NOTE: Apparently there is a bug with Adreno devices:
				// http://www.tasharen.com/forum/index.php?topic=8415.0
 #if !UNITY_ANDROID
				// If the number of vertices in the buffer is less than half of the full buffer, trim it
				if (!trim && (vertexCount << 1) < verts.Count) trim = true;
 #endif
#endif
				mTriangles = (vertexCount >> 1);

				if (mMesh.vertexCount != vertexCount)
				{
					mMesh.Clear();
					setIndices = true;
				}
#if UNITY_4_7
				var hasUV2 = (uv2 != null && uv2.Count == vertexCount);
				var hasNormals = (norms != null && norms.Count == vertexCount);
				var hasTans = (tans != null && tans.Count == vertexCount);

				if (mTempVerts == null || mTempVerts.Length < vertexCount) mTempVerts = new Vector3[vertexCount];
				if (mTempUV0 == null || mTempUV0.Length < vertexCount) mTempUV0 = new Vector2[vertexCount];
				if (mTempCols == null || mTempCols.Length < vertexCount) mTempCols = new Color[vertexCount];

				if (hasUV2 && (mTempUV2 == null || mTempUV2.Length < vertexCount)) mTempUV2 = new Vector2[vertexCount];
				if (hasNormals && (mTempNormals == null || mTempNormals.Length < vertexCount)) mTempNormals = new Vector3[vertexCount];
				if (hasTans && (mTempTans == null || mTempTans.Length < vertexCount)) mTempTans = new Vector4[vertexCount];

				verts.CopyTo(mTempVerts);
				uvs.CopyTo(mTempUV0);
				cols.CopyTo(mTempCols);

				if (hasNormals) norms.CopyTo(mTempNormals);
				if (hasTans) tans.CopyTo(mTempTans);
				if (hasUV2) for (int i = 0, imax = verts.Count; i < imax; ++i) mTempUV2[i] = uv2[i];

				mMesh.vertices = mTempVerts;
				mMesh.uv = mTempUV0;
				mMesh.colors = mTempCols;
				mMesh.uv2 = hasUV2 ? mTempUV2 : null;
				mMesh.normals = hasNormals ? mTempNormals : null;
				mMesh.tangents = hasTans ? mTempTans : null;
#else
				mMesh.SetVertices(verts);
				mMesh.SetUVs(0, uvs);
				mMesh.SetColors(cols);

 #if UNITY_5_4 || UNITY_5_5_OR_NEWER
				mMesh.SetUVs(1, (uv2.Count == vertexCount) ? uv2 : null);
				mMesh.SetNormals((norms.Count == vertexCount) ? norms : null);
				mMesh.SetTangents((tans.Count == vertexCount) ? tans : null);
 #else
				if (uv2.Count != vertexCount) uv2.Clear();
				if (norms.Count != vertexCount) norms.Clear();
				if (tans.Count != vertexCount) tans.Clear();

				mMesh.SetUVs(1, uv2);
				mMesh.SetNormals(norms);
				mMesh.SetTangents(tans);
 #endif
#endif
				if (setIndices)
				{
#if OPTIMISE_NGUI_GC_ALLOC
                    GenerateCachedIndexBuffer(vertexCount, indexCount);
                    var originalLength = mCache.OriginalLength();
                    mCache.AsArrayOfLength((ulong)indexCount);
                    mIndices = mCache.buffer;
                    mMesh.triangles = mIndices;
                    mCache.AsArrayOfLength(originalLength);
#else
                    mIndices = GenerateCachedIndexBuffer(vertexCount, indexCount);
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
				if (mMesh != null) mMesh.Clear();
				Debug.LogError("Too many vertices on one panel: " + vertexCount);
			}

			if (mRenderer == null) mRenderer = gameObject.GetComponent<MeshRenderer>();

			if (mRenderer == null)
			{
				mRenderer = gameObject.AddComponent<MeshRenderer>();
#if UNITY_EDITOR
				mRenderer.enabled = isActive;
#endif
#if !UNITY_4_7
				if (mShadowMode == ShadowMode.None)
				{
					mRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
					mRenderer.receiveShadows = false;
				}
				else if (mShadowMode == ShadowMode.Receive)
				{
					mRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
					mRenderer.receiveShadows = true;
				}
				else
				{
					mRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
					mRenderer.receiveShadows = true;
				}
#endif
			}

			if (mIsNew)
			{
				mIsNew = false;
				if (onCreateDrawCall != null) onCreateDrawCall(this, mFilter, mRenderer);
			}

			UpdateMaterials();
		}
		else
		{
			if (mFilter.mesh != null) mFilter.mesh.Clear();
			Debug.LogError("UIWidgets must fill the buffer with 4 vertices per quad. Found " + vertexCount);
		}

		verts.Clear();
		uvs.Clear();
		uv2.Clear();
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
#if OPTIMISE_NGUI_GC_ALLOC
    void GenerateCachedIndexBuffer(int vertexCount, int indexCount)
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
			if (dc.onCreateDrawCall != null)
			{
				NGUITools.Destroy(dc.gameObject);
				return;
			}

			dc.onRender = null;

			if (Application.isPlaying)
			{
				if (mActiveList.Remove(dc))
				{
					NGUITools.SetActive(dc.gameObject, false);
					mInactiveList.Add(dc);
#if OPTIMISE_NGUI_GC_ALLOC
                    CachedGeometries.PushToCachedGeometries(dc.verts, dc.uvs, dc.cols);
                    dc.verts = new List<Vector3>();
                    dc.uvs = new List<Vector2>();
                    dc.cols = new List<Color>();
#endif
                    dc.mIsNew = true;
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
