using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using System.IO;

namespace ProjectTemplate
{
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	[System.Web.Script.Services.ScriptService]

	public class ProjectServices : System.Web.Services.WebService
	{
		////////////////////////////////////////////////////////////////////////
		///replace the values of these variables with your database credentials
		////////////////////////////////////////////////////////////////////////
		private string dbID = "sweet16";
		private string dbPass = "!!Sweet16";
		private string dbName = "sweet16";

        public object FileUploadControl { get; private set; }
        public object StatusLabel { get; private set; }

        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        ///call this method anywhere that you need the connection string!
        ////////////////////////////////////////////////////////////////////////
        private string getConString() {
			return "SERVER=107.180.1.16; PORT=3306; DATABASE=" + dbName+"; UID=" + dbID + "; PASSWORD=" + dbPass;
		}
        ////////////////////////////////////////////////////////////////////////



        /////////////////////////////////////////////////////////////////////////
        //don't forget to include this decoration above each method that you want
        //to be exposed as a web service!
        [WebMethod(EnableSession = true)]
		public string TestConnection()
		{
			try
			{
				string testQuery = "select * from testQuery";

				////////////////////////////////////////////////////////////////////////
				///here's an example of using the getConString method!
				////////////////////////////////////////////////////////////////////////
				MySqlConnection con = new MySqlConnection(getConString());
				////////////////////////////////////////////////////////////////////////

				MySqlCommand cmd = new MySqlCommand(testQuery, con);
				MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
				DataTable table = new DataTable();
				adapter.Fill(table);

				return "Success";
			}
			catch (Exception e)
			{
				return "Something went wrong, please check your credentials and db name and try again.  Error: "+e.Message;
			}
		}



        //Create New Account Logic//
        [WebMethod(EnableSession = true)]
        public void RequestAccount(string fName, string lName, string email, string password, string companyName,
                                    string jobTitle, string expertise, string programStatus)
        {
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
            //the only thing fancy about this query is SELECT LAST_INSERT_ID() at the end.  All that
            //does is tell mySql server to return the primary key of the last inserted row.
            string sqlSelect = "insert into register2 (fname, lname, email, password, companyName, jobTitle, expertise, programStatus) " +
                "values(@fnameValue, @lnameValue, @emailValue, @passwordValue, @companyNameValue, @jobTitleValue, @expertiseValue, @programStatusValue); SELECT LAST_INSERT_ID();";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            //sqlCommand.Parameters.AddWithValue("@idRegister2Value", HttpUtility.UrlDecode(idRegister2));
            sqlCommand.Parameters.AddWithValue("@fnameValue", HttpUtility.UrlDecode(fName));
            sqlCommand.Parameters.AddWithValue("@lnameValue", HttpUtility.UrlDecode(lName));
            sqlCommand.Parameters.AddWithValue("@emailValue", HttpUtility.UrlDecode(email));
            sqlCommand.Parameters.AddWithValue("@passwordValue", HttpUtility.UrlDecode(password));
            sqlCommand.Parameters.AddWithValue("@companyNameValue", HttpUtility.UrlDecode(companyName));
            sqlCommand.Parameters.AddWithValue("@jobTitleValue", HttpUtility.UrlDecode(jobTitle));
            sqlCommand.Parameters.AddWithValue("@expertiseValue", HttpUtility.UrlDecode(expertise));
            sqlCommand.Parameters.AddWithValue("@programStatusValue", HttpUtility.UrlDecode(programStatus));

            //this time, we're not using a data adapter to fill a data table.  We're just
            //opening the connection, telling our command to "executescalar" which says basically
            //execute the query and just hand me back the number the query returns (the ID, remember?).
            //don't forget to close the connection!
            sqlConnection.Open();
            //we're using a try/catch so that if the query errors out we can handle it gracefully
            //by closing the connection and moving on
            try
            {
                int accountID = Convert.ToInt32(sqlCommand.ExecuteScalar());
                //here, you could use this accountID for additional queries regarding
                //the requested account.  Really this is just an example to show you
                //a query where you get the primary key of the inserted row back from
                //the database!
            }
            catch (Exception e)
            {
            }
            sqlConnection.Close();
        }


        //Login Logic//
        [WebMethod(EnableSession = true)]
        public int LogOn(string uid, string pass)
        {
            //bool success = false;
            int userId=-1;

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
            string sqlSelect = "SELECT idRegister2 FROM register2 WHERE (email=@idValue and password=@passValue);";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(uid));
            sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(pass));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);

            DataTable sqlDt = new DataTable();
            sqlDa.Fill(sqlDt);

            if (sqlDt.Rows.Count > 0)
            {
                Session["id"] = sqlDt.Rows[0]["idRegister2"];
                //success = true;
                userId = Convert.ToInt32(sqlDt.Rows[0]["idRegister2"]);
            }

            return userId;
        }

        //Log Off Method
        [WebMethod(EnableSession = true)]
        public bool LogOff()
        {
            //if they log off, then we remove the session.  That way, if they access
            //again later they have to log back on in order for their ID to be back
            //in the session!
            Session.Abandon();
            return true;
        }

        //[WebMethod(EnableSession = true)]
        //public string NewEvent(string className, string desc, string date, string time, string location, string creatorId, string rsvpCount)
        //{
        //    if (Session["id"] != null)
        //    {
        //        string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
        //        //the only thing fancy about this query is SELECT LAST_INSERT_ID() at the end.  All that
        //        //does is tell mySql server to return the primary key of the last inserted row.
        //        string sqlSelect = "insert into events (className, descr, date, time, location, creatorId, rsvpCount) " +
        //            "values(@classNameValue, @descValue, @dateValue, @timeValue, @locationValue, @creatorIdValue, @rsvpCountValue); SELECT LAST_INSERT_ID();";

        //        MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
        //        MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

        //        //sqlCommand.Parameters.AddWithValue("@idRegisterValue", HttpUtility.UrlDecode(idRegister));
        //        sqlCommand.Parameters.AddWithValue("@classNameValue", HttpUtility.UrlDecode(className));
        //        sqlCommand.Parameters.AddWithValue("@descValue", HttpUtility.UrlDecode(desc));
        //        sqlCommand.Parameters.AddWithValue("@dateValue", HttpUtility.UrlDecode(date));
        //        sqlCommand.Parameters.AddWithValue("@timeValue", HttpUtility.UrlDecode(time));
        //        sqlCommand.Parameters.AddWithValue("@locationValue", HttpUtility.UrlDecode(location));
        //        sqlCommand.Parameters.AddWithValue("@creatorIdValue", HttpUtility.UrlDecode(creatorId));
        //        sqlCommand.Parameters.AddWithValue("@rsvpCountValue", HttpUtility.UrlDecode(rsvpCount));
        //        sqlConnection.Open();
        //        //we're using a try/catch so that if the query errors out we can handle it gracefully
        //        //by closing the connection and moving on
        //        try
        //        {
        //            int accountID = Convert.ToInt32(sqlCommand.ExecuteScalar());
        //            //here, you could use this accountID for additional queries regarding
        //            //the requested account.  Really this is just an example to show you
        //            //a query where you get the primary key of the inserted row back from
        //            //the database!
        //            sqlConnection.Close();
        //            return "success";
        //        }
        //        catch (Exception e)
        //        {
        //            return "error" + e.Message;
        //        }
        //        //sqlConnection.Close();
        //    }
        //    else
        //    {
        //        return "Please log in";
        //    }
        //}



        [WebMethod(EnableSession = true)]
        public Filter[] LoadFilters(string sessionId)
        {

            //WE ONLY SHARE Events WITH LOGGED IN USERS!
            if (Session["id"] != null)
            {
                DataTable sqlDt = new DataTable("Register");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
                string sqlSelect = "select jobTitle, expertise from register2 where companyName = (select companyName from register2 where idregister2 = @idRegisterValue);";

                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@idRegisterValue", HttpUtility.UrlDecode(sessionId));
                //gonna use this to fill a data table
                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                //filling the data table
                sqlDa.Fill(sqlDt);

                //loop through each row in the dataset, creating instances
                //of our container class Event.  Fill each eveny with
                //data from the rows, then dump them in a list.
                List<Filter> filters = new List<Filter>();
                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {

                    filters.Add(new Filter
                    {
                        jobTitle = sqlDt.Rows[i]["jobTitle"].ToString(),
                        expertise = sqlDt.Rows[i]["expertise"].ToString(),
                        
                    });
                }
                //convert the list of events to an array and return!
                return filters.ToArray();
            }
            else
            {
                //if they're not logged in, return an empty event
                return new Filter[0];
            }
        }

        //load filtered profiles
        [WebMethod(EnableSession = true)]
        public Profile[] GetFilteredProfiles(string sessionId, string jobTitle, string expertise)
        {

            //WE ONLY SHARE Events WITH LOGGED IN USERS!
            if (Session["id"] != null)
            {
                DataTable sqlDt = new DataTable("register2");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;

                sessionId = HttpUtility.UrlDecode(sessionId);
                jobTitle = HttpUtility.UrlDecode(jobTitle);
                expertise = HttpUtility.UrlDecode(expertise);
                string sqlSelect = "";

                if (jobTitle != "default" && expertise != "default") {
                    sqlSelect = "select * from register2 where (programStatus = 'mentor' or programStatus = 'mentorMentee') and companyName = (select companyName from register2 where idregister2 = @idRegisterValue) and jobTitle = @jobTitleValue and expertise = @expertiseValue;";
                }
                else if (jobTitle != "default" && expertise == "default")
                {
                    sqlSelect = "select * from register2 where (programStatus = 'mentor' or programStatus = 'mentorMentee') and companyName = (select companyName from register2 where idregister2 = @idRegisterValue) and jobTitle = @jobTitleValue;";
                }
                else if (jobTitle == "default" && expertise != "default")
                {
                    sqlSelect = "select * from register2 where(programStatus = 'mentor' or programStatus = 'mentorMentee') and companyName = (select companyName from register2 where idregister2 = @idRegisterValue) and expertise = @expertiseValue;";
                }

                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@idRegisterValue", HttpUtility.UrlDecode(sessionId));
                sqlCommand.Parameters.AddWithValue("@jobTitleValue", HttpUtility.UrlDecode(jobTitle));
                sqlCommand.Parameters.AddWithValue("@expertiseValue", HttpUtility.UrlDecode(expertise));

                //gonna use this to fill a data table
                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                //filling the data table
                sqlDa.Fill(sqlDt);

                //loop through each row in the dataset, creating instances
                //of our container class Event.  Fill each eveny with
                //data from the rows, then dump them in a list.
                List<Profile> profiles = new List<Profile>();
                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {

                    profiles.Add(new Profile
                    {
                        registerId = Convert.ToInt32(sqlDt.Rows[i]["idregister2"]),
                        fName = sqlDt.Rows[i]["fName"].ToString(),
                        lName = sqlDt.Rows[i]["lName"].ToString(),
                        email = sqlDt.Rows[i]["email"].ToString(),
                        password = sqlDt.Rows[i]["password"].ToString(),
                        companyName = sqlDt.Rows[i]["companyName"].ToString(),
                        jobTitle = sqlDt.Rows[i]["jobTitle"].ToString(),
                        expertise = sqlDt.Rows[i]["expertise"].ToString(),
                        programStatus = sqlDt.Rows[i]["programStatus"].ToString(),
                        image = sqlDt.Rows[i]["image"].ToString()

                    });
                }

                //convert the list of events to an array and return!
                return profiles.ToArray();
            }
            else
            {
                //if they're not logged in, return an empty event
                return new Profile[0];
            }
        }


        //getEventInfo
        [WebMethod(EnableSession = true)]
        public Profile[] GetProfiles(string sessionId)
        {

            //WE ONLY SHARE Events WITH LOGGED IN USERS!
            if (Session["id"] != null)
            {
                DataTable sqlDt = new DataTable("register2");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
                string sqlSelect = "select * from register2 where (programStatus = 'mentor' or programStatus = 'mentorMentee') and companyName = (select companyName from register2 where idregister2 = @idRegisterValue);";


                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@idRegisterValue", HttpUtility.UrlDecode(sessionId));
                //gonna use this to fill a data table
                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                //filling the data table
                sqlDa.Fill(sqlDt);

                //loop through each row in the dataset, creating instances
                //of our container class Event.  Fill each eveny with
                //data from the rows, then dump them in a list.
                List<Profile> profiles = new List<Profile>();
                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {

                    profiles.Add(new Profile
                    {
                        registerId = Convert.ToInt32(sqlDt.Rows[i]["idregister2"]),
                        fName = sqlDt.Rows[i]["fName"].ToString(),
                        lName = sqlDt.Rows[i]["lName"].ToString(),
                        email = sqlDt.Rows[i]["email"].ToString(),
                        password = sqlDt.Rows[i]["password"].ToString(),
                        companyName = sqlDt.Rows[i]["companyName"].ToString(),
                        jobTitle = sqlDt.Rows[i]["jobTitle"].ToString(),
                        expertise = sqlDt.Rows[i]["expertise"].ToString(),
                        programStatus = sqlDt.Rows[i]["programStatus"].ToString(),
                        image = sqlDt.Rows[i]["image"].ToString()

                    });
                }

                //convert the list of events to an array and return!
                return profiles.ToArray();
            }
            else
            {
                //if they're not logged in, return an empty event
                return new Profile[0];
            }
        }





        ////Update the RSVP count for events
        //[WebMethod(EnableSession = true)]
        //public Event[] GetRSVPCount(string eventId)
        //{

        //    //WE ONLY SHARE Events WITH LOGGED IN USERS!
        //    if (Session["id"] != null)
        //    {
        //        DataTable sqlDt = new DataTable("events");

        //        string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
        //        string sqlSelect = "select rsvpCount from events where idevents = @eventIdValue;";

        //        MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
        //        MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

        //        sqlCommand.Parameters.AddWithValue("@eventIdValue", HttpUtility.UrlDecode(eventId));
        //        //gonna use this to fill a data table
        //        MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
        //        //filling the data table
        //        sqlDa.Fill(sqlDt);

        //        //loop through each row in the dataset, creating instances
        //        //of our container class Event.  Fill each eveny with
        //        //data from the rows, then dump them in a list.
        //        List<Event> events = new List<Event>();
        //        for (int i = 0; i < sqlDt.Rows.Count; i++)
        //        {

        //            events.Add(new Event
        //            {
        //                rsvpCount = Convert.ToInt32(sqlDt.Rows[i]["rsvpCount"])
        //            });
        //        }
        //        //convert the list of events to an array and return!
        //        return events.ToArray();
        //    }
        //    else
        //    {
        //        //if they're not logged in, return an empty event
        //        return new Event[0];
        //    }
        //}


        ////Update RSVP count
        //[WebMethod(EnableSession = true)]
        //public string UpdateRSVP(string eventId, string rsvpCount)
        //{
        //    //WRAPPING THE WHOLE THING IN AN IF STATEMENT TO CHECK IF THEY ARE AN ADMIN!
        //    if (Session["id"] != null)
        //    {
        //        string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
        //        //this is a simple update, with parameters to pass in values
        //        string sqlSelect = "update events set rsvpCount = @rsvpCountValue where idevents = @eventIdValue";

        //        MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
        //        MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

        //        sqlCommand.Parameters.AddWithValue("@eventIdValue", HttpUtility.UrlDecode(eventId));
        //        sqlCommand.Parameters.AddWithValue("@rsvpCountValue", HttpUtility.UrlDecode(rsvpCount));


        //        sqlConnection.Open();
        //        //we're using a try/catch so that if the query errors out we can handle it gracefully
        //        //by closing the connection and moving on
        //        try
        //        {

        //            sqlCommand.ExecuteNonQuery();
        //            sqlConnection.Close();
        //            return "Success";

        //        }
        //        catch (Exception e)
        //        {
        //            sqlConnection.Close();
        //            return "Failure";
        //        }



        //    }
        //    else
        //    {
        //        return "Log in please";
        //    }
        //}


        //getProfileInfo
        [WebMethod(EnableSession = true)]
        public Profile[] PersonalInfo(string sessionId)
        {

            //WE ONLY SHARE Events WITH LOGGED IN USERS!
            if (Session["id"] != null)
            {
                DataTable sqlDt = new DataTable("Register");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
                string sqlSelect = "select * from register2 where @idRegisterValue = idregister2;";

                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@idRegisterValue", HttpUtility.UrlDecode(sessionId));
                //gonna use this to fill a data table
                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                //filling the data table
                sqlDa.Fill(sqlDt);

                //loop through each row in the dataset, creating instances
                //of our container class Event.  Fill each eveny with
                //data from the rows, then dump them in a list.
                List<Profile> profile = new List<Profile>();
                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {

                    profile.Add(new Profile
                    {
                        registerId = Convert.ToInt32(sqlDt.Rows[i]["idregister"]),
                        fName = sqlDt.Rows[i]["fName"].ToString(),
                        lName = sqlDt.Rows[i]["lName"].ToString(),
                        companyName = sqlDt.Rows[i]["companyName"].ToString(),
                        jobTitle = sqlDt.Rows[i]["jobTitle"].ToString(),
                        expertise = sqlDt.Rows[i]["expertise"].ToString(),
                        image = sqlDt.Rows[i]["image"].ToString()
                    });
                }
                //convert the list of events to an array and return!
                return profile.ToArray();
            }
            else
            {
                //if they're not logged in, return an empty event
                return new Profile[0];
            }
        }


        //// Lists of all users for the homepage. 
        //[WebMethod(EnableSession = true)]
        //public Profile[] UserList()
        //{

        //    //WE ONLY SHARE Events WITH LOGGED IN USERS!
        //    if (Session["id"] != null)
        //    {
        //        DataTable sqlDt = new DataTable("Register");

        //        string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
        //        string sqlSelect = "select idRegister, fName, lName from Register;";

        //        MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
        //        MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

        //        //gonna use this to fill a data table
        //        MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
        //        //filling the data table
        //        sqlDa.Fill(sqlDt);

        //        //loop through each row in the dataset, creating instances
        //        //of our container class Event.  Fill each eveny with
        //        //data from the rows, then dump them in a list.
        //        List<Profile> profile = new List<Profile>();
        //        for (int i = 0; i < sqlDt.Rows.Count; i++)
        //        {

        //            profile.Add(new Profile
        //            {
        //                registerId = Convert.ToInt32(sqlDt.Rows[i]["idregister"]),
        //                fName = sqlDt.Rows[i]["fName"].ToString(),
        //                lName = sqlDt.Rows[i]["lName"].ToString(),
        //                year = "",
        //                college = "",
        //                campus = ""
        //            });
        //        }
        //        //convert the list of events to an array and return!
        //        return profile.ToArray();
        //    }
        //    else
        //    {
        //        //if they're not logged in, return an empty event
        //        return new Profile[0];
        //    }
        //}

        
        //protected void UploadButton_Click(object sender, EventArgs e)
        //{
        //    if (FileUploadControl.HasFile)
        //    {
        //        try
        //        {
        //            string filename = Path.GetFileName(FileUploadControl.FileName);
        //            FileUploadControl.SaveAs(Server.MapPath("~/") + filename);
        //            StatusLabel.Text = "Upload status: File uploaded!";
        //        }
        //        catch (Exception ex)
        //        {
        //            StatusLabel.Text = "Upload status: The file could not be uploaded. The following error occured: " + ex.Message;
        //        }
        //    }
        //}
    }

    }
    


 



