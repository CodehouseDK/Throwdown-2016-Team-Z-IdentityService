using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IdentityAndEmotionService.Models;

namespace IdentityAndEmotionService.Services
{
    public interface IFaceIdentityRepository
    {
        IEnumerable<FaceIdentity> GetFaceIdentities();

        FaceIdentity GetById(string id);

    }

    public class FaceIdentityRepository : IFaceIdentityRepository
    {
        private readonly IFaceApi _faceApi;
        private static List<FaceIdentity> _identities = new List<FaceIdentity>();

        public FaceIdentityRepository(IFaceApi faceApi)
        {
            _faceApi = faceApi;
            _faceApi.CreateFaceList();
            var directoryInfo = new DirectoryInfo("initials");
            var fileInfos = directoryInfo.EnumerateFiles("*.png");

            _identities = fileInfos.Select(async fileInfo => await CreateIdentity(fileInfo)).Select(face => face.Result).ToList();
        }

        public IEnumerable<FaceIdentity> GetFaceIdentities()
        {
            return _identities;
        }

        public FaceIdentity GetById(string id)
        {
            return _identities.FirstOrDefault(f=> f.Id == id);
        }

        private async Task<FaceIdentity> CreateIdentity(FileInfo fileInfo)
        {
            var userName = Path.GetFileNameWithoutExtension(fileInfo.Name);

            var uri = new Uri("https://throwdown2016.blob.core.windows.net/codehousethrowdown/" + fileInfo.Name);
            var persistedId = await _faceApi.AddPersonFace(uri);
            
            return new FaceIdentity
            {
                Id = persistedId,
                UserName = userName,
                Url = uri
            };
        }
    }
}
