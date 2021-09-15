using System;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860


namespace COVIDCheckinWebApp.Controllers
{
    public class CheckinController : Controller
    {
        internal string connectionString()
        {
            return "Server=tcp:amnotdaniel.duckdns.org,1433;Initial Catalog=projectstudio;Persist Security Info=False;User ID=sa;Password=ProjectStudio1;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False;Connection Timeout=30;";
        }

        // GET: /api/Checkin/
        public IActionResult Index()
        {
        return Content("API for Checkin submission. Please use api/Checkin/<action> (Manual/Unique)");
        }

        // POST: api/Checkin/manual/
        [HttpPost]
        public IActionResult Manual()
        {
            var venueId = Request.Cookies["venueId"];
            string fName = Request.Form["firstName"];
            string lName = Request.Form["lastName"];
            string phone = Request.Form["contact"];

            if (!String.IsNullOrWhiteSpace(fName) && !String.IsNullOrWhiteSpace(fName) && !String.IsNullOrWhiteSpace(phone))
            {
                string uniqueId = (lName.Substring(0, 2) + fName.Substring(0, 2)).ToUpper();
                Console.WriteLine(uniqueId);

                SqlConnection db = new SqlConnection(connectionString());

                // Open connection
                db.Open();

                // Convert names to upper.
                fName = fName.ToUpper();
                lName = lName.ToUpper();

                // Query to check for database entry matching User inputs.
                SqlCommand IDCheck = new SqlCommand("SELECT ID FROM Users WHERE firstName = @fName AND lastName = @lName AND phoneNumber = @pNum", db);

                // Query parameters.
                IDCheck.Parameters.AddWithValue("@fName", fName);
                IDCheck.Parameters.AddWithValue("@lName", lName);
                IDCheck.Parameters.AddWithValue("@pNum", phone);


                var IDResult = IDCheck.ExecuteScalar();

                // Query to check for database entry matching User inputs.
                SqlCommand uniqueIDCheck = new SqlCommand("SELECT uniqueId FROM Users WHERE firstName = @fName AND lastName = @lName AND phoneNumber = @pNum", db);

                // Query parameters.
                uniqueIDCheck.Parameters.AddWithValue("@fName", fName);
                uniqueIDCheck.Parameters.AddWithValue("@lName", lName);
                uniqueIDCheck.Parameters.AddWithValue("@pNum", phone);


                // Store query result.
                var uniqueIDResult = uniqueIDCheck.ExecuteScalar();


                if (uniqueIDResult == null)
                {
                    //Querying count of possible uniqueIds in database  //NEED FIX FOR LIKE
                    SqlCommand checkingUniqueIds = new SqlCommand("SELECT COUNT(*) AS userCount FROM Users WHERE uniqueId LIKE @uniqueId", db);

                    //Query parameters
                    checkingUniqueIds.Parameters.AddWithValue("@uniqueId", uniqueId + "%");

                    var theCount = checkingUniqueIds.ExecuteScalar();
                    theCount = theCount.ToString();

                    if (theCount != "0")
                    {
                        uniqueId = uniqueId + theCount.ToString();
                    }

                    // Query to add new User to database.
                    SqlCommand addNewUser = new SqlCommand("INSERT INTO Users (firstName, lastName, phoneNumber, uniqueId) VALUES (@fName, @lName, @pNum, @uniqueId)", db);

                    // Query parameters.
                    addNewUser.Parameters.AddWithValue("@fName", fName);
                    addNewUser.Parameters.AddWithValue("@lName", lName);
                    addNewUser.Parameters.AddWithValue("@pNum", phone);
                    addNewUser.Parameters.AddWithValue("@uniqueId", uniqueId);

                    // Executing query.
                    addNewUser.ExecuteNonQuery();

                    // Selecting newly added Users uniqueId.
                    SqlCommand idCommand = new SqlCommand("SELECT ID FROM Users WHERE firstName = @fName AND lastName = @lName AND phoneNumber = @pNum", db);

                    idCommand.Parameters.AddWithValue("@fName", fName);
                    idCommand.Parameters.AddWithValue("@lName", lName);
                    idCommand.Parameters.AddWithValue("@pNum", phone);

                    // Setting userIDResult to newly created Users uniqueId.
                    IDResult = idCommand.ExecuteScalar();
                }

                // Log checkin query.
                string logQuery = "INSERT INTO CheckinLogs (dateLog, venueId, uniqueId) VALUES(@theDate, @venueId, @ID)";
                SqlCommand logCommand = new SqlCommand(logQuery, db);

                // Query parameters.
                logCommand.Parameters.AddWithValue("@theDate", DateTime.Now);
                logCommand.Parameters.AddWithValue("@venueId", venueId);
                logCommand.Parameters.AddWithValue("@ID", IDResult);

                // Executing query.
                logCommand.ExecuteNonQuery();

                // Close connection.
                db.Close();
                return Content(uniqueId);
            }
            return Content("Error! Something went wrong...");
        }

        // POST: api/Checkin/manual/
        [HttpPost]
        public IActionResult Unique()
        {
            if (!string.IsNullOrWhiteSpace(Request.Form["username"]))
            {
                SqlConnection db = new SqlConnection(connectionString());
                // Get form data.
                string uName = Request.Form["username"];

                db.Open();

                // Check for uniqueId in database + store result.
                SqlCommand existingID = new SqlCommand("SELECT ID FROM Users WHERE uniqueId = @uName", db);
                existingID.Parameters.AddWithValue("@uName", uName);

                var existingIDResult = existingID.ExecuteScalar();

                // If uniqueId exists, log a new checkin. Else, notify user of incorrect details.
                if (existingIDResult != null)
                {
                    SqlCommand newCheckin = new SqlCommand("INSERT INTO CheckinLogs (dateLog, venueId, uniqueId) VALUES (@dateLog, @venueId, @uniqueId)", db);
                    newCheckin.Parameters.AddWithValue("@dateLog", DateTime.Now);
                    newCheckin.Parameters.AddWithValue("@venueId", Request.Cookies["venueId"]);
                    newCheckin.Parameters.AddWithValue("@uniqueId", existingIDResult);
                    newCheckin.ExecuteNonQuery();
                }
                else
                {
                    // Notify user of incorrect username.
                }

                // Close connection + end script.
                db.Close();
                return Content(uName);
            }
            return Content("Error, invalid username");
        }
    }
}