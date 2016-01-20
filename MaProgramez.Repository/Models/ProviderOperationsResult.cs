using System;
using System.Collections.Generic;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Repository.Models
{
    public class ProviderOperationsResult
    {
        public ProviderOperationsResult()
        {
            AvailableOperations = new List<Operation>();
            AvailableSlots = new List<Slot>();
            AvailableHours = new List<int>();
            AvailableMinutesForSelectedHour = new List<int>();
            SelectedOperationIds = new List<int>();
        }

        //TODO: fill in with slot or provider address if not programming per slot or slot has no address (no user)
        public Address DisplayAddress { get; set; }
        
        public List<Operation> AvailableOperations { get; set; }

        public List<Slot> AvailableSlots { get; set; }

        public string FirstAvailableDate { get; set; }

        public List<int> AvailableHours { get; set; }
        
        public List<int> AvailableMinutesForSelectedHour { get; set; }

        #region Properties set into screens (to be sent back)

        public int SelectedHour { get; set; }

        public List<int> SelectedOperationIds { get; set; }

        #endregion

        public string Mesaj { get; set; }
    }
}