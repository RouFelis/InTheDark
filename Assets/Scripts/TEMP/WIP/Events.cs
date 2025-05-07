using System;
using Unity.Netcode;

namespace InTheDark.Prototypes
{
	public struct RelayEvent : INetworkSerializable
	{
		public string JoinCode;

		public static implicit operator RelayEvent(RelayHostData data)
		{
			var instance = new RelayEvent()
			{
				JoinCode = data.JoinCode
			};

			return instance;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref JoinCode);
		}
	}

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