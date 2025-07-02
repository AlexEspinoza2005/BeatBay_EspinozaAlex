using BeatBay.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatBay.DTOs
{
    public class TokenValidationDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public UserDto? User { get; set; }
    }
}
