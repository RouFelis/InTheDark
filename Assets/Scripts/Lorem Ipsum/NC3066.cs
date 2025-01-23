using System;

namespace InTheDark.LoremIpsum
{
	public class NC3066<T> : I3066<T>, IDisposable
	{
		private class NC3066_909 : IDisposable
		{
			public void Dispose()
			{
				GC.SuppressFinalize(this);
			}
		}

		public void Dispose()
		{
			Dispose(this, EventArgs.Empty);
			GC.SuppressFinalize(this);
		}

		public IDisposable Subscribe(I0566<T> i0566)
		{
			return new NC3066_909();
		}

		protected virtual void Dispose(object sender, EventArgs e)
		{

		}
	}
}