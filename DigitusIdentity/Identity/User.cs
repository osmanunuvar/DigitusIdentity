using Microsoft.AspNetCore.Identity;

namespace DigitusIdentity.Identity
{
    public class User:IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
