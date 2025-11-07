// Models/TestAnswer.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.Models
{
    public class TestAnswer
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public string QuestionKey { get; set; } = string.Empty;

        public string Answer { get; set; } = string.Empty;

        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
