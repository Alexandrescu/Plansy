using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MaProgramez.Website.Helpers
{
    public class TableHelper
    {
        public static string GetTableHeaderIcon(string sortBy, string column, bool asceding)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return "fa-sort";
            }

            if (!sortBy.Equals(column))
            {
                return string.Empty;
            }

            return asceding ? "fa fa-sort-desc" : "fa fa-sort-asc";
        }
    }
}