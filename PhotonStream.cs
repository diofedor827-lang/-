using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000F8 RID: 248
public class PhotonStream
{
	// Token: 0x060005B3 RID: 1459 RVA: 0x00037AE4 File Offset: 0x00035EE4
	public PhotonStream(bool write, object[] incomingData)
	{
		this.write = write;
		if (incomingData == null)
		{
			this.writeData = new Queue<object>(10);
		}
		else
		{
			this.readData = incomingData;
		}
	}

	// Token: 0x060005B4 RID: 1460 RVA: 0x00037B12 File Offset: 0x00035F12
	public void SetReadStream(object[] incomingData, byte pos = 0)
	{
		this.readData = incomingData;
		this.currentItem = pos;
		this.write = false;
	}

	// Token: 0x060005B5 RID: 1461 RVA: 0x00037B29 File Offset: 0x00035F29
	internal void ResetWriteStream()
	{
		this.writeData.Clear();
	}

	// Token: 0x17000054 RID: 84
	// (get) Token: 0x060005B6 RID: 1462 RVA: 0x00037B36 File Offset: 0x00035F36
	public bool isWriting
	{
		get
		{
			return this.write;
		}
	}

	// Token: 0x17000055 RID: 85
	// (get) Token: 0x060005B7 RID: 1463 RVA: 0x00037B3E File Offset: 0x00035F3E
	public bool isReading
	{
		get
		{
			return !this.write;
		}
	}

	// Token: 0x17000056 RID: 86
	// (get) Token: 0x060005B8 RID: 1464 RVA: 0x00037B49 File Offset: 0x00035F49
	public int Count
	{
		get
		{
			return (!this.isWriting) ? this.readData.Length : this.writeData.Count;
		}
	}

	// Token: 0x060005B9 RID: 1465 RVA: 0x00037B70 File Offset: 0x00035F70
	public object ReceiveNext()
	{
		if (this.write)
		{
			Debug.LogError("Error: you cannot read this stream that you are writing!");
			return null;
		}
		object result = this.readData[(int)this.currentItem];
		this.currentItem += 1;
		return result;
	}

	// Token: 0x060005BA RID: 1466 RVA: 0x00037BB4 File Offset: 0x00035FB4
	public object PeekNext()
	{
		if (this.write)
		{
			Debug.LogError("Error: you cannot read this stream that you are writing!");
			return null;
		}
		return this.readData[(int)this.currentItem];
	}

	// Token: 0x060005BB RID: 1467 RVA: 0x00037BE7 File Offset: 0x00035FE7
	public void SendNext(object obj)
	{
		if (!this.write)
		{
			Debug.LogError("Error: you cannot write/send to this stream that you are reading!");
			return;
		}
		this.writeData.Enqueue(obj);
	}

	// Token: 0x060005BC RID: 1468 RVA: 0x00037C0B File Offset: 0x0003600B
	public object[] ToArray()
	{
		return (!this.isWriting) ? this.readData : this.writeData.ToArray();
	}

	// Token: 0x060005BD RID: 1469 RVA: 0x00037C30 File Offset: 0x00036030
	public void Serialize(ref bool myBool)
	{
		if (this.write)
		{
			this.writeData.Enqueue(myBool);
		}
		else if (this.readData.Length > (int)this.currentItem)
		{
			myBool = (bool)this.readData[(int)this.currentItem];
			this.currentItem += 1;
		}
	}

	// Token: 0x060005BE RID: 1470 RVA: 0x00037C98 File Offset: 0x00036098
	public void Serialize(ref int myInt)
	{
		if (this.write)
		{
			this.writeData.Enqueue(myInt);
		}
		else if (this.readData.Length > (int)this.currentItem)
		{
			myInt = (int)this.readData[(int)this.currentItem];
			this.currentItem += 1;
		}
	}

	// Token: 0x060005BF RID: 1471 RVA: 0x00037D00 File Offset: 0x00036100
	public void Serialize(ref string value)
	{
		if (this.write)
		{
			this.writeData.Enqueue(value);
		}
		else if (this.readData.Length > (int)this.currentItem)
		{
			value = (string)this.readData[(int)this.currentItem];
			this.currentItem += 1;
		}
	}

	// Token: 0x060005C0 RID: 1472 RVA: 0x00037D60 File Offset: 0x00036160
	public void Serialize(ref char value)
	{
		if (this.write)
		{
			this.writeData.Enqueue(value);
		}
		else if (this.readData.Length > (int)this.currentItem)
		{
			value = (char)this.readData[(int)this.currentItem];
			this.currentItem += 1;
		}
	}

	// Token: 0x060005C1 RID: 1473 RVA: 0x00037DC8 File Offset: 0x000361C8
	public void Serialize(ref short value)
	{
		if (this.write)
		{
			this.writeData.Enqueue(value);
		}
		else if (this.readData.Length > (int)this.currentItem)
		{
			value = (short)this.readData[(int)this.currentItem];
			this.currentItem += 1;
		}
	}

	// Token: 0x060005C2 RID: 1474 RVA: 0x00037E30 File Offset: 0x00036230
	public void Serialize(ref float obj)
	{
		if (this.write)
		{
			this.writeData.Enqueue(obj);
		}
		else if (this.readData.Length > (int)this.currentItem)
		{
			obj = (float)this.readData[(int)this.currentItem];
			this.currentItem += 1;
		}
	}

	// Token: 0x060005C3 RID: 1475 RVA: 0x00037E98 File Offset: 0x00036298
	public void Serialize(ref PhotonPlayer obj)
	{
		if (this.write)
		{
			this.writeData.Enqueue(obj);
		}
		else if (this.readData.Length > (int)this.currentItem)
		{
			obj = (PhotonPlayer)this.readData[(int)this.currentItem];
			this.currentItem += 1;
		}
	}

	// Token: 0x060005C4 RID: 1476 RVA: 0x00037EF8 File Offset: 0x000362F8
	public void Serialize(ref Vector3 obj)
	{
		if (this.write)
		{
			this.writeData.Enqueue(obj);
		}
		else if (this.readData.Length > (int)this.currentItem)
		{
			obj = (Vector3)this.readData[(int)this.currentItem];
			this.currentItem += 1;
		}
	}

	// Token: 0x060005C5 RID: 1477 RVA: 0x00037F68 File Offset: 0x00036368
	public void Serialize(ref Vector2 obj)
	{
		if (this.write)
		{
			this.writeData.Enqueue(obj);
		}
		else if (this.readData.Length > (int)this.currentItem)
		{
			obj = (Vector2)this.readData[(int)this.currentItem];
			this.currentItem += 1;
		}
	}

	// Token: 0x060005C6 RID: 1478 RVA: 0x00037FD8 File Offset: 0x000363D8
	public void Serialize(ref Quaternion obj)
	{
		if (this.write)
		{
			this.writeData.Enqueue(obj);
		}
		else if (this.readData.Length > (int)this.currentItem)
		{
			obj = (Quaternion)this.readData[(int)this.currentItem];
			this.currentItem += 1;
		}
	}

	// Token: 0x0400084D RID: 2125
	private bool write;

	// Token: 0x0400084E RID: 2126
	private Queue<object> writeData;

	// Token: 0x0400084F RID: 2127
	private object[] readData;

	// Token: 0x04000850 RID: 2128
	internal byte currentItem;
}
