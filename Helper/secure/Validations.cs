using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Helper.secure
{
    public class Validations
    {
        public Validations() { }

        public static bool IsPhoneNumberValid(string phoneNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(phoneNumber))
                {
                    throw new ValidationException("Phone number cannot be null or empty.");
                }

                string pattern = @"^(\+?\d{1,4})?[\s\-]?\(?\d{1,4}\)?[\s\-]?\d{1,4}[\s\-]?\d{1,4}$";
                return Regex.IsMatch(phoneNumber, pattern);
            }
            catch (Exception ex)
            {
                HandleValidationException(ex);
                return false;
            }
        }


        public static bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Weak Password: Empty or null password.");
                return false;
            }

            int score = CalculatePasswordStrength(password);

            if (score < 3)
            {
                Console.WriteLine("Weak Password: Does not meet minimum criteria.");
                return false;
            }

            if (score == 3 || score == 4)
            {
                Console.WriteLine("Medium Password: Good, but could be stronger.");
                return true;
            }

            Console.WriteLine("Strong Password: Meets all criteria.");
            return true;
        }

        private static int CalculatePasswordStrength(string password)
        {
            int score = 0;

            if (Regex.IsMatch(password, "[a-z]")) score++; 
            if (Regex.IsMatch(password, "[A-Z]")) score++; 
            if (Regex.IsMatch(password, @"\d")) score++;
            if (Regex.IsMatch(password, @"[!@#$%^&*()_=+;:,<.>/?~]")) score++; 
            if (password.Length >= 6) score++; 

            return score;
        }

        public static bool IsEmailPhoneValid(string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
                {
                    return false;
                }

                string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                bool isEmailValid = Regex.IsMatch(input, emailPattern);

                if (isEmailValid)
                {
                    return true;
                }
                else
                {
                    bool isPhoneNumberValid = IsPhoneNumberValid(input);
                    return isPhoneNumberValid;
                }
            }
            catch (Exception ex)
            {
                HandleValidationException(ex);
                return false;
            }
        }

      
        public static bool ValidateGhanaID(string idNumber)
        {
            if (string.IsNullOrWhiteSpace(idNumber))
            {
                return false;
            }

            Regex IdFormat = new Regex(@"^[Gg][Hh][Aa]-\d{9}-\d$", RegexOptions.Compiled);

            if (!IdFormat.IsMatch(idNumber))
            {
                return false;
            }

            string numericPart = idNumber.Replace("Gha-", "", StringComparison.OrdinalIgnoreCase).Replace("-", "");
            return true;
        }


        private static void HandleValidationException(Exception ex)
        {
            Console.WriteLine($"Validation Error: {ex.Message}");
        }
    }
}
