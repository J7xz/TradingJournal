using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace J7Journal.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Color { get; set; }

        public Tag()
        {
            Color = "#6c757d";
        }
    }
}