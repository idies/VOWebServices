using System;

namespace VOServices
{
	/// <summary>
	/// This class holds all the WCS parameters
    /// @Deoyani Nandrekar-Heinis Feb 2015
    /// </summary> 
	public class WCS
	{
		public int NAXIS1, NAXIS2;
		public string CTYPE1, CTYPE2;
		public string CUNIT1, CUNIT2;
		public string EPOCH;
		public double CRPIX1, CRPIX2; // col, row
		public double CRVAL1, CRVAL2; // dec, ra
		public double CD1_1, CD1_2, CD2_1, CD2_2;

		public WCS()
		{
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

        $Log: WCS.cs,v $
        Revision 1.1.1.1  2003/03/06 19:44:49  budavari
        First import of SdssFields Web Services

*/
