﻿using System;

namespace NewsSearch.Core
{
    public interface IResult
    {
        string Title { get; set; }
        DateTime PublicationDate { get; set; }
        string WebUrl { get; set; }
        string Description { get; set; }
        string Extract { get; set; }

        string SubSourceName { get; set; }
        string SubSourceFavIcon { get; set; }
        string SubSourceDomain { get; set; }
        string SectionId { get; set; }
        string SectionName { get; set; }
        string ApiUrl { get; set; }
        string Id { get; set; }

        string Image { get; set; }
        string Embeded { get; set; }
        string Language { get; set; }
        string UserPublisher { get; set; }
        string UserImage { get; set; }
        string UserLink { get; set; }
        string Geolocation { get; set; }
    }

    public enum ResultFields
    {
        Title,
        PublicationDate,
        WebUrl,
        Description,
        Extract,
        SubSourceName,
        SubSourceFavIcon,
        SubSourceDomain,
        SectionId,
        SectionName,
        ApiUrl,
        Id,
        Image,
        Embeded,
        Language,
        UserPublisher,
        UserImage,
        UserLink,
        Geolocation
    }

}
