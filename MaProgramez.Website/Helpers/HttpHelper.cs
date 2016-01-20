
namespace MaProgramez.Website.Utility
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Text;

    public static class HttpHelper
    {
        public static String PreparePOSTForm(string url, NameValueCollection data)
        {
            string formID = "PostForm";
            var strForm = new StringBuilder();
            strForm.Append("<form id=\"" + formID + "\" name=\"" + formID + "\" action=\"" + url + "\" method=\"POST\">");
            foreach (string key in data)
            {
                strForm.Append("<input type=\"hidden\" name=\"" + key + "\" value=\"" + data[key] + "\">");
            }
            strForm.Append("</form>");
            var strScript = new StringBuilder();
            strScript.Append("<script language='javascript'>");
            strScript.Append("var v" + formID + " = document." + formID + ";");
            strScript.Append("v" + formID + ".submit();");
            strScript.Append("</script>");
            return strForm.ToString() + strScript.ToString();
        }

        public static void RedirectAndPOST(string destinationUrl, NameValueCollection data)
        {
            string strForm = PreparePOSTForm(destinationUrl, data);
           
            var req = (HttpWebRequest)HttpWebRequest.Create(destinationUrl);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.CookieContainer = new CookieContainer();

            byte[] postByteArray = Encoding.UTF8.GetBytes(strForm);
            req.ContentLength = postByteArray.Length;
            Stream postStream = req.GetRequestStream();
            postStream.Write(postByteArray, 0, postByteArray.Length);
            postStream.Close();

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            //page.Controls.Add(new LiteralControl(strForm));
        }

        //public static void RedirectAndPOST(Page page, string destinationUrl, NameValueCollection data)
        //{
        //    string strForm = PreparePOSTForm(destinationUrl, data);
        //    page.Controls.Add(new LiteralControl(strForm));
        //}

        public static string SendPostRequest(string data, string url)
        {

            //Data parameter Example
            //string data = "name=" + value

            var httpRequest = HttpWebRequest.Create(url);
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/x-www-form-urlencoded";
            httpRequest.ContentLength = data.Length;

            var streamWriter = new StreamWriter(httpRequest.GetRequestStream());
            streamWriter.Write(data);
            streamWriter.Close();

            var response = (HttpWebResponse)httpRequest.GetResponse();
            var datastream = response.GetResponseStream();
            
            using (var reader = new StreamReader(datastream))
            {
                return reader.ReadToEnd();
            }         
        }

        public static byte[] SendPostRequestByteArray(string data, string url)
        {
            var httpRequest = HttpWebRequest.Create(url);
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/x-www-form-urlencoded";
            httpRequest.ContentLength = data.Length;

            var streamWriter = new StreamWriter(httpRequest.GetRequestStream());
            streamWriter.Write(data);
            streamWriter.Close();

            var response = (HttpWebResponse)httpRequest.GetResponse();
            var datastream = response.GetResponseStream();

            return ReadToEnd(datastream);
        }

        public static string SendPostWithFileRequest(string postData, string url)
        {
            //Data parameter Example
            //string data = "name=" + value

            var request = WebRequest.Create(url);
            request.Method = "POST";
            byte[] byteArray = Encoding.ASCII.GetBytes(postData);
            
            //Set the headers
            /*
            var w = new WebHeaderCollection
            {
                "User-Agent: Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.7.7) Gecko/20050414 Firefox/1.0.3",
                "Accept: text/xml,application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/ /*;q=0.5",
                "Accept-Language: en-us,en;q=0.5",
                "Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.7"
            };
            request.Headers = w;
             */
            // Set the ContentType property of the WebRequest.
            request.ContentType = "multipart/form-data; boundary=AaB03x";
            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;
            // Get the request stream.
            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.
            dataStream.Close();
            // Get the response.
            var response = request.GetResponse();
            // Display the status.
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            var reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            Console.WriteLine(responseFromServer);
            // Clean up the streams.
            reader.Close();
            dataStream.Close();
            response.Close();

            return responseFromServer;


            //var httpRequest = HttpWebRequest.Create(url);
            //httpRequest.Method = "POST";
            ////httpRequest.ContentType = "application/x-www-form-urlencoded";
            //httpRequest.ContentType = "multipart/form-data";
            ////httpRequest.Headers.Add("Content-Type: multipart/form-data");
            //httpRequest.ContentLength = data.Length;

            //var streamWriter = new StreamWriter(httpRequest.GetRequestStream());
            //streamWriter.Write(data);
            //streamWriter.Close();

            //return (HttpWebResponse)httpRequest.GetResponse();
        }

        public static string PostToExternalUrl(string url, NameValueCollection parameters)
        {
            using(var client = new WebClient())
            {
                byte[] responsebytes = client.UploadValues(url, "POST", parameters);
                string responsebody = Encoding.UTF8.GetString(responsebytes);
                return responsebody;
            }
        }

        public static HttpWebResponse SendGetRequest(string url)
        {

            var httpRequest = HttpWebRequest.Create(url);
            httpRequest.Method = "GET";

            return (HttpWebResponse)httpRequest.GetResponse();
        }

        public static byte[] ReadToEnd(Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }
    }
}