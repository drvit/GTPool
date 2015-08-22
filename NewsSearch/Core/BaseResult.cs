using System;

namespace NewsSearch.Core
{
    public abstract class BaseResult : IResult
    {
        public string Title { get; set; }
        public DateTime PublicationDate { get; set; }
        public string WebUrl { get; set; }
        public string Description { get; set; }
        public string Extract { get; set; }
        public string SubSourceName { get; set; }
        public string SubSourceFavIcon { get; set; }
        public string SubSourceDomain { get; set; }
        public string SectionId { get; set; }
        public string SectionName { get; set; }
        public string ApiUrl { get; set; }
        public string Id { get; set; }
        public string Image { get; set; }
        public string Embeded { get; set; }
        public string Language { get; set; }
        public string UserPublisher { get; set; }
        public string UserImage { get; set; }
        public string UserLink { get; set; }
        public string Geolocation { get; set; }
    }
}