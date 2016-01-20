using System.Collections.Generic;
using System.Linq;
using System.Web;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Website.Utility
{
    public static class SessionUtility
    {
        #region SESSIONS

        public static string OfferImages = "OfferImages";

        public static string Request = "Request";

        public static string RequestId = "RequestId";

        public static string UserCarsFlag = "UserCarsFlag";

        public static string SaveCarFlag = "SaveCarFlag";

        public static string Addresses = "Addresses";

        #endregion

        #region PUBLIC STATIC METHODS
        
        public static Address AddAddresses(Address address)
        {
            if (address != null)
            {
                if (HttpContext.Current.Session[SessionUtility.Addresses] == null)
                {
                    address.Id = 0;
                    address.AddressType = AddressType.InvoiceAddress;
                    if (address.CountryId == 0)
                    {
                        address.CountryId = 1; //Default country = ROMANIA
                    }
                    var list = new List<Address> { address };
                    HttpContext.Current.Session[SessionUtility.Addresses] = list;
                }
                else
                {
                    var list = HttpContext.Current.Session[SessionUtility.Addresses] as List<Address>;
                    address.Id = list.Count();
                    address.AddressType = list.Any() ? AddressType.PlaceOfBusinessAddress : AddressType.InvoiceAddress;
                    if (address.CountryId == 0)
                    {
                        address.CountryId = 1; //Default country = ROMANIA
                    }

                    list.Add(address);
                    HttpContext.Current.Session[SessionUtility.Addresses] = list;
                }

                return address;
            }

            return null;
        }

        public static Address UpdateAddress(Address address)
        {
            if (HttpContext.Current.Session[SessionUtility.Addresses] != null && address != null)
            {
                var list = HttpContext.Current.Session[SessionUtility.Addresses] as List<Address>;
                var oldAddress = list.FirstOrDefault(x => x.Id == address.Id);
                oldAddress.AddressText = address.AddressText;
                oldAddress.PostalCode = address.PostalCode;
                oldAddress.CityId = address.CityId;

                if (address.CountryId == 0)
                {
                    oldAddress.CountryId = 1; //default country = ROMANIA
                }

                HttpContext.Current.Session[SessionUtility.Addresses] = list;
                return oldAddress;
            }

            return null;
        }

        public static void RemoveAddress(int addressId)
        {
            if (HttpContext.Current.Session[SessionUtility.Addresses] != null)
            {
                var list = HttpContext.Current.Session[SessionUtility.Addresses] as List<Address>;
                var oldAddress = list.FirstOrDefault(x => x.Id == addressId);
                if (oldAddress != null)
                {
                    list.Remove(oldAddress);
                    HttpContext.Current.Session[SessionUtility.Addresses] = list;
                }
            }
        }

        public static List<Address> GetAddresses()
        {
            if (HttpContext.Current.Session[SessionUtility.Addresses] != null)
            {
                var list = HttpContext.Current.Session[SessionUtility.Addresses] as List<Address>;
                return list;
            }

            return new List<Address>();
        }

        public static void ClearAddresses()
        {
            if (HttpContext.Current.Session[SessionUtility.Addresses] != null)
            {
                ClearSession(SessionUtility.Addresses);
            }
        }

        #endregion

        #region PRIVATE FIELDS

        private static void ClearSession(string sessionName)
        {
            HttpContext.Current.Session.Contents.Remove(sessionName);
        }

        #endregion
    }
}