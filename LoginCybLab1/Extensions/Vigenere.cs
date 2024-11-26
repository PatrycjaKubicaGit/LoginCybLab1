using System.Text;

namespace LoginCybLab1.Extensions
{
    public class Vigenere
    {
        public static string Encrypt(string text, string key)
        {
            StringBuilder encryptedText = new StringBuilder();
            key = PrepareKey(text, key);

            for (int i = 0; i < text.Length; i++)
            {
                char encryptedChar = (char)((text[i] + key[i] - 2 * 'A') % 26 + 'A');
                encryptedText.Append(encryptedChar);
            }

            return encryptedText.ToString();
        }

        public static string Decrypt(string text, string key)
        {
            StringBuilder decryptedText = new StringBuilder();
            key = PrepareKey(text, key);

            for (int i = 0; i < text.Length; i++)
            {
                char decryptedChar = (char)((text[i] - key[i] + 26) % 26 + 'A');
                decryptedText.Append(decryptedChar);
            }

            return decryptedText.ToString();
        }

        private static string PrepareKey(string text, string key)
        {
            StringBuilder extendedKey = new StringBuilder(key);
            while (extendedKey.Length < text.Length)
            {
                extendedKey.Append(key);
            }

            return extendedKey.ToString().Substring(0, text.Length);
        }

        public static string NormalizeInput(string input)
        {
            return input.ToUpper().Replace(" ", "");
        }
    }
}



