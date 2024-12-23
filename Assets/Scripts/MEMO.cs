namespace InTheDark.LoremIpsum
{
	/// <summary>
	/// 메모장임 ㅇㅇ
	/// </summary>
	public static class MEMO
	{
		// 씬에 이미 배치되있던 NetworkObject들은 자동으로 Spawn되는 듯
		// 아니면 이렇게 세팅해둔건가

		// SceneEvent sceneEvent => onSceneEvent 대리자 사용을 위해서 필요

		// 아니 왜 위쪽 시야는 못 보는 거니

		// 코드가 전체적으로 개떡같다 수정해야징

		// 아니 내 메모가!!!!!

		// 사용 예정 개체마다 직렬화 직후 개체를 받아오는 개체를 할당해줘야..?

		// Subscriber

		// OLD
		// 소리 듣기 구현 => 해결, 근데 수정 필요. 밑 참조
		// 몬스터 별 패턴 다양화 => 가능
		// 몬스터 생성 태그 설정 => 아직 불가능

		// NEW
		// 이벤트리스너 할당 문제

		// Decorate

		// Instruction

		// Perception
		// Context

		// A의 상속성 대해

		// 1.사운드가 들리면 방향성에 따라서 이동하도록 되면하고 안되면 빠르게 포기 (중요) => 가능...은 한데...

		// 2.플레이어 발견 시 바로 붙는게 아니라 약간 벽 뒤에 숨어서 플레이어를 관찰하는 느낌으로 행동패턴 한번 가능한지? => 엄...
		// 벽 뒤로 숨을 수 있는가? => 엄...
		// 플레이어 발견 시 ai의 행동을 변경할 수 있는가? => 엄...

		// 3.날아다니는거 되는지? => X
		// 바닥을 파인 바닥 만들어서
	}
}

//using System;

//namespace InTheDark.LoremIpsum
//{
//	/// <summary>
//	/// 
//	/// </summary>
//	public static class EventHandle
//	{
//		/// <summary>
//		/// 
//		/// </summary>
//		/// <typeparam name="TSource"></typeparam>
//		/// <param name="observable"></param>
//		/// <param name="onNext"></param>
//		/// <returns></returns>
//		public static IDisposable Subscribe<TSource>(this IObservable<TSource> emitter, Action<TSource> onNext)
//		{
//			return default;
//		}

//		/// <summary>
//		/// 
//		/// </summary>
//		/// <typeparam name="TSource"></typeparam>
//		/// <typeparam name="TResult"></typeparam>
//		/// <param name="observable"></param>
//		/// <param name="predicate"></param>
//		/// <returns></returns>
//		public static IObservable<TResult> Select<TSource, TResult>(this IObservable<TSource> emitter, Func<TSource, TResult> predicate)
//		{
//			return default;
//		}

//		/// <summary>
//		/// 
//		/// </summary>
//		/// <typeparam name="TSource"></typeparam>
//		/// <param name="observable"></param>
//		/// <param name="predicate"></param>
//		/// <returns></returns>
//		public static IObservable<TSource> Where<TSource>(this IObservable<TSource> emitter, Func<TSource, bool> predicate)
//		{
//			return default;
//		}
//	}
//}