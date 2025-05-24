namespace EmberAI.Futureverse.AssetRegistry
{
    public abstract class BaseGraphQLRequest
    {
        protected string QueryName { get; set; }
        protected int ResponseLimit { get; set; } = 100;
        protected string StartFrom { get; set; } = "";
        protected string[] Addresses { get; set; }
        
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
}