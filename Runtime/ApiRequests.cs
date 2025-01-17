﻿using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace BlockadeLabsSDK
{
    public class ApiRequests
    {
        public static async Task<List<SkyboxStyle>> GetSkyboxStyles(string apiKey)
        {
            var getSkyboxStylesRequest = UnityWebRequest.Get(
                "https://backend.blockadelabs.com/api/v1/skybox/styles" + "?api_key=" + apiKey
            );

            await getSkyboxStylesRequest.SendWebRequest();

            if (getSkyboxStylesRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Get skybox styles error: " + getSkyboxStylesRequest.error);
                getSkyboxStylesRequest.Dispose();
            }
            else
            {
                var skyboxStylesList =
                    JsonConvert.DeserializeObject<List<SkyboxStyle>>(getSkyboxStylesRequest.downloadHandler.text);

                getSkyboxStylesRequest.Dispose();

                return skyboxStylesList;
            }

            return null;
        }

        public static async Task<string> CreateSkybox(List<SkyboxStyleField> skyboxStyleFields, int id, string apiKey)
        {
            // Create a dictionary of string keys and dictionary values to hold the JSON POST params
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("skybox_style_id", id.ToString());

            foreach (var field in skyboxStyleFields)
            {
                if (field.value != "")
                {
                    parameters.Add(field.key, field.value);
                }
            }

            string parametersJsonString = JsonConvert.SerializeObject(parameters);

            var createSkyboxRequest = new UnityWebRequest();
            createSkyboxRequest.url = "https://backend.blockadelabs.com/api/v1/skybox?api_key=" + apiKey;
            createSkyboxRequest.method = "POST";
            createSkyboxRequest.downloadHandler = new DownloadHandlerBuffer();
            createSkyboxRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(parametersJsonString));
            createSkyboxRequest.timeout = 60;
            createSkyboxRequest.SetRequestHeader("Accept", "application/json");
            createSkyboxRequest.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");

            await createSkyboxRequest.SendWebRequest();

            if (createSkyboxRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Create Skybox Error: " + createSkyboxRequest.error);
                createSkyboxRequest.Dispose();
            }
            else
            {
                var result = JsonConvert.DeserializeObject<CreateSkyboxResult>(createSkyboxRequest.downloadHandler.text);
                
                createSkyboxRequest.Dispose();
            
                if (result?.obfuscated_id == null)
                {
                    return "";
                }
            
                return result.obfuscated_id;
            }
            
            return "";
        }

        public static async Task<Dictionary<string, string>> GetImagine(string imagineObfuscatedId, string apiKey)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
           
            var getImagineRequest = UnityWebRequest.Get(
                "https://backend.blockadelabs.com/api/v1/imagine/requests/obfuscated-id/" + imagineObfuscatedId + "?api_key=" + apiKey
            );

            await getImagineRequest.SendWebRequest();

            if (getImagineRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Get Imagine Error: " + getImagineRequest.error);
                getImagineRequest.Dispose();
            }
            else
            {
                var status = JsonConvert.DeserializeObject<GetImagineResult>(getImagineRequest.downloadHandler.text);
                
                getImagineRequest.Dispose();

                if (status?.request != null)
                {
                    if (status.request?.status == "complete")
                    {
                        result.Add("textureUrl", status.request.file_url);
                        result.Add("prompt", status.request.prompt);
                        result.Add("depthMapUrl", status.request.depth_map_url);
                    }
                }
            }

            return result;
        }
        
        public static async Task<byte[]> GetImagineImage(string textureUrl)
        {
            var imagineImageRequest = UnityWebRequest.Get(textureUrl);
            await imagineImageRequest.SendWebRequest();

            if (imagineImageRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Get Imagine Image Error: " + imagineImageRequest.error);
                imagineImageRequest.Dispose();
            }
            else
            {
                var image = imagineImageRequest.downloadHandler.data;
                imagineImageRequest.Dispose();

                return image;
            }

            return null;
        }
    }
}