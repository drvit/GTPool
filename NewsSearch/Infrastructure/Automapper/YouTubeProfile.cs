using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using NewsSearch.Core.Sources;

namespace NewsSearch.Infrastructure.Automapper
{
    public class YouTubeProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<Dictionary<string, object>, YouTubeSearch>()
                .ForMember(dest => dest.Total, opts => opts.MapFrom(src => GetChieldValueForKey(src.GetValueForKey("pageInfo"), "totalResults")))
                .ForMember(dest => dest.Results, opts => opts.MapFrom(src => (object[])src.GetValueForKey("items")));

            CreateMap<object[], IEnumerable<YouTubeResult>>()
                .ConvertUsing(
                    src =>
                        src.Select(x =>
                            Mapper.Map<Dictionary<string, object>, YouTubeResult>(
                                new Dictionary<string, object>((Dictionary<string, object>)x,
                                    StringComparer.InvariantCultureIgnoreCase))).ToList());

            CreateMap<Dictionary<string, object>, YouTubeResult>()
                .ForMember(dest => dest.Title, opts => opts.MapFrom(src => GetChieldValueForKey(src.GetValueForKey("snippet"), "title")))
                .ForMember(dest => dest.PublicationDate,
                    opts => opts.MapFrom(src => GetChieldValueForKey(src.GetValueForKey("snippet"), "publishedAt")))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => GetChieldValueForKey(src.GetValueForKey("snippet"), "Description")))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => GetChieldValueForKey(src.GetValueForKey("id"), "videoId")))
                .ForMember(dest => dest.WebUrl, opts => opts.MapFrom(src => FormatLink(GetChieldValueForKey(src.GetValueForKey("id"), "videoId"))))
                .ForMember(dest => dest.SubSourceName, opts => opts.MapFrom(src => GetChieldValueForKey(src.GetValueForKey("snippet"), "channelTitle")))
                .ForMember(dest => dest.SubSourceFavIcon, opts => opts.MapFrom(src => GetChieldValueForKey(src.GetValueForKey("thumbnails"), "default")))
                .ForMember(dest => dest.SubSourceDomain, opts => opts.MapFrom(src => FormatDomainLink(GetChieldValueForKey(src.GetValueForKey("snippet"), "channelId"))));
        }

        private static object GetChieldValueForKey(object parent, string childKey)
        {
            if (parent == null)
                return null;

            var dictionary = new Dictionary<string, object>((Dictionary<string, object>) parent,
                StringComparer.InvariantCultureIgnoreCase);

            return dictionary.ContainsKey(childKey) ? dictionary[childKey] : null;
        }

        private static string FormatLink(object link)
        {
            var ret = string.Empty;

            if (link != null && !string.IsNullOrEmpty(link.ToString()))
                ret = string.Format("https://www.youtube.com/watch?v={0}", link);

            return ret;
        }

        private static string FormatDomainLink(object link)
        {
            var ret = string.Empty;

            if (link != null && !string.IsNullOrEmpty(link.ToString()))
                ret = string.Format("https://www.youtube.com/channel/{0}", link);

            return ret;
        }
    }
}