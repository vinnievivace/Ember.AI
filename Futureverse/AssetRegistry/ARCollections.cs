using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

namespace EmberAI.Futureverse.AssetRegistry
{
    #region REQUEST ....................................................................................................

    [Serializable]
    public class ARCollectionsRequest : BaseGraphQLRequest
    {
        public ARCollectionsRequest(string walletAddress)
        {
            QueryName = "Collections";
            Addresses = new [] { walletAddress };
        }

        public ARCollectionsRequest(string[] walletAddresses)
        {
            QueryName = "Collections";
            Addresses = walletAddresses;       
        }

        public override string GetQuery()
        {
            StringBuilder queryString = new StringBuilder();

            queryString.Append("query ").Append(QueryName).Append(" {");
            queryString.Append(" collections(");

            // addresses
            if (Addresses != null && Addresses.Length > 0)
            {
                queryString.Append("addresses: [");
                for (int i = 0; i < Addresses.Length; i++)
                {
                    queryString.Append("\"").Append(Addresses[i]).Append("\"");
                    if (i < Addresses.Length - 1)
                        queryString.Append(", ");
                }
                queryString.Append("]");
            }

            queryString.Append($", first: {ResponseLimit}, after: \"{StartFrom}\"");
            queryString.Append(") {");
            queryString.Append(" edges { node { chainId chainType id location name } }");
            queryString.Append(" pageInfo { endCursor hasNextPage }");
            queryString.Append(" }");
            queryString.Append(" }");

            return JsonConvert.SerializeObject(new { query = queryString.ToString() });
        }
    }
    
    #endregion
    
    #region RESPONSE ...................................................................................................
    
    [Serializable]
    public class ARCollectionResponse : BaseGraphQLResponse
    {
        public CollectionData data;
    }

    [Serializable]
    public class CollectionData
    {
        public CollectionList collections;
    }

    [Serializable]
    public class CollectionList
    {
        public List<CollectionEdge> edges;
        public PageInfo pageInfo;
    }

    [Serializable]
    public class CollectionEdge
    {
        public CollectionNode node;
    }

    [Serializable]
    public class CollectionNode
    {
        public string chainId;
        public string chainType;
        public string id;
        public string location;
        public string name;
    }

    
    
    #endregion
}
