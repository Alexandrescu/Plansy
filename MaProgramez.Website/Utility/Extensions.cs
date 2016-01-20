// ReSharper disable CheckNamespace
namespace System
// ReSharper restore CheckNamespace
{
    using Linq;
    using Collections.Generic;
    using Collections.ObjectModel;
    using MaProgramez.Website.Utility;

    public static class Extensions
    {
        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="enumerable">The enumerable.</param>
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> enumerable)
        {
            foreach (var entry in enumerable)
            {
                collection.Add(entry);
            }
        }

        /// <summary>
        /// Returns the nullable datetime representation of the object.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns>the nullable datetime representation of the object.</returns>
        public static DateTime? ToNullableDateTime(this object o)
        {
            return TypeConverters.NullableDateTime(o);
        }

        /// <summary>
        /// Toes the nullable bool.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
        public static bool? ToNullableBool(this object o)
        {
            return TypeConverters.NullableBool(o);
        }

        /// <summary>
        /// Returns the nullable char representation of the object.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns>the nullable char representation of the object.</returns>
        public static char? ToNullableChar(this object o)
        {
            return TypeConverters.NullableChar(o);
        }

        /// <summary>
        /// Returns the nullable integer representation of the object.
        /// </summary>
        /// <param name="o">The object.</param>
        /// <returns>the nullable integer representation of the object.</returns>
        public static int? ToNullableInteger(this object o)
        {
            return TypeConverters.NullableInteger(o);
        }

        /// <summary>
        /// Returns the integer representation of the object.
        /// </summary>
        /// <param name="o">The object.</param>
        /// <returns></returns>
        public static int ToInteger(this object o)
        {
            return TypeConverters.NotNullInteger(o);
        }

        /// <summary>
        /// Returns the decimal representation of the object.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
        public static decimal ToDecimal(this object o)
        {
            return TypeConverters.NotNullDecimal(o);
        }

        /// <summary>
        /// Returns the nullable integer representation of the object.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns>The nullable short representation of the object.</returns>
        public static short? ToNullableShort(this object o)
        {
            return TypeConverters.NullableShort(o);
        }

        /// <summary>
        /// Returns the nullable decimal representation of the object.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns>the nullable decimal representation of the object.</returns>
        public static decimal? ToNullableDecimal(this object o)
        {
            return TypeConverters.NullableDecimal(o);
        }

        /// <summary>
        /// Checks if the provided item is contained by the items array.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="item">The item.</param>
        /// <param name="items">The items array.</param>
        /// <returns><c>true</c> if the item is contained in the array; otherwise <c>false</c></returns>
        public static bool In<T>(this T item, params T[] items)
        {
            return items.Contains(item);
        }
    }
}
