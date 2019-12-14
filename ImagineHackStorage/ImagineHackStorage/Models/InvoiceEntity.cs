namespace ImagineHackStorage.Models
{
    public class InvoiceEntity : MongoEntity<string>
    {
        public int AmountPaid { get; set; }
        public int AmountToPay { get; set; }
        public Article Article { get; set; }
    }

    public class Article
    {
        public string Name { get; set; }
        public int Price { get; set; }
    }
}