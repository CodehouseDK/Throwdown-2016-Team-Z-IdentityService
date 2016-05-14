using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IdentityAndEmotionService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace IdentityAndEmotionService.Services
{
    public interface IFaceApi
    {
        Task<string> AddPersonFace(Uri image);

        Task CreateFaceList();

        Task<List<Face>> GetFacesFromStream(Stream stream);

        Task<Uri> SaveAsync(Stream stream,string blobName);

        Task<FaceWithConfidence> FindSimilar(string id);
    }

   

    public class FaceApi : IFaceApi
    {
       
        
        private readonly string _url = "face/v1.0/detect?returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,smile,facialHair,glasses";
        private readonly string _groupId = "1";

        private readonly HttpClient _client;
        private IConfigurationSection _configuration;

        public FaceApi(IConfigurationRoot configuration)
        {

            _configuration = configuration.GetSection("Face");
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://api.projectoxford.ai/");
            _client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _configuration.GetSection("Key").Value);
        }

      
        public async Task<string> AddPersonFace(Uri image)
        {
            var message = "{\"url\":\"" + image + "\"}";
            var content = new StringContent(message);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var url = $"face/v1.0/facelists/{_groupId}/persistedFaces";
            var post = await _client.PostAsync(url, content);
            var str = await post.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Person>(str).PersistedFaceId;
        }

        public async Task CreateFaceList()
        {
             await _client.DeleteAsync("face/v1.0/facelists/" +_groupId);
            var message = "{\"name\":\"codehouse\",\"userData\":\"nothing\"}";
            var content = new StringContent(message);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            await _client.PutAsync("face/v1.0/facelists/" + _groupId, content);
            return;
        }


        public async Task<List<Face>> GetFacesFromStream(Stream stream)
        {
            var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            var post = _client.PostAsync(_url, content);
            var response = await post;
            var asString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Face>>(asString);
        }

        public async Task<Uri> SaveAsync(Stream stream,string blobName)
        {
            var storAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials("throwdown2016", _configuration.GetSection("BlobKey").Value), false);
            var storClient = storAccount.CreateCloudBlobClient();
            var cont = storClient.GetContainerReference("codehousethrowdown");
            await cont.CreateIfNotExistsAsync();
            await cont.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            
            var blockBlob = cont.GetBlockBlobReference(blobName);
            var exists = await blockBlob.ExistsAsync();
            if (exists)
            {
                return blockBlob.Uri;
            }
            stream.Position = 0;
            await blockBlob.UploadFromStreamAsync(stream);

            return blockBlob.Uri;
        }
    
        public async Task<FaceWithConfidence> FindSimilar(string id)
        {
            var message = "{\"faceId\":\""+id+ "\",\"faceListId\":\""+_groupId+"\", \"maxNumOfCandidatesReturned\":1}";
            var content = new StringContent(message);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var url = $"face/v1.0/findsimilars";
            var post = await _client.PostAsync(url, content);
            var str = await post.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<FaceWithConfidence>>(str).FirstOrDefault();
        }
    }
    public class FaceWithConfidence
    {
        public string PersistedFaceId { get; set; }
        public float Confidence { get; set; }
    }
}