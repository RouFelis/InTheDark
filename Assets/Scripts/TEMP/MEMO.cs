using InTheDark.Prototypes;

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
		// �� �̿ܿ��� IsClient��!

		// logistics

		// IUpStream<TSource>
		// IDownStream<TSource>
		// IStreamHandler

		// RIS

		// ServerRpc() == Rpc(SendTo.Server) == ��������!
		// ClientRpc() == Rpc(SendTo.NotServer) == �ٸ� ��������!
		// �ٵ� ���� SendTo.Everyone ���� �Ǵ� ���� �� ���� ��?
		// �ƴѰ�

		// �ð��� ��ȯ (O)
		// AI �� �� ��� ����

		// AI ���� �� �Ѿ�� ����
		// Player ���� �ϸ� ����
		// ���� ��� ��� �߰�
		// ���� �ǰ� ȿ���� �߰�

		// ���� ����Ͻ� �����̶� ���� ������Ʈ�� ������ �и��ؾ� ��

		// ���� ��ġ ���� (�÷��̾� ��ó X -> �þ� �� ����)
		// ���� ��� ��ü ��� �����ϱ� (Ƽ��)

		// ���� ���: ���� ���� -> �ܹ� ����
		// LightManager �����ϱ�

		// �׳� ������ ���� �ȵ� ���¿��� ��ġ ��Ƽ� �׷��ſ���...

		// ���� ���� ��� ���� �� �÷��̾ (�װų�) ���� ������ ��Ż�ϰų� ���� ��� �� ������ �� ���� ����
		// �÷��̾� ��� �� ��� ����

		// ���� ���� �� �� �浹 �� ƨ��� (�̰� �� �ȵ�)

		// ���� �����ؿ���
		// 1. ������ (���� ���� && ���� ����)
		// 2. ��ġ�� (�� �÷��̾� ��ġ��)
		// 3. ���� (????)

		// ���� Ŭ�� ������� -> �̰� ��� �ذ��ϱ� �ؾ� �ϴµ�
		// ���� transform ����ȭ

		// ���� ��ų ���� ����
		// ���� ��ų �ִϸ��̼�Ŀ�� ����?

		// �� ���� ��ȯ ��� ���� �����ؾ� �ϴµ�

		// NavMesh ����� �����ϵ��� �ؾ߰ڴ� ���� �� �����Ǵ°� ���� ������ ���� ������������
		// ������ ��¡ �� �߻� ��� �߰�

		// ���� �浹 �� �÷��̾� �ݴ�������� �и��� -> �ؿ���!!!!!
		// �Ź̰����� ��� �߰� - Arachnophobia -> ���� �׳� �𵨸� �ٲٸ� �ذ��;;;
		// ������ �߰�

		// �Ҹ� ���� ��ġ �� �� ���� ����
		// �÷��̾� �� �� ���� ����

		// Animator�� MeshRenderer ���� �� ������ View Ŭ���� �����!

		// ������? -> ������ ���� ����

		// ������
		// ���ݴ��ϸ� ��ų �����ϰ� ���� -> ���� �ð� �� �ٽ� �ൿ
		// ü�� / ������ ��� ���� ������ �� ��
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