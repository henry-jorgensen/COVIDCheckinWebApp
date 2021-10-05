using COVIDCheckinWebApp.Code;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace COVIDCheckinWebApp.Controllers
{
    public class QrCodeGenerationController : ControllerBase
    {
        internal string connectionString()
        {
            return ReadConfig.ReadConnection("projectstudiodb");
            //return "Server=tcp:amnotdaniel.duckdns.org,1433;Initial Catalog=projectstudio;Persist Security Info=False;User ID=sa;Password=ProjectStudio1;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False;Connection Timeout=30;";
        }



        // GET: /api/QrCodeGeneration/
        public IActionResult Index()
        {
            return Content("API for Checkin submission. Please use api/Checkin/<action> (Manual/Unique)");
        }

        // POST: api/QrCodeGeneration/manual/
        [HttpPost]
        public IActionResult Manual()
        {
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

                    if (theCount.ToString() != "0")
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

                }

                // Close connection.
                db.Close();
                return Content(uniqueId);
            }
            return Content("Error! Something went wrong...");
        }

        // POST: api/QrCodeGeneration/manual/
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
                if (existingIDResult == null)
                {
                    db.Close();
                    return StatusCode(500);
                }

                // Close connection + end script.
                db.Close();
                return Content(uName);
            }
            else
            {
                return StatusCode(500);
            }

        }

        // GET: /api/QrCodeGeneration/QrCode?text=1234
        public IActionResult QrCode(string text)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            byte[] bytes = new byte[0];

            using (MemoryStream ms = new MemoryStream())
            {
                qrCodeImage.Save(ms, ImageFormat.Png);
                bytes = ms.ToArray();
            }

            return new FileStreamResult(new MemoryStream(bytes), "image/png");
        }

        // GET: /api/QrCodeGeneration/SaveQrCode?text=1234
        public IActionResult SaveQrCode(string text)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            byte[] bytes = new byte[0];

            using (MemoryStream ms = new MemoryStream())
            {
                qrCodeImage.Save(ms, ImageFormat.Png);
                bytes = ms.ToArray();
            }

            return new FileStreamResult(new MemoryStream(bytes), "application/zip")
            {
                FileDownloadName = "qrcode.png"
            };
        }
    }
}
