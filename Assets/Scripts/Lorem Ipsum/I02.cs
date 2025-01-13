using System;

namespace InTheDark.LoremIpsum
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface I02<out T> : IDisposable
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="i00"></param>
		/// <returns></returns>
		public IDisposable Subscribe(I01<T> i01);
	}

	/// <summary>
	/// 
	/// </summary>
	public static partial class I02
	{
		public static IDisposable Subscribe<T>(this I02<T> i02, Action<T> onNext)
		{
			return i02.Subscribe(default);
		}

		public static IDisposable Subscribe<T>(this I02<T> i02, Action onCompleted, Action<T> onNext)
		{
			return i02.Subscribe(default);
		}
	}
}
