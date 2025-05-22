using System;
using System.Text;
using EmberAI.Attributes;
using EmberAI.Futureverse.FuturePass;
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

        // constants for the ETH collection IDs required by AssetRegistry.
		private const string TNLCollectionID = "1:evm:0x6bca6de2dbdc4e0d41f7273011785ea16ba47182";
    	private const string FlufCollectionID = "1:evm:0xccc441ac31f02cd96c153db6fd5fe0a2f4e6a68d";
    	private const string AIFACollectionID = "1:evm:0x96be46c50e882dbd373081d08e0cde2b055adf6c";
        private const string ASMBrainCollectionID = "1:evm:0xd0318da435dbce0b347cc6faa330b5a9889e3585";
        private const string PBCollectionID = "1:evm:0x35471f47c3c0bc5fc75025b97a19ecdde00f78f8";
        
        // constants for the TRN collection IDs required by AssetRegistry
        private const string GoblinCollectionID = "7668:root:3172";
    	private const string AlteredStateCollectionID = "7668:root:100452";
        private const string AtemVehicleCollectionID = "7668:root:16484";
        private const string PBUnleashedID = "7668:root:17508";
    	private const string PBMouthCollectionID = "7668:root:18532";
    	private const string PBEarCollectionID = "7668:root:19556";
    	private const string PBClothingCollectionID = "7668:root:20580";
    	private const string PBNeckCollectionID = "7668:root:21604";
    	private const string PBAnimationCollectionID = "7668:root:22628";
    	private const string PBHeadCollectionID = "7668:root:23652";
    	private const string PBEyewearCollectionID = "7668:root:24676";
    	private const string PBNoseCollectionID = "7668:root:25700";

    	private string[] PBCompleteCollectionIDs = { 
            PBCollectionID, 
            PBUnleashedID, 
            PBAnimationCollectionID, 
            PBClothingCollectionID, 
            PBEarCollectionID, 
            PBEyewearCollectionID, 
            PBHeadCollectionID, 
            PBMouthCollectionID, 
            PBNeckCollectionID,
        	PBNoseCollectionID
    	};
        
        [BoxGroup("Settings"), SerializeField, ReadOnly]
        private string AREndPoint = "https://ar-api.futureverse.app/graphql";
        
        [BoxGroup("Settings"), SerializeField]
        private string _TRNAddress = "0xfFffFfff00000000000000000000000000001297";

        [BoxGroup("Settings"), SerializeField]
        private string _EOAAddress = "0xbC2561EcdaD28555e686303D71d446706f204A12";
        
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

        

        private void OnEnable()
        {
            FPAuthManager.Instance.OnLoginComplete += OnFPLoginComplete;
        }

        private void OnDisable()
        {
            FPAuthManager.Instance.OnLoginComplete -= OnFPLoginComplete;
        }

        #endregion

        #region General ................................................................................................

        private void Initialize()
        {
            if (FPAuthManager.Instance.Authenticated)
            {
                _TRNAddress = FPAuthManager.Instance.TRNAddress;
                _EOAAddress = FPAuthManager.Instance.ETHAddress;
                
               GetUsersCollections(new []{_TRNAddress, _EOAAddress});
            }
            else
            {
                throw new Exception("FuturePass not authenticated, unable to initialize AssetRegistryManager");
            }
        }
        
        // https://ar-api.futureverse.app/graphql
        // https://futureverse.mintlify.app/build-an-experience/assets/collectibles-nfts/get-collections-owned-by-a-user
        
        private async void GetUsersCollections(string[] wallets)
        {
            string queryString = new ARCollectionsRequest(wallets).GetQuery();

            using UnityWebRequest request = new UnityWebRequest(AREndPoint, "POST");
            
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
            }
            else
            {
                Debug.LogError($"Error: {request.error}");
            }
        }
        
        // https://futureverse.mintlify.app/build-an-experience/assets/collectibles-nfts/get-collection
        private async void GetCollectionAssets(string collectionId)
        {
            string queryString = new ARAssetsRequest(new []{_TRNAddress, _EOAAddress}, new []{collectionId}).GetQuery();

            using UnityWebRequest request = new UnityWebRequest(AREndPoint, "POST");
            
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(queryString));
            request.uploadHandler.contentType = "application/json";
            request.downloadHandler = new DownloadHandlerBuffer();
                
            //DebugWebRequest(request);

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                GraphQLResponse response = JsonConvert.DeserializeObject<GraphQLResponse>(request.downloadHandler.text);
                    
                Debug.Log("response.data = " + response.data);
                Debug.Log(" response raw = " + request.downloadHandler.text);
                
                Log(LogLevel.Log, "edge 1 node name = " + response.data.collections);
                    
                //Debug.Log($"Received {response.data.collections.edges.Count} collections");
                
            }
            else
            {
                Debug.LogError($"Error: {request.error}");
            }
        }
        
        #endregion

        #region Debug ..................................................................................................

        // TODO move to some Debug / API Util
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

        private void OnFPLoginComplete()
        {
            Initialize();
        }
        
        #endregion

#endregion
    }
}