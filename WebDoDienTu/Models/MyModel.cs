using WebDoDienTu.Areas.Identity.Pages.Account;

namespace WebDoDienTu.Models
{
    public class MyModel
    {
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<Category> Categories { get; set; }
    }
}
