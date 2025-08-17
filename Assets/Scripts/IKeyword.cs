using System;

using UnityEngine;

namespace InTheDark.Example.Keywords
{
	public interface IKeyword
	{
		// TODO: 이벤트 핸들 추가
	} 

	// Keyword 생성 시
	// Keyword 적용 시
	// Keyword 제거 시

	public interface IKeywordHandler<in TKeyword> where TKeyword : IKeyword
	{
		// TODO: 이벤트 핸들 추가
		// TODO: 이벤트 처리 메서드 추가
	}

	// 옵저버 패턴
	// 대리자 인스턴스
	public interface IKeywordObservable<out TKeyword> where TKeyword : IKeyword
	{
		public IDisposable Subscribe__(IKetwordObserver<TKeyword> observer__);
	}

	// 대리자 인스턴스
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
		// TODO: 마이그레이션 메서드 추가

		public void Migration__<UKeyword>(IKeywordInputHandler<UKeyword> handler__) where UKeyword : TKeyword;
	}

	public interface IKeywordOutputHandler<in TKeyword> : IKeywordHandler<TKeyword> where TKeyword : IKeyword
	{
		// TODO: 마이그레이션 메서드 추가

		public void Migration__<UKeyword>(IKeywordOutputHandler<UKeyword> handler__) where UKeyword : TKeyword;
	}
}