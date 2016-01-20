using MaProgramez.Api.Results;
using MaProgramez.Repository.BusinessLogic;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;
using MaProgramez.Repository.Models;
using MaProgramez.Repository.Utility;
using MaProgramez.Resources;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace MaProgramez.Api.Controllers
{
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        #region Private Fields

        private readonly AuthRepository _repo;

        #endregion Private Fields

        #region Public Constructors

        public AccountController()
        {
            _repo = new AuthRepository();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-NL");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("nl-NL");
        }

        #endregion Public Constructors

        #region Private Properties

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        #endregion Private Properties

        #region Public Methods

        [AllowAnonymous]
        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordParameters changePasswordParameters)
        {
            return Ok(await _repo.ChangePassword(
                    changePasswordParameters.UserId,
                    changePasswordParameters.OldPassword,
                    changePasswordParameters.NewPassword,
                    changePasswordParameters.ConfirmPassword
                ));
        }

        [AllowAnonymous]
        [Route("ForgotPassword")]
        [HttpPost]
        public async Task<IHttpActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ApplicationUser user = await _repo.FindByNameAsync(model.Email);
                    if (user == null || !(await _repo.IsEmailConfirmedAsync(user.Id)))
                    {
                        // Don't reveal that the user does not exist or is not confirmed
                        return Ok(Resource.ForgotPasswordConfirmationView_Content);
                    }

                    string code = await _repo.GeneratePasswordResetTokenAsync(user.Id);
                    string callbackUrl = Common.GetDbConfig("SiteUrl") + "/ro/Account/ResetPassword?userId=" + user.Id +
                                         "&code=" + code;
                    string mailContent = string.Concat(Resource.ForgotPasswordEmail_Content,
                        " <a href=\"", callbackUrl, "\">Reset</a>");

                    var message = new IdentityMessage()
                    {
                        Destination = model.Email,
                        Body = mailContent,
                        Subject = Resource.ResetPassword_Subject,
                    };

                    await MailAndSmsUtility.SendMailAsync(message);

                    return Ok(Resource.ForgotPasswordConfirmationView_Content);
                }
                catch (Exception ex)
                {
                    return Ok(ex.Message);
                }
            }

            // If we got this far, something failed, redisplay form
            return Ok(Resource.Error);
        }

        // GET api/Account/ExternalLogin
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
        [HttpPost]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            string redirectUri = string.Empty;

            if (error != null)
            {
                return BadRequest(Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            string redirectUriValidationResult = ValidateClientAndRedirectUri(Request, ref redirectUri);

            if (!string.IsNullOrWhiteSpace(redirectUriValidationResult))
            {
                return BadRequest(redirectUriValidationResult);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            ApplicationUser user =
                await _repo.FindAsync(new UserLoginInfo(externalLogin.LoginProvider, externalLogin.ProviderKey));

            bool hasRegistered = user != null;

            redirectUri =
                string.Format("{0}#external_access_token={1}&provider={2}&haslocalaccount={3}&external_user_name={4}",
                    redirectUri,
                    externalLogin.ExternalAccessToken,
                    externalLogin.LoginProvider,
                    hasRegistered,
                    externalLogin.UserName);

            return Redirect(redirectUri);
        }

        [Authorize]
        [Route("GetUserDetails")]
        [HttpPost]
        public IHttpActionResult GetUserDetails(UserModel userModel)
        {
            if (string.IsNullOrWhiteSpace(userModel.UserId))
            {
                return Ok("userId lipseste");
            }

            var user = RetrieveOthers.GetUserById(userModel.UserId);

            if (user == null)
            {
                return Ok("userId invalid");
            }

            return Ok(new UserModel()
            {
                Email = user.Email,
                LastName = user.LastName,
                FirstName = user.FirstName,
                Phone = user.PhoneNumber,
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ObtainLocalAccessToken")]
        public async Task<IHttpActionResult> ObtainLocalAccessToken(string provider, string externalAccessToken)
        {
            if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(externalAccessToken))
            {
                return BadRequest("Provider or external access token is not sent");
            }

            ParsedExternalAccessToken verifiedAccessToken =
                await VerifyExternalAccessToken(provider, externalAccessToken);
            if (verifiedAccessToken == null)
            {
                return BadRequest("Invalid Provider or External Access Token");
            }

            ApplicationUser user = await _repo.FindAsync(new UserLoginInfo(provider, verifiedAccessToken.user_id));

            bool hasRegistered = user != null;

            if (!hasRegistered)
            {
                return BadRequest("Userul extern nu este inregistrat");
            }

            //generate access token response
            JObject accessTokenResponse = GenerateLocalAccessTokenResponse(user.UserName);

            return Ok(accessTokenResponse);
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        [HttpPost]
        public async Task<IHttpActionResult> Register(UserModel userModel)
        {
            userModel.Email = userModel.UserName;
                        if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await _repo.RegisterUser(userModel);

            IHttpActionResult errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                return errorResult;
            }

            var user = await _repo.FindByNameAsync(userModel.Email);
            //Send SMS
            await _repo.SendPhoneNumberValidationCode(user.Id, user.PhoneNumber);

            return Ok();
        }

        // POST api/Account/RegisterExternal
        [AllowAnonymous]
        [Route("RegisterExternal")]
        [HttpPost]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ParsedExternalAccessToken verifiedAccessToken =
                await VerifyExternalAccessToken(model.Provider, model.ExternalAccessToken);
            if (verifiedAccessToken == null)
            {
                return BadRequest("Invalid Provider or External Access Token");
            }

            ApplicationUser user = await _repo.FindAsync(new UserLoginInfo(model.Provider, verifiedAccessToken.user_id));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                return BadRequest("Acest user este deja inregistrat");
            }

            user = new ApplicationUser { UserName = model.UserName };

            IdentityResult result = await _repo.CreateAsync(user);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            var info = new ExternalLoginInfo
            {
                DefaultUserName = model.UserName,
                Login = new UserLoginInfo(model.Provider, verifiedAccessToken.user_id)
            };

            result = await _repo.AddLoginAsync(user.Id, info.Login);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            //generate access token response
            JObject accessTokenResponse = GenerateLocalAccessTokenResponse(model.UserName);

            return Ok(accessTokenResponse);
        }

        [Authorize]
        [Route("SaveUserDetails")]
        [HttpPost]
        public async Task<IHttpActionResult> SaveUserDetails(UserModel userModel)
        {
            var user = AccountBusinessLogic.SaveUserDetails(userModel);

            if (!user.PhoneNumberConfirmed)
            {
                await _repo.SendPhoneNumberValidationCode(userModel.UserId, userModel.Phone);
            }

            return Ok(user);
        }

        [Authorize]
        [Route("SendPhoneNumberValidationCode")]
        [HttpPost]
        public async Task<IHttpActionResult> SendPhoneNumberValidationCode(AddPhoneNumberViewModel model)
        {
            // Generate the token and send it
            await _repo.SendPhoneNumberValidationCode(model.UserId, model.Number);
            return Ok();
        }

        [Authorize]
        [Route("VerifyPhoneNumber")]
        [HttpPost]
        public async Task<IHttpActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            var result = await _repo.ChangePhoneNumberAsync(model.UserId, model.PhoneNumber, model.Code);

            if (result.Succeeded)
            {
                RetrieveOthers.ActivateAccount(model.UserId);
                return Ok();
            }

            return BadRequest(Resource.FailedToVerifyCode);
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repo.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion Protected Methods

        #region Private Methods

        private JObject GenerateLocalAccessTokenResponse(string userName)
        {
            TimeSpan tokenExpiration = TimeSpan.FromDays(1);

            var identity = new ClaimsIdentity(OAuthDefaults.AuthenticationType);

            identity.AddClaim(new Claim(ClaimTypes.Name, userName));
            identity.AddClaim(new Claim("role", "user"));

            var props = new AuthenticationProperties
            {
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.Add(tokenExpiration),
            };

            var ticket = new AuthenticationTicket(identity, props);

            string accessToken = Startup.OAuthBearerOptions.AccessTokenFormat.Protect(ticket);

            var tokenResponse = new JObject(
                new JProperty("userName", userName),
                new JProperty("access_token", accessToken),
                new JProperty("token_type", "bearer"),
                new JProperty("expires_in", tokenExpiration.TotalSeconds.ToString()),
                new JProperty(".issued", ticket.Properties.IssuedUtc.ToString()),
                new JProperty(".expires", ticket.Properties.ExpiresUtc.ToString())
                );

            return tokenResponse;
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error + "; ");
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private string GetQueryString(HttpRequestMessage request, string key)
        {
            IEnumerable<KeyValuePair<string, string>> queryStrings = request.GetQueryNameValuePairs();

            if (queryStrings == null) return null;

            KeyValuePair<string, string> match =
                queryStrings.FirstOrDefault(keyValue => string.Compare(keyValue.Key, key, true) == 0);

            if (string.IsNullOrEmpty(match.Value)) return null;

            return match.Value;
        }

        private string ValidateClientAndRedirectUri(HttpRequestMessage request, ref string redirectUriOutput)
        {
            Uri redirectUri;

            string redirectUriString = GetQueryString(Request, "redirect_uri");

            if (string.IsNullOrWhiteSpace(redirectUriString))
            {
                return "redirect_uri nu este completat";
            }

            bool validUri = Uri.TryCreate(redirectUriString, UriKind.Absolute, out redirectUri);

            if (!validUri)
            {
                return "redirect_uri invalid";
            }

            string clientId = GetQueryString(Request, "client_id");

            if (string.IsNullOrWhiteSpace(clientId))
            {
                return "client_Id nu este completat";
            }

            Client client = _repo.FindClient(clientId);

            if (client == null)
            {
                return string.Format("Client_id '{0}' nu este inregistrat in sistem.", clientId);
            }

            if (
                !string.Equals(client.AllowedOrigin, redirectUri.GetLeftPart(UriPartial.Authority),
                    StringComparison.OrdinalIgnoreCase))
            {
                return string.Format("Adresa URL nu este autorizata de configurarea Client_id '{0}'.", clientId);
            }

            redirectUriOutput = redirectUri.AbsoluteUri;

            return string.Empty;
        }

        private async Task<ParsedExternalAccessToken> VerifyExternalAccessToken(string provider, string accessToken)
        {
            ParsedExternalAccessToken parsedToken = null;

            string verifyTokenEndPoint = "";

            if (provider == "Facebook")
            {
                //You can get it from here: https://developers.facebook.com/tools/accesstoken/
                //More about debug_tokn here: http://stackoverflow.com/questions/16641083/how-does-one-get-the-app-access-token-for-debug-token-inspection-on-facebook
                const string appToken = "810874718985444|H4Jwj90oqFi7zB_tqRah1YQ_jYQ";
                verifyTokenEndPoint =
                    string.Format("https://graph.facebook.com/debug_token?input_token={0}&access_token={1}", accessToken,
                        appToken);
            }
            else if (provider == "Google")
            {
                verifyTokenEndPoint = string.Format("https://www.googleapis.com/oauth2/v1/tokeninfo?access_token={0}",
                    accessToken);
            }
            else
            {
                return null;
            }

            var client = new HttpClient();
            var uri = new Uri(verifyTokenEndPoint);
            HttpResponseMessage response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();

                dynamic jObj = JsonConvert.DeserializeObject(content);

                parsedToken = new ParsedExternalAccessToken();

                if (provider == "Facebook")
                {
                    parsedToken.user_id = jObj["data"]["user_id"];
                    parsedToken.app_id = jObj["data"]["app_id"];

                    if (
                        !string.Equals(Startup.FacebookAuthOptions.AppId, parsedToken.app_id,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                }
                else if (provider == "Google")
                {
                    parsedToken.user_id = jObj["user_id"];
                    parsedToken.app_id = jObj["audience"];

                    if (
                        !string.Equals(Startup.GoogleAuthOptions.ClientId, parsedToken.app_id,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                }
            }

            return parsedToken;
        }

        #endregion Private Methods

        #region Private Classes

        private class ExternalLoginData
        {
            #region Public Properties

            public string ExternalAccessToken { get; set; }

            public string LoginProvider { get; set; }

            public string ProviderKey { get; set; }

            public string UserName { get; set; }

            #endregion Public Properties

            #region Public Methods

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer) ||
                    String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name),
                    ExternalAccessToken = identity.FindFirstValue("ExternalAccessToken"),
                };
            }

            #endregion Public Methods
        }

        #endregion Private Classes
    }
}