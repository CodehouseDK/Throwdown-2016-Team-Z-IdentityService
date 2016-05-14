using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityAndEmotionService.Models;
using IdentityAndEmotionService.Services;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace IdentityAndEmotionService.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFaceApi _faceApi;
        private static ConnectionMultiplexer _connection;
      
        private readonly string _forwardChannelName = "users";

        private readonly IFaceIdentityRepository _faceIdentityRepository;

        public HomeController(IFaceIdentityRepository faceIdentityRepository, IFaceApi faceApi, IConfigurationRoot configuration)
        {
            _faceIdentityRepository = faceIdentityRepository;
            _faceApi = faceApi;
            var section = configuration.GetSection("RedisConfiguration");
            _connection = ConnectionMultiplexer.Connect(section.Value);
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IEnumerable<Face>> Index(IFormFile file)
        {
            var faces = await _faceApi.GetFacesFromStream(file.OpenReadStream());
            foreach (var face in faces)
            {
                var similarFace = await _faceApi.FindSimilar(face.FaceId);
                var id = _faceIdentityRepository.GetById(similarFace.PersistedFaceId);
                face.Identity = id;
                _connection.GetSubscriber().Publish(_forwardChannelName, face.Identity.UserName);
            }
            var message = JsonConvert.SerializeObject(faces);
            WebsocketConnections.Broadcast(message).Wait();
            return faces;
        }
    }
}