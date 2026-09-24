using System;

namespace MobileAdsPackage
{
    /// <summary>
    /// Represents reward details earned by a player after completing a rewarded ad.
    /// </summary>
    [Serializable]
    public struct AdReward
    {
        public string Type;
        public double Amount;

        public AdReward(string type, double amount)
        {
            Type = type;
            Amount = amount;
        }

        public override string ToString()
        {
            return $"{Amount} {Type}";
        }
    }

    /// <summary>
    /// Result received when Mobile Ads initialization finishes.
    /// </summary>
    [Serializable]
    public struct AdInitResult
    {
        public bool Success;
        public string Message;
        public AdNetwork ActiveNetwork;

        public AdInitResult(bool success, string message, AdNetwork activeNetwork)
        {
            Success = success;
            Message = message;
            ActiveNetwork = activeNetwork;
        }
    }

    /// <summary>
    /// Details about an ad error (load error or display error).
    /// </summary>
    [Serializable]
    public struct AdErrorInfo
    {
        public int Code;
        public string Message;
        public AdType AdType;
        public AdNetwork Network;

        public AdErrorInfo(int code, string message, AdType adType, AdNetwork network)
        {
            Code = code;
            Message = message;
            AdType = adType;
            Network = network;
        }

        public override string ToString()
        {
            return $"[{Network}] {AdType} Error ({Code}): {Message}";
        }
    }
}
