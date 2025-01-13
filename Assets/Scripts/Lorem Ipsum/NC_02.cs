using System;

namespace InTheDark.LoremIpsum
{
	public sealed class NC_02<T> : NC_00, I02<T>
	{
		private sealed class NC_00_nc00 : NC_00
		{

		}

		public IDisposable Subscribe(I01<T> i01)
		{
			return new NC_00_nc00();
		}
	}
}