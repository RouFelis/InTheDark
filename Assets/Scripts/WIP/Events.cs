using System;
using Unity.Netcode;

namespace InTheDark.Prototypes
{
	public struct DungeonEnterEvent : INetworkSerializable
	{
		public int BuildIndex;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref BuildIndex);
		}
	}

	public struct DungeonExitEvent : INetworkSerializable
	{
		public int BuildIndex;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref BuildIndex);
		}
	}
}