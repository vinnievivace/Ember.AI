using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace EmberAI.Futureverse.AssetRegistry
{
    public class AssetRegistryManager : EmberBehaviour
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        // Collection: Q29sbGVjdGlvbjo3NjY4OnJvb3Q6MTc1MDg= - Party Bear Unleashed

        public const string PartyBearUnleashedID = "7668:root:17508";
        public const string VinnieFP = "0xfFffFfff00000000000000000000000000001297";
        
        public static readonly string AREndPoint = "https://ar-api.futureverse.app/graphql";
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        #endregion

        #region Inspector ..............................................................................................

        #endregion

        #region Initialization .........................................................................................

        #endregion

        #region MonoBehaviours .........................................................................................

        protected override void OnAwake()
        {
            base.OnAwake();
            
            //GetUserCollections(VinnieFP);
            GetCollectionAssets(VinnieFP, PartyBearUnleashedID);
        }

        #endregion

        #region General ................................................................................................

        // https://ar-api.futureverse.app/graphql
        // https://futureverse.mintlify.app/build-an-experience/assets/collectibles-nfts/get-collections-owned-by-a-user
        
        private async void GetUserCollections(string FPAddress)
        {
            string queryString = new ARCollectionsRequest(FPAddress).GetQuery();
            
            using (UnityWebRequest request = new UnityWebRequest(AREndPoint, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(queryString));
                request.uploadHandler.contentType = "application/json";
                request.downloadHandler = new DownloadHandlerBuffer();
                
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    GraphQLResponse response = JsonConvert.DeserializeObject<GraphQLResponse>(request.downloadHandler.text);
                    
                    Debug.Log($"Received {response.data.collections.edges.Count} collections");
                    Debug.Log($"Has next page: {response.data.collections.pageInfo.hasNextPage}");

                    DebugCollections(response.data.collections);
                    
                    GetCollectionAssets(FPAddress, response.data.collections.edges[0].node.id);
                }
                else
                {
                    Debug.LogError($"Error: {request.error}");
                }
            }
        }
        
        // https://futureverse.mintlify.app/build-an-experience/assets/collectibles-nfts/get-collection
        private async void GetCollectionAssets(string FPAddress, string collectionId)
        {
            string queryString = new ARAssetsRequest(FPAddress, collectionId).GetQuery();
            
            using (UnityWebRequest request = new UnityWebRequest(AREndPoint, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(queryString));
                request.uploadHandler.contentType = "application/json";
                request.downloadHandler = new DownloadHandlerBuffer();
                
                DebugWebRequest(request);

                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    GraphQLResponse response = JsonConvert.DeserializeObject<GraphQLResponse>(request.downloadHandler.text);
                    
                    Debug.Log(response.data);
                    Debug.Log(request.downloadHandler.text);
                    
                    //Debug.Log($"Received {response.data.collections.edges.Count} collections");
                    //Debug.Log($"Has next page: {response.data.collections.pageInfo.hasNextPage}");
                }
                else
                {
                    Debug.LogError($"Error: {request.error}");
                }
            }
        }
        
        #endregion

        #region Debug ..................................................................................................

        private static void DebugWebRequest(UnityWebRequest request)
        {
            var curl = new StringBuilder();

            // Initial command with method and URL
            curl.Append("curl --location --request ")
                .Append(request.method)
                .Append(" '")
                .Append(request.url)
                .Append("' \\n");

            // Common headers
            string[] commonHeaders = { "Content-Type", "Accept", "Authorization", "User-Agent", "Cookie" };

            foreach (var header in commonHeaders)
            {
                string headerValue = request.GetRequestHeader(header);
                if (!string.IsNullOrEmpty(headerValue))
                {
                    curl.Append("--header '")
                        .Append(header)
                        .Append(": ")
                        .Append(headerValue)
                        .Append("' \\n");
                }
            }

            // Add body data if present
            if (request.uploadHandler != null)
            {
                byte[] bodyData = request.uploadHandler.data;
                if (bodyData != null && bodyData.Length > 0)
                {
                    string bodyString = Encoding.UTF8.GetString(bodyData);
                    curl.Append("--data-raw '")
                        .Append(bodyString.Replace("'", "\\'"))
                        .Append("'");
                }
            }

            Debug.Log(curl.ToString());
        }
        
        private void DebugCollections(CollectionConnection collections)
        {
            foreach (Edge collection in collections.edges)
            {
                
                string collectionAssetID = collection.node.chainId + ":" + collection.node.chainType + ":" + collection.node.location;
                
                Debug.Log($"Collection: {collection.node.id} - {collection.node.name} + " + collectionAssetID);
            }
        }

        #endregion

        #region Event Handlers .........................................................................................

        #endregion

#endregion
    }
}