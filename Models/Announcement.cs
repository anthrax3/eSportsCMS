using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebDevEsports.Models
{
    public class Announcement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [RegularExpression("^[a-zA-Z0-9 ]*$", ErrorMessage = "Invalid entry")]
        public string Title { get; set; }

        [Required]
        [StringLength(5000, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [RegularExpression("^[a-zA-Z0-9 ]*$", ErrorMessage = "Invalid entry")]
        public string Content { get; set; }
        
        public DateTime DateTime { get; set; }

        public string AuthorDisplayName { get; set; }

        public virtual ApplicationUser Author { get; set; }

        public int NumberViews { get; set; }

        public string ImageName { get; set; }
    }
}
