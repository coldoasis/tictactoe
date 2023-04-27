using Microsoft.Data.SqlClient;
using ProjectDotNet.Models;
namespace ProjectDotNet.Data
{
    public class ProductList
    {
        
        public static List<Product> GetProducts()
        {
            List<Product> products = new List<Product>();
            string connectionString = @"Server=(local);Database=eCartdb;Integrated Security=true;encrypt=false";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"SELECT ProductId, ProductName, ProductDescription, ProductImgSrc, ProductPrice
                            FROM Product";

                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Product product = new Product()
                    {
                        ProductId = (int)reader["ProductId"],
                        ProductName = (string)reader["ProductName"],
                        ProductDescription = (string)reader["ProductDescription"],
                        ProductImgSrc = (string)reader["ProductImgSrc"],
                        ProductPrice = (decimal)reader["ProductPrice"]
                    };
                    products.Add(product);
                }
                return products;
            }
        }

        public static List<Product> filterlist(string searchquery, List<Product> products)
        {
            List<Product> filteredlist = new List<Product>();
            var iter = products.Where(x => x.ProductDescription.ToLower().Contains(searchquery.ToLower()) ||
                        x.ProductName.ToLower().Contains(searchquery.ToLower()))
                        .Select(x => x);

            foreach(Product product in iter)
            {
                filteredlist.Add(product);
            }
            return filteredlist;
        }

        public static Product GetProductsFromId(List<Product> products, int productId)
        {
            var product = products.FirstOrDefault(x => x.ProductId == productId);
            return product;

        }

        public static Dictionary<int, List<int>> GetProductRating()
        {
            Dictionary<int, List<int>> productRating = new Dictionary<int, List<int>>();
            string connectionString = @"Server=localhost;Database=eCartdb;Integrated Security=true;encrypt=false";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string sql = "Select p.productId as productId, avg(c.productrating) as rating, count(p.productId) as totalRating from product p inner join customerPurchases c on p.productId = c.productId where c.productrating is not null group by p.productId ";

                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    productRating.Add((int)reader["productId"],
                        new List<int>(){
                            (int)reader["rating"],
                            (int)reader["totalRating"]
                    });
                }
                conn.Close();
                return productRating;
            }
        }


    }
}
