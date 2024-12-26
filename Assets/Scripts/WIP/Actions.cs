using System;

namespace InTheDark.Prototypes
{
	public abstract class GameAction : IDisposable
	{
		public virtual void Dispose()
		{

		}

		public abstract void Invoke();
	}

	public class Enter : GameAction
	{
		public int BuildIndex;

		public override void Invoke()
		{
			Game.OnDungeonEnter?.Invoke(new DungeonEnterEvent()
			{
				BuildIndex = BuildIndex
			});
		}
	}

	public class Exit : GameAction
	{
		public int BuildIndex;

		public override void Invoke()
		{
			Game.OnDungeonExit?.Invoke(new DungeonExitEvent()
			{
				BuildIndex = BuildIndex
			});
		}
	}
}