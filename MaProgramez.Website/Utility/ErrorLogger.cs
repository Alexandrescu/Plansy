namespace MaProgramez.Website.Utility
{
    using Elmah;
    using System;
    using System.Web;

    public static class ErrorLogger
    {
        /// <summary>
        /// Log error to Elmah
        /// </summary>
        public static void LogError(Exception ex, string contextualMessage = null)
        {
            try
            {
                // log error to Elmah
                if (contextualMessage != null)
                {
                    // log exception with contextual information that's visible when
                    // clicking on the error in the Elmah log
                    var annotatedException = ex == null ? new Exception(contextualMessage) : new Exception(contextualMessage, ex);
                    ErrorSignal.FromCurrentContext().Raise(annotatedException, HttpContext.Current);
                }
                else
                {
                    ErrorSignal.FromCurrentContext().Raise(ex, HttpContext.Current);
                }
            }
            catch
            {
                // uh oh! just keep going
            }
        }
    }
}