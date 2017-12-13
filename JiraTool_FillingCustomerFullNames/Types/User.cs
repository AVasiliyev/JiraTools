namespace JiraTool_FillingCustomerFullNames.Types
{
    class User
    {
        public User(string email, string fullName, string url)
        {
            Email = email;
            FullName = fullName;
            Url = url;
        }

        public string Email { get; set; }
        public string FullName { get; set; }
        public string Url { get; set; }
    }
}
