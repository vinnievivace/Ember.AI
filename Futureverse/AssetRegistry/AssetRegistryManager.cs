using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core;
using EmberAI.Attributes;
using EmberAI.Core;
using EmberAI.Futureverse.FuturePass;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace EmberAI.Futureverse.AssetRegistry
{
    public class AssetRegistryManager : EmberSingleton<AssetRegistryManager>
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        public Action OnCollectionsLoaded;
        public Action<List<AssetItem>> OnAssetsLoaded;
        
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
        private const string GenesisWalkerCollectionID = "1:evm:0x258aeac01672e6857972707fc129a6a39d09758b";
        
        // constants for the TRN collection IDs required by AssetRegistry
        
        public const string AlteredStateCollectionID = "7668:root:100452";
        
        public const string GoblinCollectionID = "7668:root:3172";
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

        public override void InitializeDependencies()
        {
            base.InitializeDependencies();
            
            description = "Futureverse: AssetRegistry Manager - GraphQL for Wallet contents";
        }

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
            }
            else
            {
                throw new Exception("FuturePass not authenticated, unable to initialize AssetRegistryManager");
            }
        }
        
        
        
        private async Task<TResponse> SendARRequest<TRequest, TResponse>(TRequest graphQLRequest) where TRequest : BaseGraphQLRequest where TResponse : BaseGraphQLResponse, new()
        {
            string queryString = graphQLRequest.GetQuery();
            
            Log(LogLevel.Log, "QUERY: ");
            Log(LogLevel.Log, queryString);

            using UnityWebRequest request = new (AREndPoint, "POST");
            
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(queryString));
            request.uploadHandler.contentType = "application/json";
            request.downloadHandler = new DownloadHandlerBuffer();
                
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Log(LogLevel.Log, "RAW RESPONSE:");
                Log(LogLevel.Log, request.downloadHandler.text);
                
                return JsonConvert.DeserializeObject<TResponse>(request.downloadHandler.text);
            }
            
            Log(LogLevel.Error, request.error);

            return null;
        }
        
        // https://ar-api.futureverse.app/graphql
        // https://futureverse.mintlify.app/build-an-experience/assets/collectibles-nfts/get-collections-owned-by-a-user
        private async void GetCollections(string[] wallets)
        {
            ARCollectionResponse response = await SendARRequest<ARCollectionsRequest, ARCollectionResponse>(new ARCollectionsRequest(wallets));
            
            if(response == null) throw new Exception("Failed to get collections");
            
            Debug.Log(response.data);;

            Debug.Log($"Received {response.data.collections.edges.Count} collections");
            Debug.Log($"Has next page: {response.data.collections.pageInfo.hasNextPage}");
            
            OnCollectionsLoaded?.Invoke();
        }

        public void GetAssets(string collectionId)
        {
            GetAssets(new []{_TRNAddress, _EOAAddress}, collectionId);
        }
        
        private async void GetAssets(string[] wallets, string collectionId)
        {
            ARAssetsResponse response = await SendARRequest<ARAssetsRequest, ARAssetsResponse>(new ARAssetsRequest(wallets, new []{collectionId}));
            
            if(response == null) throw new Exception("Failed to get collections");
            
            var assets = new List<AssetItem>();
            
            foreach (AssetEdge edge in response.data.assets.edges)
            {
                try
                {
                    string tokenID = edge.node.tokenId;
                    string imagePath = edge.node.metadata.properties.image;
                    string glbPath = edge.node.metadata.properties.glb_url;

                    // clumsy, but different collections have different metadata, so not really my clumsy!!
                    if (glbPath.IsEmptyString()) glbPath = edge.node.metadata.properties.model;
                
                    assets.Add(new AssetItem(tokenID, imagePath, glbPath));
                }
                catch 
                {
                    Debug.LogError("invalid meta data for token " + edge.node.tokenId + ", cannot add to asset list");
                }
                
            }
            
            OnAssetsLoaded?.Invoke(assets);
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
        
        private void DebugCollections(CollectionList collections)
        {
            foreach (CollectionEdge collection in collections.edges)
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