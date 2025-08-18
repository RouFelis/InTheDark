using UnityEngine;

namespace InTheDark.Example.Keywords
{
	public interface ICredit : IKeyword
	{

	}

	public class Credit : ICredit
	{

	}

	public class CreditInputHandler : IKeywordInputHandler<ICredit>
	{
		public void Migration__<UKeyword>(IKeywordInputHandler<UKeyword> handler__) where UKeyword : ICredit
		{
			throw new System.NotImplementedException();
		}
	}

	public class CreditOutputHandler : IKeywordOutputHandler<ICredit>
	{
		public void Migration__<UKeyword>(IKeywordOutputHandler<UKeyword> handler__) where UKeyword : ICredit
		{
			throw new System.NotImplementedException();
		}
	}
}