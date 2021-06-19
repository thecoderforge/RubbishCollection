using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using HtmlAgilityPack;
using TheCoderForge.Net;

namespace TheCoderForge.RubbishCollection
{
    public class RubbishCollectionApp : IRubbishCollectionApp
    {
        /// <summary>
        ///     Returns English language Description of the next pickup, ready to be shown to the user
        /// </summary>
        /// <returns></returns>
        public string DescribeNextPickup()
        {
            // try submitting a 'form' with the address hard coded.
            // <form method="POST" action="/next-bin-collection#collectionday" class="for form--bloc" id="addressform">
            //     <select class="form__select" name="uprn" id="uprn" aria-required="true" 																												
            //      <option value="10070922346|1 MARSEY ROAD, BOLTON, BL2 5BB">1 MARSEY ROAD, BOLTON, BL2 5BB</option>

            // this value property represents our address....
            // hard-coded for now, but will try to get it from alexa config when i work out how
            string addressElement = "10070922346|1 MARSEY ROAD, BOLTON, BL2 5BB";
            string uri = "https://www.bolton.gov.uk/next-bin-collection#collectionday";
            string strPost = $"uprn={addressElement}";

            HttpHelper httpHelper = new HttpHelper();
            string response = httpHelper.PostFormString(uri, strPost);

            RubbishCollectionApp app = new RubbishCollectionApp();

            //    // use a test response (stored as a resource of the app):
            //    //string response = Resources.TestResponse;

            // scrape the result from the html we got back....

            // we will need the html agility pack
            // Install-Package HtmlAgilityPack
            // and
            // using HtmlAgilityPack;

            List<KeyValuePair<string, DateTime>> resultsList = app.ParseHtmlDocument(response);

            string results = "";

            foreach (var item in resultsList)
            {

                var dateTime = item.Value;
                var datePostfix = (dateTime.Day % 10 == 1 && dateTime.Day % 100 != 11) ? "st"
                                  : (dateTime.Day % 10 == 2 && dateTime.Day % 100 != 12) ? "nd"
                                  : (dateTime.Day % 10 == 3 && dateTime.Day % 100 != 13) ? "rd"
                                  : "th";

                var dateDescription = $"on {item.Value:dddd d}{datePostfix} {item.Value:MMMM}";

                if (item.Value.Date == DateTime.Now.Date) dateDescription = "Today";
                if (item.Value.Date == DateTime.Now.Date.AddDays(1)) dateDescription = "Tomorrow";

                results += $"{item.Key} will be collected {dateDescription}{Environment.NewLine}";
            }

            return results.ToString();
        }




        /// <summary>
        ///     Returns English language Description of the next pickup, ready to be shown to the user
        /// </summary>
        /// <returns></returns>
        public Task<string> DescribeNextPickupAsync() { throw new NotImplementedException(nameof(DescribeNextPickupAsync)); }


        /// <summary>
        ///     Parses a HTML document to find the rubbish collection data
        /// </summary>
        /// <param name="htmlDocument">The HTML document.</param>
        /// <returns></returns>
        public List<KeyValuePair<string, DateTime>> ParseHtmlDocument(HtmlAgilityPack.HtmlDocument htmlDocument)
        {


            // store our results here:
            SortedList<DateTime, KeyValuePair<string, DateTime>> collections = new SortedList<DateTime, KeyValuePair<string, DateTime>>();

            // the first (hopefully) unique element has class bin-calender applied to it.
            // we need an XPath expression to find that element:
            string xPath = "//div[contains(@class, 'bin-calendar js-calendar')]";
            HtmlNode calenderNode = htmlDocument.DocumentNode.SelectSingleNode(xPath);

            // within the calendernode the structure is something like this...
            // <div class='bin-calendar'>
            //   <div> // repeated element for each type of bin pickup [grey,beige,burgundy, green]
            //      <div></div> // contains image of bin
            //      <div class='media-body'>
            //          <p></p>  // intro blurb, which identifies the bin type
            //          <ul>
            //              <li></li> // a li element for the next 3 dates of bin pickup

            foreach (HtmlNode mediaBlock in calenderNode.ChildNodes)
            {
                // inside each mediaBlock, we need to find the media-body node:
                xPath = "div[contains(@class, 'media-body')]";
                HtmlNode mediaBody = mediaBlock.SelectSingleNode(xPath);

                // each mediaBody contains 2 elements...  a <P> element and a <UL> element.
                // the <P> element can be used to identify the type of bin collected
                // the <ul> contains 3 <li> elements for the next 3 dates

                if (mediaBody != null)
                {
                    string binName = "";
                    DateTime firstDate = DateTime.MinValue;

                    foreach (HtmlNode div in mediaBody.ChildNodes)
                        switch (div.Name)

                        {
                            case"#text":
                                // we can ignore these
                                break;

                            case"p":
                                // this string will identify the type of bin we are looking at... one of [grey,beige,burgundy,green]
                                // 'Your next {} bin collection(s) will be on'

                                binName = div.InnerText.Replace("\r\n", "").Trim();
                                binName = binName.Replace("Your next ", "");
                                binName = binName.Replace(" bin collection(s) will be on", "");

                                break;

                            case"ul":

                                // each ui has 3 li's
                                // each li is a pick up date.
                                // we are only interested in teh earliest - ie the first li

                                HtmlNode firstDateNode = div.ChildNodes[1]; // child 0 is '#text'
                                string firstDateText = firstDateNode.InnerText.Replace("\r\n", "").Trim();
                                firstDate = DateTime.Parse(firstDateText);

                                break;
                            default:

                                // there shouldn't be any of these!
                                Debugger.Break();

                                throw new Exception($"Unexpected div [{div.Name}] found in mediaBody");
                        }

                    // the data includes multiple bins that are collected on the same day
                    // we can simplify these days to a common name
                    if (binName == "green") binName = "Recycling";
                    if (binName == "burgundy") binName = "Recycling";
                    if (binName == "beige") binName = "Recycling";
                    if (binName == "grey") binName = "Rubbish";

                    // add to collection:
                    // we can ignore multiple values on the same date
                    try
                    {
                        collections.Add(firstDate, new KeyValuePair<string, DateTime>(binName, firstDate));

                    }
                    catch (System.ArgumentException e)
                    {
                        if (string.Compare(e.Message, "An entry with the same key already exists.") == 0)
                        {
                            // we can ignore multiple values on the same date
                        }
                        else
                        {
                            Console.WriteLine(e);
                            Debugger.Break();

                            throw;
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Debugger.Break();

                        throw;
                    }

                }
            }

            // simplify the data so we can return it.
            List<KeyValuePair<string, DateTime>> returnValue = new List<KeyValuePair<string, DateTime>>();

            List<KeyValuePair<DateTime, KeyValuePair<string, DateTime>>> tempp = collections.ToList();

            foreach (KeyValuePair<string, DateTime> item in collections.Values) returnValue.Add(item);

            return returnValue;
        }

        /// <summary>
        ///     Parses a HTML document to find the rubbish collection data
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string, DateTime>> ParseHtmlDocument(string html)
        {

            // store our results here:
            SortedList<DateTime, KeyValuePair<string, DateTime>> collections = new SortedList<DateTime, KeyValuePair<string, DateTime>>();

            HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(html);

            return ParseHtmlDocument(htmlDocument);
        }

    }
}
