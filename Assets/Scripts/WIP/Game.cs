using System;

namespace InTheDark.Prototypes
{
	// static class 좀 짜치는데
	// 일단 임시
	public static class Game
	{
		public static Action<DungeonEnterEvent> OnDungeonEnter;
		public static Action<DungeonExitEvent> OnDungeonExit;

		static Game()
		{
			OnDungeonEnter = default;
			OnDungeonExit = default;
		}
	}
}
