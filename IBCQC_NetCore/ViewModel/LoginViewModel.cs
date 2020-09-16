using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IBCQC_NetCore.ViewModel
{
   // [Validator(typeof(LoginViewModelValidator))]
    public class LoginViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    //public class LoginViewModelValidator : AbstractValidator<LoginViewModel>
    //{
    //    public LoginViewModelValidator()
    //    {
    //        RuleFor(x => x.Username).NotEmpty();

    //        RuleFor(x => x.Password).NotEmpty();
    //    }
    //}
}
