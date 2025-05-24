using System;

namespace EmberAI.Futureverse.AssetRegistry
{
    public abstract class BaseGraphQLResponse
    {
       
    }
    
    [Serializable]
    public class PageInfo
    {
        public string endCursor;
        public bool hasNextPage;
    }
}