using System.Security.Cryptography;

namespace Core.Utilities
{
    public static class VerificationCodeHelper
    {
        public static string GenerateSixDigitCode()
        {
            var n = RandomNumberGenerator.GetInt32(0, 1_000_000);
            return n.ToString("D6");
        }
    }
}
