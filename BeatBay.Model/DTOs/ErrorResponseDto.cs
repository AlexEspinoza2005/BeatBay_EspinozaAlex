using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatBay.DTOs
{
    public class ErrorResponseDto
    {
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
