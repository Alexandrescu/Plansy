using MaProgramez.Repository.Entities;

namespace MaProgramez.Website.ViewModels
{
    public class ConfirmationViewModel
    {
        #region PROPERTIES

        public string Title { get; set; }

        public string Message { get; set; }

        public string Link { get; set; }

        public ConfirmationType Type { get; set; }

        #endregion
    }
}