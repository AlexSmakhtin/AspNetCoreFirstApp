using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS8618

namespace AspNetCoreFirstApp
{
    public class SmtpConfig
    {
        [Required]
        public string Host { get; set; }
        [Range(25, 25)]
        public int Port { get; set; }
        [EmailAddress]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public bool UseSsl { get; set; }
        [StringLength(254, MinimumLength = 1)]
        public string EmailText { get; set; }
        [Range(1, 1000)]
        public int RetryCount { get; set; }
        [Range(1, 10_000)]
        public int WaitForNextTry { get; set; }
    }
}