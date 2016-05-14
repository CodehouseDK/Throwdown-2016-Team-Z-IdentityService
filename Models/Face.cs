using IdentityAndEmotionService.Controllers;

namespace IdentityAndEmotionService.Models
{
    public class Face
    {
        public string FaceId { get; set; }
      
        public FaceAttributes FaceAttributes { get; set; }

        public FaceIdentity Identity { get; set; }
    }
}