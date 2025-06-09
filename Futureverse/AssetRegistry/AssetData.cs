using System;
using JetBrains.Annotations;
using UnityEngine;

namespace EmberAI.Futureverse.AssetRegistry
{
    [Serializable]
    public class AssetItem
    {
       public string TokenID { get; private set; }
       public string ImagePath { get; private set; }
       public string GLBPath { get; private set; }
       
       public string CollectionID { get; private set; }
       
       // unique properties for various collections
       
       [CanBeNull] 
       public string TransparentImagePath { get; set; }
       
       public AssetItem(string tokenID, string imagePath, string glbPath, string collectionID)
       {
           TokenID = tokenID;
           ImagePath = imagePath;
           GLBPath = glbPath;
           CollectionID = collectionID;
       }
    }
}