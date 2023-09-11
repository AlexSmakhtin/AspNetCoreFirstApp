using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#pragma warning disable CS8618

namespace AspNetCoreFirstApp
{
    public class SmtpConfig
    {
        [Required]
        public string Host { get; set; }
        [Required]
        public int Port { get; set; }
        [EmailAddress]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public bool UseSsl { get; set; }
        [Required]
        public string EmailText { get; set; }

    }
}
