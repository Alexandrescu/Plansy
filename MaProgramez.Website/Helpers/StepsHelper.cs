using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MaProgramez.Website.Helpers
{
    public static class StepsHelper
    {
        public static MvcHtmlString Steps(int numberOfSteps, int currentStep)
        {
            var result = string.Empty;
            result += "<div class=\"steps\">";
            result += "<ul>";


            if (numberOfSteps > 0 && currentStep > 0)
            {
                if (currentStep < numberOfSteps)
                {
                    for (var i = 1; i <= numberOfSteps - 1; i++)
                    {
                        if (currentStep == i)
                        {
                            result += string.Format("<li class=\"steps-current\">{0}</li>", i);
                        }
                        else if (i < currentStep)
                        {
                            result += "<li class=\"steps-completed\"><i class=\"fa fa-check\"></i></li>";
                        }
                        else
                        {
                            result += string.Format("<li>{0}</li>", i);
                        }

                        result += "<li class=\"additional-step\">&nbsp;</li><li class=\"additional-step\">&nbsp;</li><li class=\"additional-step\">&nbsp;</li>";
                    }

                    result += string.Format("<li>{0}</li>", numberOfSteps);
                }
                else
                {
                    for (var i = 1; i <= numberOfSteps - 1; i++)
                    {
                        result += "<li class=\"steps-completed\"><i class=\"fa fa-check\"></i></li>";
                        result += "<li class=\"additional-step\">&nbsp;</li><li class=\"additional-step\">&nbsp;</li><li class=\"additional-step\">&nbsp;</li>";
                    }

                    result += string.Format("<li class=\"steps-current\">{0}</li>", numberOfSteps);
                }
            }

            result += "</ul>";
            result += " <div class=\"clear\"></div>";
            result += "</div>";
            result += " <div class=\"clear\"></div>";

            return new MvcHtmlString(result);
        }
    }
}