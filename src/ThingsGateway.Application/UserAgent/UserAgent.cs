﻿using UAParser;

namespace ThingsGateway.Application
{
    /// <summary>
    /// 当前登录用户信息
    /// </summary>
    public static class UserAgent
    {
        public static Parser Parser;

        static UserAgent()
        {
            Parser = Parser.GetDefault();
        }
    }
}