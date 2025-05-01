using System;

namespace LoremIpsum
{
	public interface IEvent<out T>
	{
		public IDisposable Subscribe(IAction<T> i1118);
	} 
}