using System.ComponentModel.DataAnnotations;

namespace HealthConnect.Models
{
    public class Blog_Table
    {
        [Key]public int blog_id { get; set; }
        public int user_id { get; set; }
        public string blog_image { get; set; }
        public string blog_title { get; set; }
        public string blog_content { get; set; }
        public DateOnly blog_date { get; set; }


        public User_Table Writer { get; set; }
    }
}
