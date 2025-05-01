using System;

namespace LoremIpsum
{
	public interface IAction<in T> : IDisposable
	{
		public void OnCompleted();

		public void OnNext(T value);
	} 
}