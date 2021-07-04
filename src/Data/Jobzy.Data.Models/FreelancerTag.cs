﻿namespace Jobzy.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    public class FreelancerTag
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(25)]
        public string Text { get; set; }

        public Freelancer Freelancer { get; set; }
    }
}
