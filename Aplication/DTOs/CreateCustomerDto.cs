namespace Application.DTOs
{
    public class CreateCustomerDto
    {
        public string Document { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
    }
}
