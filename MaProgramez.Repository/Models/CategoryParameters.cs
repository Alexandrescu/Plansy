namespace MaProgramez.Repository.Models
{
    /// <summary>
    /// Used as a parameter for all Category related API metods
    /// </summary>
    public class CategoryParameters
    {
        #region PROPERTIES
        
        public int CategoryId { get; set; }

        public int? ParentCategoryId { get; set; }

        public int? CityId { get; set; }
        
        public string SearchText { get; set; }

        #endregion

        #region CONSTRUCTOR

        public CategoryParameters()
        {

        }

        #endregion
    }
}