using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Repository.Models;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Http;

namespace MaProgramez.Api.Controllers
{
    [RoutePrefix("api/Provider")]
    public class ProviderController : ApiController
    {
        #region Public Constructors

        public ProviderController()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-NL");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("nl-NL");
        }

        #endregion Public Constructors

        #region Public Methods

        // Pass either the provider id either the slot id
        [Authorize]
        [Route("AddProviderOrSlotToFavourites")]
        [HttpPost]
        public void AddProviderOrSlotToFavourites(FavoriteViewModel favorite)
        {
            RetrieveOthers.AddToFavorites(favorite.UserId, favorite.ProviderId, favorite.SlotId);
        }

        [Authorize]
        [Route("GetFavoriteProviders")]
        [HttpPost]
        public IHttpActionResult GetFavoriteProviders(ProviderParameters provider)
        {
            return Ok(RetrieveLists.GetFavoriteProviders(provider.UserId));
        }

        [Authorize]
        [Route("GetProvider")]
        [HttpPost]
        public IHttpActionResult GetProvider(ProviderParameters providerParameters)
        {
            // load full provider - param bool false initial. Daca e true => incarca si categorii si
            // sloturi de adus si lista de categorii ale providerului, Pe load full provider = false
            // sa aduca si adresa cu lat/lon si nume

            var fullProviderDetails = new FullProviderDetails
            {
                Provider = RetrieveOthers.GetProviderById(providerParameters.ProviderId)
            };

            if (providerParameters.FullyLoadProvider)
            {
                fullProviderDetails.ProviderOperations =
                        RetrieveLists.GetSlotOperationsByProvider(providerParameters.ProviderId)
                        .Select(po => po.Operation)
                        .ToList();

                fullProviderDetails.ProviderSlots = RetrieveLists.GetSlotsByProvider(providerParameters.ProviderId, providerParameters.CategoryId);

                fullProviderDetails.ProviderCategories =
                     fullProviderDetails.ProviderSlots
                    .Select(ps => ps.Category)
                    .Distinct()
                    .ToList();
            }

            return Ok(fullProviderDetails);
        }

        [Authorize]
        [Route("GetProviderOperations")]
        [HttpPost]
        public IHttpActionResult GetProviderOperations(ProviderParameters providerParameters)
        {
            var result = RetrieveLists.GetProviderSlotOperations(
                providerParameters.ProviderId,
                providerParameters.CategoryId,
                providerParameters.SelectedOperations,
                providerParameters.SelectedDate,
                providerParameters.SelectedHour,
                providerParameters.SelectedSlotId,
                providerParameters.UserId);

            return Ok(result);
            //adauga selected operations la param (lista de int), date, hh, mm selectate, slotId
            //return: lista de ops, data, lista de ore disp, lista de min pt prima ora
        }

        [AllowAnonymous]
        [Route("GetProviders")]
        [HttpPost]
        public IHttpActionResult GetProviders(ProviderParameters providerParameters)
        {
            return Ok(RetrieveLists.GetProvidersFiltered(providerParameters.CategoryId,
                                                         providerParameters.CityId,
                                                         providerParameters.Page,
                                                         providerParameters.PageSize,
                                                         providerParameters.SearchText
                                                         ));
        }

        [Authorize]
        [Route("HasProgrammingPerSlot")]
        [HttpPost]
        public IHttpActionResult HasProgrammingPerSlot(string providerId)
        {
            return Ok(RetrieveOthers.HasProgrammingPerSlot(providerId));
        }

        [Authorize]
        [Route("IsFavorite")]
        [HttpPost]
        public IHttpActionResult IsFavorite(FavoriteViewModel favorite)
        {
            string userId = favorite.UserId;
            int slotId = favorite.SlotId.HasValue ? favorite.SlotId.Value : 0;
            string providerId = favorite.ProviderId;

            return Ok(RetrieveOthers.IsFavourite(userId, slotId, providerId));
        }

        [Authorize]
        [Route("RemoveProviderOrSlotToFavorites")]
        [HttpPost]
        public void RemoveProviderOrSlotToFavorites(FavoriteViewModel favorite)
        {
            RetrieveOthers.RemoveFavorite(favorite.UserId, favorite.ProviderId, favorite.SlotId);
        }

        #endregion Public Methods
    }
}