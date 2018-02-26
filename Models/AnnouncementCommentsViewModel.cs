using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebDevEsports.Models
{
    public class AnnouncementCommentsViewModel
    {
        public Announcement Announcement { get; set; }
        public List<Comment> Comments { get; set; }

        public int AnnouncementID { get; set; }
        public string Content { get; set; }
        public DateTime DateTime { get; set; }
        public string AuthorDisplayName { get; set; }
        public int NumberViews { get; set; }
        
        [Required(AllowEmptyStrings = false)]
        [StringLength(140, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        [RegularExpression("^[a-zA-Z0-9 ]*$", ErrorMessage = "Invalid entry")]
        public string Comment { get; set; }
    }
}
