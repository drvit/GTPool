using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using NewsSearch.Models;

namespace NewsSearch.Infrastructure.Automapper
{
    public class SearchProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<Dictionary<string, object>, GuardianSearch>()
                .ForMember(dest => dest.Status, opts => opts.MapFrom(src => GetValueForKey(src, "Status")))
                .ForMember(dest => dest.Total, opts => opts.MapFrom(src => GetValueForKey(src, "Total")))
                .ForMember(dest => dest.StartIndex, opts => opts.MapFrom(src => GetValueForKey(src, "StartIndex")))
                .ForMember(dest => dest.PageSize, opts => opts.MapFrom(src => GetValueForKey(src, "PageSize")))
                .ForMember(dest => dest.CurrentPage, opts => opts.MapFrom(src => GetValueForKey(src, "CurrentPage")))
                .ForMember(dest => dest.Pages, opts => opts.MapFrom(src => GetValueForKey(src, "Pages")))
                .ForMember(dest => dest.OrderBy, opts => opts.MapFrom(src => GetValueForKey(src, "OrderBy")))
                .ForMember(dest => dest.Results, opts => opts.MapFrom(src => (object[])GetValueForKey(src, "Results")));

            CreateMap<object[], List<SourceResult>>()
                .ConvertUsing(
                    src =>
                        src.Select(x =>
                            Mapper.Map<Dictionary<string, object>, SourceResult>(
                                new Dictionary<string, object>((Dictionary<string, object>) x,
                                    StringComparer.InvariantCultureIgnoreCase))).ToList());

            CreateMap<Dictionary<string, object>, SourceResult>()
                .ForMember(dest => dest.SectionId, opts => opts.MapFrom(src => GetValueForKey(src, "SectionId")))
                .ForMember(dest => dest.Title, opts => opts.MapFrom(src => GetValueForKey(src, "WebTitle")))
                .ForMember(dest => dest.PublicationDate, opts => opts.MapFrom(src => GetValueForKey(src, "WebPublicationDate")))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => GetValueForKey(src, "Id")))
                .ForMember(dest => dest.WebUrl, opts => opts.MapFrom(src => GetValueForKey(src, "WebUrl")))
                .ForMember(dest => dest.ApiUrl, opts => opts.MapFrom(src => GetValueForKey(src, "ApiUrl")))
                .ForMember(dest => dest.SectionName, opts => opts.MapFrom(src => GetValueForKey(src, "SectionName")));

        }

        private object GetValueForKey(IReadOnlyDictionary<string, object> dictionary, string key)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : null;
        }
    }
}