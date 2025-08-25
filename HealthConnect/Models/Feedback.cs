using System.ComponentModel.DataAnnotations;

namespace HealthConnect.Models
{
    public class Feedback
    {
        [Key]
        public int feedback_id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string message { get; set; }
        public DateOnly date { get; set; }
      
    }
}
