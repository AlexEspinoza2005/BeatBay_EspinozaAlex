using System.ComponentModel.DataAnnotations;
using BeatBay.Model;

namespace BeatBay.API.DTOs
{
    public class AdminActionLogDto
    {
        public int Id { get; set; }
        public int AdminUserId { get; set; }
        public string AdminUserName { get; set; }
        public string ActionType { get; set; }
        public string Description { get; set; }
        public DateTime ActionDate { get; set; }
    }
}
