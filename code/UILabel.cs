public class UILabel : UIWidget
{
#if OPTIMISE_NGUI_GC_ALLOC
    public static List<Vector3> vertsForOnFill = new List<Vector3>();
    public static List<Vector2> uvsForOnFill = new List<Vector2>();
    public static List<Color> colsForOnFill = new List<Color>();
#endif

    public override void OnFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
		if (!isValid) return;

		int offset = verts.Count;
		Color col = color;
		col.a = finalAlpha;
		
		if (mFont != null && mFont.premultipliedAlphaShader) col = NGUITools.ApplyPMA(col);

		string text = processedText;
		int start = verts.Count;

		UpdateNGUIText();

		NGUIText.tint = col;
#if OPTIMISE_NGUI_GC_ALLOC
        vertsForOnFill.Clear();
        uvsForOnFill.Clear();
        colsForOnFill.Clear();
        NGUIText.Print(text, vertsForOnFill, uvsForOnFill, colsForOnFill);
        var diff = effectStyle == Effect.None ? 0 : vertsForOnFill.Count - offset;
        if (effectStyle == Effect.Outline)
        {
            diff += 3 * diff;
        }
        else if (effectStyle == Effect.Outline8)
        {
            diff += 7 * diff;
        }
        diff += vertsForOnFill.Count;
        if (geometry.verts.Capacity < diff)
        {
            CachedGeometries.PullFromCachedGeometries(diff, geometry);
            verts = geometry.verts;
            uvs = geometry.uvs;
            cols = geometry.cols;
        }
        verts.AddRange(vertsForOnFill);
        uvs.AddRange(uvsForOnFill);
        cols.AddRange(colsForOnFill);
#else
        NGUIText.Print(text, verts, uvs, cols);
#endif
        NGUIText.bitmapFont = null;
		NGUIText.dynamicFont = null;

		// Center the content within the label vertically
		Vector2 pos = ApplyOffset(verts, start);

		// Effects don't work with packed fonts
		if (mFont != null && mFont.packedFontShader) return;
		// Apply an effect if one was requested
		if (effectStyle != Effect.None)
		{
			int end = verts.Count;
			pos.x = mEffectDistance.x;
			pos.y = mEffectDistance.y;

			ApplyShadow(verts, uvs, cols, offset, end, pos.x, -pos.y);

			if ((effectStyle == Effect.Outline) || (effectStyle == Effect.Outline8))
			{
				offset = end;
				end = verts.Count;

				ApplyShadow(verts, uvs, cols, offset, end, -pos.x, pos.y);

				offset = end;
				end = verts.Count;

				ApplyShadow(verts, uvs, cols, offset, end, pos.x, pos.y);

				offset = end;
				end = verts.Count;

				ApplyShadow(verts, uvs, cols, offset, end, -pos.x, -pos.y);

				if (effectStyle == Effect.Outline8)
				{
					offset = end;
					end = verts.Count;

					ApplyShadow(verts, uvs, cols, offset, end, -pos.x, 0);

					offset = end;
					end = verts.Count;

					ApplyShadow(verts, uvs, cols, offset, end, pos.x, 0);

					offset = end;
					end = verts.Count;

					ApplyShadow(verts, uvs, cols, offset, end, 0, pos.y);

					offset = end;
					end = verts.Count;

					ApplyShadow(verts, uvs, cols, offset, end, 0, -pos.y);
				}
			}
		}

		if (onPostFill != null)
			onPostFill(this, offset, verts, uvs, cols);
	}
}
