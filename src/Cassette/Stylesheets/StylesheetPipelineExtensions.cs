using System;
using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public static class StylesheetPipelineExtensions
    {
        public static StylesheetPipeline EmbedImages(this StylesheetPipeline pipeline, ImageEmbedType type = ImageEmbedType.DataUriForIE8)
        {
            return pipeline.EmbedImages(url => true, type);
        }

        public static StylesheetPipeline EmbedImages(this StylesheetPipeline pipeline, Func<string, bool> shouldEmbedUrl, ImageEmbedType type = ImageEmbedType.DataUriForIE8)
        {
            if (type == ImageEmbedType.Mhtml)
            {
                // Stylesheets containing MHTML must be served as message/rfc822
                pipeline.InsertBefore<ExpandCssUrls>(new AssignContentType("message/rfc822"));

                // MHTML must be added after minification or it won't work
                pipeline.Append(new MhtmlBlockRenderer());
            }
            else
            {
                bool ie8Support = (type == ImageEmbedType.DataUriForIE8);
                pipeline.InsertBefore<ExpandCssUrls>(new ConvertImageUrlsToDataUris(shouldEmbedUrl, ie8Support));
            }
            return pipeline;
        }

        public static StylesheetPipeline EmbedFonts(this StylesheetPipeline pipeline)
        {
            pipeline.InsertBefore<ExpandCssUrls>(new ConvertFontUrlsToDataUris());
            return pipeline;
        }

        public static StylesheetPipeline EmbedFonts(this StylesheetPipeline pipeline, Func<string, bool> shouldEmbedUrl)
        {
            pipeline.InsertBefore<ExpandCssUrls>(new ConvertFontUrlsToDataUris(shouldEmbedUrl));
            return pipeline;
        }
    }
}