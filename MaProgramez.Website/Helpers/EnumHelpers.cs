using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace MaProgramez.Website.Helpers
{
    public static class EnumHelpers
    {
        public static IEnumerable<SelectListItem> GetItems(this Type enumType, int? seletedValue)
        {
            if (!typeof(Enum).IsAssignableFrom(enumType))
            {
                throw new ArgumentException("Type must be an enum");
            }

            var names = Enum.GetNames(enumType);
            var values = Enum.GetValues(enumType).Cast<int>();

            var items = names.Zip(values, (name, value) => new SelectListItem
            {
                Text = FirstLetterToUpper(GetDescription(enumType, name)),
                Value = value.ToString(),
                Selected = value == seletedValue
            });

            return items;
        }

        public static IEnumerable<SelectListItem> GetItems(this Type enumType, IEnumerable<int> seletedValues)
        {
            if (!typeof(Enum).IsAssignableFrom(enumType))
            {
                throw new ArgumentException("Type must be an enum");
            }

            var names = Enum.GetNames(enumType);
            var values = Enum.GetValues(enumType).Cast<int>();

            var items = names.Zip(values, (name, value) => new SelectListItem
            {
                Text = FirstLetterToUpper(GetDescription(enumType, name)),
                Value = value.ToString(),
                Selected = seletedValues != null && seletedValues.Any(x => x == value)
            });

            return items;
        }

        public static string DisplayDescription(this Enum value)
        {
            var enumType = value.GetType();
            var enumValue = Enum.GetName(enumType, value);
            var member = enumType.GetMember(enumValue)[0];

            var attrs = member.GetCustomAttributes(typeof(DisplayAttribute), false);
            //var outString = ((DisplayAttribute)attrs[0]).Description;

            try
            {
                if (((DisplayAttribute)attrs[0]).ResourceType != null)
                {
                    return ((DisplayAttribute)attrs[0]).GetDescription();
                }
            }
            catch { }

            return enumValue;
        }

        public static string DisplayName(this Enum value)
        {
            var enumType = value.GetType();
            var enumValue = Enum.GetName(enumType, value);
            var member = enumType.GetMember(enumValue)[0];

            var attrs = member.GetCustomAttributes(typeof(DisplayAttribute), false);
            //var outString = ((DisplayAttribute)attrs[0]).Description;


            try
            {
                if (((DisplayAttribute)attrs[0]).ResourceType != null)
                {
                    return ((DisplayAttribute)attrs[0]).GetName();
                }
            }
            catch { }

            return enumValue;
        }

        private static string GetName(Type enumType, string name)
        {
            var result = name;
            var attribute = enumType.GetField(name)
                .GetCustomAttributes(inherit: false)
                .OfType<DisplayAttribute>()
                .FirstOrDefault();

            if (attribute != null)
            {
                result = attribute.GetName();
            }

            return result;
        }

        public static string GetDescription(Type enumType, string name)
        {
            var result = name;
            var attribute = enumType.GetField(name)
                .GetCustomAttributes(inherit: false)
                .OfType<DisplayAttribute>()
                .FirstOrDefault();

            if (attribute != null)
            {
                result = attribute.GetDescription();
            }

            return result;
        }

        public static string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }
    }
}