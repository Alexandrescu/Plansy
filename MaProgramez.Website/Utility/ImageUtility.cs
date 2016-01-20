namespace MaProgramez.Website.Utility
{
    using ImageResizer;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Helpers;

    public class ImageUtility
    {
        #region PRIVATE FIELDS

        private static string[] validImageTypes = new string[]
                            {
                                "image/gif",
                                "image/jpeg",
                                "image/pjpeg",
                                "image/png"
                            };

        #endregion

        #region PUBLIC STATIC FIELDS

        public static int TotalOfAllowedImages = 10;

        #endregion

        #region PUBLIC STATIC METHODS

        public static bool AreUploadedImagesValid(IEnumerable<HttpPostedFileBase> files)
        {
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (file != null)
                    {
                        if (file.ContentLength == 0 || !CheckFileSize(file))
                        {
                            return false;
                        }

                        if (!validImageTypes.Contains(file.ContentType))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public static bool IsImageValid(HttpPostedFileBase file)
        {
            if (file != null)
            {
                if (file.ContentLength == 0)
                {
                    return false;
                }

                if (!validImageTypes.Contains(file.ContentType))
                {
                    return false;
                }
            }

            return true;
        }

        public static string SaveImage(HttpPostedFileBase file, string fileName, string folderName, string version, bool addWatermark = true)
        {
            if (!string.IsNullOrWhiteSpace(fileName) && !string.IsNullOrWhiteSpace(folderName)
                && file != null && file.ContentLength > 0 && IsImageValid(file))
            {
                var directory = HttpContext.Current.Server.MapPath("~/Images/" + folderName + "");
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }

                var imageVersion = string.Empty;
                if (!string.IsNullOrWhiteSpace(version))
                {
                    switch (version)
                    {
                        case "thumb":
                            imageVersion = "maxwidth=300&maxheight=300";
                            break;
                        case "small":
                            imageVersion = "maxwidth=600&maxheight=600";
                            break;
                        case "medium":
                            imageVersion = "maxwidth=900&maxheight=900";
                            break;
                        case "large":
                            imageVersion = "maxwidth=1200&maxheight=1200";
                            break;
                        default:
                            imageVersion = "maxwidth=900&maxheight=900";
                            break;
                    }

                    var path = Path.Combine(directory, fileName);
                    ImageBuilder.Current.Build(new ImageJob(file.InputStream, path, new Instructions(imageVersion), false, false));

                    if (addWatermark)
                    {
                        AddWatermark(path);
                    }

                    //file.SaveAs(path);
                    return string.Format("/Images/{1}/{2}", "Images", folderName, fileName);
                }
            }

            return string.Empty;
        }


        public static bool RemoveImage(string folder, string fileName)
        {
            string sourcePath = Path.Combine(HttpContext.Current.Server.MapPath("~/Images/" + folder + ""), fileName);
            if (System.IO.File.Exists(sourcePath))
            {
                System.IO.File.Delete(sourcePath);

                return true;
            }

            return false;
        }

        public static bool CheckFileSize(HttpPostedFileBase file)
        {
            // 8 MB
            if (file.ContentLength <= 8 * 1024 * 1024)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region PRIVATE METHODS

        private static void AddWatermark(string path)
        {
            var image = new WebImage(path);
            var watermark = System.Web.HttpContext.Current.Server.MapPath("~/Images/Watermark/watermark.png");
            image.AddImageWatermark(watermark, 100, 100, "Left", "Top", 80, 5);
            image.Save(path);
        }

        #endregion
    }
}