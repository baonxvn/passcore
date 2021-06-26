namespace Unosquare.PassCore.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common;
    using Helpers;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Models;
    using Serilog;
    using Swan.Net;

    /// <summary>
    /// Represents a controller class holding all of the server-side functionality of this tool.
    /// </summary>
    [Route("api/[controller]")]
    public class PasswordController : Controller
    {
        private readonly ILogger _logger;
        private readonly ClientSettings _options;
        private readonly IPasswordChangeProvider _passwordChangeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordController" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="optionsAccessor">The options accessor.</param>
        /// <param name="passwordChangeProvider">The password change provider.</param>
        public PasswordController(ILogger logger, IOptions<ClientSettings> optionsAccessor, IPasswordChangeProvider passwordChangeProvider)
        {
            _logger = logger;
            _options = optionsAccessor.Value;
            _passwordChangeProvider = passwordChangeProvider;
        }

        [HttpGet]
        [Route("gfyvhxnueb")]
        public IActionResult GetUserInfo(string username, string pw)
        {
            _logger.Information("START PasswordController.GetUserInfo: ");
            _logger.Information("PasswordController.GetUserInfo: " + username);

            try
            {
                var obj = _passwordChangeProvider.GetUserInfo(username, pw);
                return Json(obj);
            }
            catch (Exception e)
            {
                return Json(e.Message);
            }

        }

        [HttpGet]
        public IActionResult Get()
        {
            _logger.Information("START PasswordController.Get");
            return Json(_options);
        }

        [HttpGet]
        [Route("GetUserDirectoryEntry")]
        public IActionResult GetUserDirectoryEntry(string username, string pw)
        {
            _logger.Information("START PasswordController.GetUserDirectoryEntry: ");
            try
            {
                var obj = _passwordChangeProvider.GetUserDirectoryEntry(username, pw);
                return Json(obj);
            }
            catch (Exception e)
            {
                return Json(e.Message);
            }
        }

        /// <summary>
        /// Returns the ClientSettings object as a JSON string.
        /// </summary>
        /// <returns>A Json representation of the ClientSettings object.</returns>
        [HttpGet]
        [Route("GetUserPrincipal")]
        public IActionResult GetUserPrincipal(string username, string pw)
        {
            _logger.Information("START PasswordController.GetUserPrincipal: ");
            try
            {
                var obj = _passwordChangeProvider.GetUserPrincipal(username, pw);
                return Json(obj);
            }
            catch (Exception e)
            {
                return Json(e.Message);
            }
        }

        /// <summary>
        /// Returns generated password as a JSON string.
        /// </summary>
        /// <returns>A Json with a password property which contains a random generated password.</returns>
        [HttpGet]
        [Route("GeneratedPassword")]
        public IActionResult GeneratedPassword()
        {
            _logger.Information("START PasswordController.GetGeneratedPassword: ");
            using var generator = new PasswordGenerator();
            return Json(new { password = generator.Generate(_options.PasswordEntropy) });
        }

        /// <summary>
        /// Given a POST request, processes and changes a User's password.
        /// </summary>
        /// <param name="model">The value.</param>
        /// <returns>A task representing the async operation.</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChangePasswordModel model)
        {
            _logger.Information("START PasswordController.Post: ");
            // Validate the request
            if (model == null)
            {
                _logger.Warning("Null model");

                return BadRequest(ApiResult.InvalidRequest());
            }

            if (model.NewPassword != model.NewPasswordVerify)
            {
                _logger.Warning("Invalid model, passwords don't match");

                return BadRequest(ApiResult.InvalidRequest());
            }

            // Validate the model
            if (ModelState.IsValid == false)
            {
                _logger.Warning("Invalid model, validation failed");

                return BadRequest(ApiResult.FromModelStateErrors(ModelState));
            }

            //BAONX
            //            // Validate the Captcha
            //            try
            //            {
            //                if (await ValidateRecaptcha(model.Recaptcha).ConfigureAwait(false) == false)
            //                    throw new InvalidOperationException("Invalid Recaptcha response");
            //            }
            //#pragma warning disable CA1031 // Do not catch general exception types
            //            catch (Exception ex)
            //#pragma warning restore CA1031 // Do not catch general exception types
            //            {
            //                _logger.Warning(ex, "Invalid Recaptcha");
            //                return BadRequest(ApiResult.InvalidCaptcha());
            //            }

            var result = new ApiResult();

            try
            {
                //BAONX
                //if (_options.MinimumDistance > 0 &&
                //    _passwordChangeProvider.MeasureNewPasswordDistance(model.CurrentPassword, model.NewPassword) < _options.MinimumDistance)
                //{
                //    result.Errors.Add(new ApiErrorItem(ApiErrorCode.MinimumDistance));
                //    return BadRequest(result);
                //}

                //if (_options.MinimumScore > 0 && Zxcvbn.Core.EvaluatePassword(model.NewPassword).Score < _options.MinimumScore)
                //{
                //    result.Errors.Add(new ApiErrorItem(ApiErrorCode.MinimumScore));
                //    return BadRequest(result);
                //}

                var resultPasswordChange = _passwordChangeProvider.PerformPasswordChange(
                        model.Username,
                        model.CurrentPassword,
                        model.NewPassword);

                if (resultPasswordChange == null)
                    return Json(result);

                result.Errors.Add(resultPasswordChange);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.Error(ex, "Failed to update password");

                result.Errors.Add(new ApiErrorItem(ApiErrorCode.Generic, ex.Message));
            }

            _logger.Warning(Json(result).ToString());
            return BadRequest(result);
        }

        private async Task<bool> ValidateRecaptcha(string recaptchaResponse)
        {
            _logger.Information("START PasswordController.ValidateRecaptcha: ");
            // skip validation if we don't enable recaptcha
            if (string.IsNullOrWhiteSpace(_options.Recaptcha.PrivateKey))
                return true;

            // immediately return false because we don't 
            if (string.IsNullOrEmpty(recaptchaResponse))
                return false;

            var requestUrl = new Uri(
                $"https://www.google.com/recaptcha/api/siteverify?secret={_options.Recaptcha.PrivateKey}&response={recaptchaResponse}");

            var validationResponse = await JsonClient.Get<Dictionary<string, object>>(requestUrl)
                .ConfigureAwait(false);

            return Convert.ToBoolean(validationResponse["success"], System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
