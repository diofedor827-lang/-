using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200009F RID: 159
public class PickupDemoGui : MonoBehaviour
{
	// Token: 0x06000385 RID: 901 RVA: 0x0002A4D0 File Offset: 0x000288D0
	public void OnGUI()
	{
		if (!PhotonNetwork.inRoom)
		{
			return;
		}
		if (this.ShowScores)
		{
			GUILayout.Label("Your Score: " + PhotonNetwork.player.GetScore(), new GUILayoutOption[0]);
		}
		if (this.ShowDropButton)
		{
			foreach (PickupItem pickupItem in PickupItem.DisabledPickupItems)
			{
				if (pickupItem.PickupIsMine && pickupItem.SecondsBeforeRespawn <= 0f)
				{
					if (GUILayout.Button("Drop " + pickupItem.name, new GUILayoutOption[0]))
					{
						pickupItem.Drop();
					}
					GameObject gameObject = PhotonNetwork.player.TagObject as GameObject;
					if (gameObject != null && GUILayout.Button("Drop here " + pickupItem.name, new GUILayoutOption[0]))
					{
						Vector3 a = UnityEngine.Random.insideUnitSphere;
						a.y = 0f;
						a = a.normalized;
						Vector3 newPosition = gameObject.transform.position + this.DropOffset * a;
						pickupItem.Drop(newPosition);
					}
				}
			}
		}
		if (this.ShowTeams)
		{
			foreach (PunTeams.Team key in PunTeams.PlayersPerTeam.Keys)
			{
				GUILayout.Label("Team: " + key.ToString(), new GUILayoutOption[0]);
				List<PhotonPlayer> list = PunTeams.PlayersPerTeam[key];
				foreach (PhotonPlayer photonPlayer in list)
				{
					GUILayout.Label(string.Concat(new object[]
					{
						"  ",
						photonPlayer.ToStringFull(),
						" Score: ",
						photonPlayer.GetScore()
					}), new GUILayoutOption[0]);
				}
			}
			if (GUILayout.Button("to red", new GUILayoutOption[0]))
			{
				PhotonNetwork.player.SetTeam(PunTeams.Team.red);
			}
			if (GUILayout.Button("to blue", new GUILayoutOption[0]))
			{
				PhotonNetwork.player.SetTeam(PunTeams.Team.blue);
			}
		}
	}

	// Token: 0x04000583 RID: 1411
	public bool ShowScores;

	// Token: 0x04000584 RID: 1412
	public bool ShowDropButton;

	// Token: 0x04000585 RID: 1413
	public bool ShowTeams;

	// Token: 0x04000586 RID: 1414
	public float DropOffset = 0.5f;
}
