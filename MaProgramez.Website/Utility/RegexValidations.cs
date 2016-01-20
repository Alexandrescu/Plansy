using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Ajax.Utilities;

namespace MaProgramez.Website.Utility
{
    public class RegexValidations
    {
        public static bool ContainsPhoneNumber(string text)
        {
            if(text.IsNullOrWhiteSpace()) 
                return false;

            const string phoneNoPattern = @"(^[0,+,7)]\D*[.,/,(?:\s*\d\s*)]{9,})";
                //@"(\D*((02)|(07)||(03)||(+407)||(+402)||(+403)||(00407)||(00402)||(00403)) [0-9]{8})";
               // @"(\D*(?:\s*\d\s*){8,})";
            
            var m = Regex.Match(text, phoneNoPattern);

            return m.Success;
        }

        public static bool ContainsEmailAddress(string text)
        {
            if (text.IsNullOrWhiteSpace())
                return false;

            const string emailPattern = @"(\D*(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?))";
            var m = Regex.Match(text, emailPattern);

            return m.Success;
        }
    }
}