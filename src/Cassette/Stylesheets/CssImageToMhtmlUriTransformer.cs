using System;
using System.Text;
using System.Text.RegularExpressions;
using Cassette.IO;

namespace Cassette.Stylesheets
{
    class CssImageToMhtmlUriTransformer : CssUrlToDataUriTransformer
    {
        static readonly Regex BackgroundUrlRegex = new Regex(
            @"\b(?<start>background .*? url \s* \() \s* (?<quote>[""']?) (?<path>.*?)\.(?<extension>png|jpg|jpeg|gif) \<quote> \s* (?<end>\) .*? ;)",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
        );

        public CssImageToMhtmlUriTransformer(Func<string, bool> shouldEmbedUrl)
            : base(shouldEmbedUrl, BackgroundUrlRegex)
        {
        }

        protected override CssUrlMatchTransformer CreateCssUrlMatchTransformer(Match match, IAsset asset)
        {
            return new CssBackgroundImageUrlMatchTransformer(match, asset);
        }

        class CssBackgroundImageUrlMatchTransformer : CssUrlMatchTransformer
        {
            readonly string fullProperty;
            readonly string start;
            readonly string end;

            public CssBackgroundImageUrlMatchTransformer(Match match, IAsset asset)
                : base(match, asset)
            {
                fullProperty = match.Value;
                start = match.Groups["start"].Value;
                end = match.Groups["end"].Value;
            }

            protected override string DataUri
            {
                get
                {
                    return string.Format(
                        "mhtml:{0}!{1}",
                        GetUrl(),
                        GetFileName()
                    );
                }
            }

            private string GetUrl()
            {
                return "";
            }

            private string GetFileName()
            {
                return (Path.Replace("/", "") + Extension).ToLower();
            }

            protected override string GetContentType(string extension)
            {
                if (extension.Equals("jpg", StringComparison.OrdinalIgnoreCase))
                {
                    return "image/jpeg";
                }
                else
                {
                    return "image/" + extension.ToLowerInvariant();
                }
            }

            public override bool CanTransform
            {
                get
                {
                    return base.CanTransform;
                }
            }
            
            public override void Transform(StringBuilder css)
            {
                InsertCopyOfPropertyWithUrlReplacedWithDataUri(css);
            }

            void InsertCopyOfPropertyWithUrlReplacedWithDataUri(StringBuilder css)
            {
                var newBackgroundImageProperty = start + DataUri + end;
                var afterFullProperty = MatchIndex + fullProperty.Length;
                css.Insert(afterFullProperty, newBackgroundImageProperty);
            }
        }
    }
}