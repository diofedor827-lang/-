using System;

// Token: 0x020000DD RID: 221
public class ParameterCode
{
	// Token: 0x0400072A RID: 1834
	public const byte SuppressRoomEvents = 237;

	// Token: 0x0400072B RID: 1835
	public const byte EmptyRoomTTL = 236;

	// Token: 0x0400072C RID: 1836
	public const byte PlayerTTL = 235;

	// Token: 0x0400072D RID: 1837
	public const byte EventForward = 234;

	// Token: 0x0400072E RID: 1838
	[Obsolete("Use: IsInactive")]
	public const byte IsComingBack = 233;

	// Token: 0x0400072F RID: 1839
	public const byte IsInactive = 233;

	// Token: 0x04000730 RID: 1840
	public const byte CheckUserOnJoin = 232;

	// Token: 0x04000731 RID: 1841
	public const byte ExpectedValues = 231;

	// Token: 0x04000732 RID: 1842
	public const byte Address = 230;

	// Token: 0x04000733 RID: 1843
	public const byte PeerCount = 229;

	// Token: 0x04000734 RID: 1844
	public const byte GameCount = 228;

	// Token: 0x04000735 RID: 1845
	public const byte MasterPeerCount = 227;

	// Token: 0x04000736 RID: 1846
	public const byte UserId = 225;

	// Token: 0x04000737 RID: 1847
	public const byte ApplicationId = 224;

	// Token: 0x04000738 RID: 1848
	public const byte Position = 223;

	// Token: 0x04000739 RID: 1849
	public const byte MatchMakingType = 223;

	// Token: 0x0400073A RID: 1850
	public const byte GameList = 222;

	// Token: 0x0400073B RID: 1851
	public const byte Secret = 221;

	// Token: 0x0400073C RID: 1852
	public const byte AppVersion = 220;

	// Token: 0x0400073D RID: 1853
	[Obsolete("TCP routing was removed after becoming obsolete.")]
	public const byte AzureNodeInfo = 210;

	// Token: 0x0400073E RID: 1854
	[Obsolete("TCP routing was removed after becoming obsolete.")]
	public const byte AzureLocalNodeId = 209;

	// Token: 0x0400073F RID: 1855
	[Obsolete("TCP routing was removed after becoming obsolete.")]
	public const byte AzureMasterNodeId = 208;

	// Token: 0x04000740 RID: 1856
	public const byte RoomName = 255;

	// Token: 0x04000741 RID: 1857
	public const byte Broadcast = 250;

	// Token: 0x04000742 RID: 1858
	public const byte ActorList = 252;

	// Token: 0x04000743 RID: 1859
	public const byte ActorNr = 254;

	// Token: 0x04000744 RID: 1860
	public const byte PlayerProperties = 249;

	// Token: 0x04000745 RID: 1861
	public const byte CustomEventContent = 245;

	// Token: 0x04000746 RID: 1862
	public const byte Data = 245;

	// Token: 0x04000747 RID: 1863
	public const byte Code = 244;

	// Token: 0x04000748 RID: 1864
	public const byte GameProperties = 248;

	// Token: 0x04000749 RID: 1865
	public const byte Properties = 251;

	// Token: 0x0400074A RID: 1866
	public const byte TargetActorNr = 253;

	// Token: 0x0400074B RID: 1867
	public const byte ReceiverGroup = 246;

	// Token: 0x0400074C RID: 1868
	public const byte Cache = 247;

	// Token: 0x0400074D RID: 1869
	public const byte CleanupCacheOnLeave = 241;

	// Token: 0x0400074E RID: 1870
	public const byte Group = 240;

	// Token: 0x0400074F RID: 1871
	public const byte Remove = 239;

	// Token: 0x04000750 RID: 1872
	public const byte PublishUserId = 239;

	// Token: 0x04000751 RID: 1873
	public const byte Add = 238;

	// Token: 0x04000752 RID: 1874
	public const byte Info = 218;

	// Token: 0x04000753 RID: 1875
	public const byte ClientAuthenticationType = 217;

	// Token: 0x04000754 RID: 1876
	public const byte ClientAuthenticationParams = 216;

	// Token: 0x04000755 RID: 1877
	public const byte JoinMode = 215;

	// Token: 0x04000756 RID: 1878
	public const byte ClientAuthenticationData = 214;

	// Token: 0x04000757 RID: 1879
	public const byte MasterClientId = 203;

	// Token: 0x04000758 RID: 1880
	public const byte FindFriendsRequestList = 1;

	// Token: 0x04000759 RID: 1881
	public const byte FindFriendsResponseOnlineList = 1;

	// Token: 0x0400075A RID: 1882
	public const byte FindFriendsResponseRoomIdList = 2;

	// Token: 0x0400075B RID: 1883
	public const byte LobbyName = 213;

	// Token: 0x0400075C RID: 1884
	public const byte LobbyType = 212;

	// Token: 0x0400075D RID: 1885
	public const byte LobbyStats = 211;

	// Token: 0x0400075E RID: 1886
	public const byte Region = 210;

	// Token: 0x0400075F RID: 1887
	public const byte UriPath = 209;

	// Token: 0x04000760 RID: 1888
	public const byte WebRpcParameters = 208;

	// Token: 0x04000761 RID: 1889
	public const byte WebRpcReturnCode = 207;

	// Token: 0x04000762 RID: 1890
	public const byte WebRpcReturnMessage = 206;

	// Token: 0x04000763 RID: 1891
	public const byte CacheSliceIndex = 205;

	// Token: 0x04000764 RID: 1892
	public const byte Plugins = 204;

	// Token: 0x04000765 RID: 1893
	public const byte NickName = 202;

	// Token: 0x04000766 RID: 1894
	public const byte PluginName = 201;

	// Token: 0x04000767 RID: 1895
	public const byte PluginVersion = 200;

	// Token: 0x04000768 RID: 1896
	public const byte ExpectedProtocol = 195;

	// Token: 0x04000769 RID: 1897
	public const byte CustomInitData = 194;

	// Token: 0x0400076A RID: 1898
	public const byte EncryptionMode = 193;

	// Token: 0x0400076B RID: 1899
	public const byte EncryptionData = 192;

	// Token: 0x0400076C RID: 1900
	public const byte RoomOptionFlags = 191;
}
