using AutoMapper;

namespace NewsSearch.Infrastructure.Automapper
{
    public class InitializeProfiles
    {
        public static void CustomProfiles()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<GuardianProfile>();
                x.AddProfile<SocialMentionProfile>();
                x.AddProfile<YouTubeProfile>();
            });
        }
    }
}