using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using NewsSearch.Models;
using NewsSearch.Infrastructure;

namespace NewsSearch.Infrastructure.Automapper
{
    public class SearchProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<Dictionary<string, object>, GuardianSearch>()
                .ForMember(dest => dest.Status, opts => opts.MapFrom(src => src.GetValueForKey("Status")))
                .ForMember(dest => dest.Total, opts => opts.MapFrom(src => src.GetValueForKey("Total")))
                .ForMember(dest => dest.StartIndex, opts => opts.MapFrom(src => src.GetValueForKey("StartIndex")))
                .ForMember(dest => dest.PageSize, opts => opts.MapFrom(src => src.GetValueForKey("PageSize")))
                .ForMember(dest => dest.CurrentPage, opts => opts.MapFrom(src => src.GetValueForKey("CurrentPage")))
                .ForMember(dest => dest.Pages, opts => opts.MapFrom(src => src.GetValueForKey("Pages")))
                .ForMember(dest => dest.OrderBy, opts => opts.MapFrom(src => src.GetValueForKey("OrderBy")))
                .ForMember(dest => dest.Results, opts => opts.MapFrom(src => (object[])src.GetValueForKey("Results")));

            CreateMap<object[], List<SourceResult>>()
                .ConvertUsing(
                    src =>
                        src.Select(x =>
                            Mapper.Map<Dictionary<string, object>, SourceResult>(
                                new Dictionary<string, object>((Dictionary<string, object>)x,
                                    StringComparer.InvariantCultureIgnoreCase))).ToList());

            CreateMap<Dictionary<string, object>, SourceResult>()
                .ForMember(dest => dest.SectionId, opts => opts.MapFrom(src => TestSource(opts.GetType(), src.GetValueForKey("SectionId"))))
                .ForMember(dest => dest.Title, opts => opts.MapFrom(src => src.GetValueForKey("WebTitle")))
                .ForMember(dest => dest.PublicationDate, opts => opts.MapFrom(src => src.GetValueForKey("WebPublicationDate")))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.GetValueForKey("Id")))
                .ForMember(dest => dest.WebUrl, opts => opts.MapFrom(src => src.GetValueForKey("WebUrl")))
                .ForMember(dest => dest.ApiUrl, opts => opts.MapFrom(src => src.GetValueForKey("ApiUrl")))
                .ForMember(dest => dest.SectionName, opts => opts.MapFrom(src => src.GetValueForKey("SectionName")));

        }

        private static object TestSource(Type sourceType, object value)
        {
            return value;
        }


    }
}