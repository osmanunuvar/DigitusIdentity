using System.Collections.Generic;

namespace DigitusIdentity.Models
{
    public static class OnlineUser
    {
        public static List<LoggedInUser> LoggedInUsers { get; set; } = new List<LoggedInUser>();
    }
}
