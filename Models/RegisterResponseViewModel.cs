
namespace CalendarEvents.IDP.Models
{
    public class RegisterResponseViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public RegisterResponseViewModel(ApplicationUser applicationUser)
        {
            this.Id = applicationUser.Id;
            this.Name = applicationUser.Name;
            this.Email = applicationUser.Email;
        }
    }
}
