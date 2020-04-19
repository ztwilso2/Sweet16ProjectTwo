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
        private string getConString()
        {
            return "SERVER=107.180.1.16; PORT=3306; DATABASE=" + dbName + "; UID=" + dbID + "; PASSWORD=" + dbPass;
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
                return "Something went wrong, please check your credentials and db name and try again.  Error: " + e.Message;
            }
        }



        //Create New Account Logic//
        [WebMethod(EnableSession = true)]
        public void RequestAccount(string fName, string lName, string email, string password, string companyName,
                                    string jobTitle, string expertise, string programStatus)
        {
            var menteeCount = 0;

            if (HttpUtility.UrlDecode(programStatus) == "mentee")
            {
                menteeCount = -1;
            }

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
            //the only thing fancy about this query is SELECT LAST_INSERT_ID() at the end.  All that
            //does is tell mySql server to return the primary key of the last inserted row.
            string sqlSelect = "insert into register2 (fname, lname, email, password, companyName, jobTitle, expertise, programStatus, numofMentees) " +
                "values(@fnameValue, @lnameValue, @emailValue, @passwordValue, @companyNameValue, @jobTitleValue, @expertiseValue, @programStatusValue, @menteeCount); SELECT LAST_INSERT_ID();";

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
            sqlCommand.Parameters.AddWithValue("@menteeCount", menteeCount);

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
        public Profile LogOn(string uid, string pass)
        {
            
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
            string sqlSelect = "SELECT idRegister2, programStatus FROM register2 WHERE (email=@idValue and password=@passValue);";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(uid));
            sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(pass));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);

            DataTable sqlDt = new DataTable();
            sqlDa.Fill(sqlDt);

            Profile loginProfile;
            if (sqlDt.Rows.Count > 0)
            {

                loginProfile = new Profile
                {
                    registerId = Convert.ToInt32(sqlDt.Rows[0]["idregister2"]),
                    programStatus = sqlDt.Rows[0]["programStatus"].ToString()

                };

                Session["id"] = sqlDt.Rows[0]["idRegister2"];
                               
            }
            else
            {
                loginProfile = new Profile
                {
                    registerId = -1,
                    programStatus = ""

                };
            }

            return loginProfile;
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

        

        [WebMethod(EnableSession = true)]
        public Company[] LoadCompanies()
        {

            //WE ONLY SHARE Events WITH LOGGED IN USERS!
            
                DataTable sqlDt = new DataTable("companies");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
                string sqlSelect = "select * from companies";

                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                //gonna use this to fill a data table
                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                //filling the data table
                sqlDa.Fill(sqlDt);

                //loop through each row in the dataset, creating instances
                //of our container class Event.  Fill each eveny with
                //data from the rows, then dump them in a list.
                List<Company> companies = new List<Company>();
                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {

                    companies.Add(new Company
                    {
                        companyName = sqlDt.Rows[i]["companyName"].ToString(),
                        
                    });
                }
                //convert the list of events to an array and return!
                return companies.ToArray();
            
        }



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

                if (jobTitle != "default" && expertise != "default")
                {
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
                        numOfMentees = Convert.ToInt32(sqlDt.Rows[i]["numofMentees"]),
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
                        numOfMentees = Convert.ToInt32(sqlDt.Rows[i]["numofMentees"]),
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


        //REQUEST MENTOR//
        [WebMethod(EnableSession = true)]
        public bool RequestMentor(string mentorId, string menteeId)
        {
            

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
            //the only thing fancy about this query is SELECT LAST_INSERT_ID() at the end.  All that
            //does is tell mySql server to return the primary key of the last inserted row.
            string sqlSelect = "insert into MentorMenteeRequests (mentorId, menteeId) " +
                "values(@mentorValue, @menteeValue); SELECT LAST_INSERT_ID();";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            //sqlCommand.Parameters.AddWithValue("@idRegister2Value", HttpUtility.UrlDecode(idRegister2));
            sqlCommand.Parameters.AddWithValue("@mentorValue", Convert.ToInt32(HttpUtility.UrlDecode(mentorId)));
            sqlCommand.Parameters.AddWithValue("@menteeValue", Convert.ToInt32(HttpUtility.UrlDecode(menteeId)));
                   
            sqlConnection.Open();
            //we're using a try/catch so that if the query errors out we can handle it gracefully
            //by closing the connection and moving on
            try
            {
                int accountID = Convert.ToInt32(sqlCommand.ExecuteScalar());
                
                sqlConnection.Close();
                return true;
                //here, you could use this accountID for additional queries regarding
                //the requested account.  Really this is just an example to show you
                //a query where you get the primary key of the inserted row back from
                //the database!
            }
            catch (Exception e)
            {
                
                sqlConnection.Close();
                return false;
            }
            
        }

    

        //LOAD MENTEE REQUESTS
        [WebMethod(EnableSession = true)]
        public Profile[] LoadRequests(string sessionId)
        {

            //WE ONLY SHARE Events WITH LOGGED IN USERS!
            if (Session["id"] != null)
            {
                DataTable sqlDt = new DataTable("register2");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
                string sqlSelect = "select menteeId, fName, lName, jobTitle, expertise from register2 " +
                                    "JOIN MentorMenteeRequests ON idregister2 = menteeId " +
                                    "where mentorId = @mentorValue;";


                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@mentorValue", Convert.ToInt32(HttpUtility.UrlDecode(sessionId)));
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
                        registerId = Convert.ToInt32(sqlDt.Rows[i]["menteeId"]),
                        fName = sqlDt.Rows[i]["fName"].ToString(),
                        lName = sqlDt.Rows[i]["lName"].ToString(),
                        jobTitle = sqlDt.Rows[i]["jobTitle"].ToString(),
                        expertise = sqlDt.Rows[i]["expertise"].ToString(),
                      
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


        //LOAD MENTEES
        [WebMethod(EnableSession = true)]
        public Profile[] LoadMentees(string sessionId)
        {

            //WE ONLY SHARE Events WITH LOGGED IN USERS!
            if (Session["id"] != null)
            {
                DataTable sqlDt = new DataTable("register2");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
                string sqlSelect = "select menteeId, fName, lName, jobTitle, expertise from register2 " +
                                    "JOIN MentorMenteePairs ON idregister2 = menteeId " +
                                    "where mentorId = @mentorValue;";


                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@mentorValue", Convert.ToInt32(HttpUtility.UrlDecode(sessionId)));
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
                        registerId = Convert.ToInt32(sqlDt.Rows[i]["menteeId"]),
                        fName = sqlDt.Rows[i]["fName"].ToString(),
                        lName = sqlDt.Rows[i]["lName"].ToString(),
                        jobTitle = sqlDt.Rows[i]["jobTitle"].ToString(),
                        expertise = sqlDt.Rows[i]["expertise"].ToString(),

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


        //LOAD MENTEES
        [WebMethod(EnableSession = true)]
        public Profile[] LoadMentors(string sessionId)
        {

            //WE ONLY SHARE Events WITH LOGGED IN USERS!
            if (Session["id"] != null)
            {
                DataTable sqlDt = new DataTable("register2");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
                string sqlSelect = "select mentorId, fName, lName, jobTitle, expertise from register2 " +
                                    "JOIN MentorMenteePairs ON idregister2 = mentorId " +
                                    "where menteeId = @menteeValue;";


                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@menteeValue", Convert.ToInt32(HttpUtility.UrlDecode(sessionId)));
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
                        registerId = Convert.ToInt32(sqlDt.Rows[i]["mentorId"]),
                        fName = sqlDt.Rows[i]["fName"].ToString(),
                        lName = sqlDt.Rows[i]["lName"].ToString(),
                        jobTitle = sqlDt.Rows[i]["jobTitle"].ToString(),
                        expertise = sqlDt.Rows[i]["expertise"].ToString(),

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


        //DENY MENTEE REQUEST//
        [WebMethod(EnableSession = true)]
        public bool DenyRequest(string mentorId, string requestId)
        {


            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
            //the only thing fancy about this query is SELECT LAST_INSERT_ID() at the end.  All that
            //does is tell mySql server to return the primary key of the last inserted row.
            string sqlSelect = "delete from MentorMenteeRequests where mentorId = @mentorValue and menteeId = @requestValue;";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            //sqlCommand.Parameters.AddWithValue("@idRegister2Value", HttpUtility.UrlDecode(idRegister2));
            sqlCommand.Parameters.AddWithValue("@mentorValue", Convert.ToInt32(HttpUtility.UrlDecode(mentorId)));
            sqlCommand.Parameters.AddWithValue("@requestValue", Convert.ToInt32(HttpUtility.UrlDecode(requestId)));

            sqlConnection.Open();
            //we're using a try/catch so that if the query errors out we can handle it gracefully
            //by closing the connection and moving on
            try
            {
                int accountID = Convert.ToInt32(sqlCommand.ExecuteScalar());

                sqlConnection.Close();
                return true;
                //here, you could use this accountID for additional queries regarding
                //the requested account.  Really this is just an example to show you
                //a query where you get the primary key of the inserted row back from
                //the database!
            }
            catch (Exception e)
            {

                sqlConnection.Close();
                return false;
            }

        }


        //ACCEPT MENTEE REQUEST//
        [WebMethod(EnableSession = true)]
        public int AcceptRequest(string mentorId, string requestId)
        {


            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
            //the only thing fancy about this query is SELECT LAST_INSERT_ID() at the end.  All that
            //does is tell mySql server to return the primary key of the last inserted row.
            string sqlSelect = "insert into MentorMenteePairs (menteeId, mentorId)" +
                               "select menteeId, mentorId from MentorMenteeRequests where mentorId = @mentorValue and menteeId = @requestValue limit 1;" +
                               "delete from MentorMenteeRequests where mentorId = @mentorValue and menteeId = @requestValue;" +
                               "select numofMentees from register2 where idregister2 = @mentorValue;";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            //sqlCommand.Parameters.AddWithValue("@idRegister2Value", HttpUtility.UrlDecode(idRegister2));
            sqlCommand.Parameters.AddWithValue("@mentorValue", Convert.ToInt32(HttpUtility.UrlDecode(mentorId)));
            sqlCommand.Parameters.AddWithValue("@requestValue", Convert.ToInt32(HttpUtility.UrlDecode(requestId)));

            sqlConnection.Open();
            //we're using a try/catch so that if the query errors out we can handle it gracefully
            //by closing the connection and moving on
            try
            {
                int menteeCount = Convert.ToInt32(sqlCommand.ExecuteScalar());

                sqlConnection.Close();
                return menteeCount;
                //here, you could use this accountID for additional queries regarding
                //the requested account.  Really this is just an example to show you
                //a query where you get the primary key of the inserted row back from
                //the database!
            }
            catch (Exception e)
            {

                sqlConnection.Close();
                return -1;
            }

        }


        //Update Mentee count
        [WebMethod(EnableSession = true)]
        public bool UpdateMenteeCount(string userId, string menteeCount)
        {
            //WRAPPING THE WHOLE THING IN AN IF STATEMENT TO CHECK IF THEY ARE AN ADMIN!
            if (Session["id"] != null)
            {
                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
                //this is a simple update, with parameters to pass in values
                string sqlSelect = "update register2 set numofMentees = @menteeCountValue where idregister2 = @mentorId";

                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@mentorId", HttpUtility.UrlDecode(userId));
                sqlCommand.Parameters.AddWithValue("@menteeCountValue", Convert.ToInt32(HttpUtility.UrlDecode(menteeCount)));


                sqlConnection.Open();
                //we're using a try/catch so that if the query errors out we can handle it gracefully
                //by closing the connection and moving on
                try
                {

                    sqlCommand.ExecuteNonQuery();
                    sqlConnection.Close();
                    return true;

                }
                catch (Exception e)
                {
                    sqlConnection.Close();
                    return false;
                }



            }
            else
            {
                return false;
            }
        }


        //getProfileInfo
        [WebMethod(EnableSession = true)]
        public Profile[] PersonalInfo(string sessionId)
        {

            //WE ONLY SHARE Events WITH LOGGED IN USERS!
            if (Session["id"] != null)
            {
                DataTable sqlDt = new DataTable("register2");

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
                        registerId = Convert.ToInt32(sqlDt.Rows[i]["idregister2"]),
                        fName = sqlDt.Rows[i]["fName"].ToString(),
                        lName = sqlDt.Rows[i]["lName"].ToString(),
                        companyName = sqlDt.Rows[i]["companyName"].ToString(),
                        jobTitle = sqlDt.Rows[i]["jobTitle"].ToString(),
                        expertise = sqlDt.Rows[i]["expertise"].ToString(),
                        email = sqlDt.Rows[i]["email"].ToString(),
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

        // WEBSERVICE TO SEND A MESSAGE
        //[WebMethod(EnableSession = true)]
        //public string SendMessage(string idmessageBoard, string message, string date)
        //{
        //    If(Session[“id”] != null)
        //        DataTable sqlDt = new DataTable("messageBoard");
        //        string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
        //        String sqlSelect = “select * from messages, date";
        //        MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
        //        MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
        // }

        // WEBSERVICE TO SEE CHAT HISTORY (SHOWS LAST 10 MESSAGES)
        //[WebMethod(EnableSession = true)]
        //public string GetChatHistory(string idmessageBoard, string message,from)
        //{
        //    If(Session[“id”] != null)
        //        DataTable sqlDt = new DataTable("messageBoard");
        //        string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["sweet16"].ConnectionString;
        //        String sqlSelect = “select * from ";
        //        MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
        //        MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
        // }

    }

}


