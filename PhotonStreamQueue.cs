using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000101 RID: 257
public class PhotonStreamQueue
{
	// Token: 0x060006AF RID: 1711 RVA: 0x0003B7CB File Offset: 0x00039BCB
	public PhotonStreamQueue(int sampleRate)
	{
		this.m_SampleRate = sampleRate;
	}

	// Token: 0x060006B0 RID: 1712 RVA: 0x0003B808 File Offset: 0x00039C08
	private void BeginWritePackage()
	{
		if (Time.realtimeSinceStartup < this.m_LastSampleTime + 1f / (float)this.m_SampleRate)
		{
			this.m_IsWriting = false;
			return;
		}
		if (this.m_SampleCount == 1)
		{
			this.m_ObjectsPerSample = this.m_Objects.Count;
		}
		else if (this.m_SampleCount > 1 && this.m_Objects.Count / this.m_SampleCount != this.m_ObjectsPerSample)
		{
			Debug.LogWarning("The number of objects sent via a PhotonStreamQueue has to be the same each frame");
			Debug.LogWarning(string.Concat(new object[]
			{
				"Objects in List: ",
				this.m_Objects.Count,
				" / Sample Count: ",
				this.m_SampleCount,
				" = ",
				this.m_Objects.Count / this.m_SampleCount,
				" != ",
				this.m_ObjectsPerSample
			}));
		}
		this.m_IsWriting = true;
		this.m_SampleCount++;
		this.m_LastSampleTime = Time.realtimeSinceStartup;
	}

	// Token: 0x060006B1 RID: 1713 RVA: 0x0003B929 File Offset: 0x00039D29
	public void Reset()
	{
		this.m_SampleCount = 0;
		this.m_ObjectsPerSample = -1;
		this.m_LastSampleTime = float.NegativeInfinity;
		this.m_LastFrameCount = -1;
		this.m_Objects.Clear();
	}

	// Token: 0x060006B2 RID: 1714 RVA: 0x0003B956 File Offset: 0x00039D56
	public void SendNext(object obj)
	{
		if (Time.frameCount != this.m_LastFrameCount)
		{
			this.BeginWritePackage();
		}
		this.m_LastFrameCount = Time.frameCount;
		if (!this.m_IsWriting)
		{
			return;
		}
		this.m_Objects.Add(obj);
	}

	// Token: 0x060006B3 RID: 1715 RVA: 0x0003B991 File Offset: 0x00039D91
	public bool HasQueuedObjects()
	{
		return this.m_NextObjectIndex != -1;
	}

	// Token: 0x060006B4 RID: 1716 RVA: 0x0003B9A0 File Offset: 0x00039DA0
	public object ReceiveNext()
	{
		if (this.m_NextObjectIndex == -1)
		{
			return null;
		}
		if (this.m_NextObjectIndex >= this.m_Objects.Count)
		{
			this.m_NextObjectIndex -= this.m_ObjectsPerSample;
		}
		return this.m_Objects[this.m_NextObjectIndex++];
	}

	// Token: 0x060006B5 RID: 1717 RVA: 0x0003BA00 File Offset: 0x00039E00
	public void Serialize(PhotonStream stream)
	{
		if (this.m_Objects.Count > 0 && this.m_ObjectsPerSample < 0)
		{
			this.m_ObjectsPerSample = this.m_Objects.Count;
		}
		stream.SendNext(this.m_SampleCount);
		stream.SendNext(this.m_ObjectsPerSample);
		for (int i = 0; i < this.m_Objects.Count; i++)
		{
			stream.SendNext(this.m_Objects[i]);
		}
		this.m_Objects.Clear();
		this.m_SampleCount = 0;
	}

	// Token: 0x060006B6 RID: 1718 RVA: 0x0003BAA0 File Offset: 0x00039EA0
	public void Deserialize(PhotonStream stream)
	{
		this.m_Objects.Clear();
		this.m_SampleCount = (int)stream.ReceiveNext();
		this.m_ObjectsPerSample = (int)stream.ReceiveNext();
		for (int i = 0; i < this.m_SampleCount * this.m_ObjectsPerSample; i++)
		{
			this.m_Objects.Add(stream.ReceiveNext());
		}
		if (this.m_Objects.Count > 0)
		{
			this.m_NextObjectIndex = 0;
		}
		else
		{
			this.m_NextObjectIndex = -1;
		}
	}

	// Token: 0x04000896 RID: 2198
	private int m_SampleRate;

	// Token: 0x04000897 RID: 2199
	private int m_SampleCount;

	// Token: 0x04000898 RID: 2200
	private int m_ObjectsPerSample = -1;

	// Token: 0x04000899 RID: 2201
	private float m_LastSampleTime = float.NegativeInfinity;

	// Token: 0x0400089A RID: 2202
	private int m_LastFrameCount = -1;

	// Token: 0x0400089B RID: 2203
	private int m_NextObjectIndex = -1;

	// Token: 0x0400089C RID: 2204
	private List<object> m_Objects = new List<object>();

	// Token: 0x0400089D RID: 2205
	private bool m_IsWriting;
}
