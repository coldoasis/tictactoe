using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace ProjectDotNet.Models
{
    public class Purchases
    {
        public DateTime PurchaseDate { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }

        public string ProductImgSrc { get; set; }
        public string ActivationCode { get; set; }
        public int ProductRating { get; set; }
    }
}
