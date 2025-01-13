using System;

namespace InTheDark.LoremIpsum
{
	/// <summary>
	/// 
	/// </summary>
	public interface I01<in T> : IDisposable
	{
		/// <summary>
		/// 
		/// </summary>
		public void OnCompleted();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void OnNext(T value);
	}
}