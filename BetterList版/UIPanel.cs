public class UIPanel : UIRect
{
#if OPTIMISE_NGUI_GC_ALLOC
    static List<UIWidget> widgetsInDrawCall = new List<UIWidget>();

    void FillDrawCallBuffers(UIDrawCall dc, int verticesCount)
    {
        //dc.verts.GrowIfMust(verticesCount);
        //dc.uvs.GrowIfMust(verticesCount);
        //dc.cols.GrowIfMust(verticesCount);
        BetterList<Vector3> notUsed = null;
        CachedGeometries.PullFromCachedGeometries(verticesCount, ref dc.verts, ref dc.uvs, ref dc.cols, ref notUsed);
        foreach (var widget in widgetsInDrawCall)
        {
            if (generateNormals) widget.WriteToBuffers(dc.verts, dc.uvs, dc.cols, dc.norms, dc.tans);
            else widget.WriteToBuffers(dc.verts, dc.uvs, dc.cols, null, null);
        }
        widgetsInDrawCall.Clear();
    }
#endif

	void FillAllDrawCalls ()
	{
		for (int i = 0; i < drawCalls.Count; ++i)
			UIDrawCall.Destroy(drawCalls[i]);
		drawCalls.Clear();

		Material mat = null;
		Texture tex = null;
		Shader sdr = null;
		UIDrawCall dc = null;
#if OPTIMISE_NGUI_GC_ALLOC
        int verticesCount = 0;
#endif
        if (mSortWidgets) SortWidgets();

		for (int i = 0; i < widgets.Count; ++i)
		{
			UIWidget w = widgets[i];

			if (w.isVisible && w.hasVertices)
			{
				Material mt = w.material;
				Texture tx = w.mainTexture;
				Shader sd = w.shader;

				if (mat != mt || tex != tx || sdr != sd)
				{
#if OPTIMISE_NGUI_GC_ALLOC
                    if (dc != null && verticesCount != 0)
#else
                    if (dc != null && dc.verts.size != 0)
#endif
                    {
                        drawCalls.Add(dc);
#if OPTIMISE_NGUI_GC_ALLOC
                        FillDrawCallBuffers(dc, verticesCount);
                        verticesCount = 0;
#endif
                        dc.UpdateGeometry();
						dc = null;
					}

					mat = mt;
					tex = tx;
					sdr = sd;
				}

				if (mat != null || sdr != null || tex != null)
				{
					if (dc == null)
					{
						dc = UIDrawCall.Create(this, mat, tex, sdr);
						dc.depthStart = w.depth;
						dc.depthEnd = dc.depthStart;
						dc.panel = this;
					}
					else
					{
						int rd = w.depth;
						if (rd < dc.depthStart) dc.depthStart = rd;
						if (rd > dc.depthEnd) dc.depthEnd = rd;
					}

					w.drawCall = dc;
#if !OPTIMISE_NGUI_GC_ALLOC
                    if (generateNormals) w.WriteToBuffers(dc.verts, dc.uvs, dc.cols, dc.norms, dc.tans);
                    else w.WriteToBuffers(dc.verts, dc.uvs, dc.cols, null, null);
#else
                    widgetsInDrawCall.Add(w);
                    verticesCount += w.geometry.verts.size;
#endif
                }
			}
			else w.drawCall = null;
		}
#if OPTIMISE_NGUI_GC_ALLOC
        if (dc != null && verticesCount != 0)
#else
        if (dc != null && dc.verts.size != 0)
#endif
        {
            drawCalls.Add(dc);
#if OPTIMISE_NGUI_GC_ALLOC
            FillDrawCallBuffers(dc, verticesCount);
#endif
            dc.UpdateGeometry();
		}
	}

	/// <summary>
	/// Fill the geometry for the specified draw call.
	/// </summary>

	bool FillDrawCall (UIDrawCall dc)
	{
		if (dc != null)
		{
			dc.isDirty = false;
#if OPTIMISE_NGUI_GC_ALLOC
            int verticesCount = 0;
#endif
            for (int i = 0; i < widgets.Count; )
			{
				UIWidget w = widgets[i];

				if (w == null)
				{
#if UNITY_EDITOR
					Debug.LogError("This should never happen");
#endif
					widgets.RemoveAt(i);
					continue;
				}

				if (w.drawCall == dc)
				{
					if (w.isVisible && w.hasVertices)
					{
#if !OPTIMISE_NGUI_GC_ALLOC
                        if (generateNormals) w.WriteToBuffers(dc.verts, dc.uvs, dc.cols, dc.norms, dc.tans);
                        else w.WriteToBuffers(dc.verts, dc.uvs, dc.cols, null, null);
#else
                        widgetsInDrawCall.Add(w);
                        verticesCount += w.geometry.verts.size;
#endif
                    }
					else w.drawCall = null;
				}
				++i;
			}
#if !OPTIMISE_NGUI_GC_ALLOC
            if (dc.verts.size != 0)
            {
#else
            if (verticesCount != 0)
            {
                FillDrawCallBuffers(dc, verticesCount);
#endif
                dc.UpdateGeometry();
				return true;
			}
		}
		return false;
	}
}
