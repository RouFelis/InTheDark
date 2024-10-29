using System;

namespace InTheDark.Prototypes
{
	public abstract class Action : IDisposable
	{
		public virtual void Dispose()
		{

		}

		public abstract void Invoke();
	}

	public class Enter : Action
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

	public class Exit : Action
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