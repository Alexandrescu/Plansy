using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Microsoft.Ajax.Utilities;

namespace MaProgramez.Website.Helpers
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Creates a dropdown list for an Enum property
        /// </summary>
        /// <exception cref="System.ArgumentException">If the property type is not a valid Enum</exception>
        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> html, Expression<Func<TModel, TEnum>> expression, string emptyItemText = "", string emptyItemValue = "", object htmlAttributes = null)
        {
            return html.DropDownListFor(expression, GetEnumSelectList<TEnum>(true, emptyItemText, emptyItemValue), htmlAttributes);
        }

        /// <summary>
        /// Creates a checkbox list for an Enum property
        /// </summary>
        public static MvcHtmlString EnumCheckBoxListFor<TModel, TEnum>(this HtmlHelper<TModel> html, Expression<Func<TModel, IEnumerable<TEnum>>> expression, object htmlAttributes = null)
        {
            return html.CheckBoxListFor(expression, GetEnumSelectList<TEnum>(), htmlAttributes);
        }

        /// <summary>
        /// Returns a checkbox for each of the provided <paramref name="items"/>.
        /// </summary>
        public static MvcHtmlString CheckBoxListFor<TModel, TValue>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TValue>> expression, IEnumerable<SelectListItem> items, object htmlAttributes = null)
        {
            var listName = ExpressionHelper.GetExpressionText(expression);
            var metaData = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

            // items = GetCheckboxListWithDefaultValues(metaData.Model, items);
            return htmlHelper.CheckBoxList(listName, items, htmlAttributes);
        }

        /// <summary>
        /// Returns a checkbox for each of the provided <paramref name="items"/>.
        /// </summary>
        public static MvcHtmlString CheckBoxList(this HtmlHelper htmlHelper, string listName, IEnumerable<SelectListItem> items, object htmlAttributes = null)
        {
            var container1 = new TagBuilder("div");
            container1.MergeAttribute("class", "col-md-12");

            foreach (var item in items)
            {
                var container2 = new TagBuilder("div");
                container2.MergeAttribute("class", "col-md-3");

                var box = new TagBuilder("div");
                box.MergeAttribute("class", item.Selected ? "car-pref-box checked" : "car-pref-box");

                var label = new TagBuilder("label");
                label.MergeAttribute("class", "checkbox"); // default class
                label.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);
                label.SetInnerText(item.Text);

                var cb = new TagBuilder("input");
                cb.MergeAttribute("type", "checkbox");
                cb.MergeAttribute("name", listName);
                cb.MergeAttribute("value", item.Value ?? item.Text);
                if (item.Selected)
                    cb.MergeAttribute("checked", "checked");

                var img = new TagBuilder("img");
                img.MergeAttribute("src", "/Content/images/" + Enum.GetName(typeof(Repository.Entities.Day), int.Parse(item.Value)) + ".png");
                img.MergeAttribute("width", "48");
                img.MergeAttribute("height", "48");

                box.InnerHtml += cb.ToString(TagRenderMode.SelfClosing) + img.ToString(TagRenderMode.SelfClosing) + label;


                container2.InnerHtml = box.ToString();
                container1.InnerHtml += container2.ToString();
            }

            return new MvcHtmlString(container1.ToString());
        }

        public static MvcHtmlString TextCheckBoxListFor<TModel, TValue>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TValue>> expression, IEnumerable<SelectListItem> items, object htmlAttributes = null)
        {
            var listName = ExpressionHelper.GetExpressionText(expression);
            var metaData = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

            // items = GetCheckboxListWithDefaultValues(metaData.Model, items);
            return htmlHelper.TextCheckBoxList(listName, items, htmlAttributes);
        }

        /// <summary>
        /// Returns a checkbox for each of the provided <paramref name="items"/>.
        /// </summary>
        public static MvcHtmlString TextCheckBoxList(this HtmlHelper htmlHelper, string listName, IEnumerable<SelectListItem> items, object htmlAttributes = null)
        {
            var container1 = new TagBuilder("div");
            container1.MergeAttribute("class", "col-md-12");

            foreach (var item in items)
            {
                var container2 = new TagBuilder("div");
                container2.MergeAttribute("class", "col-md-3");

                var box = new TagBuilder("div");
                box.MergeAttribute("class", item.Selected ? "oferta checked" : "oferta");

                char[] delimiterChars = {';'};
                string[] words = item.Text.Split(delimiterChars);

                var label1 = new TagBuilder("label");
                label1.MergeAttribute("class", "checkbox"); // default class
                label1.MergeAttribute("style", "font-weight: bold");
                label1.MergeAttribute("style", "font-size: 20px");
                label1.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);
                label1.SetInnerText(words[2]);

                var label2 = new TagBuilder("label");
                label2.MergeAttribute("class", "checkbox"); // default class
                label2.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);
                label2.InnerHtml = "<span class=\"fa fa-money\"></i> " + words[1] + " RON";

                var label3 = new TagBuilder("label");
                label3.MergeAttribute("class", "checkbox"); // default class
                label3.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);
                label3.InnerHtml = "<span class=\"glyphicon glyphicon-time\"></i> " + words[0] + " minute";

                var cb = new TagBuilder("input");
                cb.MergeAttribute("type", "checkbox");
                cb.MergeAttribute("name", listName);
                cb.MergeAttribute("value", item.Value ?? item.Text);
                if (item.Selected)
                    cb.MergeAttribute("checked", "checked");     

                box.InnerHtml += cb.ToString(TagRenderMode.SelfClosing) + label1 + label2 + label3;


                container2.InnerHtml = box.ToString();
                container1.InnerHtml += container2.ToString();
            }

            container1.Attributes.Add("onclick", "ChangeCheckBox();");

            return new MvcHtmlString(container1.ToString());
        }

        private static IEnumerable<SelectListItem> GetEnumSelectList<TEnum>(bool addEmptyItemForNullable = false, string emptyItemText = "", string emptyItemValue = "")
        {
            var enumType = typeof(TEnum);
            var nullable = false;

            if (!enumType.IsEnum)
            {
                enumType = Nullable.GetUnderlyingType(enumType);

                if (enumType != null && enumType.IsEnum)
                {
                    nullable = true;
                }
                else
                {
                    throw new ArgumentException("Not a valid Enum type");
                }
            }

            var selectListItems = (from v in Enum.GetValues(enumType).Cast<TEnum>()
                                   select new SelectListItem
                                   {
                                       Text = EnumHelpers.GetDescription(enumType, v.ToString()),
                                       Value = v.ToString()
                                   }).ToList();

            if (nullable && addEmptyItemForNullable)
            {
                selectListItems.Insert(0, new SelectListItem { Text = emptyItemText, Value = emptyItemValue });
            }

            return selectListItems;
        }

        private static IEnumerable<SelectListItem> GetCheckboxListWithDefaultValues(object defaultValues, IEnumerable<SelectListItem> selectList)
        {
            var defaultValuesList = defaultValues as IEnumerable;

            if (defaultValuesList == null)
                return selectList;

            IEnumerable<string> values = from object value in defaultValuesList
                                         select Convert.ToString(value, CultureInfo.CurrentCulture);

            var selectedValues = new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
            var newSelectList = new List<SelectListItem>();

            selectList.ForEach(item =>
            {
                item.Selected = (item.Value != null) ? selectedValues.Contains(item.Value) : selectedValues.Contains(item.Text);
                newSelectList.Add(item);
            });

            return newSelectList;
        }
    }
}