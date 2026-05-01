using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Engine
{
    public class RuleResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        private RuleResult(bool success, string message)
        {
            IsSuccess = success;
            Message = message;
        }
        public static RuleResult Pass()
         => new(true, "OK");

        public static RuleResult Fail(string message)
            => new(false, message);
    }

}
