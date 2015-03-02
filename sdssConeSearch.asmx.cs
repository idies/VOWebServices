using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using VOServices.casjobs;
using net.ivoa.VOTable;


namespace VOServices
{
    /// <summary>
    /// Summary description for sdssConeSearch
    /// @Deoyani Nandrekar-Heinis Feb 2015
    /// </summary> 
    
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [WebService(Namespace = "http://skyservice.pha.jhu.edu/",
         Description = "This is an <b>XML Web Services</b> interface to find fields in the <b>Sloan Digital Sky Survey</b>.<br>"
         + "Send comments to <b>Deoyani Nandrekar-Heinis</b> -- deoyani[at]pha.jhu.edu")]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class sdssConeSearch : System.Web.Services.WebService
    {
        //[WebMethod]
        //public string HelloWorld()
        //{
        //    return "Hello World Again test 123!!!";
        //}


        private static  long CJobsWSID = long.Parse(ConfigurationSettings.AppSettings["CJobsWSID"]);		
		private static string CJobsTARGET = ""; //ConfigurationSettings.AppSettings["CJobsTARGET"];		
		private static string ConeSelect = ConfigurationSettings.AppSettings["ConeSelect"];
        private static string CJobsPasswd = ConfigurationSettings.AppSettings["CJobsPWD"];
        //private static string CJobsURL = ConfigurationSettings.AppSettings["CJobsURL"];
		
        private bool valid_input(System.Double ra, System.Double dec, System.Double sr) {
			

			// if ((sr < 0.0) || sr > MAX)) return false;

			if ((sr < 0.0) ) return false;
			if ((ra < 0.0) || (ra > 360.0)) return false;
			if ((dec <- 90.0) || (dec > 90.0)) return false;

			return true;
		}
       
        [WebMethod(Description = "ConeSearch from the Sloan Digital Sky Survey")]
        public VOTABLE ConeSearch(System.Double ra, System.Double dec, System.Double sr)
        {
            CJobsTARGET = (HttpContext.Current.Request.RequestContext.RouteData.Values["anything1"] as string).ToUpper().Replace("CONE", "");

            VOTABLE v;
            if (!valid_input(ra, dec, sr))
                throw new Exception(" Wrong input parameters ");

            sr *= 60.0; // in arcminutes because dbo.fGetNearbyObjEq requires  arcminutes ;

            StringBuilder qry = new StringBuilder();
            qry.Append("select " + ConeSelect);
            qry.Append("  from PhotoPrimary p, dbo.fGetNearbyObjEq(" + ra + "," + dec + "," + sr + ") n");
            qry.Append("  where p.objId=n.objId");

            sr /= 60.0; // back to degrees as the ervices requieres;

            JobsSoapClient cjobs = new JobsSoapClient();
            //cjobs.Url = CJobsURL;

            DataSet ds = cjobs.ExecuteQuickJobDS(CJobsWSID, CJobsPasswd, qry.ToString(), CJobsTARGET, "FOR CONESEARCH", false);
            Hashtable ucds = FetchUCDS(ds, cjobs);
            VOTABLE vot = VOTableUtil.DataSet2VOTable(ds);
            vot.DESCRIPTION = new anyTEXT();
            // = "ConeSearch results from the Sloan Digital Sky Survey ";
            vot.RESOURCE[0].TABLE[0].Items = new object[ds.Tables[0].Columns.Count + 3];
            Hashtable votypes = VOTableUtil.getdataTypeTable();
            PARAM p = new PARAM();
            p.name = "inputRA";
            p.datatype = (dataType)votypes[typeof(System.Single)];
            p.value = ra.ToString();
            p.unit = "degrees";
            vot.RESOURCE[0].TABLE[0].Items[0] = p;
            p = new PARAM();
            p.name = "inputDEC";
            p.datatype = (dataType)votypes[typeof(System.Single)];
            p.unit = "degrees";
            p.value = dec.ToString();
            vot.RESOURCE[0].TABLE[0].Items[1] = p;
            p = new PARAM();
            p.name = "inputSR";
            p.datatype = (dataType)votypes[typeof(System.Single)];
            p.unit = "degrees";
            p.value = sr.ToString();
            vot.RESOURCE[0].TABLE[0].Items[2] = p;

            vot.DESCRIPTION.Any = new System.Xml.XmlNode[1];
            XmlDocument doc = new XmlDocument();

            vot.DESCRIPTION.Any[0] = doc.CreateTextNode("DESCRIPTION");

            vot.DESCRIPTION.Any[0].InnerText = "ConeSearch results from the Sloan Digital Sky Survey " + CJobsTARGET;

            //vot.RESOURCE[0].Items[ind] = p;

            for (int x = 0; x < ds.Tables[0].Columns.Count; x++)
            {
                DataColumn col = ds.Tables[0].Columns[x];
                FIELD f = new FIELD();
                f.datatype = (dataType)votypes[col.DataType];
                f.ID = fix(col.ColumnName);
                f.ucd = ucds[fix(col.ColumnName)] != null ? ucds[fix(col.ColumnName)].ToString() : "UNKNOWN";
                vot.RESOURCE[0].TABLE[0].Items[x + 3] = f;
            }

            return vot;
           
        }

        private Hashtable FetchUCDS(DataSet ds, JobsSoapClient cjobs)
        {
            string qry = "select name,ucd from dbcolumns where tablename = 'photoobjall' and (";

            for (int x = 0; x < ds.Tables[0].Columns.Count; x++)
            {
                qry += " name like '" + fix(ds.Tables[0].Columns[x].ColumnName) + "'";
                if (x < ds.Tables[0].Columns.Count - 1) qry += " or ";
            }
            qry += ")";
            Hashtable rst = new Hashtable();
            try
            {
                DataSet d = cjobs.ExecuteQuickJobDS(CJobsWSID, CJobsPasswd, qry.ToString(), CJobsTARGET, "CONESEARCH UCD LOOKUP", false);
                foreach (DataRow row in d.Tables[0].Rows)
                {

                    if (!rst.Contains(row[0]))
                        rst.Add(row[0].ToString().ToUpper().Trim(), row[1]);
                }
                string crap = "";
                foreach (object o in rst.Keys)
                    crap += o.ToString() + " ";
            }
            catch
            {
                rst.Add("OBJID", "ID_MAIN");
                rst.Add("RA", "POS_EQ_RA_MAIN");
                rst.Add("DEC", "POS_EQ_DEC_MAIN");
            }
            return rst;

        }

        private string fix(string s)
        {
            return Regex.Replace(s, "<C[0-9]+\\/>", "").ToUpper().Trim();
        }

    }
}
