using System;
using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class ConvertImageUrlsToMhtmlUris : AddTransformerToAssets
    {
        public ConvertImageUrlsToMhtmlUris()
            : base(new CssImageToMhtmlUriTransformer(anyUrl => true))
        {   
        }

        public ConvertImageUrlsToMhtmlUris(Func<string, bool> shouldEmbedUrl)
            : base(new CssImageToMhtmlUriTransformer(shouldEmbedUrl))
        {
        }
    }
}