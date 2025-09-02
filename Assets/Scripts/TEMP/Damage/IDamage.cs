using InTheDark.Example.Keywords;
using System;
using System.Collections.Generic;

using UnityEngine;

namespace InTheDark.Prototypes
{
	public interface IKeywordObserver<in TKeyword> where TKeyword : IKeyword
	{
		public void OnNext(TKeyword keyword);

		public void OnCompleted();
	}

	public interface IKeywordObservable<out TKeyword> where TKeyword : IKeyword
	{
		public IDisposable Subscribe(IKeywordObserver<TKeyword> observer);
	}

	public interface IDamage : IKeyword
	{
		public float BaseValue
		{ 
			get; set;
		}

		public float RateValue 
		{ 
			get; set;
		}

		public float BonusValue
		{
			get; set; 
		}

		public float TotalValue
		{
			get;
		}

		public IKeywordInputHandler<IDamage> DamageInputHandler
		{ 
			get; 
		}

		public IKeywordOutputHandler<IDamage> DamageOutputHandler
		{ 
			get;
		}
	}

	public sealed class KeywordObservable<TKeyword> : IKeywordObservable<TKeyword> where TKeyword : IKeyword
	{
		public IDisposable Subscribe(IKeywordObserver<TKeyword> observer)
		{
			throw new NotImplementedException();
		}
	}

	public class Damage : IDamage, IKeywordObservable<IDamage>, IDisposable
	{
		private float _baseValue;

		private float _rateValue = 1.0F;
		private float _bonusValue;

		private IKeywordInputHandler<IDamage> _damageInputHandler;
		private IKeywordOutputHandler<IDamage> _damageOutputHandler;

		private List<IKeywordObserver<IDamage>> _observers = new();

		// 기본값: 0.0, 최솟값: 0.0
		public float BaseValue
		{
			get => _baseValue;
			set => _baseValue = Mathf.Max(value, 0.0F);
		}

		// 기본값: 1.0, 최솟값: 0.0
		public float RateValue
		{
			get => _rateValue;
			set => _rateValue = Mathf.Max(value, 0.0F);
		}

		// 기본값: 0.0
		public float BonusValue
		{
			get => _bonusValue;
			set => _bonusValue = value;
		}

		public float TotalValue => Mathf.Max(_baseValue * _rateValue + _bonusValue, 0.0F);

		public IKeywordInputHandler<IDamage> DamageInputHandler => _damageInputHandler;

		public IKeywordOutputHandler<IDamage> DamageOutputHandler => _damageOutputHandler;

		private Damage()
		{

		}

		public Damage(float baseValue, float rateValue, float bonusValue, IDamageInputHandler damageInputHandler, IDamageOutputHandler damageOutputHandler)
		{
			if (baseValue < 0.0F)
				throw new ArgumentOutOfRangeException(nameof(baseValue), "Base value must be non-negative.");

			if (rateValue < 0.0F)
				throw new ArgumentOutOfRangeException(nameof(rateValue), "Rate value must be non-negative.");

			if (damageInputHandler is null)
				throw new ArgumentNullException(nameof(damageInputHandler));

			if (damageOutputHandler is null)
				throw new ArgumentNullException(nameof(damageOutputHandler));

			_baseValue = Mathf.Max(baseValue, 0.0F);

			_rateValue = Mathf.Max(rateValue, 0.0F);
			_bonusValue = bonusValue;

			_damageInputHandler = damageInputHandler;
			_damageOutputHandler = damageOutputHandler;
		}

		// 실행
		public void Update()
		{
			OnUpdate();
		}

		public void OnUpdate()
		{
			if (_damageInputHandler.IsEnable)
				_damageInputHandler.OnUpdate(this);

			if (_damageOutputHandler.IsEnable)
				_damageOutputHandler.OnUpdate(this);
		}

		protected class Unsubscriber : IDisposable
		{
			private List<IKeywordObserver<IDamage>> _observers;
			private IKeywordObserver<IDamage> _observer;

			public Unsubscriber(List<IKeywordObserver<IDamage>> observers, IKeywordObserver<IDamage> observer)
			{
				_observers = observers;
				_observer = observer;
			}

			public void Dispose()
			{
				_observers = null;
				_observer = null;

				GC.SuppressFinalize(this);
			}
		}

		public IDisposable Subscribe(IKeywordObserver<IDamage> observer)
		{
			if (observer is null)
				throw new ArgumentNullException(nameof(observer));

			if (!_observers.Contains(observer))
				_observers.Add(observer);

			return new Unsubscriber(_observers, observer);
		}

		// 할당 해제
		public void Dispose()
		{
			_baseValue = 0.0F;

			_rateValue = 0.0F;
			_bonusValue = 0.0F;

			_damageInputHandler = null;
			_damageOutputHandler = null;

			GC.SuppressFinalize(this);
		}
	}
}