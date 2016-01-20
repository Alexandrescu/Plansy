using System.ComponentModel.DataAnnotations;
using MaProgramez.Resources;

namespace MaProgramez.Repository.Entities
{
    public enum ApplicationTypes
    {
        JavaScript = 0,
        NativeConfidential = 1
    };

    public enum AddressType
    {
        InvoiceAddress = 1,
        PlaceOfBusinessAddress = 2
    };

    public enum Day
    {
        Luni = 1,
        Marti = 2,
        Miercuri = 3,
        Joi = 4,
        Vineri = 5,
        Sambata = 6,
        Duminica = 0
    };

    public enum NotificationType
    {
        [Display(Description = "NotificationType_SystemAlert", ResourceType = typeof(Resource))]
        SystemAlert = 1,

        [Display(Description = "NotificationType_Advertisement", ResourceType = typeof(Resource))]
        Advertisement = 2,

        [Display(Description = "NotificationType_Confirmation", ResourceType = typeof(Resource))]
        Confirmation = 3,

        [Display(Description = "NotificationType_Cancelation", ResourceType = typeof(Resource))]
        Cancelation = 4,

        [Display(Description = "NotificationType_ReSchedule", ResourceType = typeof(Resource))]
        ReSchedule = 5,

        [Display(Description = "NotificationType_NewSchedule", ResourceType = typeof(Resource))]
        NewSchedule = 6,

        [Display(Description = "NotificationType_Reminder", ResourceType = typeof(Resource))]
        Reminder = 7
    };

    public enum InvoiceState
    {
        Open = 1,
        Valid = 2,
        Storno = 3,
        Cancelled = 4,
    };

    public enum PaymentMethod
    {
        Ramburs = 1,
        Card,
        ViramentBancar,
        Numerar
    };

    public enum ScheduleState
    {
        Pending = 1,
        Valid = 2,
        CancelledByUser = 3,
        CancelledByProvider = 4,
    };

    public enum ConfirmationType
    {
        Success = 1,
        Warning,
        Error
    };

  
}