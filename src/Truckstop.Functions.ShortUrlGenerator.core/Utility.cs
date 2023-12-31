﻿using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Functions.ShortUrlGenerator.Domain
{
    public static class Utility
    {
        //reshuffled for randomisation, same unique characters just jumbled up, you can replace with your own version
        //private const string ConversionCode = "FjTG0s5dgWkbLf_8etOZqMzNhmp7u6lUJoXIDiQB9-wRxCKyrPcv4En3Y21aASHV";
        //private static readonly int Base = ConversionCode.Length;
        //sets the length of the unique code to add to vanity
        //private const int MinVanityCodeLength = 5;

        public static async Task<string> GetValidEndUrl(StorageTableHelper stgHelper)
        {

            var newKey = await stgHelper.GetNextTableId();
            string getCode() => WebEncoders.Base64UrlEncode(BitConverter.GetBytes(newKey));
            if (await stgHelper.IfShortUrlEntityExistByKey(getCode()))
                return await GetValidEndUrl(stgHelper);

            return string.Join(string.Empty, getCode());
        }

        //public static string Encode(int i)
        //{
        //    if (i == 0)
        //        return ConversionCode[0].ToString();

        //    return GenerateUniqueRandomToken(i);
        //}

        public static string GetShortUrl(string host, string vanity)
        {
            return host + "/" + vanity;
        }

        // generates a unique, random, and alphanumeric token for the use as a url 
        //(not entirely secure but not sequential so generally not guessable)
        //public static string GenerateUniqueRandomToken(int uniqueId)
        //{
        //    using (var generator = RandomNumberGenerator.Create())
        //    {
        //        //minimum size I would suggest is 5, longer the better but we want short URLs!
        //        var bytes = new byte[MinVanityCodeLength];
        //        generator.GetBytes(bytes);
        //        var chars = bytes
        //            .Select(b => ConversionCode[b % ConversionCode.Length]);
        //        var token = new string(chars.ToArray());
        //        var reversedToken = string.Join(string.Empty, token.Reverse());
        //        return uniqueId + reversedToken;
        //    }
        //}
    }
}
