using System.Text;
using Newtonsoft.Json;

namespace EmberAI.Futureverse.AssetRegistry
{
    public class ARAssetsRequest : BaseGraphQLRequest
    {
        public string[] CollectionIds { get; set; }

        public ARAssetsRequest(string[] addresses, string[] collectionIds = null)
        {
            QueryName = "GetAssets";
            Addresses = addresses;
            CollectionIds = collectionIds;
        }

        public ARAssetsRequest(string address, string collectionId)
        {
            QueryName = "GetAssets";
            Addresses = new string[] { address };
            CollectionIds = new string[] { collectionId };
        }

        public override string GetQuery()
        {
            StringBuilder queryString = new StringBuilder();

            queryString.Append("query ").Append(QueryName).Append("(");
            queryString.Append("$addresses: [ChainAddress!]!, $collectionIds: [CollectionId!], $after: String, $first: Float");
            queryString.Append(") {");

            queryString.Append(" assets(addresses: $addresses, collectionIds: $collectionIds, after: $after, first: $first) {");

            queryString.Append(" pageInfo { endCursor hasNextPage }");
            queryString.Append(" edges {");
            queryString.Append(" node {");
            queryString.Append(" id collectionId tokenId assetType");
            queryString.Append(" assetTree { data nodeId }");
            queryString.Append(" schema { name namespace schema version }");
            queryString.Append(" ownership { __typename ... on SFTAssetOwnership { id balancesOf(addresses: $addresses) { balance owner { address } } } ... on NFTAssetOwnership { owner { address } } }");
            queryString.Append(" metadata { attributes properties uri }");
            queryString.Append(" links { __typename ... on SFTAssetLink { parentLinks(addresses: $addresses) { collectionId tokenId } } ... on NFTAssetLink { parentLink { collectionId tokenId } } }");
            queryString.Append(" collection { id chainId chainType location name }");
            queryString.Append(" }");
            queryString.Append(" }");
            queryString.Append(" }");
            queryString.Append(" }");

            var queryObj = new
            {
                query = queryString.ToString(),
                variables = new
                {
                    addresses = Addresses,
                    collectionIds = CollectionIds,
                    after = StartFrom,
                    first = ResponseLimit
                }
            };

            return JsonConvert.SerializeObject(queryObj);
        }
    }
}