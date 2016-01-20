namespace MaProgramez.Website.Utility
{
    using System;
    using System.Globalization;
    using System.Linq;

    public static class TypeConverters
    {
        /// <summary>
        /// The culture information
        /// </summary>
        public static readonly CultureInfo Culture = new CultureInfo("nl-NL", true);

        /// <summary>
        /// return default value for integer if the object is null
        /// </summary>
        /// <param name="i">Input object.</param>
        /// <returns>Interger conversion.</returns>
        public static int NotNullInteger(object i)
        {
            try
            {
                if ((i == null) || (i.ToString() == string.Empty))
                {
                    return 0;
                }
                else
                {
                    return Convert.ToInt32(i, CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// return default value for decimal if the object is null
        /// </summary>
        /// <param name="d">Input object.</param>
        /// <returns>Decimal conversion.</returns>
        public static decimal NotNullDecimal(object d)
        {
            try
            {
                if ((d == null) || (d.ToString() == string.Empty))
                {
                    return 0;
                }
                else
                {
                    return Convert.ToDecimal(d, CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// return empty string if the object is null
        /// </summary>
        /// <param name="s">Input object.</param>
        /// <returns>String conversion.</returns>
        public static string NotNullString(object s)
        {
            try
            {
                if ((s == null) || (s.ToString() == string.Empty))
                {
                    return string.Empty;
                }
                else
                {
                    return s.ToString();
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// return empty string if the object is null or 0
        /// </summary>
        /// <param name="s">Input object.</param>
        /// <param name="noZero">Validate zero or not.</param>
        /// <returns>String conversion.</returns>
        public static string NotNullString(object s, bool noZero)
        {
            if ((s == null) || (s.ToString() == string.Empty))
            {
                return string.Empty;
            }
            else
            {
                if (noZero && (Convert.ToDecimal(s) == 0))
                {
                    return string.Empty;
                }
                else
                {
                    return s.ToString();
                }
            }
        }

        /// <summary>
        /// Converts a datarow column value to datetime. Returns MinValue when date is null.
        /// </summary>
        /// <param name="s">Input value.</param>
        /// <returns>Output datetime.</returns>
        public static DateTime NotNullDateTime(object s)
        {
            DateTime? outDate = s as DateTime?;

            return outDate != null ? outDate.Value : DateTime.MinValue;
        }

        /// <summary>
        /// Parses a string and returns the DateTime representation. If the string is null or the parse failed it will return DateTime.MinValue.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <returns>The parsed DateTime object.</returns>
        public static DateTime ParseDateTimeFromString(object s)
        {
            string dateString = s as string;
            if (dateString == null)
            {
                return DateTime.MinValue;
            }

            DateTime date = DateTime.MinValue;
            DateTime.TryParse(dateString, Culture, DateTimeStyles.None, out date);
            return date;
        }

        /// <summary>
        /// Returns the nullable datetime representation of the object.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns>
        /// the nullable datetime representation of the object.
        /// </returns>
        public static DateTime? NullableDateTime(object o)
        {
            if (o == DBNull.Value)
            {
                return null;
            }

            DateTime date;

            if (o is DateTime)
            {
                return (DateTime)o;
            }

            if (DateTime.TryParse(o.ToString(), Culture, DateTimeStyles.None, out date))
            {
                return date;
            }

            return null;
        }

        /// <summary>
        /// Nullables the bool.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
        public static bool? NullableBool(object o)
        {
            if (o == DBNull.Value)
            {
                return null;
            }

            bool val;
            if (bool.TryParse(o.ToString(), out val))
            {
                return (bool?)val;
            }

            return null;
        }

        /// <summary>
        /// Returns the nullable char representation of the object.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns>the nullable char representation of the object.</returns>
        public static char? NullableChar(object o)
        {
            if (string.Empty.Equals(o))
            {
                return null;
            }

            if (o is string)
            {
                return o.ToString().FirstOrDefault();
            }

            return o == DBNull.Value ? null : (char?)o;
        }

        /// <summary>
        /// Returns the nullable integer representation of the object.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns>the nullable integer representation of the object.</returns>
        public static int? NullableInteger(object o)
        {
            if (o == DBNull.Value)
            {
                return null;
            }

            int integer;
            if (int.TryParse(o.ToString(), out integer))
            {
                return (int?)integer;
            }

            return null;
        }

        /// <summary>
        /// Nullables the short.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
        public static short? NullableShort(object o)
        {
            if (o == DBNull.Value)
            {
                return null;
            }

            short value;
            if (short.TryParse(o.ToString(), out value))
            {
                return (short?)value;
            }

            return null;
        }

        /// <summary>
        /// Returns the nullable decimal representation of the object.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns>the nullable decimal representation of the object.</returns>
        public static decimal? NullableDecimal(object o)
        {
            if (o == DBNull.Value)
            {
                return null;
            }

            decimal dec;
            if (decimal.TryParse(o.ToString(), out dec))
            {
                return (decimal?)dec;
            }

            return null;
        }
    }
}
