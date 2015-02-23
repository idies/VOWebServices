using System;

namespace VOServices
{
	/// <summary>
	/// A simple class that holds (ra,dec) and (mu,nu) coordinates.
    /// @Deoyani Nandrekar-Heinis Feb 2015
    /// </summary> 
	public class SdssCoord
	{
		public double ra, dec, mu, nu;
		
		/// <summary>
		/// Default constructor
		/// </summary>
		public SdssCoord()
		{
		}

		/// <summary>
		/// Constructor with coords to set
		/// </summary>
		/// <param name="ra">RA (double)</param>
		/// <param name="dec">Dec (double)</param>
		/// <param name="mu">mu (double)</param>
		/// <param name="nu">nu (double)</param>
		public SdssCoord(double ra, double dec, double mu, double nu)
		{
			this.ra = ra;
			this.dec = dec;
			this.mu = mu;
			this.nu = nu;
		}
		
		/// <summary>
		/// Get the CVS revision number (string)
		/// </summary>
		static public string Revision
		{
			get { return "$Revision: 1.1.1.1 $"; }
		}
	}
}

/*
Revision History

        $Log: SdssCoord.cs,v $
        Revision 1.1.1.1  2003/03/06 19:44:49  budavari
        First import of SdssFields Web Services

*/
