using System;

using UnityEngine;

namespace InTheDark.Example.Keywords
{
	public interface IKeyword
	{
		// TODO: �̺�Ʈ �ڵ� �߰�
	} 

	// Keyword ���� ��
	// Keyword ���� ��
	// Keyword ���� ��

	public interface IKeywordHandler<in TKeyword> where TKeyword : IKeyword
	{
		// TODO: �̺�Ʈ �ڵ� �߰�
		// TODO: �̺�Ʈ ó�� �޼��� �߰�
	}

	// ������ ����
	// �븮�� �ν��Ͻ�
	public interface IKeywordObservable<out TKeyword> where TKeyword : IKeyword
	{
		public IDisposable Subscribe__(IKetwordObserver<TKeyword> observer__);
	}

	// �븮�� �ν��Ͻ�
	public interface IKetwordObserver<in TKeyword> where TKeyword : IKeyword
	{
		public void OnCompleted__();

		public void OnNext__(TKeyword keyword__);

		public void OnError__(Exception error__);
	}

	public static class KeywordObservables
	{
		public static IDisposable Subscribe<TKeyword>(this IKeywordObservable<TKeyword> observable__, Action<TKeyword> onNext__) where TKeyword : IKeyword
		{
			if (observable__ == null) throw new ArgumentNullException(nameof(observable__));
			if (onNext__ == null) throw new ArgumentNullException(nameof(onNext__));
			return observable__.Subscribe__(default);
		}
	}

	public interface IKeywordInputHandler<in TKeyword> : IKeywordHandler<TKeyword> where TKeyword : IKeyword
	{
		// TODO: ���̱׷��̼� �޼��� �߰�

		public void Migration__<UKeyword>(IKeywordInputHandler<UKeyword> handler__) where UKeyword : TKeyword;
	}

	public interface IKeywordOutputHandler<in TKeyword> : IKeywordHandler<TKeyword> where TKeyword : IKeyword
	{
		// TODO: ���̱׷��̼� �޼��� �߰�

		public void Migration__<UKeyword>(IKeywordOutputHandler<UKeyword> handler__) where UKeyword : TKeyword;
	}
}