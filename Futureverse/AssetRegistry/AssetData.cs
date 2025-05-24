using System;

namespace EmberAI.Futureverse.AssetRegistry
{
    [Serializable]
    public class AssetItem
    {
       public string TokenID { get; private set; }
       public string ImagePath { get; private set; }
       public string GLBPath { get; private set; }
       
       public AssetItem(string tokenID, string imagePath, string glbPath)
       {
           TokenID = tokenID;
           ImagePath = imagePath;
           GLBPath = glbPath;
       }
    }
}