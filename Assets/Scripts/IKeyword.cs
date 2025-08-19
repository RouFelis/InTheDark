using System;

using UnityEngine;

namespace InTheDark.Example.Keywords
{
	public interface IKeyword
	{

	} 

	public interface IKeywordInputHandler<TKeyword> where TKeyword : IKeyword
	{

	}

	public interface IKeywordOutputHandler<TKeyword> where TKeyword : IKeyword
	{

	}
}