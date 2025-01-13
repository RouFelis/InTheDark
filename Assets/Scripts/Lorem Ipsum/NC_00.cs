using System;

namespace InTheDark.LoremIpsum
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class NC_00 : I00
	{
		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			Dispose(this, EventArgs.Empty);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void Dispose(object sender, EventArgs e)
		{

		}
	}
}