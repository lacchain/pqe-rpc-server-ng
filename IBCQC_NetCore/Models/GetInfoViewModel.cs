using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IBCQC_NetCore.Models
{
 
    public class GetInfoViewModel
    {
        public int Format { get; set; }
    }

    //public class GetInfoViewModelValidator : AbstractValidator<GetInfoViewModel>
    //{
    //    public GetInfoViewModelValidator()
    //    {
    //        RuleFor(x => x.Format).LessThanOrEqualTo(1)
    //                              .WithMessage("The supported formats are 0 (decimal, default), and 1 (hex)");
    //    }
    //}



}
