using System;

namespace AVBDelivery.Models
{
    public class SiteAnnouncement
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
        public string Text { get; set; } = null!;
    }
}
