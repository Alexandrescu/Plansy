//-----------------------------------------------------------------------
// <copyright file="Common.cs" company="SpinIT">
//     Copyright (c) Spin IT 2014. All rights reserved.
// </copyright>
// <author>SpinIT</author>
//-----------------------------------------------------------------------

using System;
using MaProgramez.Repository.DbContexts;
using Microsoft.Ajax.Utilities;

namespace MaProgramez.Website.Utility
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Xml;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The common methods and fields class
    /// </summary>
    public static class Common
    {
        #region METHODS

        /// <summary>
        /// Gets the app config.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetAppConfig(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// Gets the specified name configuration value from database.
        /// </summary>
        /// <param name="name">The name of the config.</param>
        /// <returns></returns>
        public static string GetDbConfig(string name)
        {
            using (var db = new AppDbContext())
            {
                var config = db.Configs.FirstOrDefault(c => c.Name.ToLower() == name.ToLower());
                return config == null ? string.Empty : config.Value;
            }
        }

        /// <summary>
        /// Returns the first working day after the specified date
        /// It excludes Sundays and non working days configured in NonWorkingDays table in DB
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="hoursIncrement">The hours increment.</param>
        /// <returns></returns>
        public static DateTime GetFirstWorkingDayForDate(DateTime date, int hoursIncrement)
        {
            using (var db = new AppDbContext())
            {
                date = date.AddHours(hoursIncrement);

                var nonWorkingDates = db.DefaultNonWorkingDays.Where(d => d.StartDateTime > date);

                var found = false;

                while (!found)
                {
                    found = true;

                    if (nonWorkingDates.Any(d => d.StartDateTime == date) || date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        date = date.AddDays(1);
                        found = false;
                    }
                }

                return date;
            }
        }

        /// <summary>
        /// Dictionaries to string - used to create parameters for url calls.
        /// </summary>
        /// <param name="dict">The dictionary.</param>
        /// <returns></returns>
        public static string DictToString(Dictionary<string, string> dict)
        {
            var builder = new StringBuilder();

            foreach (var kvp in dict)
            {
                builder.Append(kvp.Key + "=" + HttpUtility.UrlPathEncode(kvp.Value) + "&");
            }

            return builder.ToString().TrimEnd('&');
        }

        public static bool IsPhoneNumber(string str)
        {
            if(str.IsNullOrWhiteSpace()) 
                return false;
            return str.Count() == 10 && str.All(c => c >= '0' && c <= '9');
        }

        public static string GetExcerpt(string fullText)
        {
            if (string.IsNullOrWhiteSpace(fullText) || fullText.Length < 100)
            {
                return fullText;
            }

            return string.Format("{0} [...]", fullText.Substring(0, 100));
        }

        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public static string GetPublicLink(string title, int number)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                var link = string.Format("{0}-{1}", Regex.Replace(title, "[^a-zA-Z0-9_]", "-"), number).ToLower();
                return link;
            }

            return string.Empty;
        }

        public static DateTime ChangeTime(this DateTime dateTime, int hours, int minutes, int seconds, int milliseconds)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                hours,
                minutes,
                seconds,
                milliseconds,
                dateTime.Kind);
        }

        #endregion
    }
}
