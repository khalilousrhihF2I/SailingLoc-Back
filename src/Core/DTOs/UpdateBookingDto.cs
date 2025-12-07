using System;

namespace Core.DTOs
{
    public class UpdateBookingDto
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // only status is updatable
    }
}
