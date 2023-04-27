using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProjectDotNet.Models;

namespace ProjectDotNet.Data
{
    public class PurchasesData
    {
        public static Dictionary<int, List<Purchases>> GetPurchases()
        {
            Dictionary<int, List<Purchases>> purchases = new Dictionary<int, List<Purchases>>();
            string connectionString = @"Server=(local);Database=eCartdb;Integrated Security=true;encrypt=false";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"SELECT cp.PurchaseDate, cp.CustomerId, cp.ProductId, p.ProductName, p.ProductDescription, p.ProductImgSrc, cp.ActivationCode, cp.ProductRating
                    FROM CustomerPurchases cp, Product p
                    WHERE cp.ProductId = p.ProductId order by cp.ProductRating desc";

                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Purchases purchase = new Purchases()
                    {
                        PurchaseDate = (DateTime)reader["PurchaseDate"],
                        CustomerId = (int)reader["CustomerId"],
                        ProductId = (int)reader["ProductId"],
                        ProductName = (string)reader["ProductName"],
                        ProductDescription = (string)reader["ProductDescription"],
                        ProductImgSrc = (string)reader["ProductImgSrc"],
                        ActivationCode = (string)reader["ActivationCode"],
                        ProductRating = (int)reader["ProductRating"]
                    };

                    if (!purchases.ContainsKey(purchase.ProductId))
                    {
                        purchases[purchase.ProductId] = new List<Purchases>();
                    }
                    purchases[purchase.ProductId].Add(purchase);
                }
                return purchases;
            }
        }

        public static void UpdateProductRating(string customerId, string productId, string ratingId)
        {
            string connectionString = @"Server=localhost;Database=eCartdb;Integrated Security=true;encrypt=false";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string sql = "Update CustomerPurchases set ProductRating = @rating where ProductId = @productId and CustomerId = @customerId";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@rating", Convert.ToInt32(ratingId));
                cmd.Parameters.AddWithValue("@productId", Convert.ToInt32(productId));
                cmd.Parameters.AddWithValue("@customerId", Convert.ToInt32(customerId));

                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static void SavePurchase(Purchases purchase) 
        {
            string connectionString = @"Server=localhost;Database=eCartdb;Integrated Security=true;encrypt=false";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string sql = "INSERT INTO CustomerPurchases (PurchaseDate, CustomerId, ProductId, ActivationCode, ProductRating) VALUES (@PurchaseDate, @CustomerId, @ProductId, @ActivationCode, 0)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@PurchaseDate", purchase.PurchaseDate);
                cmd.Parameters.AddWithValue("@CustomerId", purchase.CustomerId);
                cmd.Parameters.AddWithValue("@ProductId", purchase.ProductId);
                cmd.Parameters.AddWithValue("@ActivationCode", purchase.ActivationCode);

                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static bool ActivationCodeExists(string activationCode)
        {
            Dictionary<int, List<Purchases>> purchases = GetPurchases();
            foreach(KeyValuePair<int,List<Purchases>> kvp in purchases)
            {
                if(kvp.Value.Any(x=> x.ActivationCode == activationCode))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
