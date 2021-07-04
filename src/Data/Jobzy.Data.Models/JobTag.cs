﻿namespace Jobzy.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    public class JobTag
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(25)]
        public string Text { get; set; }

        public Job Job { get; set; }
    }
}
