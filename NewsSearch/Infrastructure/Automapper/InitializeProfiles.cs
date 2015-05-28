using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsSearch.Infrastructure.Automapper
{
    public class InitializeProfiles
    {
        public static void CustomProfiles()
        {
            Mapper.Initialize(x => x.AddProfile<SearchProfile>());
        }
    }
}