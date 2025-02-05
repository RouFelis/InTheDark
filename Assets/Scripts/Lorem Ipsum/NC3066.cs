using System;

namespace InTheDark.LoremIpsum
{
	public interface I789<T>
	{
		public I0566<T> Build();

		public I789<T> OnCompleted(Action onCompleted);

		public I789<T> OnNext(Action<T> onNext);
	}

	public static partial class NC0566
	{
		private class NC789<T> : I789<T>
		{
			public I0566<T> Build()
			{
				throw new NotImplementedException();
			}

			public I789<T> OnCompleted(Action onCompleted)
			{
				throw new NotImplementedException();
			}

			public I789<T> OnNext(Action<T> onNext)
			{
				throw new NotImplementedException();
			}
		}

		public static I789<T> GetBuilder<T>()
		{
			return new NC789<T>();
		}
	}

	public static partial class NC3066
	{
		public static I3066<TSource> Distinct<TSource>(this I3066<TSource> observable)
		{
			return default;
		}

		public static I3066<TSource> Where<TSource>(this I3066<TSource> observable, Predicate<TSource> onWhere)
		{
			return default;
		}
		
		public static I3066<TResult> Select<TSource, TResult>(this I3066<TSource> observable, Converter<TSource, TResult> onWhere)
		{
			return default;
		}

		public static IDisposable Subscribe<TSource>(this I3066<TSource> observable, Action<TSource> onNext = null, Action onCompleted = null)
		{
			return default;
		}

		public static I3066<TSource> Repeat<TSource>(this I3066<TSource> observable, int count)
		{
			return default;
		}
	}

	public class NC3066<T> : I3066<T>
	{
		private class NC909
		{

		}

		public IDisposable Subscribe(I0566<T> observer)
		{
			throw new NotImplementedException();
		}
	}
}