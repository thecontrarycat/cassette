using System.IO;
using System.Security.Cryptography;
using Cassette.IO;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class CssImageToMhtmlUriTransformer_Tests
    {
        public CssImageToMhtmlUriTransformer_Tests()
        {
            transformer = new CssImageToMhtmlUriTransformer(url => true);

            directory = new Mock<IDirectory>();
            asset = new Mock<IAsset>();
            var file = new Mock<IFile>();
            asset.SetupGet(a => a.SourceFile.FullPath)
                 .Returns("asset.css");
            asset.SetupGet(a => a.SourceFile)
                 .Returns(file.Object);
            file.SetupGet(f => f.Directory)
                .Returns(directory.Object);
        }

        UrlGenerator generator;
        readonly Mock<IAsset> asset;
        readonly Mock<IDirectory> directory;
        CssImageToMhtmlUriTransformer transformer;

        [Fact]
        public void TransformInsertsImageUrlWithMhtmlUriAfterTheExistingImage()
        {
            StubFile("test.png", new byte[] { 1, 2, 3 });
            
            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            getResult().ReadToEnd().ShouldEqual(
                "p { background-image: url(test.png);background-image: url(mhtml:http://example.org/test.css!testpng); }"
            );
        }

        [Fact]
        public void TransformInsertsImageUrlWithMhtmlUriAfterEachExistingImage()
        {
            StubFile("test1.png", new byte[] { 1, 2, 3 });
            StubFile("test2.png", new byte[] { 1, 2, 3 });

            var css = "p { background-image: url(test1.png); } " +
                      "a { background-image: url(test2.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            getResult().ReadToEnd().ShouldEqual(
                "p { background-image: url(test1.png);background-image: url(mhtml:http://example.org/test.css!test1png); } " +
                "a { background-image: url(test2.png);background-image: url(mhtml:http://example.org/test.css!test2png); }"
            );
        }

        [Fact]
        public void ImageUrlCanHaveSubDirectory()
        {
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/styles/jquery-ui/jquery-ui.css");
            asset.SetupGet(a => a.SourceFile.Directory).Returns(directory.Object);
            StubFile("images/test.png", new byte[] { 1, 2, 3 });

            var css = "p { background-image: url(images/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            getResult().ReadToEnd().ShouldEqual(
                "p { background-image: url(images/test.png);background-image: url(mhtml:http://example.org/test.css!imagestestpng); }"
            );
        }

        [Fact]
        public void GivenFileDoesNotExists_WhenTransform_ThenUrlIsNotChanged()
        {
            var file = new Mock<IFile>();
            file.SetupGet(f => f.Exists).Returns(false);
            directory.Setup(d => d.GetFile(It.IsAny<string>()))
                     .Returns(file.Object);

            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            getResult().ReadToEnd().ShouldEqual(
                "p { background-image: url(test.png); }"
            );
        }

        [Fact]
        public void GivenPredicateToTestImagePathReturnsFalse_WhenTransform_ThenImageIsNotTransformedToMhtmlUri()
        {
            transformer = new CssImageToMhtmlUriTransformer(path => false);

            StubFile("test.png", new byte[] { 1, 2, 3 });

            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            getResult().ReadToEnd().ShouldEqual(
                "p { background-image: url(test.png); }"
            );
        }

        void StubFile(string filename, byte[] bytes)
        {
            var file = new Mock<IFile>();
            directory.Setup(d => d.GetFile(filename))
                .Returns(file.Object);
            file.SetupGet(f => f.Directory)
                .Returns(directory.Object);
            file.SetupGet(f => f.Exists)
                .Returns(true);
            file.Setup(d => d.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                .Returns(() => new MemoryStream(bytes));
        }
    }
}