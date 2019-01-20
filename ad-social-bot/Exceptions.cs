using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ad_social_bot
{
    public class InvalidAccessToken : ApplicationException
    {
        public InvalidAccessToken() { }

        public InvalidAccessToken(string message) : base(message) { }

        public InvalidAccessToken(string message, Exception inner) : base(message, inner) { }

    }

    public class InvalidAdSocialAuthorization : ApplicationException
    {
        public InvalidAdSocialAuthorization() { }

        public InvalidAdSocialAuthorization(string message) : base(message) { }

        public InvalidAdSocialAuthorization(string message, Exception inner) : base(message, inner) { }

    }

    public class SociaAdRobotCheck : ApplicationException
    {
        public SociaAdRobotCheck() { }

        public SociaAdRobotCheck(string message) : base(message) { }

        public SociaAdRobotCheck(string message, Exception inner) : base(message, inner) { }

    }

    public class VKCaptchaNeeded : ApplicationException
    {
        public VKCaptchaNeeded() { }

        public VKCaptchaNeeded(string message) : base(message) { }

        public VKCaptchaNeeded(string message, Exception inner) : base(message, inner) { }

    }
}
