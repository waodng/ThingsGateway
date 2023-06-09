﻿using Furion.DynamicApiController;

using Microsoft.AspNetCore.Mvc;

namespace ThingsGateway.Web.Entry.Controllers
{
    /// <summary>
    /// B端登录控制器
    /// </summary>
    [ApiDescriptionSettings(CateGoryConst.ThingsGatewayCore, Order = 200)]
    [Route("auth/b")]
    [LoggingMonitor]
    public class AuthController : IDynamicApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// B端登录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("login")]
        [Description(EventSubscriberConst.Login)]
        public async Task<LoginOutPut> Login(LoginInput input)
        {
            return await _authService.Login(input);
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        [HttpPost("loginOut")]
        [Description(EventSubscriberConst.LoginOut)]
        [Authorize]
        public async Task LoginOut()
        {
            await _authService.LoginOut();
        }
    }
}