using EmailService.Domain.Entities;
using Xunit;

namespace EmailService.Tests.Domain
{
    public class EmailAddressTests
    {
        [Theory]
        [InlineData("user@example.com")]
        [InlineData("first.last@sub.domain.org")]
        public void ValidEmails_Pass(string email)
        {
            var addr = EmailAddress.Create(email);
            Assert.Equal(email, addr.Value);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("user@")]
        [InlineData("@domain.com")]
        public void InvalidEmails_Throw(string email)
        {
            Assert.ThrowsAny<Exception>(() => EmailAddress.Create(email));
        }
    }
}
