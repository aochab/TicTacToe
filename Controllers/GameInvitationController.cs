using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TicTacToe.Models;
using TicTacToe.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace TicTacToe.Controllers
{
    public class GameInvitationController : Controller
    {
        private IStringLocalizer<GameInvitationController> _stringLocalizer;
        private IUserService _userService;
        public GameInvitationController(IUserService userService,
            IStringLocalizer<GameInvitationController> stringLocalizer)
        {
            _userService = userService;
            _stringLocalizer = stringLocalizer;
        }

       [HttpGet]
       public async Task<IActionResult> Index(string email)
        {
            var gameInvitationModel = new GameInvitationModel {
                InvitedBy = email };
            HttpContext.Session.SetString("email", email);
            return View(gameInvitationModel);
        }

        [HttpPost]
        public IActionResult Index(GameInvitationModel gameInvitationModel,
            [FromServices]IEmailService emailService)
        {
            var gameInvitationService =
                Request.HttpContext.RequestServices.GetService <IGameInvitationService>();
            if (ModelState.IsValid)
            {
                emailService.SendEmail(gameInvitationModel.EmailTo,
                    _stringLocalizer["Invitation to game TicTactoe"],
                    _stringLocalizer[$"Welcome, {0} invites you to game TicTacTo. To join, click here {1}",
                    gameInvitationModel.InvitedBy, Url.Action("GameInvitationConfirmation", "GameInvitation", new
                    {
                        gameInvitationModel.InvitedBy,
                        gameInvitationModel.EmailTo
                    }, Request.Scheme, Request.Host.ToString())]);
                var invitation = gameInvitationService.Add(gameInvitationModel).Result;
                return RedirectToAction("GameInvitationConfirmation", new { id = invitation.Id });
            }

            return View(gameInvitationModel);
        }

        [HttpGet]
        public IActionResult GameInvitationConfirmation(Guid id,
            [FromServices]IGameInvitationService gameInvitationService)
        {
            var gameInvitation = gameInvitationService.Get(id).Result;
            return View(gameInvitation);
        }
    }
}