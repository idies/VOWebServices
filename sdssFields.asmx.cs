using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.Configuration;
using System.Text;
using System.Data.SqlClient;
using VOServices.casjobs;


namespace VOServices
{
	/// <summary>
	/// Web Services interface to find fields in the SDSS around a given pointing within the search radius.	
	/// @Deoyani Nandrekar-Heinis
	/// Date: 2015/02/10 
	/// </summary>    
	[WebService(Namespace="http://skyservice.pha.jhu.edu/", 
		 Description="This is an <b>XML Web Services</b> interface to find fields in the <b>Sloan Digital Sky Survey</b>.<br>"
		 + "Send comments to <b>Deoyani Nandrekar-Heinis</b> -- deoyani[at]pha.jhu.edu")]
	public class sdssFields : System.Web.Services.WebService
	{
        string cmdTemplate,  CJobsPasswd, dataRelease, cmdTemplate2; //urlPrefix,;
        long CJobsWSID;
        int n = 0; //sdss dr release number

		/// <summary>
		/// Default constructor, init connection string, sqlcommand template and url prefix
		/// </summary>
		public sdssFields()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
			// Config
			CJobsWSID = long.Parse(ConfigurationSettings.AppSettings["CJobsWSID"]);                    
            CJobsPasswd = ConfigurationSettings.AppSettings["CJobsPWD"];
			cmdTemplate = ConfigurationSettings.AppSettings["CmdTemplate"];
            cmdTemplate2 = ConfigurationSettings.AppSettings["CmdTemplate2"];
			
            dataRelease = getDataRelease();            
		}



		#region Component Designer generated code
		
		//Required by the Web Services Designer 
		private IContainer components = null;
				
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion



        public string getDataRelease() {
            string value = (HttpContext.Current.Request.RequestContext.RouteData.Values["anything1"] as string).ToUpper();
            int  indexDR ;    string dr = "";
            try
            {
                indexDR = value.IndexOf("DR");
            }catch(Exception e) {
                throw new InvalidOperationException("Add DataRelease in URL you are querying e.g DR10");
            }            
            
            bool isNumeric = false;

            try
            {
                isNumeric = int.TryParse(value.Substring(indexDR + 2, 2), out n);
                if (isNumeric)
                    dr = value.Substring(indexDR, 4);
                }
            catch (Exception e)
            {
                try
                {
                    isNumeric = int.TryParse(value.Substring(indexDR + 2, 1), out n);

                    if (isNumeric)
                        dr = value.Substring(indexDR, 3);
                }
                catch (Exception ex) {
                    throw new Exception("There is no proper Data Release mentioned in URL e.g. DR10");
               }
            }

            //if(n>7)
            //    urlPrefix = ConfigurationSettings.AppSettings["sdss3UrlPrefix"];
            //else
            //    urlPrefix = ConfigurationSettings.AppSettings["sdssUrlPrefix"];
            return dr;
        }

		
		/// <summary>
		/// Run a free form sql query and return the results in a DataSet passed as a ref
		/// </summary>
		/// <param name="sql">sql query to run (string)</param>
		/// <param name="ds">output data set (DataSet</param>
		private void Query(string sql, ref DataSet ds)
		{
            JobsSoapClient cjobs = new JobsSoapClient();
            ds = cjobs.ExecuteQuickJobDS(CJobsWSID, CJobsPasswd, sql, dataRelease, "FOR SDSS Fields "+dataRelease, false);               
		}

		/// <summary>
		/// Build SQL query from the input parameters
		/// </summary>
		/// <param name="ra">RA of center in degrees (double)</param>
		/// <param name="dec">Dec of center in degrees (double)</param>
		/// <param name="radius">Search radius in arcmins (double)</param>
		/// <returns>SQL query (string)</returns>
		public string SqlSelectCommand(double ra, double dec, double radius)
		{
			StringBuilder sql = new StringBuilder(cmdTemplate);
			sql.Replace("TEMPLATE",ra+","+dec+","+radius);
			return sql.ToString();
		}

        /// <summary>
        /// Build SQL query from the input parameters
        /// </summary>
        /// <param name="ra">RA of center in degrees (double)</param>
        /// <param name="dec">Dec of center in degrees (double)</param>
        /// <param name="size">Search size in degrees (double)</param>
        /// <returns>SQL query (string)</returns>
        public string SqlSelectCommand(double ra, double dec, double dra, double ddec)
        {
            StringBuilder sql = new StringBuilder(cmdTemplate2);
            sql.AppendFormat("and f.raMax >= {0}", ra - dra);
            sql.AppendFormat("and f.raMin <= {0}", ra + dra);
            sql.AppendFormat("and f.decMax >= {0}", dec - ddec);
            sql.AppendFormat("and f.decMin <= {0}", dec + ddec);
            return sql.ToString();
        }

		//[WebMethod(Description="Return a result of the template query in a <b>DataSet</b>.<br><b>Input 1:</b> ra in degrees (double)<br><b>Input 2:</b> dec in degrees (double)<br><b>Input 3:</b> radius in arcmins (double)<br><b>Output:</b> list of fields (DataSet)")]
		/// <summary>
		/// Build SQL command for search and return the result as a DataSet
		/// </summary>
		/// <param name="ra">RA of center in degrees (double)</param>
		/// <param name="dec">Dec of center in degrees (double)</param>
		/// <param name="radius">Search radius in arcmins (double)</param>
		/// <returns>Result (DataSet)</returns>
		public DataSet DataSetOfFields (double ra, double dec, double radius)
		{
			DataSet ds = new DataSet();
			this.Query (SqlSelectCommand(ra,dec,radius), ref ds);
			return ds;
		}

        /// <summary>
        /// Build SQL command for search and return the result as a DataSet
        /// </summary>
        /// <param name="ra">RA of center in degrees (double)</param>
        /// <param name="dec">Dec of center in degrees (double)</param>
        /// <param name="dra">Delta RA in deg (double)</param>
        /// <param name="ddec">Delta Dec in deg (double)</param>
        /// <returns>Result (DataSet)</returns>
        public DataSet DataSetOfFields(double ra, double dec, double dra, double ddec)
        {
            DataSet ds = new DataSet();
            this.Query(SqlSelectCommand(ra, dec, dra, ddec), ref ds);
            return ds;
        }

		/// <summary>
		/// Runs the query and returns all matching fields
		/// </summary>
		/// <param name="ra">RA of center in degrees (double)</param>
		/// <param name="dec">Dec of center in degrees (double)</param>
		/// <param name="radius">Search radius in arcmins (double)</param>
		/// <returns>All fields (Field[])</returns>
		[WebMethod(Description="Return all fields around the given pointing within the search radius.<br>"
			 + "<b>Input 1:</b> ra in degrees (double)<br>"
			 + "<b>Input 2:</b> dec in degrees (double)<br>"
			 + "<b>Input 3:</b> radius in arcmins (double)<br>"
			 + "<b>Output:</b> array of fields (Field[])")]
		public Field[] FieldArray (double ra, double dec, double radius)
		{
			DataSet ds = DataSetOfFields(ra,dec,radius);
			int num = ds.Tables[0].Rows.Count;
			Field[] field = new Field[num];
			for (int i=0; i<num; i++)
			{
				DataRow row = ds.Tables[0].Rows[i];
				field[i] = new Field(ds.Tables[0].Rows[i]);
                for (int j = 0; j < field[i].passband.Length; j++)
                {
                    //if (n < 7) 
                    //    field[i].passband[j].url = this.FieldUrl(field[i], field[i].passband[j].filter);
                    //else
                        field[i].passband[j].url = field[i].bandUrls[j];
                }
			}	
			return field;
		}


        /// <summary>
        /// Runs the query and returns all matching fields
        /// </summary>
        /// <param name="ra">RA of center in degrees (double)</param>
        /// <param name="dec">Dec of center in degrees (double)</param>
        /// <param name="dra">Delta RA in degrees (double)</param>
        /// <param name="ddec">Delta Dec in degrees (double)</param>
        /// <returns>All fields (Field[])</returns>
        [WebMethod(Description = "Return all fields around the given pointing within the search radius.<br>"
             + "<b>Input 1:</b> ra in degrees (double)<br>"
             + "<b>Input 2:</b> dec in degrees (double)<br>"
             + "<b>Input 3:</b> dra in degrees (double)<br>"
             + "<b>Input 4:</b> ddec in degrees (double)<br>"
             + "<b>Output:</b> array of fields (Field[])")]
        public Field[] FieldArrayRect(double ra, double dec, double dra, double ddec)
        {
            DataSet ds = DataSetOfFields(ra, dec, dra, ddec);
            int num = ds.Tables[0].Rows.Count;
            Field[] field = new Field[num];
            for (int i = 0; i < num; i++)
            {
                DataRow row = ds.Tables[0].Rows[i];
                field[i] = new Field(ds.Tables[0].Rows[i]);
                for (int j = 0; j < field[i].passband.Length; j++)
                {
                    //if (n < 7)
                    //    field[i].passband[j].url = this.FieldUrl(field[i], field[i].passband[j].filter);
                    //else
                        field[i].passband[j].url = field[i].bandUrls[j];
                }
            }
            return field;
        }

		/// <summary>
		/// Simple interface to get basic of the relevant fields
		/// </summary>
		/// <param name="ra">RA of center in degrees (double)</param>
		/// <param name="dec">Dec of center in degrees (double)</param>
		/// <param name="radius">Search radius in arcmins (double)</param>
		/// <returns>Basic info of fields (string[])</returns>
		[WebMethod(Description="Return a simple list of strings that have <b>run, rerun, camcol, field</b>.<br>"
			 + "<b>Input 1:</b> ra in degrees (double)<br>"
			 + "<b>Input 2:</b> dec in degrees (double)<br>"
			 + "<b>Input 3:</b> radius in arcmins (double)<br>"
			 + "<b>Output:</b> list of fields (string[])")]
		public string[] ListOfFields (double ra, double dec, double radius)
		{
			DataSet ds = DataSetOfFields(ra,dec,radius);
			int num = ds.Tables[0].Rows.Count;
			string[] ret = new string[num];
			for (int i=0; i<num; i++)
			{
				DataRow row = ds.Tables[0].Rows[i];
				ret[i] = row["run"]+","+row["rerun"]+","+row["camcol"]+","+row["field"];
			}	
			return ret;
		}

		/// <summary>
		/// Util func to return a bunch of 0s, used to create the URLs
		/// </summary>
		/// <param name="n">Number of zeros</param>
		/// <returns>zeros (string)</returns>
		protected string Zeros(int n)
		{
			string ret = "";
			for (int i=0; i<n; i++) ret+="0";
			return ret;
		}

        /// <summary>
        /// Simple interface to get the urls of fields within the search radius
        /// </summary>
        /// <param name="ra">RA of center in degrees (double)</param>
        /// <param name="dec">Dec of center in degrees (double)</param>
        /// <param name="radius">Search radius in arcmins (double)</param>
        /// <param name="band">Passband name, one of u,g,r,i,z (string)</param>
        /// <returns>URLs (string[])</returns>
        [WebMethod(Description = "Return only the URLs of the (gzip'd) <b>FITS</b> images in the given photometric band.<br>"
             + "<b>Input 1:</b> ra in degrees (double)<br>"
             + "<b>Input 2:</b> dec in degrees (double)<br>"
             + "<b>Input 3:</b> radius in arcmins (double)<br>"
             + "<b>Input 4</b>: band name, any combination of 'u', 'g', 'r', 'i', 'z' (string)<br>"
             + "<b>Output:</b> list of urls to fits files (string[])")]
        public string[] UrlOfFields(double ra, double dec, double radius, string band)
        {
            band = band.ToLower();
            char[] bands = { 'u', 'g', 'r', 'i', 'z' };
            //string[] bands = band.Split(',');
            DataSet ds = DataSetOfFields(ra, dec, radius);
            int num = ds.Tables[0].Rows.Count;
            Field[] field = new Field[num];
            string[] ret = new string[num * band.Length];
            int j = 0;
            for (int i = 0; i < num; i++)
            {
                DataRow row = ds.Tables[0].Rows[i];
                field[i] = new Field(ds.Tables[0].Rows[i]);
               
                    foreach (char b in band)
                    {
                        if ("ugriz".IndexOf(b) == -1)
                            throw new ArgumentException("Band should be one of \"ugriz\", it was " + b);                      

                        int pos = Array.IndexOf(bands, b);
                        ret[j++] = field[i].bandUrls[pos];
                    }
                
            }
            return ret;
        }

        ///// <summary>
        ///// Get URL for field given by run, rerun, etc...
        ///// </summary>
        ///// <param name="run"></param>
        ///// <param name="rerun"></param>
        ///// <param name="camcol"></param>
        ///// <param name="field"></param>
        ///// <param name="band"></param>
        ///// <returns>URL (string)</returns>
        //public string FieldUrl(int run, int rerun, int camcol, int field, string band)
        //{
        //    band = band.ToLower();
        //    string bands = "ugriz";
        //    if (bands.IndexOf(band) < 0)
        //        throw new ApplicationException("Parameter 'band' is invalid! Should be one of u,g,r,i or z...");


        //    string run6digit = Zeros(6 - run.ToString().Length) + run;
        //    string field4digit = Zeros(4 - field.ToString().Length) + field;
        //    if (n < 8)
        //        return this.urlPrefix + "" + run + "/" + rerun + "/corr/"
        //            + camcol + "/fpC-" + run6digit + "-" + band + camcol + "-" + field4digit + ".fit.gz";
        //    else
        //        // Since field id on file system always 4 digit and run always 6 digit appending the string with zeros            
        //        return this.urlPrefix + "/" + rerun + "/" + run + "/" + camcol + "/frame-" + band
        //               + "-" + run6digit + "-" + camcol
        //               + "-" + field4digit + ".fits.bz2";
        //}



        ///// <summary>
        ///// Get URL for field - used by FieldArray()
        ///// </summary>
        ///// <param name="f">Field</param>
        ///// <param name="band"></param>
        ///// <returns>URL (string)</returns>
        //public string FieldUrl(Field f, string band)
        //{
        //    return FieldUrl(f.run, f.rerun, f.camcol, f.field, band);
        //}
	}
}


