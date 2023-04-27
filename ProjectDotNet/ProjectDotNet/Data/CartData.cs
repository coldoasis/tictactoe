using Microsoft.Data.SqlClient;
using ProjectDotNet.Models;
namespace ProjectDotNet.Data
{
    public class CartData
    {
        
        public static Dictionary<Product,int> Cartlist (Dictionary<int, int> cartdata)
        {
            Dictionary<Product, int> myCart = new Dictionary<Product, int>();
            if (cartdata!=null) 
            {
                List<Product> products = GetCartProduct(cartdata);
                List<int> quantityList = GetCartProductCount(cartdata);
                myCart = products.Zip(quantityList, (k, v) => new { k, v })
                                      .ToDictionary(x => x.k, x => x.v);
            }
            
            return myCart;
        }

        public static List<Product> GetCartProduct(Dictionary<int,int> cartdata)
        {
            List<int>productid = new List<int>();
            List<Product> products = new List<Product>();
            foreach (KeyValuePair<int,int> kvp in cartdata)
            {
                productid.Add(kvp.Key);
            }
            foreach(int i in productid)
            {
                List<Product> fulllist = ProductList.GetProducts();
                
                products.Add(ProductList.GetProductsFromId(fulllist, i));
            }
            return products;
        }

        public static List<int> GetCartProductCount(Dictionary<int, int> cartdata) 
        {
            List<int>quantity = new List<int>();
            foreach(KeyValuePair<int,int> kvp in cartdata)
            {
                quantity.Add(kvp.Value);
            }
            return quantity;
        }
    }
}
