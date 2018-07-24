public abstract class UIBasicSprite : UIWidget
{
	void SimpleFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
		Vector4 v = drawingDimensions;
		Vector4 u = drawingUVs;
		Color gc = drawingColor;
#if OPTIMISE_NGUI_GC_ALLOC
        if(geometry.verts.Capacity < 4)
        {
            CachedGeometries.PullFromCachedGeometries(4, geometry);
            verts = geometry.verts;
            uvs = geometry.uvs;
            cols = geometry.cols;
        }
#endif
		verts.Add(new Vector3(v.x, v.y));
		verts.Add(new Vector3(v.x, v.w));
		verts.Add(new Vector3(v.z, v.w));
		verts.Add(new Vector3(v.z, v.y));

		uvs.Add(new Vector2(u.x, u.y));
		uvs.Add(new Vector2(u.x, u.w));
		uvs.Add(new Vector2(u.z, u.w));
		uvs.Add(new Vector2(u.z, u.y));

		if (!mApplyGradient)
		{
			cols.Add(gc);
			cols.Add(gc);
			cols.Add(gc);
			cols.Add(gc);
		}
		else
		{
			AddVertexColours(cols, ref gc, 1, 1);
			AddVertexColours(cols, ref gc, 1, 2);
			AddVertexColours(cols, ref gc, 2, 2);
			AddVertexColours(cols, ref gc, 2, 1);
		}
	}

	void SlicedFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
		Vector4 br = border * pixelSize;
		
		if (br.x == 0f && br.y == 0f && br.z == 0f && br.w == 0f)
		{
			SimpleFill(verts, uvs, cols);
			return;
		}

		Color gc = drawingColor;
		Vector4 v = drawingDimensions;

		mTempPos[0].x = v.x;
		mTempPos[0].y = v.y;
		mTempPos[3].x = v.z;
		mTempPos[3].y = v.w;

		if (mFlip == Flip.Horizontally || mFlip == Flip.Both)
		{
			mTempPos[1].x = mTempPos[0].x + br.z;
			mTempPos[2].x = mTempPos[3].x - br.x;

			mTempUVs[3].x = mOuterUV.xMin;
			mTempUVs[2].x = mInnerUV.xMin;
			mTempUVs[1].x = mInnerUV.xMax;
			mTempUVs[0].x = mOuterUV.xMax;
		}
		else
		{
			mTempPos[1].x = mTempPos[0].x + br.x;
			mTempPos[2].x = mTempPos[3].x - br.z;

			mTempUVs[0].x = mOuterUV.xMin;
			mTempUVs[1].x = mInnerUV.xMin;
			mTempUVs[2].x = mInnerUV.xMax;
			mTempUVs[3].x = mOuterUV.xMax;
		}

		if (mFlip == Flip.Vertically || mFlip == Flip.Both)
		{
			mTempPos[1].y = mTempPos[0].y + br.w;
			mTempPos[2].y = mTempPos[3].y - br.y;

			mTempUVs[3].y = mOuterUV.yMin;
			mTempUVs[2].y = mInnerUV.yMin;
			mTempUVs[1].y = mInnerUV.yMax;
			mTempUVs[0].y = mOuterUV.yMax;
		}
		else
		{
			mTempPos[1].y = mTempPos[0].y + br.y;
			mTempPos[2].y = mTempPos[3].y - br.w;

			mTempUVs[0].y = mOuterUV.yMin;
			mTempUVs[1].y = mInnerUV.yMin;
			mTempUVs[2].y = mInnerUV.yMax;
			mTempUVs[3].y = mOuterUV.yMax;
		}

#if OPTIMISE_NGUI_GC_ALLOC
        var newSize = 0;
        for (int x = 0; x < 3; ++x)
        {
            for (int y = 0; y < 3; ++y)
            {
                if (centerType == AdvancedType.Invisible && x == 1 && y == 1) continue;
                newSize += 4;
            }
        }
        if(geometry.verts.Capacity < newSize)
        {
            CachedGeometries.PullFromCachedGeometries(newSize, geometry);
            verts = geometry.verts;
            uvs = geometry.uvs;
            cols = geometry.cols;
        }
#endif

		for (int x = 0; x < 3; ++x)
		{
			int x2 = x + 1;

			for (int y = 0; y < 3; ++y)
			{
				if (centerType == AdvancedType.Invisible && x == 1 && y == 1) continue;

				int y2 = y + 1;

				verts.Add(new Vector3(mTempPos[x].x, mTempPos[y].y));
				verts.Add(new Vector3(mTempPos[x].x, mTempPos[y2].y));
				verts.Add(new Vector3(mTempPos[x2].x, mTempPos[y2].y));
				verts.Add(new Vector3(mTempPos[x2].x, mTempPos[y].y));

				uvs.Add(new Vector2(mTempUVs[x].x, mTempUVs[y].y));
				uvs.Add(new Vector2(mTempUVs[x].x, mTempUVs[y2].y));
				uvs.Add(new Vector2(mTempUVs[x2].x, mTempUVs[y2].y));
				uvs.Add(new Vector2(mTempUVs[x2].x, mTempUVs[y].y));

				if (!mApplyGradient)
				{
					cols.Add(gc);
					cols.Add(gc);
					cols.Add(gc);
					cols.Add(gc);
				}
				else
				{
					AddVertexColours(cols, ref gc, x, y);
					AddVertexColours(cols, ref gc, x, y2);
					AddVertexColours(cols, ref gc, x2, y2);
					AddVertexColours(cols, ref gc, x2, y);
				}
			}
		}
	}

	void TiledFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
		Texture tex = mainTexture;
		if (tex == null) return;

		Vector2 size = new Vector2(mInnerUV.width * tex.width, mInnerUV.height * tex.height);
		size *= pixelSize;
		if (tex == null || size.x < 2f || size.y < 2f) return;

		Color c = drawingColor;
		Vector4 v = drawingDimensions;
		Vector4 u;

		if (mFlip == Flip.Horizontally || mFlip == Flip.Both)
		{
			u.x = mInnerUV.xMax;
			u.z = mInnerUV.xMin;
		}
		else
		{
			u.x = mInnerUV.xMin;
			u.z = mInnerUV.xMax;
		}

		if (mFlip == Flip.Vertically || mFlip == Flip.Both)
		{
			u.y = mInnerUV.yMax;
			u.w = mInnerUV.yMin;
		}
		else
		{
			u.y = mInnerUV.yMin;
			u.w = mInnerUV.yMax;
		}

		float x0 = v.x;
		float y0 = v.y;

		float u0 = u.x;
		float v0 = u.y;

#if OPTIMISE_NGUI_GC_ALLOC
        var newSize = 0;
        while (y0 < v.w)
        {
            x0 = v.x;
            float y1 = y0 + size.y;

            if (y1 > v.w)
            {
                y1 = v.w;
            }

            while (x0 < v.z)
            {
                float x1 = x0 + size.x;

                if (x1 > v.z)
                {
                    x1 = v.z;
                }
                newSize += 4;
                x0 += size.x;
            }
            y0 += size.y;
        }
        if (geometry.verts.Capacity < newSize)
        {
            CachedGeometries.PullFromCachedGeometries(newSize, geometry);
            verts = geometry.verts;
            uvs = geometry.uvs;
            cols = geometry.cols;
        }

        x0 = v.x;
        y0 = v.y;
#endif

        while (y0 < v.w)
		{
			x0 = v.x;
			float y1 = y0 + size.y;
			float v1 = u.w;

			if (y1 > v.w)
			{
				v1 = Mathf.Lerp(u.y, u.w, (v.w - y0) / size.y);
				y1 = v.w;
			}

			while (x0 < v.z)
			{
				float x1 = x0 + size.x;
				float u1 = u.z;

				if (x1 > v.z)
				{
					u1 = Mathf.Lerp(u.x, u.z, (v.z - x0) / size.x);
					x1 = v.z;
				}

				verts.Add(new Vector3(x0, y0));
				verts.Add(new Vector3(x0, y1));
				verts.Add(new Vector3(x1, y1));
				verts.Add(new Vector3(x1, y0));

				uvs.Add(new Vector2(u0, v0));
				uvs.Add(new Vector2(u0, v1));
				uvs.Add(new Vector2(u1, v1));
				uvs.Add(new Vector2(u1, v0));

				cols.Add(c);
				cols.Add(c);
				cols.Add(c);
				cols.Add(c);

				x0 += size.x;
			}
			y0 += size.y;
		}
	}
}
