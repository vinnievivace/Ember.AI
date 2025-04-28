using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

namespace EmberAI.Futureverse.AssetRegistry
{
    [Serializable]
    public class ARCollectionsRequest : BaseGraphQLRequest
    {
        public ARCollectionsRequest(string FPAddress)
        {
            QueryName = "Collections";
            Addresses = new string[] { FPAddress };
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

    public abstract class BaseGraphQLRequest
    {
        public string QueryName { get; set; }
        public int ResponseLimit { get; set; } = 100;
        public string StartFrom { get; set; } = "";
        public string[] Addresses { get; set; }
        public GraphQLEdge[] Edges { get; set; }
        public GraphQLPageInfo PageInfo { get; set; }

        public abstract string GetQuery();
    }

    public class GraphQLEdge
    {
        public GraphQLNode node { get; set; }
    }

    public class GraphQLNode
    {
        public string chainId { get; set; }
        public string chainType { get; set; }
        public string id { get; set; }
        public string location { get; set; }
        public string name { get; set; }
    }

    public class GraphQLPageInfo
    {
        public string endCursor { get; set; }
        public bool hasNextPage { get; set; }
    }
    
    
    [Serializable]
    public class GraphQLResponse
    {
        public Data data;
    }

    [Serializable]
    public class Data
    {
        public CollectionConnection collections;
    }

    [Serializable]
    public class CollectionConnection
    {
        public List<Edge> edges;
        public PageInfo pageInfo;
    }

    [Serializable]
    public class Edge
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

    [Serializable]
    public class PageInfo
    {
        public string endCursor;
        public bool hasNextPage;
    }
}
