using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Helper.secure
{
    public static class PhoneOperatorChecker
    {
        private static readonly Regex VodafonePattern = new(@"^(?:\+233|0)?(20[0-9]{7}|50[0-9]{7})$");
        private static readonly Regex MtnPattern = new(@"^(?:\+233|0)?(24[0-9]{7}|54[0-9]{7}|55[0-9]{7}|59[0-9]{7})$");
        private static readonly Regex AirtelTigoPattern = new(@"^(?:\+233|0)?(27[0-9]{7}|57[0-9]{7})$");

        public static string CheckOperator(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return "Invalid Number";

            if (VodafonePattern.IsMatch(phoneNumber))
                return "vodafone";
            if (MtnPattern.IsMatch(phoneNumber))
                return "mtn";
            if (AirtelTigoPattern.IsMatch(phoneNumber))
                return "airteltigo";

            return "Other Operator";
        }
    }
}
