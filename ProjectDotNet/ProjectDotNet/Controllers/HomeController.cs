using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectDotNet.Data;
using ProjectDotNet.Models;
using System.Diagnostics;
using ProjectDotNet.Extensions;

namespace ProjectDotNet.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string searchquery)
        {
            HttpContext.Session.SetString("guest", "guest_user");

            TempData["PreviousAction"] = "Index";

            List <Product> display = ProductList.GetProducts();
            Dictionary<int, List<int>> productRating = ProductList.GetProductRating();
            if (searchquery != null)
            {
                List<Product> filtered = ProductList.filterlist(searchquery, display);
                ViewBag.ProductList = filtered;
            }
            else
            {
                ViewBag.ProductList = display;
            }
            TempData["IsLoggedIn"]  = HttpContext.Session.GetString("IsLoggedIn");
            ViewBag.Username = HttpContext.Session.GetString("username");
            ViewBag.ProductRating = productRating;

            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (username != null && password != null)
            {
                User targetuser = UserData.GetUser(username, password);
                if (targetuser != null)
                {
                    HttpContext.Session.SetString("IsLoggedIn", "true");
                    HttpContext.Session.Remove("guest");
                    HttpContext.Session.SetString("username", username);
                    HttpContext.Session.SetInt32("customerId", targetuser.Id);

                    // return a JSON response with the login status
                    return Json(new { success = true, redirectUrl = (string)TempData["PreviousAction"] });
                }
            }

            // if the login is unsuccessful, return a JSON response with the error message
            return Json(new { success = false, errorMessage = "Invalid username or password." });
        }


        [HttpPost]
        public JsonResult StoreCount(int productId, int count)
        {
            // Retrieve the cart data from the session
            var cartData = HttpContext.Session.GetObject<Dictionary<int, int>>("cartData") ?? new Dictionary<int, int>();

            // Update the cart data with the new product count
            if (cartData.ContainsKey(productId))
            {
                cartData[productId] = count;
            }
            else
            {
                cartData.Add(productId, count);
            }

            // Save the updated cart data back to the session
            HttpContext.Session.SetObject("cartData", cartData);

            // Calculate the total count of all products in the cart
            int totalCount = cartData.Values.Sum();

            // Update the CartCount in the session
            HttpContext.Session.SetInt32("CartCount", totalCount);

            // Return the updated cart data and total count to the client
            return Json(new { cartData = cartData, totalCount = totalCount });
        }




        public IActionResult Cart()
        {
            var cartData = (Dictionary<int, int>)HttpContext.Session.GetObject<Dictionary<int, int>>("cartData");
            Dictionary<Product, int> cartlist = CartData.Cartlist(cartData);
            ViewBag.Total = TotalSum(cartlist);
            ViewBag.Cart = cartlist;

            TempData["IsLoggedIn"] = HttpContext.Session.GetString("IsLoggedIn");
            TempData["PreviousAction"] = "Cart";
            return View();
        }

        public decimal TotalSum(Dictionary<Product, int> cartlist)
        {
            if (cartlist.Count > 0)
            {
                decimal total = 0;
                foreach (KeyValuePair<Product, int> kvp in cartlist)
                {
                    total += kvp.Key.ProductPrice * kvp.Value;

                }
                return total;
            }
            return 0;
        }

        public IActionResult Purchases()
        {
            string isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            TempData["IsLoggedIn"] = HttpContext.Session.GetString("IsLoggedIn");
            TempData["PreviousAction"] = "Purchases";
            if (isLoggedIn == "true")
            {
                var purchases = PurchasesData.GetPurchases();
                return View(purchases);
            }

            return RedirectToAction("Login");
        }

        public IActionResult Checkout()
        {
            Dictionary<int, int> cartData = HttpContext.Session.GetObject<Dictionary<int, int>>("cartData");
            string isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");

            if (isLoggedIn == "true")
            {
                // Clear the cart and save purchases
                if (cartData != null && cartData.Count > 0)
                {
                    var customerId = HttpContext.Session.GetInt32("customerId") ?? 0; // Get customerId from session
                    var purchaseDate = DateTime.Now;
                    var cartList = CartData.Cartlist(cartData);

                    // Save purchases
                    foreach (KeyValuePair<Product, int> kvp in cartList)
                    {
                        for (int i = 0; i < kvp.Value; i++)
                        {
                            string activationCode = GenerateUniqueActivationCode(kvp.Value); // Generate a unique activation code for each item purchased

                            var purchase = new Purchases
                            {
                                PurchaseDate = purchaseDate,
                                CustomerId = customerId,
                                ProductId = kvp.Key.ProductId,
                                ActivationCode = activationCode
                            };

                            PurchasesData.SavePurchase(purchase);
                        }
                    }

                    // Clear the cart
                    HttpContext.Session.Remove("cartData");
                }

                HttpContext.Session.SetInt32("CartCount", 0);
                return RedirectToAction("Purchases");
            }

            TempData["PreviousAction"] = "Checkout";
            return RedirectToAction("Login");
        }



        public IActionResult GiveRating(string customerId, string productId, string ratingId)
        {
            PurchasesData.UpdateProductRating(customerId, productId, ratingId);
            return RedirectToAction("Purchases");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.SetString("IsLoggedIn", "false");
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult UpdateCartCount()
        {
            HttpContext.Session.SetInt32("CartCount", 0);
            return Json(new { success = true });
        }
        [HttpGet]
        public IActionResult GetCartCount()
        {
            int cartCount = HttpContext.Session.GetInt32("CartCount") ?? 0;
            return Json(cartCount);
        }

        [HttpPost]
        public IActionResult CartViewChangeQuantity(int productid, int newquantity)
        {

            Dictionary<int, int> cartData = HttpContext.Session.GetObject<Dictionary<int, int>>("cartData");

            if (cartData.ContainsKey(productid))
            {
                cartData[productid] = newquantity;
                HttpContext.Session.SetObject("cartData", cartData);
                var cartList = CartData.Cartlist(cartData);
                foreach (KeyValuePair<Product, int> kvp in cartList)
                {
                    if (kvp.Key.ProductId == productid)
                    {
                        var price = kvp.Key.ProductPrice;
                        var quantity = kvp.Value;
                        return Json(new { status = true, productprice = price });
                    }
                }
            }
            return Json(new { status = false });

        }

        private static string GenerateUniqueActivationCode(int quantity)
        {
            int seed = (int)DateTime.Now.Ticks & 0x0000FFFF;
            Random random = new Random(seed);
            string activationCode;

            do
            {
                // Generate random alphabet from a to z
                char alphabet1 = (char)random.Next('a', 'z' + 1);
                char alphabet2 = (char)random.Next('a', 'z' + 1);
                char alphabet3 = (char)random.Next('a', 'z' + 1);

                // Generate random numbers from 0 to 9
                int num1 = random.Next(10);
                int num2 = random.Next(10);
                int num3 = random.Next(10);
                int num4 = random.Next(10);

                // Format the activation code with a counter for each item purchased
                for (int i = 1; i <= quantity; i++)
                {
                    activationCode = $"{alphabet1}{num1}{num2}{num3}{num4}-{num1}{num2}{num3}-{alphabet2}{alphabet3}{alphabet1}-{num2}{num3}{num4}-{i}";

                    if (!PurchasesData.ActivationCodeExists(activationCode))
                    {
                        return activationCode;
                    }
                }

            } while (true);

            return null;
        }



        public IActionResult Privacy()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}