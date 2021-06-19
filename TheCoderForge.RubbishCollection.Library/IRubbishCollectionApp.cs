using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheCoderForge.RubbishCollection
{
    public interface IRubbishCollectionApp
    {
        /// <summary>
        ///     Returns  English language Description of the next pickup, ready to be shown to the user
        /// </summary>
        /// <returns></returns>
        string DescribeNextPickup();

        /// <summary>
        ///     Returns English language Description of the next pickup, ready to be shown to the user
        /// </summary>
        /// <returns></returns>
        Task<string> DescribeNextPickupAsync();

  

        /// <summary>
        ///     Parses a HTML document to find the rubbish collection data
        /// </summary>
        /// <param name="htmlDocument">The HTML document.</param>
        /// <returns></returns>
        List<KeyValuePair<string, DateTime>> ParseHtmlDocument(HtmlAgilityPack.HtmlDocument htmlDocument);


        /// <summary>
        ///     Parses a HTML document to find the rubbish collection data
        /// </summary>
        /// <returns></returns>
        List<KeyValuePair<string, DateTime>> ParseHtmlDocument(string html);
    }
}
