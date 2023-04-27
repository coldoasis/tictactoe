using Microsoft.Data.SqlClient;
using ProjectDotNet.Models;
namespace ProjectDotNet.Data
{
    public class UserData
    {
        
        public static User GetUser(string username, string password)
        {
            string connectionString = @"Server=(local);Database=eCartdb;Integrated Security=true;encrypt=false";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"SELECT CustomerId, Username, Password
                            FROM CustomerAccount 
                            WHERE Username = @username AND Password = @password";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);
                SqlDataReader reader = cmd.ExecuteReader();
                User _user = null;
                while (reader.Read())
                {
                    _user = new User()
                    {
                        Id = (int)reader["CustomerId"],
                        Username = (string)reader["Username"],
                        Password = (string)reader["Password"]
                    };
                }
                return _user;
            }

        }

    }
}
