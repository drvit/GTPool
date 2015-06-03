using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NewsSearch.Core.Sources;

namespace NewsSearch.Infrastructure.Automapper
{
    public class SocialMentionProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<Dictionary<string, object>, SocialMentionSearch>()
                .ForMember(dest => dest.Total, opts => opts.MapFrom(src => src.GetValueForKey("Count")))
                .ForMember(dest => dest.Results, opts => opts.MapFrom(src => (object[])src.GetValueForKey("items")));

            CreateMap<object[], IEnumerable<SocialMentionResult>>()
                .ConvertUsing(
                    src =>
                        src.Select(x =>
                            Mapper.Map<Dictionary<string, object>, SocialMentionResult>(
                                new Dictionary<string, object>((Dictionary<string, object>)x,
                                    StringComparer.InvariantCultureIgnoreCase))).ToList());

            CreateMap<Dictionary<string, object>, SocialMentionResult>()
                .ForMember(dest => dest.Title, opts => opts.MapFrom(src => src.GetValueForKey("Title")))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.GetValueForKey("Description")))
                .ForMember(dest => dest.PublicationDate,
                    opts => opts.MapFrom(src => ConvertTimestamp(src.GetValueForKey("timestamp"))))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.GetValueForKey("Id")))
                .ForMember(dest => dest.WebUrl, opts => opts.MapFrom(src => src.GetValueForKey("link")))
                .ForMember(dest => dest.SectionName, opts => opts.MapFrom(src => src.GetValueForKey("type")))
                .ForMember(dest => dest.SubSourceName, opts => opts.MapFrom(src => src.GetValueForKey("domain")))
                .ForMember(dest => dest.SubSourceFavIcon, opts => opts.MapFrom(src => src.GetValueForKey("favicon")))
                .ForMember(dest => dest.SubSourceDomain, opts => opts.MapFrom(src => FormatLink(src.GetValueForKey("domain"))))
                .ForMember(dest => dest.Embeded, opts => opts.MapFrom(src => src.GetValueForKey("Embeded")))
                .ForMember(dest => dest.Language, opts => opts.MapFrom(src => src.GetValueForKey("Language")))
                .ForMember(dest => dest.UserPublisher, opts => opts.MapFrom(src => src.GetValueForKey("user")))
                .ForMember(dest => dest.UserImage, opts => opts.MapFrom(src => src.GetValueForKey("user_image")))
                .ForMember(dest => dest.UserLink, opts => opts.MapFrom(src => src.GetValueForKey("user_link")))
                .ForMember(dest => dest.Geolocation, opts => opts.MapFrom(src => src.GetValueForKey("geo")));
        }

        private static string FormatLink(object link)
        {
            var ret = string.Empty;

            if (link != null && !link.ToString().Contains("http"))
                ret = string.Format("http://{0}", link);

            return ret;
        }

        private static DateTime? ConvertTimestamp(object timestamp)
        {
            //6/24/2013 10:07:04 AM
            long ts;
            if (timestamp != null && long.TryParse(timestamp.ToString(), out ts))
                return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(ts).ToLocalTime();

            return DateTime.Now;
        }
    }
}