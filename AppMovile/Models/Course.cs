using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppMovile.Models
{
    public class Course
    {
        public string Key { get; set; }  // this line for Firebase key
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }

        public Course() { } // Default constructor needed for Firebase

        public Course(string title, string description, string category, string imageUrl)
        {
            Title = title;
            Description = description;
            Category = category;
            ImageUrl = imageUrl;
        }
    }


}
