using System;

using UnityEngine;

namespace InTheDark.Prototypes
{
	public interface IKeyword
	{

	}

	public interface IKeywordHandler<TKeyword> where TKeyword : IKeyword
	{
		public bool IsEnable
		{
			get;
		}

		public object Owner
		{
			get;
		}

		public void OnUpdate(TKeyword damage);
	}

	public interface IKeywordInputHandler<TKeyword> : IKeywordHandler<TKeyword> where TKeyword : IKeyword
	{

	}

	public interface IKeywordOutputHandler<TKeyword> : IKeywordHandler<TKeyword> where TKeyword : IKeyword
	{

	}
}