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
            CreateMap<Dictionary<string, object>, GuardianSearchResult>()
                .ForMember(dest => dest.Status, opts => opts.MapFrom(src => src["Status"]))
                .ForMember(dest => dest.Total, opts => opts.MapFrom(src => src["Total"]))
                .ForMember(dest => dest.StartIndex, opts => opts.MapFrom(src => src["StartIndex"]))
                .ForMember(dest => dest.PageSize, opts => opts.MapFrom(src => src["PageSize"]))
                .ForMember(dest => dest.CurrentPage, opts => opts.MapFrom(src => src["CurrentPage"]))
                .ForMember(dest => dest.Pages, opts => opts.MapFrom(src => src["Pages"]))
                .ForMember(dest => dest.OrderBy, opts => opts.MapFrom(src => src["OrderBy"]))
                .ForMember(dest => dest.Results, opts => opts.MapFrom(src => (object[])src["Results"]));

            CreateMap<object[], List<GuardianResult>>()
                .ConvertUsing(
                    src =>
                        src.Select(x =>
                            Mapper.Map<Dictionary<string, object>, GuardianResult>(
                                new Dictionary<string, object>((Dictionary<string, object>) x,
                                    StringComparer.InvariantCultureIgnoreCase))).ToList());

            CreateMap<Dictionary<string, object>, GuardianResult>()
                .ForMember(dest => dest.SectionId, opts => opts.MapFrom(src => src["SectionId"]))
                .ForMember(dest => dest.WebTitle, opts => opts.MapFrom(src => src["WebTitle"]))
                .ForMember(dest => dest.WebPublicationDate, opts => opts.MapFrom(src => src["WebPublicationDate"]))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src["Id"]))
                .ForMember(dest => dest.WebUrl, opts => opts.MapFrom(src => src["WebUrl"]))
                .ForMember(dest => dest.ApiUrl, opts => opts.MapFrom(src => src["ApiUrl"]))
                .ForMember(dest => dest.SectionName, opts => opts.MapFrom(src => src["SectionName"]));

        }
    }
}