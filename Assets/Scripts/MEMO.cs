namespace InTheDark.LoremIpsum
{
	/// <summary>
	/// �޸����� ����
	/// </summary>
	public static class MEMO
	{
		// ���� �̹� ��ġ���ִ� NetworkObject���� �ڵ����� Spawn�Ǵ� ��
		// �ƴϸ� �̷��� �����صаǰ�
		// �⺻ ����̾��׿�~

		// SceneEvent sceneEvent => onSceneEvent �븮�� ����� ���ؼ� �ʿ�

		// �ƴ� �� ���� �þߴ� �� ���� �Ŵ�

		// �ڵ尡 ��ü������ �������� �����ؾ�¡

		// �ƴ� �� �޸�!!!!!

		// ��� ���� ��ü���� ����ȭ ���� ��ü�� �޾ƿ��� ��ü�� �Ҵ������..?

		// Subscriber

		// Instruction

		// Perception
		// Context

		// IsHost, IsClient, IsServer, IsOwner �� true ���� ��������������������������������������

		// logistics

		// IUpStream<TSource>
		// IDownStream<TSource>
		// IStreamHandler

		// ServerRpc() == Rpc(SendTo.Server) == ��������!
		// ClientRpc() == Rpc(SendTo.NotServer) == �ٸ� ��������!
		// �ٵ� ���� SendTo.Everyone ���� �Ǵ� ���� �� ���� ��?
		// �ƴѰ�

		// �ð��� ��ȯ (O)
		// AI �� �� ��� ����

		// AI ���� �� �Ѿ�� ����
		// Player ���� �ϸ� ����

		// ���� ��ġ ���� (�÷��̾� ��ó X -> �þ� �� ����)
		// ���� ��� ��ü ��� �����ϱ� (Ƽ��)

		// �׳� ������ ���� �ȵ� ���¿��� ��ġ ��Ƽ� �׷��ſ���...
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