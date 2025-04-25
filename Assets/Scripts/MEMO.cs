using InTheDark.Prototypes;

namespace InTheDark.LoremIpsum
{
	/// <summary>
	/// 메모장임 ㅇㅇ
	/// </summary>
	public static class MEMO
	{
		// 씬에 이미 배치되있던 NetworkObject들은 자동으로 Spawn되는 듯
		// 아니면 이렇게 세팅해둔건가
		// 기본 사양이었네요~

		// SceneEvent sceneEvent => onSceneEvent 대리자 사용을 위해서 필요

		// 아니 왜 위쪽 시야는 못 보는 거니

		// 코드가 전체적으로 개떡같다 수정해야징

		// 아니 내 메모가!!!!!

		// 사용 예정 개체마다 직렬화 직후 개체를 받아오는 개체를 할당해줘야..?

		// Subscriber

		// Instruction

		// Perception
		// Context

		// IsHost, IsClient, IsServer, IsOwner 다 true 들어옴 ㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋ
		// 그 이외에는 IsClient만!

		// logistics

		// IUpStream<TSource>
		// IDownStream<TSource>
		// IStreamHandler

		// RIS

		// ServerRpc() == Rpc(SendTo.Server) == 서버에게!
		// ClientRpc() == Rpc(SendTo.NotServer) == 다른 유저에게!
		// 근데 보통 SendTo.Everyone 쓰게 되는 일이 더 많은 듯?
		// 아닌가

		// 시간차 소환 (O)
		// AI 모델 및 모션 적용

		// AI 몬스터 방 넘어가게 변경
		// Player 공격 하면 감지
		// 몬스터 사망 모션 추가
		// 몬스터 피격 효과음 추가

		// 슬슬 비즈니스 로직이랑 게임 오브젝트용 로직을 분리해야 함

		// 소한 위치 조정 (플레이어 근처 X -> 시야 밖 생성)
		// 상점 기능 대체 방법 생각하기 (티비)

		// 공격 방식: 지속 피해 -> 단발 피해
		// LightManager 제거하기

		// 그냥 던전이 생성 안된 상태에서 위치 잡아서 그런거였네...

		// 몬스터 공격 모션 진행 중 플레이어가 (죽거나) 범위 밖으로 이탈하거나 몬스터 사망 시 데미지 안 들어가게 수정
		// 플레이어 사망 시 대상 해제

		// 몬스터 돌진 후 벽 충돌 시 튕기기 (이거 외 안되)

		// 공격 구현해오기
		// 1. 레이저 (고정 포대 && 몬스터 장착)
		// 2. 박치기 (후 플레이어 밀치기)
		// 3. 당기기 (????)

		// 몬스터 클라 복사버그 -> 이거 어떻게 해결하긴 해야 하는데
		// 몬스터 transform 동기화

		// 몬스터 스킬 재사용 가능
		// 몬스터 스킬 애니메이션커브 적용?

		// 아 몬스터 소환 방식 조금 변경해야 하는데
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